using Nager.Country;
using System.Collections.Generic;
using System.Linq;
using UNLowCoder.Core.Data;
using UNLowCoder.Extensions.Classes;

namespace UNLowCoder.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<UnLocodeCountry> Filter(this IEnumerable<UnLocodeCountry> countries, Region region) 
        => countries.Where(c => c.CountryInfo()?.Region == region);

    public static IEnumerable<UnLocodeCountry> Filter(this IEnumerable<UnLocodeCountry> countries, Region region, SubRegion subRegion)
        => countries.Where(c => c.CountryInfo()?.Region == region && c.CountryInfo()?.SubRegion == subRegion);

    public static IEnumerable<UnLocodeCountry> Filter(this IEnumerable<UnLocodeCountry> countries,  SubRegion subRegion)
        => countries.Where(c => c.CountryInfo()?.SubRegion == subRegion);

    public static IEnumerable<UnLocodeCountry> Filter(this IEnumerable<UnLocodeCountry> countries, ContinentInfo continent)
        => countries.Where(c => c.Continent() == continent);

    public static IEnumerable<UnLocodeCountry> GetAllCountries(this IEnumerable<UnLocodeLocation> locations) => locations.Select(s => s.Country).Distinct();
    public static IEnumerable<UnLocodeCountry> GetAllCountries(this IEnumerable<UnLocodeSubdivision> subdivisions) => subdivisions.Select(s => s.Country).Distinct();
}