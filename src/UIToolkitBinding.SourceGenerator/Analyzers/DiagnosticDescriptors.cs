using Microsoft.CodeAnalysis;

namespace UIToolkitBinding.Analyzers;

public static class DiagnosticDescriptors
{
    const string Category = "UIToolkitBinding.SourceGenerator";

    public const string MustBePartialId = "UITKBIND001";
    public const string InvalidNestId = "UITKBIND002";
    public const string InvalidSetAccessorId = "UITKBIND003";
    public const string UnnecessaryDataSourceAttributeId = "UITKBIND004";
    public const string UnnecessaryBindableFieldAttributeId = "UITKBIND005";
    public const string InvalidInheritanceId = "UITKBIND006";
    public const string DontCreatePropertyAttributeShouldBeGivenId = "UITKBIND007";
    public const string BindableFieldReferencedDirectlyId = "UITKBIND008";
    public const string FieldConflictsWithGeneratedPropertyId = "UITKBIND009";

    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: MustBePartialId,
        title: "UITKDataSource object type must be partial",
        messageFormat: "UITKDataSource '{0}' must be partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: GetHelpLink(MustBePartialId));

    public static readonly DiagnosticDescriptor InvalidNest = new(
        id: InvalidNestId,
        title: "The parent of nested UITKDataSource object type must be partial",
        messageFormat: "The parent '{0}'of nested type '{1}' must be partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: GetHelpLink(InvalidNestId));

    public static readonly DiagnosticDescriptor InvalidSetAccessor = new(
        id: InvalidSetAccessorId,
        title: "The accessibility modifier of the set accessor must be more restrictive than the property",
        messageFormat: "The accessibility modifier of set accessor must be more restrictive than the property.  property: {0}, set accessor: {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: GetHelpLink(InvalidSetAccessorId));

    public static readonly DiagnosticDescriptor UnnecessaryDataSourceAttribute = new(
        id: UnnecessaryDataSourceAttributeId,
        title: "No need to assign the UITKDataSourceObject attribute to a static class",
        messageFormat: "Static types cannot be used as a data source, so UITKDataSourceObject attribute is not necessary",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: GetHelpLink(UnnecessaryDataSourceAttributeId));

    public static readonly DiagnosticDescriptor NoNeedToAssignUITKBindableFieldAttributeForStaticField = new(
        id: UnnecessaryBindableFieldAttributeId,
        title: "No need to assign the UITKBindableField attribute.",
        messageFormat: "Since static fields cannot bind value, there is no need to assign the UITKBindableField attribute to static fields.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: GetHelpLink(UnnecessaryBindableFieldAttributeId));

    public static readonly DiagnosticDescriptor NoNeedToAssignUITKBindableFieldAttribute = new(
        id: UnnecessaryBindableFieldAttributeId,
        title: "No need to assign the UITKBindableField attribute.",
        messageFormat: "You do not need to assign a UITKBindableField attribute to a class that does not have any UITKDataSource attribute assigned.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: GetHelpLink(UnnecessaryBindableFieldAttributeId));

    public static readonly DiagnosticDescriptor InvalidInheritance = new(
        id: InvalidInheritanceId,
        title: "The parent class implemented INotifyBindablePropertyChanged must be given UITKDataSourceObject attribute.",
        messageFormat: "The parent class '{0}' implemented INotifyBindablePropertyChanged must be given UITKDataSourceObject attribute.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: GetHelpLink(InvalidInheritanceId));

    public static readonly DiagnosticDescriptor DontCreatePropertyAttributeShouldBeGiven = new(
        id: DontCreatePropertyAttributeShouldBeGivenId,
        title: "DontCreateProperty attribute should be given",
        messageFormat: "If a field with UITKBindableField attribute is public or has SerializeField attribute, DontCreateProperty attribute should be given. By giving the DontCreateProperty attribute, the binding will go through the property rather than the field.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: GetHelpLink(DontCreatePropertyAttributeShouldBeGivenId));

    public static readonly DiagnosticDescriptor BindableFieldReferencedDirectly = new(
        id: BindableFieldReferencedDirectlyId,
        title: "Direct field reference to [UITKBindableField] backing field",
        messageFormat: "The field '{0}' annotated with [UITKBindableField] should not be referenced directly. Use property instead.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: GetHelpLink(BindableFieldReferencedDirectlyId));

    public static readonly DiagnosticDescriptor FieldConflictsWithGeneratedProperty = new(
        id: FieldConflictsWithGeneratedPropertyId,
        title: "Field conflicts with generated property",
        messageFormat: "The field '{0}' conflicts with generated property",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: GetHelpLink(FieldConflictsWithGeneratedPropertyId));

    internal static string GetHelpLink(string diagnosticId) => $"https://github.com/kyw-kyw/UIToolkitBinding/blob/main/doc/analyzers/{diagnosticId}.md";
}
