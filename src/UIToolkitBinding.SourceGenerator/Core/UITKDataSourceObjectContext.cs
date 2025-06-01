using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UIToolkitBinding.Core;

internal record UITKDataSourceObjectContext(string Namespace, string ClassName, EquatableArray<UITKBindableMemberContext> Members)
{
    public static UITKDataSourceObjectContext? Create(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol)
    {
        var className = typeDeclaration.Identifier.ToFullString().Trim();

        if (!typeDeclaration.Modifiers.Any(static x => x.IsKind(SyntaxKind.PartialKeyword))
            || typeDeclaration.Modifiers.Any(static x => x.IsKind(SyntaxKind.StaticKeyword))
            || typeDeclaration.Parent is TypeDeclarationSyntax) return null;

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
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IFieldSymbol fieldSymbol)
            {
                var memberContext = UITKBindableFieldContext.Create(fieldSymbol);
                if (memberContext != null) members[count++] = memberContext;
            }
        }

        return new UITKDataSourceObjectContext(ns, className, members.AsSpan(0, count).ToArray());
    }
}
