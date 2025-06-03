using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UIToolkitBinding.Core;

internal record UITKDataSourceObjectContext
{
    public required string Namespace { get; init; }
    public required string ClassName { get; init; }
    public EquatableArray<UITKBindableMemberContext> Members { get; init; }
    public bool IsDerivedUITKDataSourceObjectClass { get; init; }
    public bool HasInterfaceImplemented { get; init; }

    public static UITKDataSourceObjectContext? Create(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol)
    {
        var className = typeDeclaration.Identifier.ToFullString().Trim();

        if (!typeDeclaration.Modifiers.Any(static x => x.IsKind(SyntaxKind.PartialKeyword))
            || typeDeclaration.Modifiers.Any(static x => x.IsKind(SyntaxKind.StaticKeyword))
            || typeDeclaration.Parent is TypeDeclarationSyntax
            || !IsValidInheritance(typeSymbol, out var isderived)) return null;

        string ns = string.Empty;
        if (typeDeclaration.Parent is NamespaceDeclarationSyntax nsDeclaration)
        {
            ns = nsDeclaration.Name.ToFullString();
        }
        else if (typeDeclaration.Parent is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclaration)
        {
            ns = fileScopedNamespaceDeclaration.Name.ToFullString();
        }

        int count = 0;
        var members = new UITKBindableMemberContext[typeSymbol.GetMembers().Length];
        bool hasInterfaceImplemented = false;
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IFieldSymbol fieldSymbol)
            {
                var memberContext = UITKBindableFieldContext.Create(fieldSymbol);
                if (memberContext != null) members[count++] = memberContext;
            }
            if (member is IEventSymbol eventSymbol)
            {
                if (eventSymbol.Type.ToDisplayString() == "System.EventHandler<UnityEngine.UIElements.BindablePropertyChangedEventArgs>")
                {
                    hasInterfaceImplemented = true;
                }
            }
        }

        return new UITKDataSourceObjectContext()
        {
            Namespace = ns,
            ClassName = className,
            Members = members.AsSpan(0, count).ToArray(),
            IsDerivedUITKDataSourceObjectClass = isderived,
            HasInterfaceImplemented = hasInterfaceImplemented,
        };
    }

    static bool IsValidInheritance(INamedTypeSymbol typeSymbol, out bool isderived)
    {
        isderived = false;
        while (typeSymbol.BaseType is { } baseType)
        {
            if (baseType.ContainsAttribute(AttributeConstants.UITKDataSourceObjectAttribute))
            {
                isderived = true;
                return IsValidInheritance(baseType, out var _);
            }
            if (baseType.Interfaces.Any(x => x.ToDisplayString() == "UnityEngine.UIElements.INotifyBindablePropertyChanged")) return false;
            typeSymbol = baseType;
        }
        return true;
    }
}
