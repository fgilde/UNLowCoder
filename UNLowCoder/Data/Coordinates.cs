using System;
using System.Threading.Tasks;
using CsvHelper;
using Nominatim.API.Geocoders;
using Nominatim.API.Interfaces;
using Nominatim.API.Models;
using Nominatim.API.Web;

namespace UNLowCoder.Core.Data;

public partial record Coordinates
{
    public const double EarthRadiusKm = 6371.0;

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
        double lat1Rad = ToRadians(Latitude);
        double lat2Rad = ToRadians(target.Latitude);
        double deltaLat = ToRadians(target.Latitude - Latitude);
        double deltaLon = ToRadians(target.Longitude - Longitude);

        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }
    
    public Task<GeocodeResponse> GetGeocodeDetailsAsync(ReverseGeocodeRequest? request = null)
    {
        var factory = new SimpleHttpFactory();

        INominatimWebInterface re = new NominatimWebInterface(factory);
                var geocoder = new ReverseGeocoder(re);

        request ??= new ReverseGeocodeRequest
        {
            BreakdownAddressElements = true,
            DedupeResults = true,
            ShowAlternativeNames = true,
            ShowExtraTags = true,
            Latitude = Latitude,
            Longitude = Longitude,
        };
        return geocoder.ReverseGeocode(request);
    }

    private static double ToRadians(double degrees) => (Math.PI / 180.0) * degrees;
    
    
}
