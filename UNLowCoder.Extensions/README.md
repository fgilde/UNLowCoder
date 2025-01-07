# UnLowCoder.Extensions
[![NuGet](https://img.shields.io/nuget/v/UNLowCoder.Extensions)](https://www.nuget.org/packages/UNLowCoder.Extensions/)

This package contains the extended functionality for the UNLocode Contracts and lists from
[UNLowCoder.Core](https://www.nuget.org/packages/UNLowCoder.Core/) .


## Usage

### Install the nuget package

Add a [Nuget package](https://www.nuget.org/packages/UNLowCoder.Extensions/) reference to the project file in the project:<br>

`<PackageReference Include="UNLowCoder.Extensions" Version="<version>" />`


You can then use features like this.
```csharp
var all_countries = UnLocodeParser.ParseZipArchive("C:\Directory\loc241csv.zip");

// FILTERING
var EUCountries = all_countries.Filter(Region.Europe);
var EUAndAsiaCountries = all_countries.Filter(Region.Europe, Region.Asia);
var nothEUCountries = all_countries.Filter(SubRegion.NorthernEurope);
var westEUCountries = all_countries.Filter(SubRegion.WesternEurope);

// ROUTING with Itinero
var munden = UnLocodes.Locations.DE.MUD;
var munich = UnLocodes.Locations.DE.MUN;
var routerDE = await munden.Country.BuildRouterAsync("TEST");

var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
var route = munden.RouteTo(munich, routerDE, profile);

// Nager.Country
var germany = munden.Country.CountryInfo();
var borderOfGer = countries.Filter(ci.BorderCountries);


```

