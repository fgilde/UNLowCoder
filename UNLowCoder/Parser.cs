using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UNLowCoder.Core;
using UNLowCoder.Core.Data;

namespace UNLowCoder;

public static class UnLocodeParser
{
    public static List<UnLocodeCountry> ParseCsvFile(string file)
    {
        if(!File.Exists(file))
            throw new FileNotFoundException();
        return ParseCsvFile(File.OpenRead(file));
    }

    public static Task<List<UnLocodeCountry>> ParseCsvFileAsync(string file, CancellationToken ct = default)
    {
        return Task.Run(() => ParseCsvFile(file), ct);
    }

    public static Task<List<UnLocodeCountry>> ParseCsvFileAsync(Stream csvFileStream, CancellationToken ct = default)
    {
        return Task.Run(() => ParseCsvFile(csvFileStream), ct);
    }

    public static List<UnLocodeCountry> ParseCsvFile(Stream csvFileStream)
    {
        using var memoStream = new MemoryStream();

        using (var zip = new ZipArchive(memoStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = zip.CreateEntry("codelist.csv");

            using var entryStream = entry.Open();
            using var fileStream = csvFileStream;

            fileStream.CopyTo(entryStream);
        }

        memoStream.Position = 0;
        return ParseZipStream(memoStream);
    }

    /// <summary>
    /// Parse a ZIP archive containing the UN/LOCODE CSV files.
    /// </summary>
    public static Task<List<UnLocodeCountry>> ParseZipArchiveAsync(string filePath, ParseMode parseMode = ParseMode.AllEntries, CancellationToken ct = default)
    {
        using var fs = File.OpenRead(filePath);
        return ParseZipStreamAsync(fs, parseMode, ct);
    }

    /// <summary>
    /// Parse a ZIP archive containing the UN/LOCODE CSV files.
    /// </summary>
    public static List<UnLocodeCountry> ParseZipArchive(string filePath, ParseMode parseMode = ParseMode.AllEntries)
    {
        using var fs = File.OpenRead(filePath);
        return ParseZipStream(fs, parseMode);
    }

    public static Task<List<UnLocodeCountry>> ParseZipStreamAsync(Stream zipStream, ParseMode parseMode = ParseMode.AllEntries, CancellationToken ct = default)
        => Task.Run(() => ParseZipStream(zipStream, parseMode), ct);

    /// <summary>
    /// Parse a ZIP archive stream containing the UN/LOCODE CSV files.
    /// </summary>
    public static List<UnLocodeCountry> ParseZipStream(Stream zipStream, ParseMode parseMode = ParseMode.AllEntries)
    {
        var onlyNewest = parseMode == ParseMode.OnlyNewest;
        var subdivisionsByCountry = new Dictionary<string, List<UnLocodeSubdivision>>();
        var countryNames = new Dictionary<string, string>();
        var locationsByCountry = new Dictionary<string, List<UnLocodeLocation>>();

        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            // Combined vorhanden? Dann Parts überspringen
            var hasCombined = archive.Entries.Any(e => e.Name.Equals("CodeList.csv", StringComparison.OrdinalIgnoreCase));

            foreach (var entry in archive.Entries)
            {
                if (!entry.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    continue;

                var fileNameLower = entry.Name.ToLowerInvariant();

                if (fileNameLower.Contains("subdivision"))
                {
                    using var s = entry.Open();
                    ParseSubdivisionFile(s, subdivisionsByCountry);
                }
                else if (fileNameLower.Contains("codelist") || fileNameLower.Contains("code-list"))
                {
                    if (hasCombined && fileNameLower.Contains("part"))
                        continue; // Doppelzählung vermeiden

                    using var s = entry.Open();
                    ParseCodeListFile(s, countryNames, locationsByCountry);
                }
            }
        }

        if (onlyNewest)
        {
            // Subdivisions: ohne Datum -> bei Duplikaten den letzten nehmen
            foreach (var kvp in subdivisionsByCountry.ToList())
            {
                subdivisionsByCountry[kvp.Key] = kvp.Value
                    .GroupBy(s => s.CountryCode + "|" + s.SubdivisionCode)
                    .Select(g => g.Last())
                    .ToList();
            }

            // Locations: pro (Country, LocationCode) nur den neuesten behalten; X (= deletion) entfernen
            foreach (var kvp in locationsByCountry.ToList())
            {
                var grouped = kvp.Value
                    .GroupBy(l => l.CountryCode + "|" + l.LocationCode)
                    .Select(g =>
                    {
                        var newest = g.OrderByDescending(l => l.LastUpdateDate ?? DateTime.MinValue).First();
                        return newest.Change == ChangeIndicator.MarkedForDeletion ? null : newest;
                    })
                    .Where(x => x != null)
                    .ToList()!;
                locationsByCountry[kvp.Key] = grouped;
            }
        }

        if (locationsByCountry.ContainsKey("Country")) locationsByCountry.Remove("Country");
        if (subdivisionsByCountry.ContainsKey("Country")) subdivisionsByCountry.Remove("Country");

        var countries = new List<UnLocodeCountry>();

        foreach (var countryCode in locationsByCountry.Keys.OrderBy(c => c))
        {
            if (!subdivisionsByCountry.TryGetValue(countryCode, out var subs))
                subs = new List<UnLocodeSubdivision>();

            var locs = locationsByCountry[countryCode];
            countryNames.TryGetValue(countryCode, out var countryName);

            var countryObj = new UnLocodeCountry(countryCode, countryName, subs, locs);
            subs.ForEach(s => s.CountryResolverFunc = () => countryObj);
            locs.ForEach(l => l.CountryResolverFunc = () => countryObj);

            countries.Add(countryObj);
        }

        // Länder, die nur Subdivisions, aber keine Locations haben
        foreach (var countryCode in subdivisionsByCountry.Keys)
        {
            if (locationsByCountry.ContainsKey(countryCode)) continue;

            var subs = subdivisionsByCountry[countryCode];
            countryNames.TryGetValue(countryCode, out var countryName);

            var countryObj = new UnLocodeCountry(countryCode, countryName, subs, new List<UnLocodeLocation>());
            subs.ForEach(s => s.CountryResolverFunc = () => countryObj);
            countries.Add(countryObj);
        }

        return countries;
    }


    private static IEnumerable<string[]> ReadCsvRecords(TextReader reader)
    {
        // CSV ohne Header, Trennzeichen ',', Quotes '"'
        // Unterstützt: Felder in Anführungszeichen, doppelte Quotes ("") als Escapes,
        // Kommas und Zeilenumbrüche innerhalb gequoteter Felder.

        var sb = new StringBuilder();
        var fields = new List<string>();
        bool inQuotes = false;
        int c;
        while (true)
        {
            c = reader.Read();
            if (c == -1)
            {
                // EOF
                if (inQuotes)
                {
                    // Unsauberer Abschluss -> Feld abschließen
                    fields.Add(sb.ToString());
                    sb.Clear();
                    inQuotes = false;
                }

                if (fields.Count > 0 || sb.Length > 0)
                {
                    fields.Add(sb.ToString());
                    yield return fields.ToArray();
                }
                yield break;
            }

            char ch = (char)c;

            if (inQuotes)
            {
                if (ch == '"')
                {
                    int next = reader.Peek();
                    if (next == '"')
                    {
                        // Escaped Quote
                        reader.Read(); // verbrauche das zweite "
                        sb.Append('"');
                    }
                    else
                    {
                        // Quote schließt Feld
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(ch);
                }
            }
            else
            {
                if (ch == ',')
                {
                    fields.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else if (ch == '\r')
                {
                    // ignoriere, Zeilenende wird durch \n gehandhabt
                }
                else if (ch == '\n')
                {
                    fields.Add(sb.ToString().Trim());
                    sb.Clear();
                    yield return fields.ToArray();
                    fields.Clear();
                }
                else if (ch == '"')
                {
                    inQuotes = true;
                }
                else
                {
                    sb.Append(ch);
                }
            }
        }
    }

    // ---------------- Subdivisions ----------------

    private static void ParseSubdivisionFile(Stream subdivisionStream, Dictionary<string, List<UnLocodeSubdivision>> subdivisionsByCountry)
    {
        var memoryStream = new MemoryStream();
        subdivisionStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        var encoding = DetectEncoding(memoryStream);
        memoryStream.Position = 0;

        using var reader = new StreamReader(memoryStream, encoding);
        foreach (var record in ReadCsvRecords(reader))
        {
            if (record.Length < 4) continue;

            var country = SafeField(record, 0);
            var subCode = SafeField(record, 1);
            var name = SafeField(record, 2);
            var type = SafeField(record, 3);

            if (string.IsNullOrEmpty(country) || string.IsNullOrEmpty(subCode))
                continue;

            var subdivision = new UnLocodeSubdivision(country, subCode, name, type);
            if (!subdivisionsByCountry.TryGetValue(country, out var list))
            {
                list = new List<UnLocodeSubdivision>();
                subdivisionsByCountry[country] = list;
            }
            list.Add(subdivision);
        }
    }

    // ---------------- CodeList ----------------

    private static void ParseCodeListFile(Stream codeListStream, Dictionary<string, string> countryNames, Dictionary<string, List<UnLocodeLocation>> locationsByCountry)
    {
        var memoryStream = new MemoryStream();
        codeListStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        var encoding = DetectEncoding(memoryStream);
        memoryStream.Position = 0;

        using var reader = new StreamReader(memoryStream, encoding);
        foreach (var record in ReadCsvRecords(reader))
        {
            if (record.Length < 12) continue;

            var changeStr = SafeField(record, 0);
            var country = SafeField(record, 1);
            var locationCode = SafeField(record, 2);
            var name = SafeField(record, 3);
            var nameWo = SafeField(record, 4);
            var subdivision = SafeField(record, 5);
            var col6 = SafeField(record, 6);
            var col7 = SafeField(record, 7);
            var dateStr = SafeField(record, 8);
            var iata = SafeField(record, 9);
            var coordsString = SafeField(record, 10);
            var remarks = SafeField(record, 11);

            // Ländername-Zeile (".Germany") – aktualisiert countryNames und weiter
            if (!string.IsNullOrEmpty(country) && !string.IsNullOrEmpty(name) && name.StartsWith(".", StringComparison.Ordinal))
            {
                var cn = name.TrimStart('.');
                if (!string.IsNullOrEmpty(cn))
                    countryNames[country] = cn;
                continue;
            }

            if (string.IsNullOrEmpty(locationCode) || string.IsNullOrEmpty(country))
                continue;

            // Spaltenheuristik: Function/Status oder Status/Function?
            string functionStr, statusStr;
            if (LooksLikeFunction(col6) && LooksLikeStatus(col7))
            {
                functionStr = col6; statusStr = col7;
            }
            else if (LooksLikeFunction(col7) && LooksLikeStatus(col6))
            {
                functionStr = col7; statusStr = col6;
            }
            else
            {
                // Fallback: historisches Combined-Format
                statusStr = col6; functionStr = col7;
            }

            var changeIndicator = ParseChangeIndicator(changeStr);
            var status = ParseStatus(statusStr);
            var function = ParseFunction(functionStr);
            var coords = !string.IsNullOrEmpty(coordsString) ? ParseCoordinates(coordsString) : null;
            var lastUpdate = ParseDate(dateStr);

            var loc = new UnLocodeLocation(
                country,
                locationCode,
                name ?? string.Empty,
                nameWo ?? string.Empty,
                subdivision,
                status,
                function,
                lastUpdate,
                iata,
                coords,
                remarks,
                changeIndicator
            );

            if (!locationsByCountry.TryGetValue(country, out var list))
            {
                list = new List<UnLocodeLocation>();
                locationsByCountry[country] = list;
            }
            list.Add(loc);
        }
    }

    private static string SafeField(string[] record, int index)
    {
        if (record != null && index >= 0 && index < record.Length)
            return string.IsNullOrWhiteSpace(record[index]) ? null : record[index].Trim();
        return null;
    }

    private static Encoding DetectEncoding(Stream stream)
    {
        //stream.Position = 0;
        //var result = CharsetDetector.DetectFromStream(stream);
        //return result?.Detected?.Encoding ?? Encoding.UTF8;
        return Encoding.UTF8;
    }

    private static bool LooksLikeFunction(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim();
        if (s.Length < 7 || s.Length > 8) return false;
        foreach (var ch in s)
        {
            if (!(ch == '-' || char.IsDigit(ch) || ch == 'B'))
                return false;
        }
        return true;
    }

    private static bool LooksLikeStatus(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        s = s.Trim();
        if (s.Length > 3) return false;
        return s.All(char.IsLetter);
    }

    private static ChangeIndicator ParseChangeIndicator(string changeField)
    {
        if (string.IsNullOrEmpty(changeField))
            return ChangeIndicator.None;

        return changeField switch
        {
            "+" => ChangeIndicator.Added,
            "#" => ChangeIndicator.NameChanged,
            "¤" => ChangeIndicator.SubdivisionChanged,
            "X" => ChangeIndicator.MarkedForDeletion,
            "*" => ChangeIndicator.OtherChange,
            "=" => ChangeIndicator.Unchanged,
            _ => ChangeIndicator.None
        };
    }
    private static Coordinates? ParseCoordinates(string coords)
    {
        var parts = coords.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return null;

        double lat = ParseCoordPart(parts[0]);
        double lon = ParseCoordPart(parts[1]);
        return new Coordinates(lat, lon);
    }

    private static double ParseCoordPart(string part)
    {
        var dir = part[part.Length - 1];        // statt part[^1]
        var numericPart = part.Substring(0, part.Length - 1);

        var degreeLength = numericPart.Length > 4 ? 3 : 2;
        var degreesStr = numericPart.Substring(0, degreeLength);
        var minutesStr = numericPart.Substring(degreeLength);

        var degrees = double.Parse(degreesStr, CultureInfo.InvariantCulture);
        var minutes = double.Parse(minutesStr, CultureInfo.InvariantCulture);
        var decimalVal = degrees + minutes / 60.0;

        if (dir == 'S' || dir == 'W') decimalVal = -decimalVal;
        return decimalVal;
    }


    private static LocationStatus ParseStatus(string statusStr)
    {
        if (string.IsNullOrEmpty(statusStr))
            return LocationStatus.Unknown;

        return Enum.TryParse(statusStr, out LocationStatus status)
            ? status
            : LocationStatus.Unknown;
    }

    private static LocationFunction ParseFunction(string functionStr)
    {
        if (string.IsNullOrWhiteSpace(functionStr))
            return LocationFunction.None;

        functionStr = functionStr.Trim();
        if (functionStr.Length < 7) // üblich: 7..8 Zeichen; 8. evtl. 'B'
            return LocationFunction.None;

        var func = LocationFunction.None;
        for (var i = 0; i < 7 && i < functionStr.Length; i++)
        {
            var ch = functionStr[i];
            if (char.IsDigit(ch) && ch != '0')
                func |= (LocationFunction)(1 << i);
        }
        return func;
    }

    private static DateTime? ParseDate(string dateStr)
    {
        if (string.IsNullOrEmpty(dateStr) || dateStr.Length != 4)
            return null;

        int yy, mm;
        if (!int.TryParse(dateStr.Substring(0, 2), out yy)) return null;
        if (!int.TryParse(dateStr.Substring(2, 2), out mm)) return null;
        if (mm < 1 || mm > 12) return null;

        var year = yy < 50 ? 2000 + yy : 1900 + yy;
        try { return new DateTime(year, mm, 1, 0, 0, 0, DateTimeKind.Unspecified); }
        catch { return null; }
    }
}
