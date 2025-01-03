using Microsoft.CodeAnalysis;
using UNLowCoder.Core.Data;

namespace UNLowCoder.SourceGen
{
    internal class GeneratorDataContext  
    {
        public string OriginFilePath { get; set; }
        public string Namespace { get; set; }
        public string CoreDataNamespace => typeof(UnLocodeCountry).Namespace;
        public string AllPropertyName { get; set; } = "All";
        public string GetMethodName { get; set; } = "Get";
        public IReadOnlyList<UnLocodeCountry> Countries { get; set; }
        public string GeneratedFileName { get; set; }
        public string GeneratedClassName { get; set; }
        public string FullGeneratedClassName => $"{Namespace}.{GeneratedClassName}";
        public string CountryClass => nameof(UnLocodeCountry);
        public string LocationClass => nameof(UnLocodeLocation);
        public string DivisionClass => nameof(UnLocodeSubdivision);
        public string StaticCountriesClassName { get; set; } = "Countries";
        public string StaticLocationsClassName { get; set; } = "Locations";
        public string StaticDivisionsClassName { get; set; } = "Subdivisions";

        public GeneratorDataContext(AdditionalText originFile, List<UnLocodeCountry> countries, NamespaceResolver namespaceResolver)
        {            
            this.OriginFilePath = originFile.Path;
            this.Countries = countries;            
            this.GeneratedClassName = Path.GetFileNameWithoutExtension(originFile.Path);
            this.GeneratedFileName = $"{this.GeneratedClassName}.g.cs";
            this.Namespace = namespaceResolver.Resolve();
            if(Namespace.EndsWith("."))
                Namespace = Namespace.Substring(0, Namespace.Length - 1);
        }
    }
}
