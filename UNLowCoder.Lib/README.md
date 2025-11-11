# UnLowCoder.Lib
[![NuGet](https://img.shields.io/nuget/v/UNLowCoder.Lib)](https://www.nuget.org/packages/UNLowCoder.Lib/)

This package contains the core functionality to parse the UNLocode csv files and provides generated classes under the namespace `UnLocodes`.
It uses the [UNLowCoder.SourceGen](https://www.nuget.org/packages/UNLowCoder.SourceGen/) package to generate the resulting classes.

## 📚 Documentation
View the complete API documentation at: **[https://fgilde.github.io/UNLowCoder](https://fgilde.github.io/UNLowCoder)**


## Usage

### Install the nuget package

Add a [Nuget package](https://www.nuget.org/packages/UNLowCoder.Lib/) reference to the project file in the project:<br>

`<PackageReference Include="UNLowCoder.Lib" Version="<version>" />`


You can then use the classes in your code like this.
```csharp
var all_countries = UnLocodes.Countries.All;
var current = UnLocodes.Countries.Get(CultureInfo.CurrentCulture);

var germanLocations = UnLocodes.Locations.DE.All;
var location_HAM_ = UnLocodes.Locations.DE.HAM;
var allLocations = UnLocodes.Locations.All;

var allDivisions = UnLocodes.Subdivisions.All;
var division_german_hh = UnLocodes.Subdivisions.DE.DE_HH;
```

For sure you can also parse another csv zip file at runtime.

```csharp
var all = UnLocodeParser.ParseZipArchive("C:\Directory\loc241csv.zip");

```
