// See https://aka.ms/new-console-template for more information
using System.Globalization;
using UNLowCoder;
using UNLowCoder.Lib;



//var countries = UNLowCoder.Lib.UnLocodes.Countries.AllX;
//var loc = UNLowCoder.Lib.UnLocodes.Locations.ADCAN;
var countries = UNLowCoder.Lib.UnLocodes.Countries.All;
var locations = UNLowCoder.Lib.UnLocodes.Locations.All;
var country = UNLowCoder.Lib.UnLocodes.Countries.DE;
var subHH = UNLowCoder.Lib.UnLocodes.Subdivisions.DE_HH;
var codes = UnLocodeParser.ParseZipArchive("C:\\Users\\fgild\\Downloads\\loc241csv.zip", true);
var germans = codes.Where(c => c.CountryCode == "DE").ToList();
var firstLocation = germans.First().Locations.First();

var allHH = germans.First().Locations.Where(l => l.Name.Contains("Hamburg", StringComparison.InvariantCultureIgnoreCase)).ToList();
var anotherLocation = allHH.First();

var distance = firstLocation.DistanceTo(anotherLocation);

Console.ReadLine();