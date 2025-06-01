using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace UIToolkitBinding.SourceGenerator.Tests;

internal static class SourceGeneratorRunner
{
    static Compilation baseCompilation = default!;

    [ModuleInitializer]
    public static void InitializeCompilation()
    {
        var globalUsings = """
global using System;
global using System.Threading;
global using System.Threading.Tasks;
global using UIToolkitBinding;
""";
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
            .Select(x => MetadataReference.CreateFromFile(x.Location))
            .Concat(GetSelfReferences());

        var compilation = CSharpCompilation.Create("generatortest",
            references: references,
            syntaxTrees: [CSharpSyntaxTree.ParseText(globalUsings, path: "GlobalUsing.cs")],
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication, allowUnsafe: true));

        baseCompilation = compilation;

        static IEnumerable<MetadataReference> GetSelfReferences()
        {
            yield return MetadataReference.CreateFromFile(typeof(UITKDataSourceObjectAttribute).Assembly.Location);
        }
    }

    public static (Compilation, ImmutableArray<Diagnostic>) RunGenerator([StringSyntax("C#-test")] string source, string[]? preprocessorSymbols = null, AnalyzerConfigOptionsProvider? options = null)
    {
        preprocessorSymbols ??= ["NET8_0_OR_GREATER"];

        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp13, preprocessorSymbols: preprocessorSymbols);

        var driver = CSharpGeneratorDriver.Create(new UIToolkitBindingSourceGenerator()).WithUpdatedParseOptions(parseOptions);
        if (options != null)
        {
            driver = (CSharpGeneratorDriver)driver.WithUpdatedAnalyzerConfigOptions(options);
        }

        var compilation = baseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, parseOptions));
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out var diagnostics);
        return (newCompilation, diagnostics);
    }

    public static (string Key, string Reasons)[][] GetIncrementalGeneratorTrackedStepsReasons(string keyPrefixFilter, params string[] sources)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp13);
        var driver = CSharpGeneratorDriver.Create(
            [new UIToolkitBindingSourceGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true))
            .WithUpdatedParseOptions(parseOptions);

        var generatorResults = sources
            .Select(source =>
            {
                var compilation = baseCompilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, parseOptions));
                driver = driver.RunGenerators(compilation);
                return driver.GetRunResult().Results[0];
            })
            .ToArray();

        var result = generatorResults
            .Select(x => x.TrackedSteps
                .Where(x => x.Key.StartsWith(keyPrefixFilter) || x.Key == "SourceOutput")
                .Select(x =>
                {
                    if (x.Key == "SourceOutput")
                    {
                        var values = x.Value.Where(x => x.Inputs[0].Source.Name?.StartsWith(keyPrefixFilter) ?? false);
                        return (
                            x.Key,
                            Reasons: string.Join(", ", values.SelectMany(x => x.Outputs).Select(x => x.Reason).ToArray())
                        );
                    }
                    else
                    {
                        return (
                            Key: x.Key.Substring(keyPrefixFilter.Length),
                            Reasons: string.Join(", ", x.Value.SelectMany(x => x.Outputs).Select(x => x.Reason).ToArray())
                        );
                    }
                })
                .OrderBy(x => x.Key)
                .ToArray())
            .ToArray();

        return result;
    }
}
