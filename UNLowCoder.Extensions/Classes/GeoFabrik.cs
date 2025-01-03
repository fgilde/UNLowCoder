namespace UNLowCoder.Extensions.Classes;

public static class GeoFabrik
{
    private const string URL = "https://download.geofabrik.de/{0}-latest.osm.pbf";
    public static string PbfUrl(string continent, string country) => string.Format(URL, $"{continent}/{country}");
    public static string PbfUrl(string continent) => string.Format(URL, $"{continent}");
}