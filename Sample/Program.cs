// See https://aka.ms/new-console-template for more information

using System.Globalization;
using System.Text.Json;
using Itinero.Osm.Vehicles;
using Nager.Country;
using Newtonsoft.Json;
using Nextended.Core.Types;
using Nominatim.API.Geocoders;
using Nominatim.API.Interfaces;
using Nominatim.API.Models;
using Nominatim.API.Web;
using UNLowCoder;
using UNLowCoder.Core;
using UNLowCoder.Extensions;
using UNLowCoder.Lib;


Console.WriteLine("Hello, World!");


var location = UnLocodes.Locations.DE.HAM;
var withCoordinates = UnLocodes.Locations.All
    .Where(l => l.Coordinates != null)
    .ToList();

//var file = "C:\\dev\\privat\\github\\UNLowCoder\\UNLowCoder.SourceGen\\Data\\ADALV.json";
//var json = File.ReadAllText(file, System.Text.Encoding.Unicode);
//GeocodeResponse gresult = JsonConvert.DeserializeObject<GeocodeResponse>(json);

var loc = UnLocodes.Locations.Find("DEFRA");
var loc2 = UnLocodes.Locations.Find("ushou");
var geo = await loc.GetGeocodeDetailsAsync();

var geo2 = await location.GetGeocodeDetailsAsync();
var geo4 = await UnLocodes.Locations.US.CGH.GetGeocodeDetailsAsync();
var geo7 = await UnLocodes.Locations.US.CGH.GetGeocodeDetailsAsync();

var client = new HttpClient();
// Nominatim requires a valid User-Agent header
client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");

var url = $"https://nominatim.openstreetmap.org/reverse" +
          $"?lat={location.Coordinates.Latitude.ToString().Replace(",", ".")}&lon={location.Coordinates.Longitude.ToString().Replace(",", ".")}" +
          $"&format=jsonv2" +            // request the JSONv2 output
          $"&addressdetails=1" +         // breakdown into address elements
          $"&extratags=1" +              // include extra tags (if you need them)
          $"&namedetails=1";             // include alt-names (if you need them)

var payload = await client.GetStringAsync(url);
using var doc = JsonDocument.Parse(payload);

var addr = doc.RootElement
    .GetProperty("address");

var state = addr.TryGetProperty("state", out var s)
    ? s.GetString()
    : "";

Console.WriteLine($"State: {state}");


//foreach (var loc in withCoordinates)
//{
//    var path = $"D:\\dev\\privat\\github\\UNLowCoder\\UNLowCoder\\UNLowCoder.SourceGen\\Data\\{loc.FullUnLocode}.json";
//    if (File.Exists(path))
//    {
//        Console.WriteLine($"File {path} already exists.");
//        continue;
//    }

//    var result = await loc.GetGeocodeDetailsAsync();
//    var asJson = JsonConvert.SerializeObject(result, Formatting.Indented);
//    File.WriteAllBytes(path, System.Text.Encoding.Unicode.GetBytes(asJson));
//    Console.WriteLine($"File {path} created.");
//}


var all_countries = UnLocodes.Countries.All;
var c = all_countries[0];
var pfaaa = UnLocodes.Locations.PF.AAA;
var countries = UnLocodes.Countries.All.ToList();
var iatas = UnLocodes.Locations.All.Where(l => !string.IsNullOrEmpty(l.IATA)).ToList();
var germanyAndGb = countries.Filter(Alpha2Code.DE, Alpha2Code.GB).ToList();
var shang2 = countries.SelectMany(c => c.Locations).FirstOrDefault(loc => loc.FullUnLocode == "CNSHA");
var shang = UnLocodes.Locations.All.FirstOrDefault(loc => loc.FullUnLocode == "CNSHA");

var currentCountry = UnLocodes.Countries.Get(CultureInfo.CurrentCulture);

var all_sea_ports = all_countries.SelectMany(c => c.Locations).Where(loc => loc.Function == UNLowCoder.Core.Data.LocationFunction.Seaport).ToList();
var air_ports = all_countries.SelectMany(c => c.Locations).Where(loc => loc.Function == UNLowCoder.Core.Data.LocationFunction.Airport).ToList();


var EUCountries = all_countries.Filter(Region.Europe);
var EUAndAsiaCountries = all_countries.Filter(Region.Europe, Region.Asia);
var nothEUCountries = all_countries.Filter(SubRegion.NorthernEurope);
var westEUCountries = all_countries.Filter(SubRegion.WesternEurope);

var munden = UnLocodes.Locations.DE.MUD;
var munich = UnLocodes.Locations.DE.MUN;


ICountryInfo? ci = currentCountry.CountryInfo();
var borderOfGer = countries.Filter(ci.BorderCountries);

var routerDE = await munden.Country.BuildRouterAsync("TEST");

var profile = Vehicle.Car.Fastest();
//var profile = Ship.Instance.Profile;
var route = munden.RouteTo(munich, routerDE, profile);
var distanceInKm = route.TotalDistance / 1000;
var distanceDirect = munden.DistanceTo(munich);

Money amount = all_countries[10].Currency.ConvertAmount(1000, currentCountry.Currency);

var germanLocations = UnLocodes.Locations.DE.All;
var allL = UnLocodes.Locations.All;
var locHH = UnLocodes.Locations.DE.HAM;
var allD = UnLocodes.Subdivisions.All;


var germans = countries.Where(c => c.CountryCode == "DE").ToList();
var firstLocation = germans.First().Locations.First();

var allHH = germans.First().Locations.Where(l => l.Name.Contains("Hamburg", StringComparison.InvariantCultureIgnoreCase)).ToList();
var anotherLocation = allHH.First();

var distance = firstLocation.DistanceTo(anotherLocation);

Console.ReadLine();



