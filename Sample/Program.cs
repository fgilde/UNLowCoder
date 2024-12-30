// See https://aka.ms/new-console-template for more information
using System.Globalization;
using UNLowCoder;
using UNLowCoder.Lib;


Console.WriteLine("Hello, World!");
//var countries = UNLowCoder.Lib.UnLocodes.Countries.AllX;
//var loc = UNLowCoder.Lib.UnLocodes.Locations.ADCAN;
var countries = UnLocodes.Countries.All;

var c = countries[3];
var c2 = countries[3];

var equal = c.Equals(c2);
var equal2 = c == c2;

var germanLocations = UnLocodes.Locations.DE.All;
var allL = UnLocodes.Locations.All;
var allD = UnLocodes.Subdivisions.All;
var hh = UnLocodes.Subdivisions.DE.DE_HH;



var codes = UnLocodeParser.ParseZipArchive("C:\\Users\\fgild\\Downloads\\loc241csv.zip");
var germans = codes.Where(c => c.CountryCode == "DE").ToList();
var firstLocation = germans.First().Locations.First();

var allHH = germans.First().Locations.Where(l => l.Name.Contains("Hamburg", StringComparison.InvariantCultureIgnoreCase)).ToList();
var anotherLocation = allHH.First();

var distance = firstLocation.DistanceTo(anotherLocation);

Console.ReadLine();