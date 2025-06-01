using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UIToolkitBinding.CodeEmitters;
using UIToolkitBinding.Core;

namespace UIToolkitBinding;

[Generator(LanguageNames.CSharp)]
public sealed class UIToolkitBindingSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var source = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeConstants.UITKDataSourceObjectAttribute,
                static (node, ct) => true,
                static (context, ct) => context)
            .WithTrackingName("UIToolkitBindingSourceGenerator.SyntaxProvider.0_ForAttributeWithMetadataName")
            .Collect()
            .Select(static (xs, _) =>
            {
                var list = new List<UITKDataSourceObjectContext>(xs.Length);
                foreach (var x in xs)
                {
                    var typeDecl = x.TargetNode as TypeDeclarationSyntax;
                    var typeSymbol = x.TargetSymbol as INamedTypeSymbol;
                    var context = UITKDataSourceObjectContext.Create(typeDecl!, typeSymbol!);
                    if (context != null) list.Add(context);
                }
                list.Sort(static (x, y) => string.Compare(x.ClassName, y.ClassName, StringComparison.Ordinal));
                return new EquatableArray<UITKDataSourceObjectContext>(list.ToArray());
            })
            .WithTrackingName("UIToolkitBindingSourceGenerator.SyntaxProvider.1_CollectAndSelect");

        context.RegisterSourceOutput(source, Emit);
    }

    static void Emit(SourceProductionContext context, EquatableArray<UITKDataSourceObjectContext> dataSourceObjectContexts)
    {
        foreach (var dataSourceObjectContext in dataSourceObjectContexts)
        {
            var code = CodeEmitter_UITKDataSourceObject.Generate(dataSourceObjectContext);
            AddSource(context, dataSourceObjectContext.ClassName, code);
        }

        CodeEmitter.Clear();
    }

    static void AddSource(SourceProductionContext context, string fileName, string content)
    {
        var code = NormalizeNewLines(content);
        context.AddSource($"{GetSanitizedFileName(fileName)}.UIToolkitBinding.g.cs", code);

        static string GetSanitizedFileName(string fileName)
        {
            return fileName.Replace("global::", "")
                .Replace("<", "_")
                .Replace(">", "_");
        }

        static string NormalizeNewLines(string content)
        {
            return content.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }
    }
}
