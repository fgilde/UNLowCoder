using Itinero;
using Itinero.LocalGeo;
using Itinero.Profiles;
using UNLowCoder.Core.Data;

namespace UNLowCoder.Extensions;

public static class UnLocodeLocationExtensions
{
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
}