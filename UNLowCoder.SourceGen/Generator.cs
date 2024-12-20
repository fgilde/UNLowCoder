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
                // r f
                var countries = UnLocodeParser.ParseZipArchive(file.Path).Take(2).ToList();
                var ctx = new GeneratorDataContext(file, countries, namespaceResolver);
                var sourceText = codeGenerator.CreateClass(ctx);
                context.AddSource(ctx.GeneratedFileName, sourceText); //


                File.WriteAllText("D:\\test" + ctx.GeneratedFileName, sourceText.ToString());
            }
            catch (Exception e)
            {
                //
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("LOC001", "Error", e.Message, "UnLocodeGen", DiagnosticSeverity.Error, true), Location.None));
            }
        }


    }
}
