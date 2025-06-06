using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using UIToolkitBinding.Analyzers;

namespace UIToolkitBinding.SourceGenerator.Tests;

public class DiagnosticsTest
{
    [Fact]
    public async Task Error_UITKBIND001()
    {
        var code = """
[UITKDataSourceObject]
public class Hoge;
""";
        await VerifyAnalyzerDiagnostic(code, DiagnosticDescriptors.MustBePartialId, "Hoge");
    }

    [Fact]
    public async Task Error_UITKBIND002()
    {
        var code = """
public class Hoge
{
    [UITKDataSourceObject]
    public partial class Fuga
    {
    }
}
""";
        await VerifyAnalyzerDiagnostic(code, DiagnosticDescriptors.InvalidNestId, "Fuga");
    }

    [Fact]
    public async Task Error_UITKBIND003()
    {
        var code = """
[UITKDataSourceObject]
public partial class Hoge
{
    [UITKBindableField(DeclaredAccessibility.Private, SetterAccessibility.Public)] int foo;
}
""";
        await VerifyAnalyzerDiagnostic(code, DiagnosticDescriptors.InvalidSetAccessorId, "UITKBindableField(DeclaredAccessibility.Private, SetterAccessibility.Public)");
    }

    [Fact]
    public async Task Warning_UITKBIND004()
    {
        var code = """
[UITKDataSourceObject]
public static partial class Hoge;
""";
        await VerifyAnalyzerDiagnostic(code, DiagnosticDescriptors.UnnecessaryDataSourceAttributeId, "Hoge");
    }

    [Fact]
    public async Task Warning_UITKBIND005()
    {
        var code = """
[UITKDataSourceObject]
public partial class Hoge
{
    [UITKBindableField] static int foo;
}
""";
        await VerifyAnalyzerDiagnostic(code, DiagnosticDescriptors.UnnecessaryBindableFieldAttributeId, "UITKBindableField");

        var code2 = """
public partial class Hoge
{
    [UITKBindableField] int foo;
}
""";
        await VerifyAnalyzerDiagnostic(code2, DiagnosticDescriptors.UnnecessaryBindableFieldAttributeId, "UITKBindableField");
    }

    [Fact]
    public async Task Warning_UITKBIND008()
    {
        var code = """
[UITKDataSourceObject]
public partial class Hoge
{
    [UITKBindableField] int foo;

    public void Test()
    {
        foo = 2;
    }
}
""";
        await VerifyAnalyzerDiagnostic(code, DiagnosticDescriptors.BindableFieldReferencedDirectlyId, "foo");
    }

    [Fact]
    public async Task Error_UITKBIND009()
    {
        var code = """
[UITKDataSourceObject]
public partial class Hoge
{
    [UITKBindableField] int Value;
}
""";
        await VerifyAnalyzerDiagnostic(code, DiagnosticDescriptors.FieldConflictsWithGeneratedPropertyId, "Value");
    }

    static async Task VerifyAnalyzerDiagnostic(string code, string id, string diagnosticsCodeSpan = null!)
    {
        var (compilation, diagnostics) = SourceGeneratorRunner.RunGenerator(code);
        var compilationWithAnalyzers = compilation.WithAnalyzers(SourceGeneratorRunner.Analyzers);

        diagnostics = diagnostics.AddRange(await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync());
        Assert.Single(diagnostics);

        var diagnostic = diagnostics[0];
        Assert.Equal(id, diagnostic.Id);

        if (string.IsNullOrEmpty(diagnosticsCodeSpan)) return;

        var text = GetLocationText(diagnostics[0], compilation.SyntaxTrees);
        Assert.Equal(diagnosticsCodeSpan, text);
    }

    static string GetLocationText(Diagnostic diagnostic, IEnumerable<SyntaxTree> syntaxTrees)
    {
        var location = diagnostic.Location;

        var textSpan = location.SourceSpan;
        var sourceTree = location.SourceTree;
        if (sourceTree == null)
        {
            var lineSpan = location.GetLineSpan();
            if (lineSpan.Path == null) return "";

            sourceTree = syntaxTrees.FirstOrDefault(x => x.FilePath == lineSpan.Path);
            if (sourceTree == null) return "";
        }

        var text = sourceTree.GetText().GetSubText(textSpan).ToString();
        return text;
    }
}
