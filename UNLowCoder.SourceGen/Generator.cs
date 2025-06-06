﻿using Microsoft.CodeAnalysis;
using UNLowCoder;
using UNLowCoder.Core;
using UNLowCoder.SourceGen;

[Generator]
public class UnLocodeGenerator : ISourceGenerator
{
    private int i = 6;
    private bool attachDebugger = true;
    private bool generationEnabled = true;
    private DateTime LastGenerated = DateTime.MinValue;
    public void Initialize(GeneratorInitializationContext context)
    {}

    public void Execute(GeneratorExecutionContext context)
    {
        if (!generationEnabled)
            return;
        
        LastGenerated = DateTime.Now;
        if (attachDebugger && !System.Diagnostics.Debugger.IsAttached)
            System.Diagnostics.Debugger.Launch();


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
                var countries = UnLocodeParser.ParseZipArchive(file.Path, ParseMode.OnlyNewest);
                var ctx = new GeneratorDataContext(file, countries, namespaceResolver);

                // Main class
                var mainSource = codeGenerator.CreateMainClass(ctx);
                context.AddSource(ctx.GeneratedClassName + ".g.cs", mainSource);

                // Countries
                var countriesSource = codeGenerator.CreateCountriesClass(ctx);
                context.AddSource(ctx.GeneratedClassName + $".{ctx.StaticCountriesClassName}.g.cs", countriesSource);

                // Subdivisions
                var subdivisionsSource = codeGenerator.CreateSubdivisionsClass(ctx);
                context.AddSource(ctx.GeneratedClassName + $".{ctx.StaticDivisionsClassName}.g.cs", subdivisionsSource);

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
