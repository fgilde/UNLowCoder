using Newtonsoft.Json;
using Nominatim.API.Models;

namespace UNLowCoder.Extensions.GeoApi;

public class ReverseResponse : GeocodeResponse
{
    [JsonProperty("address")]
    public new AddressDetails Address { get; set; }
}