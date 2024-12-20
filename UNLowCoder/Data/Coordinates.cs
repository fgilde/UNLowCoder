using System;

namespace UNLowCoder.Core.Data;

public class Coordinates
{
    public double Latitude { get; }
    public double Longitude { get; }

    public Coordinates(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Calculates the distance between this coordinate and a target coordinate in kilometers
    /// using the Haversine formula.
    /// </summary>
    public double DistanceTo(Coordinates target)
    {
        // Radius der Erde in Kilometern
        const double EarthRadiusKm = 6371.0;

        double lat1Rad = ToRadians(this.Latitude);
        double lat2Rad = ToRadians(target.Latitude);
        double deltaLat = ToRadians(target.Latitude - this.Latitude);
        double deltaLon = ToRadians(target.Longitude - this.Longitude);

        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => (Math.PI / 180.0) * degrees;
}
