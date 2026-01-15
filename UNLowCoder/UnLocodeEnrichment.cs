namespace UNLowCoder.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Data;

public static class UnLocodeEnrichment
{
    public static List<UnLocodeCountry> EnrichCoordinates(
        List<UnLocodeCountry> countries,
        List<UnLocodeCountry> improvedCountries)
    {
        var coordByCode = improvedCountries
            .SelectMany(c => c.Locations)
            .Where(l => l.Coordinates != null)
            .GroupBy(l => l.FullUnLocode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Coordinates!, StringComparer.OrdinalIgnoreCase);

        var enriched = countries
            .Select(country =>
            {
                var newLocations = country.Locations
                    .Select(loc =>
                    {
                        if (loc.Coordinates != null)
                            return loc;

                        if (coordByCode.TryGetValue(loc.FullUnLocode, out var coords))
                        {
                            var copy = loc with { Coordinates = coords };
                            copy.CountryResolverFunc = loc.CountryResolverFunc;
                            copy.SubdivisionResolverFunc = loc.SubdivisionResolverFunc;

                            return copy;
                        }

                        return loc;
                    })
                    .ToArray();
                var c2 = country with { Locations = newLocations };

                return c2;
            })
            .ToList();

        return enriched;
    }
}
