using Nager.Country;
using System.Collections.Generic;
using System.Linq;
using UNLowCoder.Core.Data;

namespace UNLowCoder.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<UnLocodeCountry> Filter(this IEnumerable<UnLocodeCountry> countries, params Region[] regions)
        => countries.Where(c => regions.Any(r => r == c.CountryInfo()?.Region));        

    public static IEnumerable<UnLocodeCountry> Filter(this IEnumerable<UnLocodeCountry> countries, Region region, SubRegion subRegion)
        => countries.Where(c => c.CountryInfo()?.Region == region && c.CountryInfo()?.SubRegion == subRegion);

    public static IEnumerable<UnLocodeCountry> Filter(this IEnumerable<UnLocodeCountry> countries,  params SubRegion[] subRegions)
        => countries.Where(c => subRegions.Any(s => s == c.CountryInfo()?.SubRegion));

    public static IEnumerable<UnLocodeCountry> Filter(this IEnumerable<UnLocodeCountry> countries, params string[] alpha2Or3Codes)
    {
        var provider = new CountryProvider();        
        return countries.Filter(alpha2Or3Codes.Select(c => provider.GetCountry(c)).ToArray());
    }

    public static IEnumerable<UnLocodeCountry> Filter(this IEnumerable<UnLocodeCountry> countries, params Alpha2Code[] codes)
    {
        var provider = new CountryProvider();        
        return countries.Filter(codes.Select(c => provider.GetCountry(c)).ToArray());
    }

    public static IEnumerable<UnLocodeCountry> Filter(this IEnumerable<UnLocodeCountry> countries, params Alpha3Code[] codes)
    {
        var provider = new CountryProvider();
        return countries.Filter(codes.Select(c => provider.GetCountry(c)).ToArray());
    }

    public static IEnumerable<UnLocodeCountry> Filter(this IEnumerable<UnLocodeCountry> countries, params ICountryInfo[] countryInfos)
        => countries.Where(c => countryInfos.Any(ci => CountryEqualityComparer.Instance.Equals(ci, c.CountryInfo())));        

    public static IEnumerable<UnLocodeCountry> GetAllCountries(this IEnumerable<UnLocodeLocation> locations) => locations.Select(s => s.Country).Distinct();
    public static IEnumerable<UnLocodeCountry> GetAllCountries(this IEnumerable<UnLocodeSubdivision> subdivisions) => subdivisions.Select(s => s.Country).Distinct();
}