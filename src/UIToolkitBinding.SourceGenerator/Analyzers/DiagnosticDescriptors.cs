using Microsoft.CodeAnalysis;

namespace UIToolkitBinding.Analyzers;

public static class DiagnosticDescriptors
{
    const string Category = "UIToolkitBinding.SourceGenerator";

    public const string MustBePartialId = "UITKBIND001";
    public const string NestNotAllowedId = "UITKBIND002";
    public const string InvalidSetAccessorId = "UITKBIND003";
    public const string UnnecessaryDataSourceAttributeId = "UITKBIND004";
    public const string UnnecessaryBindableFieldAttributeId = "UITKBIND005";
    public const string InvalidInheritanceId = "UITKBIND006";

    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: MustBePartialId,
        title: "UITKDataSource object type must be partial",
        messageFormat: "UITKDataSource '{0}' must be partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NestNotAllowed = new(
        id: NestNotAllowedId,
        title: "UITKDataSourceObject type must not be nested",
        messageFormat: "UITKDataSource object '{0}' must be not nested",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidSetAccessor = new(
        id: InvalidSetAccessorId,
        title: "The accessibility modifier of the set accessor must be more restrictive than the property",
        messageFormat: "The accessibility modifier of set accessor must be more restrictive than the property.  property: {0}, set accessor: {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnnecessaryDataSourceAttribute = new(
        id: UnnecessaryDataSourceAttributeId,
        title: "No need to assign the UITKDataSourceObject attribute to a static class",
        messageFormat: "Static types cannot be used as a data source, so UITKDataSourceObject attribute is not necessary",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnnecessaryBindableFieldAttribute = new(
        id: UnnecessaryBindableFieldAttributeId,
        title: "No need to assign the UITKBindableField attribute to a static member",
        messageFormat: "Since static fields cannot bind value, there is no need to assign the UITKBindableField attribute to static fields.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidInheritance = new(
        id: InvalidInheritanceId,
        title: "The parent class implemented INotifyBindablePropertyChanged must be given UITKDataSourceObject attribute.",
        messageFormat: "The parent class '{0}' implemented INotifyBindablePropertyChanged must be given UITKDataSourceObject attribute.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
