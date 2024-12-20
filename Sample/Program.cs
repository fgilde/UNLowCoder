// See https://aka.ms/new-console-template for more information
using System.Globalization;
using UNLowCoder;
using UNLowCoder.Lib;



var codes = UnLocodeParser.ParseZipArchive("C:\\Users\\fgild\\Downloads\\loc241csv.zip");
var germans = codes.Where(c => c.CountryCode == "DE").ToList();
var firstLocation = germans.First().Locations.First();

var allHH = germans.First().Locations.Where(l => l.Name.Contains("Hamburg", StringComparison.InvariantCultureIgnoreCase)).ToList();
var anotherLocation = allHH.First();

var distance = firstLocation.Coordinates.CalculateDistanceTo(anotherLocation.Coordinates);

Console.ReadLine();