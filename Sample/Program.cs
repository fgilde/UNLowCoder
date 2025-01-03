// See https://aka.ms/new-console-template for more information

using System.Globalization;
using UNLowCoder;
using UNLowCoder.Core.Extensions;
using UNLowCoder.Extensions;
using UNLowCoder.Extensions.Classes;
using UNLowCoder.Lib;


Console.WriteLine("Hello, World!");

var all_countries = UnLocodes.Countries.All;
var currentCountry = UnLocodes.Countries.Get(CultureInfo.CurrentCulture);
var munden = UnLocodes.Locations.DE.MUD;
var munich = UnLocodes.Locations.DE.MUN;

var ci = currentCountry.CountryInfo();
var continent = currentCountry.Continent();

//var routerDE = await currentCountry.BuildRouterAsync("TEST");

//var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
////var profile = Ship.Instance.Profile;
//var route = munden.RouteTo(munich, routerDE, profile);
//var distanceInKm = route.TotalDistance / 1000;
//var distanceDirect = munden.DistanceTo(munich);

//var amount = all_countries[10].Currency.ConvertAmount(1000, currentCountry.Currency);

//var germanLocations = UnLocodes.Locations.DE.All;
//var allL = UnLocodes.Locations.All;
//var locHH = UnLocodes.Locations.DE.HAM;
//var allD = UnLocodes.Subdivisions.All;


var countries = UnLocodeParser.ParseZipArchive("C:\\Users\\fgild\\Downloads\\loc241csv.zip");
var germans = countries.Where(c => c.CountryCode == "DE").ToList();
var firstLocation = germans.First().Locations.First();

var allHH = germans.First().Locations.Where(l => l.Name.Contains("Hamburg", StringComparison.InvariantCultureIgnoreCase)).ToList();
var anotherLocation = allHH.First();

var distance = firstLocation.DistanceTo(anotherLocation);

Console.ReadLine();