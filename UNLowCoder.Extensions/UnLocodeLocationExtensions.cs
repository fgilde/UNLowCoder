using System.Net.Http;
using Itinero;
using Itinero.LocalGeo;
using Itinero.Profiles;
using Nager.Country;
using System.Threading.Tasks;
using UNLowCoder.Core.Data;
using UNLowCoder.Extensions.GeoApi;

namespace UNLowCoder.Extensions;

public static class UnLocodeLocationExtensions
{

    public static ICountryInfo? CountryInfo(this UnLocodeLocation location)
    {
        return location.Country.CountryInfo();
    }

    public static Route RouteTo(this UnLocodeLocation location, UnLocodeLocation target, Router router, IProfileInstance profile = null)
    {
        return location.Coordinates?.RouteTo(target.Coordinates, router, profile);
    }

    public static Route RouteTo(this Coordinates location, Coordinates target, Router router, IProfileInstance profile = null)
    {
        var start = location.RouterPoint(router, profile);
        var end = target.RouterPoint(router, profile);
        return router.Calculate(profile, start, end);
    }

    public static RouterPoint RouterPoint(this UnLocodeLocation location, Router router, IProfileInstance profile = null)
    {        
        return location.Coordinates?.RouterPoint(router, profile);
    }

    public static RouterPoint RouterPoint(this Coordinates coordinates, Router router, IProfileInstance profile = null)
    {        
        profile ??= Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
        return router.Resolve(profile, coordinates.ToItineroCoordinate());
    }

    public static Coordinate ToItineroCoordinate(this Coordinates coordinates) => new((float)coordinates.Latitude, (float)coordinates.Longitude);

    public static Task<ReverseResponse?> GetGeocodeDetailsAsync(this UnLocodeLocation location, bool forceReload = false) => GetGeocodeDetailsAsync(location.Coordinates, Geocoder.Instance, forceReload);
    public static Task<ReverseResponse?> GetGeocodeDetailsAsync(this UnLocodeLocation location, HttpClient client, bool forceReload = false) => GetGeocodeDetailsAsync(location.Coordinates, client, forceReload);
    public static Task<ReverseResponse?> GetGeocodeDetailsAsync(this UnLocodeLocation location, Geocoder geocoder, bool forceReload = false) => GetGeocodeDetailsAsync(location.Coordinates, geocoder, forceReload);
    public static async Task<ReverseResponse?> GetGeocodeDetailsAsync(this Coordinates location, bool forceReload = false) => await GetGeocodeDetailsAsync(location, Geocoder.Instance, forceReload);
    public static Task<ReverseResponse?> GetGeocodeDetailsAsync(this Coordinates location, HttpClient client, bool forceReload = false) => GetGeocodeDetailsAsync(location, new Geocoder(client), forceReload);
    public static Task<ReverseResponse?> GetGeocodeDetailsAsync(this Coordinates location, Geocoder geocoder, bool forceReload = false) => geocoder.ReverseAsync(location, forceReload);
}