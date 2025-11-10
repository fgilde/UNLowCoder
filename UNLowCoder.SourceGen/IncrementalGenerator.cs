using CsvHelper;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UNLowCoder;
using UNLowCoder.Core;
using UNLowCoder.SourceGen;

#nullable enable

[Generator(LanguageNames.CSharp)]
public sealed class UnLocodeIncrementalGenerator : IIncrementalGenerator
{
    private record struct ZipInput(AdditionalText File, long Length, DateTime LastWriteUtc);

    private bool AttachDebugger = false;
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        if (AttachDebugger && !System.Diagnostics.Debugger.IsAttached)
            System.Diagnostics.Debugger.Launch();
        context.RegisterPostInitializationOutput(pi =>
        {
            pi.AddSource("UnLocodeGenerator.Ping.g.cs", "// Ping from UnLocodeGenerator\n");
        });

        
        using var csv = new CsvWriter(null, CultureInfo.InvariantCulture );
        var cfg = csv.Configuration;

        var zipFiles = context.AdditionalTextsProvider
            .Where(static f => f.Path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            .Select(static (file, _) =>
            {
                var fi = new FileInfo(file.Path);
                return new ZipInput(file,
                    fi.Exists ? fi.Length : -1,
                    fi.Exists ? fi.LastWriteTimeUtc : DateTime.MinValue);
            });

        // Sichtbarkeit: zeigt dir, dass Roslyn die ZIP(s) sieht
        context.RegisterSourceOutput(zipFiles, static (spc, z) =>
        {
            spc.AddSource("UnLocodeGenerator.Puff.g.cs",
                $"// zip: {z.File.Path} | exists= | len={z.Length} | mtime={z.LastWriteUtc:O}\n");
        });

        var compilation = context.CompilationProvider;
        var options = context.AnalyzerConfigOptionsProvider;

        context.RegisterSourceOutput(zipFiles.Collect(), static (spc, files) =>
        {
            if (files.Length == 0)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("LOC001", "Info", "Did not find additional files", "UnLocodeGen",
                        DiagnosticSeverity.Info, true),
                    Location.None));
            }
        });

        var pipeline = zipFiles.Combine(compilation).Combine(options);

        context.RegisterSourceOutput(pipeline, static (spc, triple) =>
        {
            var ((zip, comp), optProvider) = triple;

            spc.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("LOC001", "Info", "Handle {0}", "UnLocodeGen",
                    DiagnosticSeverity.Info, true),
                Location.None, zip.File.Path));

            try
            {
                // Roslyn: out string? (nullable!)
                bool TryGet(string key, out string? value) =>
                    optProvider.GlobalOptions.TryGetValue(key, out value);

                var nsResolver = new NamespaceResolver(
                    originFilePath: zip.File.Path,
                    fallBackRootNamespace: comp.AssemblyName ?? "Generated",
                    optionsGetterFunc: TryGet);

                var countries = UnLocodeParser.ParseZipArchive(zip.File.Path, ParseMode.OnlyNewest);
                var ctx = new GeneratorDataContext(zip.File, countries, nsResolver);

                var codeGen = new UnLocodeCodeGeneratorCore();

                spc.AddSource($"{ctx.GeneratedClassName}.g.cs",
                    codeGen.CreateMainClass(ctx));

                spc.AddSource($"{ctx.GeneratedClassName}.{ctx.StaticCountriesClassName}.g.cs",
                    codeGen.CreateCountriesClass(ctx));

                spc.AddSource($"{ctx.GeneratedClassName}.{ctx.StaticDivisionsClassName}.g.cs",
                    codeGen.CreateSubdivisionsClass(ctx));

                foreach (var chunk in codeGen.CreateLocationChunks(ctx))
                {
                    spc.AddSource($"{ctx.GeneratedClassName}.{chunk.FileName}", chunk.Source);
                }
            }
            catch (Exception e)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("LOC001", "Error", "{0}", "UnLocodeGen",
                        DiagnosticSeverity.Error, true),
                    Location.None, e.ToString()));
            }
        });
    }
}
