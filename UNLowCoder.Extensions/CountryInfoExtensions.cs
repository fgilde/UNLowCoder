using System;
using Nager.Country;
using Nextended.Core.Helper;

namespace UNLowCoder.Extensions;

public static class CountryInfoExtensions
{
    public static string RegionName(this ICountryInfo country)
    {
        var subName = country.SubRegion != SubRegion.None ? $" ({country.SubRegion.ToDescriptionString()})" : string.Empty; 
        return $"{Enum.GetName(typeof(Region), country.Region)}{subName}";
    }

    public static string[] AllAvailableOsmRegions()
    {
        return
        [
            "europe",
            "asia",
            "africa",
            "australia-oceania",
            "north-america",
            "central-america",
            "south-america",
            "antarctica"
        ];
    }

    public static string OsmRegionName(this ICountryInfo country)
    {
        return country.Region switch
        {
            Region.Europe => "europe",
            Region.Asia => "asia",
            Region.Africa => "africa",
            Region.Oceania => "australia-oceania",
            Region.Americas => country.SubRegion switch
            {
                SubRegion.NorthAmerica => "north-america",
                SubRegion.CentralAmerica => "central-america",
                SubRegion.SouthAmerica => "south-america",
                _ => "america"
            },
            Region.Antarctic => "antarctica",
            Region.None => null,
            _ => null
        };
    }
}