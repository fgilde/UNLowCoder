using System.Collections.Generic;
using System.Globalization;

namespace UNLowCoder.Core.Data;

public record UnLocodeCountry
{
    public string CountryCode { get; }
    public string? CountryName { get; }
    public IReadOnlyList<UnLocodeSubdivision> Subdivisions { get; }
    public IReadOnlyList<UnLocodeLocation> Locations { get; }

    public UnLocodeCountry(
        string countryCode,
        string? countryName,
        IReadOnlyList<UnLocodeSubdivision> subdivisions,
        IReadOnlyList<UnLocodeLocation> locations)
    {
        CountryCode = countryCode;
        CountryName = countryName;
        Subdivisions = subdivisions;
        Locations = locations;
    }

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
