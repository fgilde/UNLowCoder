using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UNLowCoder.Core.Data;

namespace UNLowCoder.Extensions.GeoApi;

public class Geocoder
{
    public static Geocoder Instance { get; } = new(new HttpClient());
    
    private readonly HttpClient _http;
    private ConcurrentDictionary<string, ReverseResponse> _cache = new();

    public Geocoder(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd($"{nameof(UNLowCoder)}/{GetType().Assembly.GetName().Version} (+https://github.com/fgilde/UnLowCoder)");
    }

  
    private async Task<ReverseResponse> ReverseZoomAsync(double lat, double lon, int zoom, bool forceReload = false)
    {
        var latS = lat.ToString(CultureInfo.InvariantCulture);
        var lonS = lon.ToString(CultureInfo.InvariantCulture);

        var url = "https://nominatim.openstreetmap.org/reverse"
                  + $"?lat={Uri.EscapeDataString(latS)}"
                  + $"&lon={Uri.EscapeDataString(lonS)}"
                  + "&format=jsonv2"
                  + "&dedupe=1"
                  + "&extratags=1"
                  + "&namedetails=1"
                  + "&geojson=1"
                  + "&addressdetails=1"
                  + $"&zoom={zoom}";

        if (forceReload)
        {
            _cache.TryRemove(url, out _);
        }
        if (_cache.TryGetValue(url, out var cached))
        {
            return cached;
        }

        var json = await _http.GetStringAsync(url);
        var result = JsonConvert.DeserializeObject<ReverseResponse>(json)
               ?? throw new Exception("Invalid response");
        _cache.TryAdd(url, result);
        return result;
    }

    public Task<ReverseResponse?> ReverseAsync(Coordinates coordinates, bool forceReload = false) => coordinates is null 
        ? Task.FromResult<ReverseResponse>(null) 
        : ReverseAsync(coordinates.Latitude, coordinates.Longitude, forceReload);
    
    public async Task<ReverseResponse> ReverseAsync(double lat, double lon, bool forceReload = false)
    {
        var detailResp = await ReverseZoomAsync(lat, lon, 18, forceReload);
        var adminResp = await ReverseZoomAsync(lat, lon, 5, forceReload);
        var addr = detailResp.Address;
        if (string.IsNullOrEmpty(addr.State))
        {
            addr.State = adminResp.Address.State;
        }
        if (string.IsNullOrEmpty(addr.State)
            && !string.IsNullOrEmpty(addr.ISO3166Level4))
        {
            addr.State = addr.StateCode;
        }
        return detailResp;
    }
}