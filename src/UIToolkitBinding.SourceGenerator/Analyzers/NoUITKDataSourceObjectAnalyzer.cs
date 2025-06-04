using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace UIToolkitBinding.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NoUITKDataSourceObjectAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        DiagnosticDescriptors.NoNeedToAssignUITKBindableFieldAttribute);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationStartAction(static context =>
        {
            if (context.Compilation.GetTypeByMetadataName(AttributeConstants.UITKDataSourceObjectAttribute) is not null)
            {
                context.RegisterSyntaxNodeAction(c => Analyze(c), SyntaxKind.ClassDeclaration, SyntaxKind.RecordDeclaration);
            }
        });
    }

    static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        var declaredSymbol = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
        var semanticModel = context.SemanticModel;

        if (declaredSymbol == null) return;

        if ((declaredSymbol.TypeKind is TypeKind.Class or TypeKind.Struct) &&
            declaredSymbol.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == AttributeConstants.UITKDataSourceObjectAttribute)) return;

        AnalyzeMembers(context, semanticModel, declaredSymbol);
    }

    static void AnalyzeMembers(SyntaxNodeAnalysisContext context, SemanticModel semanticModel, INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IFieldSymbol fieldSymbol)
            {
                var bindableFieldAttribute = fieldSymbol.GetAttributes()
                    .FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == AttributeConstants.UITKBindableFieldAttribute);
                if (bindableFieldAttribute != null && bindableFieldAttribute.ApplicationSyntaxReference != null)
                {
                    var location = Location.Create(semanticModel.SyntaxTree, bindableFieldAttribute.ApplicationSyntaxReference.Span);
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NoNeedToAssignUITKBindableFieldAttribute, location));
                }
            }
        }
    }
}
