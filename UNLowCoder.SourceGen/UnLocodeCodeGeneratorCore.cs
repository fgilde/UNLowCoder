﻿using Microsoft.CodeAnalysis.Text;
using System.Text;
using UNLowCoder.Core.Data;

namespace UNLowCoder.SourceGen
{
    internal class UnLocodeCodeGeneratorCore
    {
        public SourceText CreateMainClass(GeneratorDataContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// Main class");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine($"using {context.CoreDataNamespace};");

            if (!string.IsNullOrEmpty(context.Namespace))
                sb.AppendLine($"namespace {context.Namespace};");
            sb.AppendLine($"public partial class {context.GeneratedClassName}");
            sb.AppendLine("{");
            sb.AppendLine("    public static partial class Countries {}");
            sb.AppendLine("    public static partial class Subdivisions");
            sb.AppendLine("    {");
            sb.AppendLine("        public static System.Collections.Generic.IReadOnlyList<UnLocodeSubdivision> "+context.AllPropertyName+" => new[] {")
              .Append(string.Join(", ", context.Countries.Select(c => $"{context.Namespace}.{context.GeneratedClassName}.Subdivisions.{SafeIdentifier(c.CountryCode)}.{context.AllPropertyName}")))
              .Append("}.SelectMany(x => x).ToArray();");

            sb.AppendLine("    }");
            sb.AppendLine("    public static partial class Locations");
            sb.AppendLine("    {");
            sb.AppendLine("        public static System.Collections.Generic.IReadOnlyList<UnLocodeLocation> "+context.AllPropertyName+" => new[] {")
              .Append(string.Join(", ", context.Countries.Select(c => $"{context.Namespace}.{context.GeneratedClassName}.Locations.{SafeIdentifier(c.CountryCode)}.{context.AllPropertyName}")))
              .Append("}.SelectMany(x => x).ToArray();");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return SourceText.From(sb.ToString(), Encoding.UTF8);
        }

        public SourceText CreateCountriesClass(GeneratorDataContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// Countries");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine($"using {context.CoreDataNamespace};");
            if (!string.IsNullOrEmpty(context.Namespace))
                sb.AppendLine($"namespace {context.Namespace};");
            sb.AppendLine($"public partial class {context.GeneratedClassName}");
            sb.AppendLine("{");
            sb.AppendLine("    public static partial class Countries");
            sb.AppendLine("    {");

            var allCountries = new List<string>();
            foreach (var country in context.Countries)
            {
                var identifier = SafeIdentifier(country.CountryCode);
                allCountries.Add(identifier);

                sb.AppendLine($"        public static UnLocodeCountry {identifier} => new UnLocodeCountry(");
                sb.AppendLine($"            \"{Escape(country.CountryCode)}\",");
                sb.AppendLine($"            {EscapeString(country.CountryName)},");
                sb.AppendLine($"            {context.Namespace}.{context.GeneratedClassName}.Subdivisions.{identifier}.{context.AllPropertyName},");
                sb.AppendLine($"            {context.Namespace}.{context.GeneratedClassName}.Locations.{identifier}.{context.AllPropertyName}");
                sb.AppendLine("        );");
            }

            // Get method for CultureInfo
            sb.AppendLine($"        public static UnLocodeCountry {context.GetMethodName}(System.Globalization.CultureInfo culture)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (culture == null) return null;");
            sb.AppendLine("            if (!culture.IsNeutralCulture) culture = culture.Parent;");
            sb.AppendLine("            return All.FirstOrDefault(c => c?.CultureInfo?.Name?.Equals(culture.Name, StringComparison.InvariantCultureIgnoreCase) == true);");
            sb.AppendLine("        }");

            // Get method for RegionInfo
            sb.AppendLine($"        public static UnLocodeCountry {context.GetMethodName}(System.Globalization.RegionInfo region)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (region == null) return null;");
            sb.AppendLine("            return All.FirstOrDefault(c => c?.RegionInfo?.TwoLetterISORegionName?.Equals(region.TwoLetterISORegionName, StringComparison.InvariantCultureIgnoreCase) == true);");
            sb.AppendLine("        }");

            // Get method for string (Code or Name)
            sb.AppendLine($"        public static UnLocodeCountry {context.GetMethodName}(string codeOrName)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (string.IsNullOrWhiteSpace(codeOrName)) return null;");
            sb.AppendLine("            return All.FirstOrDefault(c => ");
            sb.AppendLine("                c?.CountryCode?.Equals(codeOrName, StringComparison.InvariantCultureIgnoreCase) == true ||");
            sb.AppendLine("                c?.CountryName?.Equals(codeOrName, StringComparison.InvariantCultureIgnoreCase) == true);");
            sb.AppendLine("        }");

            sb.AppendLine($"        public static System.Collections.Generic.IReadOnlyList<UnLocodeCountry> {context.AllPropertyName} => new[] {{ {string.Join(", ", allCountries)} }};");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return SourceText.From(sb.ToString(), Encoding.UTF8);
        }

        public SourceText CreateSubdivisionsClass(GeneratorDataContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// Subdivisions");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine($"using {context.CoreDataNamespace};");
            if (!string.IsNullOrEmpty(context.Namespace))
                sb.AppendLine($"namespace {context.Namespace};");
            sb.AppendLine($"public partial class {context.GeneratedClassName}");
            sb.AppendLine("{");

            sb.AppendLine($"public static partial class Subdivisions");
            sb.AppendLine("{");

            foreach (var country in context.Countries)
            {
                var countryIdentifier = SafeIdentifier(country.CountryCode);
                sb.AppendLine($"    public static class {countryIdentifier}");
                sb.AppendLine("    {");

                var allSubdivisions = new List<string>();
                foreach (var sub in country.Subdivisions)
                {
                    var identifier = SafeIdentifier(sub.CountryCode + "_" + sub.SubdivisionCode);
                    allSubdivisions.Add(identifier);

                    sb.AppendLine($"        public static UnLocodeSubdivision {identifier} => new UnLocodeSubdivision(");
                    sb.AppendLine($"            \"{Escape(sub.CountryCode)}\",");
                    sb.AppendLine($"            \"{Escape(sub.SubdivisionCode)}\",");
                    sb.AppendLine($"            {EscapeString(sub.Name)},");
                    sb.AppendLine($"            {EscapeString(sub.Type)}");
                    sb.AppendLine("        );");
                }

                var allArray = allSubdivisions.Count > 0
                    ? $"new[] {{ {string.Join(", ", allSubdivisions)} }}"
                    : "System.Array.Empty<UnLocodeSubdivision>()";

                sb.AppendLine($"        public static System.Collections.Generic.IReadOnlyList<UnLocodeSubdivision> {context.AllPropertyName} => {allArray};");

                sb.AppendLine("    }");
            }

            sb.AppendLine("}");
            sb.AppendLine("}");
            return SourceText.From(sb.ToString(), Encoding.UTF8);
        }

        public IEnumerable<(string FileName, SourceText Source)> CreateLocationChunks(GeneratorDataContext context)
        {
            foreach (var country in context.Countries)
            {
                var sb = new StringBuilder();
                sb.AppendLine("// <auto-generated>");
                sb.AppendLine($"// Locations for {country.CountryCode}");
                sb.AppendLine("// </auto-generated>");
                sb.AppendLine($"using {context.CoreDataNamespace};");
                if (!string.IsNullOrEmpty(context.Namespace))
                    sb.AppendLine($"namespace {context.Namespace};");

                sb.AppendLine($"public partial class {context.GeneratedClassName}");
                sb.AppendLine("{");

                sb.AppendLine($"public static partial class Locations");
                sb.AppendLine("{");

                sb.AppendLine($"public static partial class {SafeIdentifier(country.CountryCode)}");
                sb.AppendLine("{");

                var allLocations = new List<string>();
                foreach (var loc in country.Locations)
                {
                    var identifier = SafeIdentifier(loc.LocationCode);
                    allLocations.Add(identifier);

                    sb.AppendLine($"    public static UnLocodeLocation {identifier} => new UnLocodeLocation(");
                    sb.AppendLine($"        \"{Escape(loc.CountryCode)}\",");
                    sb.AppendLine($"        \"{Escape(loc.LocationCode)}\",");
                    sb.AppendLine($"        {EscapeString(loc.Name)},");
                    sb.AppendLine($"        {EscapeString(loc.NameWoDiacritics)},");
                    sb.AppendLine($"        {EscapeString(loc.SubdivisionCode)},");

                    sb.AppendLine($"        LocationStatus.{loc.Status},");
                    sb.AppendLine($"        LocationFunction.{loc.Function},");
                    sb.AppendLine($"        {FormatDate(loc.LastUpdateDate)},");
                    sb.AppendLine($"        {EscapeString(loc.IATA)},");

                    sb.AppendLine($"        {FormatCoordinates(loc.Coordinates)},");
                    sb.AppendLine($"        {EscapeString(loc.Remarks)},");
                    sb.AppendLine($"        ChangeIndicator.{loc.Change}");
                    sb.AppendLine("    );");
                }

                var allArray = allLocations.Count > 0
                    ? $"new[] {{ {string.Join(", ", allLocations)} }}"
                    : "System.Array.Empty<UnLocodeLocation>()";

                sb.AppendLine($"    public static System.Collections.Generic.IReadOnlyList<UnLocodeLocation> {context.AllPropertyName} => {allArray};");
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("}");

                yield return ($"Locations.{country.CountryCode}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            }
        }

        private static string Escape(string s) => s?.Trim().Replace("\r", "").Replace("\n", "").Replace("\\", "\\\\").Replace("\"", "\\\"") ?? string.Empty;
        private static string EscapeString(string s) => s != null ? $"\"{Escape(s)}\"" : "null";
        private static string SafeIdentifier(string s)
        {
            if (string.IsNullOrEmpty(s)) return "_EMPTY";
            if (!char.IsLetter(s[0])) s = "_" + s;
            return new string(s.Select(ch => char.IsLetterOrDigit(ch) || ch == '_' ? ch : '_').ToArray());
        }
        private static string FormatDate(DateTime? dt) => dt.HasValue ? $"new DateTime({dt.Value.Year}, {dt.Value.Month}, {dt.Value.Day})" : "null";
        private static string FormatCoordinates(Coordinates? c)
        {
            if (c == null) return "null";
            return $"new Coordinates({c.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {c.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})";
        }
    }
}
