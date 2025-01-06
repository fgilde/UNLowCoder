// See https://aka.ms/new-console-template for more information

using System.Diagnostics.Metrics;
using System.Globalization;
using Itinero;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using Nager.Country;
using UNLowCoder;
using UNLowCoder.Core.Extensions;
using UNLowCoder.Extensions;
using UNLowCoder.Extensions.Classes;
using UNLowCoder.Lib;


Console.WriteLine("Hello, World!");



var all_countries = UnLocodes.Countries.All;
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


var ci = currentCountry.CountryInfo();
var borderOfGer = countries.Filter(ci.BorderCountries);

var routerDE = await currentCountry.BuildRouterAsync("TEST");

var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
//var profile = Ship.Instance.Profile;
var route = munden.RouteTo(munich, routerDE, profile);
var distanceInKm = route.TotalDistance / 1000;
var distanceDirect = munden.DistanceTo(munich);

var amount = all_countries[10].Currency.ConvertAmount(1000, currentCountry.Currency);

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