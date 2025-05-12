using System;
using System.Linq;
using Nager.Country;
using Newtonsoft.Json;
using Nominatim.API.Models;

namespace UNLowCoder.Extensions.GeoApi;

public class AddressDetails : AddressResult
{
    [JsonProperty("ISO3166-2-lvl4")]
    public string ISO3166Level4 { get; set; }

    public string StateCode => ISO3166Level4?.Split('-').LastOrDefault() ?? string.Empty;
}

public static class AddressResultExtensions
{
    public static ICountryInfo? CountryInfo(this AddressResult address)
    {
        try
        {
            ICountryProvider countryProvider = new CountryProvider();
            return countryProvider.GetCountry(address.CountryCode);
        }
        catch (Exception)
        {
            return null;
        }
    }

}
