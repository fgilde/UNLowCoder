using Microsoft.CodeAnalysis;
using UNLowCoder;
using UNLowCoder.SourceGen;

[Generator]
public class UnLocodeGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Kein SyntaxReceiver notwendig
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (!System.Diagnostics.Debugger.IsAttached)
            System.Diagnostics.Debugger.Launch();

        int i = 2;

        if (!context.AdditionalFiles.Any())
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("LOC001", "Info", "Did not find additional files", "UnLocodeGen", DiagnosticSeverity.Info, true),
                Location.None));
            return;
        }

        var codeGenerator = new UnLocodeCodeGeneratorCore();

        foreach (var file in context.AdditionalFiles.Where(p => p.Path.EndsWith(".zip")))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("LOC001", "Info", "Handle " + file.Path, "UnLocodeGen", DiagnosticSeverity.Info, true),
                Location.None));

            var namespaceResolver = new NamespaceResolver(file.Path, context.Compilation.AssemblyName, context.AnalyzerConfigOptions.GlobalOptions.TryGetValue);

            try
            {
                var countries = UnLocodeParser.ParseZipArchive(file.Path, true);
                var ctx = new GeneratorDataContext(file, countries, namespaceResolver);

                // Main class
                var mainSource = codeGenerator.CreateMainClass(ctx);
                context.AddSource(ctx.GeneratedClassName + ".g.cs", mainSource);

                // Countries
                var countriesSource = codeGenerator.CreateCountriesClass(ctx);
                context.AddSource(ctx.GeneratedClassName + ".Countries.g.cs", countriesSource);

                // Subdivisions
                var subdivisionsSource = codeGenerator.CreateSubdivisionsClass(ctx);
                context.AddSource(ctx.GeneratedClassName + ".Subdivisions.g.cs", subdivisionsSource);

                // Locations (Chunked by country)
                var locationChunks = codeGenerator.CreateLocationChunks(ctx);
                foreach (var chunk in locationChunks)
                {
                    context.AddSource(ctx.GeneratedClassName + "." + chunk.FileName, chunk.Source);
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("LOC001", "Error", e.Message, "UnLocodeGen", DiagnosticSeverity.Error, true),
                    Location.None));
            }
        }
    }
}
