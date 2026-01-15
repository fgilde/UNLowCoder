using Microsoft.CodeAnalysis;
using UNLowCoder;
using UNLowCoder.Core;
using UNLowCoder.Core.Data;
using UNLowCoder.SourceGen;

[Generator(LanguageNames.CSharp)]
public sealed class UnLocodeGenerator : IIncrementalGenerator
{
    private record struct ZipInput(AdditionalText File, long Length, DateTime LastWriteUtc);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var zipFiles = context.AdditionalTextsProvider
            .Where(static f => f.Path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            .Select(static (file, _) =>
            {
                var fi = new FileInfo(file.Path);
                return new ZipInput(file,
                    fi.Exists ? fi.Length : -1,
                    fi.Exists ? fi.LastWriteTimeUtc : DateTime.MinValue);
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
                // Roslyn-API: out string? (nullable!)
                bool TryGet(string key, out string? value) =>
                    optProvider.GlobalOptions.TryGetValue(key, out value);

                var nsResolver = new NamespaceResolver(
                    originFilePath: zip.File.Path,
                    fallBackRootNamespace: comp.AssemblyName ?? "Generated",
                    optionsGetterFunc: TryGet);

                var countries = TryEnrich(UnLocodeParser.ParseZipArchive(zip.File.Path, ParseMode.OnlyNewest));
                

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
#pragma warning disable RS1035
                var extraInfo = Directory.GetCurrentDirectory();
#pragma warning restore RS1035

                var ex = e as FileNotFoundException;
                if (ex != null)
                {
                    extraInfo = $"{ex.FileName}";
                }

                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("LOC001", "Error", "{0}", "UnLocodeGen",
                        DiagnosticSeverity.Error, true),
                    Location.None, $"ExtraInfo: {extraInfo} - {e}"));  
            }
        });
    }


    private static List<UnLocodeCountry> TryEnrich(List<UnLocodeCountry> countries)
    {
        try
        {
            var asm = typeof(UnLocodeCodeGeneratorCore).Assembly;

            var resourceName = asm
                .GetManifestResourceNames()
                .FirstOrDefault(n =>
                    n.EndsWith("code-list-improved.zip", StringComparison.OrdinalIgnoreCase));

            if (resourceName is null)
                return countries;

            using var zipStream = asm.GetManifestResourceStream(resourceName);
            if (zipStream is null)
                return countries;

            var improved = UnLocodeParser.ParseZipStream(zipStream);

            if(improved?.Any() == true)
                return UnLocodeEnrichment.EnrichCoordinates(countries, improved);
        }
        catch
        {}
        return countries;
    }
}
