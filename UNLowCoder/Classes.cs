namespace UNLowCoder;

using System;
using System.Collections.Generic;
using System.Globalization;

[Flags]
public enum LocationFunction
{
    None = 0,
    Seaport = 1 << 0,     // Position 1 == '1'?
    RailTerminal = 1 << 1, // Position 2 == '2'
    RoadTerminal = 1 << 2, // Position 3 == '3'
    Airport = 1 << 3,      // Position 4 == '4'
    PostOffice = 1 << 4,   // Position 5 == '5'
    ICD = 1 << 5,          // Position 6 == '6'
    FixedTransport = 1 << 6 // Position 7 == '7'
}

public enum LocationStatus
{
    Unknown,
    AA, // Approved by competent national government agency
    AC, // Approved by Customs
    AI, // Code adopted by International Organization for Standardization (ISO)
    RL, // Recognized location
    RQ, // Request under consideration
    QQ  // Entry that does not meet UN/LOCODE criteria (e.g., obsolete)
    // Weitere Codes nach Bedarf ergänzen
}

public record Coordinates(
    double Latitude,
    double Longitude
)
{
    /// <summary>
    /// Berechnet die Distanz zwischen dieser Koordinate und einer Zielkoordinate in Kilometern
    /// unter Verwendung der Haversine-Formel.
    /// </summary>
    public double CalculateDistanceTo(Coordinates target)
    {
        // Radius der Erde in Kilometern
        const double EarthRadiusKm = 6371.0;

        double lat1Rad = ToRadians(this.Latitude);
        double lat2Rad = ToRadians(target.Latitude);
        double deltaLat = ToRadians(target.Latitude - this.Latitude);
        double deltaLon = ToRadians(target.Longitude - this.Longitude);

        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => (Math.PI / 180.0) * degrees;
}

public record UnLocodeSubdivision(
    string CountryCode,
    string SubdivisionCode,
    string Name,
    string Type
);

public record UnLocodeLocation(
    string CountryCode,
    string LocationCode,
    string Name,
    string NameWoDiacritics,
    string? SubdivisionCode,
    LocationStatus Status,
    LocationFunction Function,
    DateTime? LastUpdateDate, // statt string Date
    string? IATA,
    Coordinates? Coordinates,
    string? Remarks,
    ChangeIndicator Change
)
{
    public string FullUnLocode => CountryCode + LocationCode;
}

public enum ChangeIndicator
{
    None,
    Added,
    NameChanged,
    SubdivisionChanged,
    MarkedForDeletion,
    OtherChange,
    Unchanged
}


public record UnLocodeCountry(
    string CountryCode,
    string? CountryName,
    IReadOnlyList<UnLocodeSubdivision> Subdivisions,
    IReadOnlyList<UnLocodeLocation> Locations
)
{
    private RegionInfo? _regionInfo;
    public RegionInfo? RegionInfo
    {
        get
        {
            if (_regionInfo == null)
            {
                try
                {
                    _regionInfo = new RegionInfo(CountryCode);
                }
                catch
                {
                    _regionInfo = null; // falls ungültiger Code
                }
            }
            return _regionInfo;
        }
    }

    public CultureInfo? _cultureInfo;
    public CultureInfo? CultureInfo
    {
        get
        {

            try
            {
                return _cultureInfo ??= new CultureInfo(CountryCode);
            }
            catch
            {
                return null;
            }

        }
    }
}
