using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Nominatim.API.Models;

namespace UNLowCoder.Core.Data;

public partial record UnLocodeLocation
{
    [JsonIgnore]    
    public UnLocodeCountry Country => CountryResolverFunc?.Invoke();
    [JsonIgnore]
    public Func<UnLocodeCountry> CountryResolverFunc { get; set; }

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
    public GeocodeResponse GeocodeDetails { get; set; }
    public string? Remarks { get; }
    public ChangeIndicator Change { get; }

    public UnLocodeLocation()
    { }

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

    public double? DistanceTo(Coordinates target) => Coordinates?.DistanceTo(target);
    public double? DistanceTo(UnLocodeLocation target) => DistanceTo(target?.Coordinates);
    
    public async Task<GeocodeResponse?> GetGeocodeDetailsAsync(ReverseGeocodeRequest? request = null, bool forceReload = false)
    {
        if (forceReload || GeocodeDetails is null)
        {
            if (Coordinates is null) return null;
            var res = await Coordinates.GetGeocodeDetailsAsync(request);
            GeocodeDetails = res;
        }

        return GeocodeDetails;
    }
}
