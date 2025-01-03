# UnLowCoder.SourceGen
[![NuGet](https://img.shields.io/nuget/v/UNLowCoder.SourceGen)](https://www.nuget.org/packages/UNLowCoder.SourceGen/)


Simple package to generate UNLocode classes from official unece csv zip package.
The zip file can be downloaded from [here](https://unece.org/trade/cefact/UNLOCODE-Download) or use this [direct link](https://service.unece.org/trade/locode/loc241csv.zip)

This package is a source generator that generates classes for the UNLocode csv files. The generated classes are readonly and can be used to access the data in the csv files.
For parsing this package uses the [UNLowCoder.Core](https://www.nuget.org/packages/UNLowCoder.Core/) package.

Implemented via [C# source generators](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)

Also this package is used by [UNLowCoder.Lib](https://www.nuget.org/packages/UNLowCoder.Lib/) that provides the generated classes for the UNLocode csv files.


## Usage

### Install the nuget package

Add a [Nuget package](https://www.nuget.org/packages/UNLowCoder.SourceGen/) reference to the project file in the project you want to generate unlocodes in:<br>

`<PackageReference Include="UNLowCoder.SourceGen" Version="<version>" />`


### Add the downloaded unlocodes csv zip file to your project 
You should rename the zip file to the class name you want to generate. For example `UNLOCODE.zip` creates the classes `UNLOCODE`.<br>

In an `ItemGroup` in your csproj file add an `AdditionFiles` element for **each unlocode zip file you want to generate classes for**
Set the `Include` attribute to the path of the file.

Example:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <AdditionalFiles Include="UnLocodes.zip" />
  </ItemGroup>

</Project>
```


You can then use the generated classes in your code like this.
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
