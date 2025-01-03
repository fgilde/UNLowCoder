using Nextended.Core.Types;
using System.Collections.Generic;
using System.Globalization;

namespace UNLowCoder.Core.Data;

public partial record UnLocodeCountry
{
    public string CountryCode { get; }
    public string? CountryName { get; }
    public IReadOnlyList<UnLocodeSubdivision> Subdivisions { get; set; }
    public IReadOnlyList<UnLocodeLocation> Locations { get; set; }

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

    private Currency? _currency;
    public Currency? Currency => RegionInfo != null ? (_currency ??= Currency.Find(RegionInfo.CurrencySymbol) ?? Currency.Find(RegionInfo.ISOCurrencySymbol)) : null;

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
                    _regionInfo = null; 
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
