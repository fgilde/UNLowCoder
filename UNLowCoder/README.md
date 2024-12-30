# UnLowCoder.Lib
[![NuGet](https://img.shields.io/nuget/v/UNLowCoder.Lib)](https://www.nuget.org/packages/UNLowCoder.Lib/)

This package contains the core functionality to parse the UNLocode csv zip files.
This package is used by the [UNLowCoder.SourceGen](https://www.nuget.org/packages/UNLowCoder.SourceGen/) and [UNLowCoder.Lib](https://www.nuget.org/packages/UNLowCoder.Lib/) 
package to provide and generate the classes for UN/Locodes.
<br>
<br>
But you can use the Core package also standalone to parse the csv zip files own your own at runtime.

## Usage

### Install the nuget package

Add a [Nuget package](https://www.nuget.org/packages/UNLowCoder.Core/) reference to the project file in the project:<br>

`<PackageReference Include="UNLowCoder.Core" Version="<version>" />`


Parsing the csv zip file at runtime is simple.

```csharp
var all = UnLocodeParser.ParseZipArchive("C:\Directory\loc241csv.zip");

```
