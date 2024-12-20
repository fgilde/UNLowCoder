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
                // code for testing to have less entries
                //int count = 3;
                //var countries = UnLocodeParser.ParseZipArchive(file.Path, true).Take(count).ToList();
                //countries.ForEach(c =>
                //{
                //    c.Locations = c.Locations.Take(count).ToList();
                //    c.Subdivisions = c.Subdivisions.Take(count).ToList();
                //});

                int x = 1;
                var countries = UnLocodeParser.ParseZipArchive(file.Path, true);
                var ctx = new GeneratorDataContext(file, countries, namespaceResolver);

                
                var mainSource = codeGenerator.CreateMainClass(ctx);
                context.AddSource(ctx.GeneratedClassName + ".g.cs", mainSource);

                var countriesSource = codeGenerator.CreateCountriesClass(ctx);
                context.AddSource(ctx.GeneratedClassName + ".Countries.g.cs", countriesSource);

                var subdivisionsSource = codeGenerator.CreateSubdivisionsClass(ctx);
                context.AddSource(ctx.GeneratedClassName + ".Subdivisions.g.cs", subdivisionsSource);

                var locationsSource = codeGenerator.CreateLocationsClass(ctx);
                context.AddSource(ctx.GeneratedClassName + ".Locations.g.cs", locationsSource);


                File.WriteAllText("D:\\test" + ctx.GeneratedFileName, mainSource.ToString());
                File.WriteAllText("D:\\test" + ctx.GeneratedClassName + ".Countries.g.cs", countriesSource.ToString());
                File.WriteAllText("D:\\test" + ctx.GeneratedClassName + ".Subdivisions.g.cs", subdivisionsSource.ToString());
                File.WriteAllText("D:\\test" + ctx.GeneratedClassName + ".Locations.g.cs", locationsSource.ToString());
            }
            catch (Exception e)
            {
                //
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("LOC001", "Error", e.Message, "UnLocodeGen", DiagnosticSeverity.Error, true), Location.None));
            }
        }


    }
}
