using Microsoft.CodeAnalysis;
using UNLowCoder;
using UNLowCoder.SourceGen;

[Generator]
public class UnLocodeGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Kein SyntaxReceiver notwendig für diesen einfachen Test
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (!System.Diagnostics.Debugger.IsAttached)
            System.Diagnostics.Debugger.Launch();
        if (!context.AdditionalFiles.Any())        
            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("LOC001", "Info", "Did not find additional files", "UnLocodeGen", DiagnosticSeverity.Info, true), Location.None));
        
        var codeGenerator = new UnLocodeCodeGeneratorCore();

        foreach (var file in context.AdditionalFiles.Where(p => p.Path.EndsWith(".zip")))
        {            
            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("LOC001", "Info", "Handle " + file.Path, "UnLocodeGen", DiagnosticSeverity.Info, true), Location.None));

            var namespaceResolver = new NamespaceResolver(file.Path, context.Compilation.AssemblyName, context.AnalyzerConfigOptions.GlobalOptions.TryGetValue);
            try
            {
                // r
                var countries = UnLocodeParser.ParseZipArchive(file.Path).Take(2).ToList();
                var ctx = new GeneratorDataContext(file, countries, namespaceResolver);

                // 1) Hauptdatei generieren: loc241csv.g.cs
                //   public partial class loc241csv { public static partial class Countries {} public static partial class Subdivisions {} public static partial class Locations {} }
                var mainSource = codeGenerator.CreateMainClass(ctx);
                context.AddSource(ctx.GeneratedClassName + ".g.cs", mainSource);

                // 2) Countries Datei: loc241csv.Countries.g.cs
                var countriesSource = codeGenerator.CreateCountriesClass(ctx);
                context.AddSource(ctx.GeneratedClassName + ".Countries.g.cs", countriesSource);

                // 3) Subdivisions Datei: loc241csv.Subdivisions.g.cs
                var subdivisionsSource = codeGenerator.CreateSubdivisionsClass(ctx);
                context.AddSource(ctx.GeneratedClassName + ".Subdivisions.g.cs", subdivisionsSource);

                // 4) Locations Datei: loc241csv.Locations.g.cs
                var locationsSource = codeGenerator.CreateLocationsClass(ctx);
                context.AddSource(ctx.GeneratedClassName + ".Locations.g.cs", locationsSource);


                File.WriteAllText("D:\\test" + ctx.GeneratedFileName, mainSource.ToString());
                File.WriteAllText("D:\\test" + ctx.GeneratedFileName + ".Countries.g.cs", countriesSource.ToString());
                File.WriteAllText("D:\\test" + ctx.GeneratedFileName + ".Subdivisions.g.cs", subdivisionsSource.ToString());
                File.WriteAllText("D:\\test" + ctx.GeneratedFileName + ".Locations.g.cs", locationsSource.ToString());
            }
            catch (Exception e)
            {
                //
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("LOC001", "Error", e.Message, "UnLocodeGen", DiagnosticSeverity.Error, true), Location.None));
            }
        }


    }
}
