using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using UIToolkitBinding.Core;

namespace UIToolkitBinding.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UITKDataSourceObjectAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            DiagnosticDescriptors.MustBePartial,
            DiagnosticDescriptors.InvalidNest,
            DiagnosticDescriptors.InvalidSetAccessor,
            DiagnosticDescriptors.UnnecessaryDataSourceAttribute,
            DiagnosticDescriptors.NoNeedToAssignUITKBindableFieldAttributeForStaticField,
            DiagnosticDescriptors.InvalidInheritance,
            DiagnosticDescriptors.DontCreatePropertyAttributeShouldBeGiven,
            DiagnosticDescriptors.FieldConflictsWithGeneratedProperty);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
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
            if (!IsValidNest(typeDeclaration, out var errorSourceDecl) && errorSourceDecl is { })
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.InvalidNest, typeDeclaration.Identifier.GetLocation(), errorSourceDecl.Identifier.ToFullString().Trim(), typeDeclaration.Identifier.ToFullString().Trim()));
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
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.NoNeedToAssignUITKBindableFieldAttributeForStaticField, location));
            return;
        }

        var fieldName = fieldSymbol.Name;
        var propertyName = UITKBindableFieldContext.ToPropertyName(fieldName);
        if (fieldName == propertyName)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.FieldConflictsWithGeneratedProperty, fieldSymbol.Locations[0], fieldName));
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

        var serializeField = fieldSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == AttributeConstants.SerializeFieldAttribute);
        var dontCreateProperty = fieldSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == AttributeConstants.DontCreatePropertyAttribute);

        if ((serializeField != null && dontCreateProperty == null)
            || (fieldSymbol.DeclaredAccessibility == Accessibility.Public && dontCreateProperty == null))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.DontCreatePropertyAttributeShouldBeGiven, fieldSymbol.Locations[0]));
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

    static bool IsValidNest(TypeDeclarationSyntax typeDeclaration, out TypeDeclarationSyntax? errorSourceDecl)
    {
        errorSourceDecl = null;
        while (typeDeclaration.Parent is { } parentDecl)
        {
            if (parentDecl is TypeDeclarationSyntax containerDecl)
            {
                if (!containerDecl.Modifiers.Any(static x => x.IsKind(SyntaxKind.PartialKeyword)))
                {
                    errorSourceDecl = containerDecl;
                    return false;
                }
                typeDeclaration = containerDecl;
            }
            else break;
        }
        return true;
    }
}
