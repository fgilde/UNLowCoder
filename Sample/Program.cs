// See https://aka.ms/new-console-template for more information

using System.Globalization;
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

var file = "C:\\dev\\privat\\github\\UNLowCoder\\UNLowCoder.SourceGen\\Data\\ADALV.json";
var json = File.ReadAllText(file, System.Text.Encoding.Unicode);
GeocodeResponse gresult = JsonConvert.DeserializeObject<GeocodeResponse>(json);




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
var countries = UnLocodeParser.ParseZipArchive("C:\\Users\\fgild\\Downloads\\loc241csv.zip");
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



