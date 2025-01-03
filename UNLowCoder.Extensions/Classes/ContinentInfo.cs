using Nager.Country;
using Nextended.Core.Helper;
using System;
using UNLowCoder.Core.Data;

namespace UNLowCoder.Extensions.Classes;


public record ContinentInfo 
{
    public ContinentInfo(Region region) : this(region, SubRegion.None)
    { }

    public ContinentInfo(Region region, SubRegion subRegion)
    {
        Region = region;
        SubRegion = subRegion;
    }

    public ContinentInfo(UnLocodeCountry country) : this(country.CountryInfo())
    { }

    public ContinentInfo(ICountryInfo country) : this(country.Region, country.SubRegion)
    { }

    public Region Region { get; }
    public SubRegion SubRegion { get; }

    public string RegionName => Enum.GetName(typeof(Region), Region);
    public string SubRegionName => SubRegion != SubRegion.None ? SubRegion.ToDescriptionString() : string.Empty;
    public string Name => $"{RegionName} ({SubRegionName})";

    public string OsmRegion =>
        Region switch
        {
            Region.Europe => "europe",
            Region.Asia => "asia",
            Region.Africa => "africa",
            Region.Oceania => "australia-oceania",
            Region.Americas => SubRegion switch
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

    public string PbfUrl => GeoFabrik.PbfUrl(OsmRegion);
}