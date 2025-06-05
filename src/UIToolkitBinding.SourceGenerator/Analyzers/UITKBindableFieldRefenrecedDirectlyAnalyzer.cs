using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using UIToolkitBinding.Core;

namespace UIToolkitBinding.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UITKBindableFieldRefenrecedDirectlyAnalyzer : DiagnosticAnalyzer
{
    public const string fieldNameKey = "FieldName";
    public const string propertyNameKey = "PropertyName";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        DiagnosticDescriptors.BindableFieldReferencedDirectly);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static context =>
        {
            if (context.Compilation.GetTypeByMetadataName(AttributeConstants.UITKDataSourceObjectAttribute) is not null)
            {
                context.RegisterOperationAction(static c => Analyze(c), OperationKind.FieldReference);
            }
        });
    }

    static void Analyze(OperationAnalysisContext context)
    {
        if (context.Operation is not IFieldReferenceOperation
            {
                Field: IFieldSymbol { IsStatic: false, IsConst: false, IsImplicitlyDeclared: false, ContainingType: INamedTypeSymbol } fieldSymbol,
                Instance.Type: ITypeSymbol typeSymbol
            }) return;

        if (context.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.Constructor, ContainingType: INamedTypeSymbol instanceType }
        && SymbolEqualityComparer.Default.Equals(instanceType, typeSymbol)) return;

        if (fieldSymbol.ContainsAttribute(AttributeConstants.UITKBindableFieldAttribute))
        {
            var properties = ImmutableDictionary.Create<string, string?>()
                .Add(fieldNameKey, fieldSymbol.Name)
                .Add(propertyNameKey, UITKBindableFieldContext.ToPropertyName(fieldSymbol.Name));
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.BindableFieldReferencedDirectly, context.Operation.Syntax.GetLocation(), properties, fieldSymbol.Name));
        }
    }
}
