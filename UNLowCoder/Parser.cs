using UtfUnknown;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using CsvHelper;

namespace UNLowCoder;

public static class UnLocodeParser
{
    public static List<UnLocodeCountry> ParseZipArchive(string filePath)
    {
        using var fs = File.OpenRead(filePath);
        return ParseZipStream(fs);
    }

    public static List<UnLocodeCountry> ParseZipStream(Stream zipStream)
    {
        var subdivisions = new Dictionary<(string country, string subCode), UnLocodeSubdivision>();
        var countryNames = new Dictionary<string, string>();
        var locationsByCountry = new Dictionary<string, List<UnLocodeLocation>>();

        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            foreach (var entry in archive.Entries)
            {
                if (entry.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    using var fileStream = entry.Open();
                    var fileNameLower = entry.Name.ToLowerInvariant();

                    if (fileNameLower.Contains("subdivision"))
                    {
                        ParseSubdivisionFile(fileStream, subdivisions);
                    }
                    else if (fileNameLower.Contains("codelist"))
                    {
                        ParseCodeListFile(fileStream, countryNames, locationsByCountry);
                    }
                }
            }
        }

        var countries = new List<UnLocodeCountry>();

        foreach (var kvp in locationsByCountry)
        {
            var countryCode = kvp.Key;
            var allLocations = kvp.Value;

            var subs = subdivisions
                .Where(x => x.Key.country == countryCode)
                .Select(x => x.Value)
                .ToList();

            countryNames.TryGetValue(countryCode, out var countryName);

            var countryObj = new UnLocodeCountry(
                countryCode,
                countryName,
                subs,
                allLocations
            );

            countries.Add(countryObj);
        }

        return countries;
    }

    private static void ParseSubdivisionFile(Stream subdivisionStream, Dictionary<(string country, string subCode), UnLocodeSubdivision> subdivisions)
    {
        using var memoryStream = new MemoryStream();
        subdivisionStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        var encoding = DetectEncoding(memoryStream);
        memoryStream.Position = 0;

        using var reader = new StreamReader(memoryStream, encoding);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        while (csv.Read())
        {
            if (csv.Parser.Record.Length < 4)
                continue;

            var country = csv.GetField<string>(0);
            var subCode = csv.GetField<string>(1);
            var name = csv.GetField<string>(2);
            var type = csv.GetField<string>(3);

            if (string.IsNullOrEmpty(country) || string.IsNullOrEmpty(subCode))
                continue;

            var subdivision = new UnLocodeSubdivision(country, subCode, name, type);
            subdivisions[(country, subCode)] = subdivision;
        }
    }

    private static Encoding DetectEncoding(Stream stream)
    {
        DetectionResult? result = CharsetDetector.DetectFromStream(stream);
        return result?.Detected?.Encoding ?? Encoding.UTF8;
    }

    private static ChangeIndicator ParseChangeIndicator(string? changeField)
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


    private static void ParseCodeListFile(
        Stream codeListStream,
        Dictionary<string, string> countryNames,
        Dictionary<string, List<UnLocodeLocation>> locationsByCountry)
    {
        using var memoryStream = new MemoryStream();
        codeListStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        var encoding = DetectEncoding(memoryStream);
        memoryStream.Position = 0;

        using var reader = new StreamReader(memoryStream, encoding);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        while (csv.Read())
        {
            var record = csv.Parser.Record;
            if (record == null || record.Length < 12) continue;

            var changeStr = GetSafeField(csv, 0); // falls Change im ersten Feld ist
            var changeIndicator = ParseChangeIndicator(changeStr);
            var country = GetSafeField(csv, 1);
            var locationCode = GetSafeField(csv, 2);
            var name = GetSafeField(csv, 3);
            var nameWo = GetSafeField(csv, 4);
            var subdivisionCode = GetSafeField(csv, 5);
            var statusStr = GetSafeField(csv, 6);
            var functionStr = GetSafeField(csv, 7);
            var dateStr = GetSafeField(csv, 8);
            var iata = GetSafeField(csv, 9);
            var coordsString = GetSafeField(csv, 10);
            var remarks = GetSafeField(csv, 11);

            // Erkennen von Länderzeilen
            var countryNameField = GetSafeField(csv, 3);
            if (!string.IsNullOrEmpty(country) && !string.IsNullOrEmpty(countryNameField) && countryNameField.StartsWith("."))
            {
                var cn = countryNameField.TrimStart('.');
                if (!string.IsNullOrEmpty(cn))
                {
                    countryNames[country] = cn;
                }
                continue;
            }

            // Wenn kein LocationCode oder kein Country, überspringen
            if (string.IsNullOrEmpty(locationCode) || string.IsNullOrEmpty(country))
            {
                continue;
            }

            var coords = !string.IsNullOrEmpty(coordsString) ? ParseCoordinates(coordsString) : null;
            var status = ParseStatus(statusStr);
            var function = ParseFunction(functionStr);
            var lastUpdate = ParseDate(dateStr);

            var loc = new UnLocodeLocation(
                country,
                locationCode,
                name ?? string.Empty,
                nameWo ?? string.Empty,
                subdivisionCode,
                status,
                function,
                lastUpdate,
                iata,
                coords,
                remarks,
                changeIndicator
            );

            if (!locationsByCountry.TryGetValue(country, out var locList))
            {
                locList = new List<UnLocodeLocation>();
                locationsByCountry[country] = locList;
            }
            locList.Add(loc);
        }
    }

    private static string? GetSafeField(CsvReader csv, int index)
    {
        if (index < csv.Parser.Record.Length)
            return csv.GetField<string>(index)?.Trim();
        return null;
    }

    private static Coordinates? ParseCoordinates(string coords)
    {
        var parts = coords.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return null;

        double ParseCoordPart(string part)
        {
            char dir = part[^1];
            var numericPart = part.Substring(0, part.Length - 1);

            int degreeLength = numericPart.Length > 4 ? 3 : 2;
            var degreesStr = numericPart.Substring(0, degreeLength);
            var minutesStr = numericPart.Substring(degreeLength);

            double degrees = double.Parse(degreesStr, CultureInfo.InvariantCulture);
            double minutes = double.Parse(minutesStr, CultureInfo.InvariantCulture);
            double decimalVal = degrees + minutes / 60.0;

            if (dir == 'S' || dir == 'W')
                decimalVal = -decimalVal;

            return decimalVal;
        }

        double lat = ParseCoordPart(parts[0]);
        double lon = ParseCoordPart(parts[1]);

        return new Coordinates(lat, lon);
    }

    private static LocationStatus ParseStatus(string? statusStr)
    {
        if (string.IsNullOrEmpty(statusStr))
            return LocationStatus.Unknown;

        // Versuche direkt zu parsen, wenn Enum so benannt ist wie Code:
        if (Enum.TryParse<LocationStatus>(statusStr, out var status))
            return status;

        return LocationStatus.Unknown;
    }

    private static LocationFunction ParseFunction(string? functionStr)
    {
        // functionStr z.B. "1-------"
        // Jede Stelle 1-7 repräsentiert ein Merkmal:
        // Position 1: 1 für Seaport
        // Position 2: 2 für RailTerminal etc.
        if (string.IsNullOrEmpty(functionStr))
            return LocationFunction.None;

        var func = LocationFunction.None;

        for (int i = 0; i < functionStr.Length && i < 7; i++)
        {
            char c = functionStr[i];
            if (c == '1')
                func |= (LocationFunction)(1 << i);
        }

        return func;
    }

    private static DateTime? ParseDate(string? dateStr)
    {
        // Format: YYMM (z.B. "0901" -> 2009-01, "9912" -> 1999-12)
        if (string.IsNullOrEmpty(dateStr) || dateStr.Length != 4)
            return null;

        if (!int.TryParse(dateStr.Substring(0, 2), out int yy))
            return null;
        if (!int.TryParse(dateStr.Substring(2, 2), out int mm))
            return null;

        if (mm < 1 || mm > 12)
            return null;

        int year = (yy < 50) ? (2000 + yy) : (1900 + yy);

        try
        {
            return new DateTime(year, mm, 1, 0, 0, 0, DateTimeKind.Unspecified);
        }
        catch
        {
            return null;
        }
    }
}
