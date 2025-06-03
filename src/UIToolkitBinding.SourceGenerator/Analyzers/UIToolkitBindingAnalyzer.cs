using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace UIToolkitBinding.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UIToolkitBindingAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            DiagnosticDescriptors.MustBePartial,
            DiagnosticDescriptors.NestNotAllowed,
            DiagnosticDescriptors.InvalidSetAccessor,
            DiagnosticDescriptors.UnnecessaryDataSourceAttribute,
            DiagnosticDescriptors.UnnecessaryBindableFieldAttribute,
            DiagnosticDescriptors.InvalidInheritance);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationStartAction(context =>
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
            declaredSymbol.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == AttributeConstants.UITKDataSourceObjectAttribute))
        {
            if (typeDeclaration.Parent is TypeDeclarationSyntax)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NestNotAllowed, typeDeclaration.Identifier.GetLocation(), typeDeclaration.Identifier.ToFullString().Trim()));
                return;
            }
            if (typeDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword)))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UnnecessaryDataSourceAttribute, typeDeclaration.Identifier.GetLocation()));
                return;
            }
            if (!typeDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MustBePartial, typeDeclaration.Identifier.GetLocation(), typeDeclaration.Identifier.ToFullString().Trim()));
            }
            if (!IsValidInheritance(declaredSymbol, out var namedTypeSymbol))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidInheritance, typeDeclaration.Identifier.GetLocation(), namedTypeSymbol!.Name));
                return;
            }

            AnalyzeMembers(context, semanticModel, declaredSymbol);
        }
    }

    static void AnalyzeMembers(SyntaxNodeAnalysisContext context, SemanticModel semanticModel, INamedTypeSymbol typeSymbol)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IFieldSymbol fieldSymbol)
            {
                AnalyzeField(context, semanticModel, fieldSymbol);
            }
        }
    }

    static void AnalyzeField(SyntaxNodeAnalysisContext context, SemanticModel semanticModel, IFieldSymbol fieldSymbol)
    {
        var bindableFieldAttribute = fieldSymbol.GetAttributes()
                    .FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == AttributeConstants.UITKBindableFieldAttribute);

        if (bindableFieldAttribute == null) return;

        if (fieldSymbol.IsStatic && bindableFieldAttribute.ApplicationSyntaxReference is not null)
        {
            var location = Location.Create(semanticModel.SyntaxTree, bindableFieldAttribute.ApplicationSyntaxReference.Span);
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UnnecessaryBindableFieldAttribute, location, fieldSymbol.Name));
            return;
        }

        if (bindableFieldAttribute.ConstructorArguments.Length > 0)
        {
            var declaredAccessibility = DeclaredAccessibility.Public;
            var setterAccessibility = SetterAccessibility.Public;
            foreach (var arg in bindableFieldAttribute.ConstructorArguments)
            {
                switch (arg.Type?.ToDisplayString())
                {
                    case "UIToolkitBinding.DeclaredAccessibility":
                        declaredAccessibility = (DeclaredAccessibility)arg.Value!;
                        break;
                    case "UIToolkitBinding.SetterAccessibility":
                        setterAccessibility = (SetterAccessibility)arg.Value!;
                        break;
                }
            }
            if ((int)declaredAccessibility - (int)setterAccessibility > 0 && bindableFieldAttribute.ApplicationSyntaxReference is not null)
            {
                var location = Location.Create(semanticModel.SyntaxTree, bindableFieldAttribute.ApplicationSyntaxReference.Span);
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidSetAccessor, location, declaredAccessibility, setterAccessibility));
            }
        }
    }

    static bool IsValidInheritance(INamedTypeSymbol typeSymbol, out INamedTypeSymbol? errorSourceType)
    {
        errorSourceType = null;
        while (typeSymbol.BaseType is { } baseTypeSymbol)
        {
            if (baseTypeSymbol.ContainsAttribute(AttributeConstants.UITKDataSourceObjectAttribute)) return IsValidInheritance(baseTypeSymbol, out errorSourceType);
            if (baseTypeSymbol.Interfaces.Any(x => x.ToDisplayString() == "UnityEngine.UIElements.INotifyBindablePropertyChanged"))
            {
                errorSourceType = baseTypeSymbol;
                return false;
            }
            typeSymbol = baseTypeSymbol;
        }
        return true;
    }
}
