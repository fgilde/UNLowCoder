using System;

namespace UNLowCoder.Core.Data;

public class UnLocodeLocation
{
    public string CountryCode { get; }
    public string LocationCode { get; }
    public string Name { get; }
    public string NameWoDiacritics { get; }
    public string? SubdivisionCode { get; }
    public LocationStatus Status { get; }
    public LocationFunction Function { get; }
    public DateTime? LastUpdateDate { get; }
    public string? IATA { get; }
    public Coordinates? Coordinates { get; }
    public string? Remarks { get; }
    public ChangeIndicator Change { get; }

    public UnLocodeLocation(
        string countryCode,
        string locationCode,
        string name,
        string nameWoDiacritics,
        string? subdivisionCode,
        LocationStatus status,
        LocationFunction function,
        DateTime? lastUpdateDate,
        string? iata,
        Coordinates? coordinates,
        string? remarks,
        ChangeIndicator change)
    {
        CountryCode = countryCode;
        LocationCode = locationCode;
        Name = name;
        NameWoDiacritics = nameWoDiacritics;
        SubdivisionCode = subdivisionCode;
        Status = status;
        Function = function;
        LastUpdateDate = lastUpdateDate;
        IATA = iata;
        Coordinates = coordinates;
        Remarks = remarks;
        Change = change;
    }

    public string FullUnLocode => CountryCode + LocationCode;
}
