using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using CsvHelper;
using UNLowCoder.Core;
using UNLowCoder.Core.Data;
using UtfUnknown;

namespace UNLowCoder;

public static class UnLocodeParser
{
    /// <summary>
    /// Parse a ZIP archive containing the UN/LOCODE CSV files.
    /// </summary>
    /// <param name="filePath">Path of zip file</param>
    /// <param name="parseMode">Mode of parsing and dealing with duplicates</param>
    /// <returns></returns>
    public static List<UnLocodeCountry> ParseZipArchive(string filePath, ParseMode parseMode = ParseMode.AllEntries)
    {
        using var fs = File.OpenRead(filePath);
        return ParseZipStream(fs, parseMode);
    }

    /// <summary>
    /// Parse a ZIP archive containing the UN/LOCODE CSV files.
    /// </summary>
    /// <param name="zipStream">Stream of zip file</param>
    /// <param name="parseMode">Mode of parsing and dealing with duplicates</param>
    /// <returns></returns>
    public static List<UnLocodeCountry> ParseZipStream(Stream zipStream, ParseMode parseMode = ParseMode.AllEntries)
    {
        var onlyNewest = parseMode == ParseMode.OnlyNewest;
        var subdivisionsByCountry = new Dictionary<string, List<UnLocodeSubdivision>>();
        var countryNames = new Dictionary<string, string>();
        var locationsByCountry = new Dictionary<string, List<UnLocodeLocation>>();

        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
        {
            foreach (var entry in archive.Entries)
                if (entry.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    using var fileStream = entry.Open();
                    var fileNameLower = entry.Name.ToLowerInvariant();

                    if (fileNameLower.Contains("subdivision"))
                        ParseSubdivisionFile(fileStream, subdivisionsByCountry);
                    else if (fileNameLower.Contains("codelist"))
                        ParseCodeListFile(fileStream, countryNames, locationsByCountry);
                }
        }

        if (onlyNewest)
        {
            // Subdivisions without Date/Change: only filter duplicates if any occur.
            // We group by (Country, SubCode) and keep the last read entry.
            foreach (var kvp in subdivisionsByCountry.ToList())
            {
                var groupedSubs = kvp.Value
                    .GroupBy(s => s.CountryCode + "|" + s.SubdivisionCode)
                    .Select(g => g.Last()) // ohne Date/ChangeIndicator einfach den letzten nehmen
                    .ToList();
                subdivisionsByCountry[kvp.Key] = groupedSubs;
            }

            // Locations: only newest entry Country+LocationCode
            foreach (var kvp in locationsByCountry.ToList())
            {
                var grouped = kvp.Value
                    .GroupBy(l => l.CountryCode + "|" + l.LocationCode)
                    .Select(g =>
                    {
                        var sorted = g.OrderByDescending(l =>
                            l.LastUpdateDate.HasValue ? l.LastUpdateDate.Value : DateTime.MinValue).ToList();
                        var newest = sorted.First();
                        if (newest.Change == ChangeIndicator.MarkedForDeletion)
                            return new {g.Key, Loc = (UnLocodeLocation) null};
                        return new {g.Key, Loc = newest};
                    })
                    .Where(x => x.Loc != null)
                    .Select(x => x.Loc)
                    .ToList();


                locationsByCountry[kvp.Key] = grouped;
            }
        }

        var countries = new List<UnLocodeCountry>();

        // Now we build the countries together
        // All subdivisions per country + all locations per country
        // If a country has no subdivisions, we take an empty list
        foreach (var countryCode in locationsByCountry.Keys.OrderBy(c => c))
        {
            List<UnLocodeSubdivision> subs;
            if (!subdivisionsByCountry.TryGetValue(countryCode, out subs))
                subs = new List<UnLocodeSubdivision>();

            var locs = locationsByCountry[countryCode];
            countryNames.TryGetValue(countryCode, out var countryName);

            var countryObj = new UnLocodeCountry(
                countryCode,
                countryName,
                subs,
                locs
            );

            countries.Add(countryObj);
        }

        // It is possible that there are countries without locations but with subdivisions.
        // These would currently not be recorded. So process these as well:
        foreach (var countryCode in subdivisionsByCountry.Keys)
            if (!locationsByCountry.ContainsKey(countryCode))
            {
                // Land hat nur Subdivisions, keine Locations
                var subs = subdivisionsByCountry[countryCode];
                countryNames.TryGetValue(countryCode, out var countryName);

                var countryObj = new UnLocodeCountry(
                    countryCode,
                    countryName,
                    subs,
                    new List<UnLocodeLocation>()
                );

                countries.Add(countryObj);
            }

        return countries;
    }

    private static void ParseSubdivisionFile(Stream subdivisionStream,
        Dictionary<string, List<UnLocodeSubdivision>> subdivisionsByCountry)
    {
        var memoryStream = new MemoryStream();
        subdivisionStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        var encoding = DetectEncoding(memoryStream);
        memoryStream.Position = 0;

        using (var reader = new StreamReader(memoryStream, encoding))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
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
                if (!subdivisionsByCountry.TryGetValue(country, out var list))
                {
                    list = new List<UnLocodeSubdivision>();
                    subdivisionsByCountry[country] = list;
                }

                list.Add(subdivision);
            }
        }
    }

    private static Encoding DetectEncoding(Stream stream)
    {
        stream.Position = 0;
        var result = CharsetDetector.DetectFromStream(stream);
        return result?.Detected?.Encoding ?? Encoding.UTF8;
    }

    private static ChangeIndicator ParseChangeIndicator(string changeField)
    {
        if (string.IsNullOrEmpty(changeField))
            return ChangeIndicator.None;

        switch (changeField)
        {
            case "+":
                return ChangeIndicator.Added;
            case "#":
                return ChangeIndicator.NameChanged;
            case "¤":
                return ChangeIndicator.SubdivisionChanged;
            case "X":
                return ChangeIndicator.MarkedForDeletion;
            case "*":
                return ChangeIndicator.OtherChange;
            case "=":
                return ChangeIndicator.Unchanged;
            default:
                return ChangeIndicator.None;
        }
    }

    private static void ParseCodeListFile(
        Stream codeListStream,
        Dictionary<string, string> countryNames,
        Dictionary<string, List<UnLocodeLocation>> locationsByCountry)
    {
        var memoryStream = new MemoryStream();
        codeListStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        var encoding = DetectEncoding(memoryStream);
        memoryStream.Position = 0;

        using (var reader = new StreamReader(memoryStream, encoding))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            while (csv.Read())
            {
                var record = csv.Parser.Record;
                if (record == null || record.Length < 12) continue;

                var changeStr = GetSafeField(csv, 0);
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

                if (!string.IsNullOrEmpty(country) && !string.IsNullOrEmpty(name) && name.StartsWith("."))
                {
                    var cn = name.TrimStart('.');
                    if (!string.IsNullOrEmpty(cn)) countryNames[country] = cn;
                    continue;
                }

                if (string.IsNullOrEmpty(locationCode) || string.IsNullOrEmpty(country))
                    continue;

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
    }

    private static string GetSafeField(CsvReader csv, int index)
    {
        if (index < csv.Parser.Record.Length)
        {
            var val = csv.GetField<string>(index);
            return val == null ? null : val.Trim();
        }

        return null;
    }

    private static Coordinates? ParseCoordinates(string coords)
    {
        var parts = coords.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return null;

        double ParseCoordPart(string part)
        {
            var dir = part[part.Length - 1];
            var numericPart = part.Substring(0, part.Length - 1);

            var degreeLength = numericPart.Length > 4 ? 3 : 2;
            var degreesStr = numericPart.Substring(0, degreeLength);
            var minutesStr = numericPart.Substring(degreeLength);

            var degrees = double.Parse(degreesStr, CultureInfo.InvariantCulture);
            var minutes = double.Parse(minutesStr, CultureInfo.InvariantCulture);
            var decimalVal = degrees + minutes / 60.0;

            if (dir == 'S' || dir == 'W')
                decimalVal = -decimalVal;

            return decimalVal;
        }

        var lat = ParseCoordPart(parts[0]);
        var lon = ParseCoordPart(parts[1]);

        return new Coordinates(lat, lon);
    }

    private static LocationStatus ParseStatus(string statusStr)
    {
        if (string.IsNullOrEmpty(statusStr))
            return LocationStatus.Unknown;

        LocationStatus status;
        if (Enum.TryParse(statusStr, out status))
            return status;

        return LocationStatus.Unknown;
    }

    private static LocationFunction ParseFunction(string functionStr)
    {
        if (string.IsNullOrEmpty(functionStr))
            return LocationFunction.None;

        var func = LocationFunction.None;

        for (var i = 0; i < functionStr.Length && i < 7; i++)
        {
            var c = functionStr[i];
            if (c == '1')
                func |= (LocationFunction) (1 << i);
        }

        return func;
    }

    private static DateTime? ParseDate(string dateStr)
    {
        if (string.IsNullOrEmpty(dateStr) || dateStr.Length != 4)
            return null;
        int yy, mm;
        if (!int.TryParse(dateStr.Substring(0, 2), out yy))
            return null;
        if (!int.TryParse(dateStr.Substring(2, 2), out mm))
            return null;

        if (mm < 1 || mm > 12)
            return null;

        var year = yy < 50 ? 2000 + yy : 1900 + yy;

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