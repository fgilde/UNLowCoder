﻿using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using UNLowCoder.Core.Data; // Namespace anpassen

namespace UNLowCoder.SourceGen
{
    internal class UnLocodeCodeGeneratorCore
    {
        public UnLocodeCodeGeneratorCore()
        {
        }

        public SourceText CreateClass(GeneratorDataContext context)
        {
            // Wir ignorieren hier deinen bisherigen translationReader, da du ja nun UN/LOCODE-Daten generieren willst.
            // context.Countries enthält alle Daten

            // Erzeuge eine Klasse mit dem Namen context.GeneratedClassName
            // public static partial class {GeneratedClassName}
            var classDecl = SyntaxFactory.ClassDeclaration(context.GeneratedClassName)                
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                              SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                              SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            // Für jedes Land erstellen wir eine statische Property
            // Außerdem erstellen wir eine "All"-Property, die ein Array aller Länder enthält.

            // Generieren wir zuerst den Code als Text und parsen ihn dann.
            var sb = new StringBuilder();

            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// Dieser Code wurde vom Source Generator erzeugt.");
            sb.AppendLine("// Änderungen an dieser Datei gehen bei neuem Build verloren.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();
            sb.AppendLine($"using UNLowCoder.Core.Data;");
            sb.AppendLine($"namespace {context.Namespace}");            
            sb.AppendLine("{");
            sb.AppendLine($"    public static partial class {context.GeneratedClassName}");
            sb.AppendLine("    {");

            // Für jedes Land ein Feld erzeugen
            // Wir konstruieren UnLocodeCountry:
            // public static UnLocodeCountry DE { get; } = new UnLocodeCountry("DE", "Germany", ...);

            foreach (var country in context.Countries)
            {
                // Subdivisions serialisieren
                var subdivisionInits = string.Join(",\r\n", country.Subdivisions.Select(s =>
                    $"new UnLocodeSubdivision(\"{Escape(s.CountryCode)}\", \"{Escape(s.SubdivisionCode)}\", {EscapeString(s.Name)}, {EscapeString(s.Type)})"));

                if (string.IsNullOrEmpty(subdivisionInits))
                    subdivisionInits = ""; // kein Eintrag
                else
                    subdivisionInits = subdivisionInits.TrimEnd(',');

                var subdivisionsArray = $"new UnLocodeSubdivision[] {{ {subdivisionInits} }}";

                // Locations serialisieren
                var locationInits = string.Join(",\r\n", country.Locations.Select(l =>
                    $"new UnLocodeLocation(" +
                    $"\"{Escape(l.CountryCode)}\", " +
                    $"\"{Escape(l.LocationCode)}\", " +
                    $"{EscapeString(l.Name)}, " +
                    $"{EscapeString(l.NameWoDiacritics)}, " +
                    $"{(l.SubdivisionCode != null ? EscapeString(l.SubdivisionCode) : "null")}, " +
                    $"LocationStatus.{l.Status}, " +
                    $"LocationFunction.{l.Function}, " +
                    $"{FormatDate(l.LastUpdateDate)}, " +
                    $"{EscapeString(l.IATA)}, " +
                    $"{FormatCoordinates(l.Coordinates)}, " +
                    $"{EscapeString(l.Remarks)}, " +
                    $"ChangeIndicator.{l.Change}" +
                    $")"
                ));

                if (string.IsNullOrEmpty(locationInits))
                    locationInits = "";
                else
                    locationInits = locationInits.TrimEnd(',');

                var locationsArray = $"new UnLocodeLocation[] {{ {locationInits} }}";

                // Land erzeugen
                sb.AppendLine($"        public static UnLocodeCountry {SafeIdentifier(country.CountryCode)} {{ get; }} = new UnLocodeCountry(");
                sb.AppendLine($"            \"{Escape(country.CountryCode)}\",");
                sb.AppendLine($"            {EscapeString(country.CountryName)},");
                sb.AppendLine($"            {subdivisionsArray},");
                sb.AppendLine($"            {locationsArray}");
                sb.AppendLine("        );");
            }

            // Alle Länder in ein Array packen
            var allCountries = string.Join(", ", context.Countries.Select(c => SafeIdentifier(c.CountryCode)));
            sb.AppendLine($"        public static System.Collections.Generic.IReadOnlyList<UnLocodeCountry> All {{ get; }} = new UnLocodeCountry[] {{ {allCountries} }};");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            var sourceAsString = sb.ToString();

            return SourceText.From(sourceAsString, Encoding.UTF8);
        }

        private static string Escape(string s)
        {
            // Escaping von Backslashes und Quotes, einfach gehalten
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string EscapeString(string? s)
        {
            if (s == null) return "null";
            // Verwendet ein verbatim string literal könnte problematisch sein wegen Newlines, hier normal:
            return $"\"{Escape(s)}\"";
        }

        private static string SafeIdentifier(string s)
        {
            // Falls CountryCode z.B. "1A" wäre, ist das kein gültiger Bezeichner.
            // Einfache Heuristik: Falls nicht mit Buchstabe anfängt, prefixen wir mit "_"
            if (string.IsNullOrEmpty(s)) return "_EMPTY";
            if (!char.IsLetter(s[0]))
                return "_" + s;
            // Auch könnte man Sonderzeichen ersetzen
            return s.Replace("-", "_").Replace(" ", "_");
        }

        private static string FormatDate(DateTime? dt)
        {
            if (dt == null) return "null";
            var d = dt.Value;
            return $"new System.DateTime({d.Year}, {d.Month}, {d.Day}, 0, 0, 0, System.DateTimeKind.Unspecified)";
        }

        private static string FormatCoordinates(Coordinates? c)
        {
            if (c == null) return "null";
            return $"new Coordinates({c.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {c.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})";
        }
    }
}
