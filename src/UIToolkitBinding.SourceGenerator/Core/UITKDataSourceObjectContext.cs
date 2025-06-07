using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UIToolkitBinding.Core;

internal record ParentDataOfNested
{
    public required string TypeDeclarationKeyword { get; init; }
    public required string ClassName { get; init; }
}

internal record UITKDataSourceObjectContext
{
    public required string Namespace { get; init; }
    public EquatableArray<string> Using { get; init; }
    internal EquatableArray<ParentDataOfNested> Parents { get; init; }
    public required string TypeDeclarationKeyword { get; init; }
    public required string ClassName { get; init; }
    public EquatableArray<UITKBindableMemberContext> Members { get; init; }
    public bool IsDerivedUITKDataSourceObjectClass { get; init; }
    public bool HasInterfaceImplemented { get; init; }

    public static UITKDataSourceObjectContext? Create(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol)
    {
        var className = typeDeclaration.Identifier.ToFullString().Trim();

        if (!typeDeclaration.Modifiers.Any(static x => x.IsKind(SyntaxKind.PartialKeyword))
            || typeDeclaration.Modifiers.Any(static x => x.IsKind(SyntaxKind.StaticKeyword))
            || !IsValidNest(typeDeclaration, out var nestClassParents)
            || !IsValidInheritance(typeSymbol, out var isderived)) return null;

        var root = typeDeclaration.SyntaxTree.GetRoot();

        var ns = GetNamespace(typeDeclaration);

        var usingNs = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>()
                .Select(x => "using " + x.Name.ToFullString().Trim() + ";")
                .ToArray();

        var usingStrings = root.DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(x => x.ToFullString().Trim())
                .Concat(["using System", "using System.Collections.Generic", "using UIToolkitBinding", "using Unity.Properties", "using UnityEngine", "using UnityEngine.UIElements"])
                .Concat(usingNs)
                .Select(x => x.Trim(';') + ";")
                .Distinct()
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();

        int count = 0;
        var members = new UITKBindableMemberContext[typeSymbol.GetMembers().Length];
        var propertyNames = new HashSet<string>();
        bool hasInterfaceImplemented = false;
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IFieldSymbol fieldSymbol)
            {
                var memberContext = UITKBindableFieldContext.Create(fieldSymbol);
                if (memberContext != null && !propertyNames.Contains(memberContext.PropertyName))
                {
                    members[count++] = memberContext;
                    propertyNames.Add(memberContext.PropertyName);
                }
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
            Using = usingStrings,
            Parents = nestClassParents,
            TypeDeclarationKeyword = GetTypeDeclarationKeyword(typeDeclaration),
            ClassName = className,
            Members = members.AsSpan(0, count).ToArray(),
            IsDerivedUITKDataSourceObjectClass = isderived,
            HasInterfaceImplemented = hasInterfaceImplemented,
        };
    }

    static string GetNamespace(TypeDeclarationSyntax typeDeclaration)
    {
        SyntaxNode current = typeDeclaration;
        while (current.Parent is { } parentDecl)
        {
            if (parentDecl is NamespaceDeclarationSyntax nsDeclaration)
            {
                return nsDeclaration.Name.ToFullString().Trim();
            }
            if (parentDecl is FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclaration)
            {
                return fileScopedNamespaceDeclaration.Name.ToFullString().Trim();
            }
            current = parentDecl;
        }
        return string.Empty;
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

    static bool IsValidNest(TypeDeclarationSyntax typeDeclaration, out ParentDataOfNested[] nestClassParents)
    {
        nestClassParents = [];
        List<ParentDataOfNested> nestData = [];
        while (typeDeclaration.Parent is { } parentDecl)
        {
            if (parentDecl is TypeDeclarationSyntax containerDecl)
            {
                if (!containerDecl.Modifiers.Any(static x => x.IsKind(SyntaxKind.PartialKeyword)))
                {
                    nestClassParents = [];
                    return false;
                }
                nestData.Add(new ParentDataOfNested()
                {
                    TypeDeclarationKeyword = GetTypeDeclarationKeyword(containerDecl),
                    ClassName = containerDecl.Identifier.ToFullString().Trim()
                });
                typeDeclaration = containerDecl;
            }
            else break;
        }
        if (nestData.Count > 0)
        {
            nestData.Reverse();
            nestClassParents = nestData.ToArray();
        }
        return true;
    }

    static string GetTypeDeclarationKeyword(TypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration.Kind() switch
        {
            SyntaxKind.RecordStructDeclaration => "record struct",
            SyntaxKind.RecordDeclaration => "record",
            SyntaxKind.StructDeclaration => "struct",
            SyntaxKind.ClassDeclaration => "class",
            _ => throw new NotSupportedException()
        };
    }
}
