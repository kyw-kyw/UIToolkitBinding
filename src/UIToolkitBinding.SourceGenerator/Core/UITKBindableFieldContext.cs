﻿using Microsoft.CodeAnalysis;

namespace UIToolkitBinding.Core;

internal sealed record UITKBindableFieldContext : UITKBindableMemberContext
{
    public required override string Type { get; init; }
    public required override string FieldName { get; init; }
    public required override string PropertyName { get; init; }
    public override DeclaredAccessibility DeclaredAccessibility { get; init; }
    public override SetterAccessibility SetterAccessibility { get; init; }
    public override bool IsOldPropertyValueDirectlyReferenced { get; init; }

    public static UITKBindableFieldContext? Create(IFieldSymbol fieldSymbol)
    {
        if (fieldSymbol.IsStatic) return null;

        var bindableFieldAttribute = fieldSymbol.GetAttributes()
            .FirstOrDefault(static x => x.AttributeClass?.ToDisplayString() == AttributeConstants.UITKBindableFieldAttribute);

        if (bindableFieldAttribute == null) return null;

        var fieldName = fieldSymbol.Name;
        var propertyName = ToPropertyName(fieldName);

        if (fieldName == propertyName) return null;

        var args = bindableFieldAttribute.ConstructorArguments;
        var declaredAccessibility = DeclaredAccessibility.Public;
        var setterAccessibility = SetterAccessibility.Public;
        if (args.Length > 0)
        {
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
        }

        var isOldPropertyValueDirectlyReferenced = HasImplementedPartialMethodWithOldValueAsArgs(fieldSymbol.ContainingType, propertyName);

        return new UITKBindableFieldContext()
        {
            Type = fieldSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            FieldName = fieldName,
            PropertyName = propertyName,
            DeclaredAccessibility = declaredAccessibility,
            SetterAccessibility = setterAccessibility,
            IsOldPropertyValueDirectlyReferenced = isOldPropertyValueDirectlyReferenced
        };
    }

    static bool HasImplementedPartialMethodWithOldValueAsArgs(INamedTypeSymbol? typeSymbol, string propertyName)
    {
        if (typeSymbol == null) return false;

        foreach (var symbol in typeSymbol.GetMembers($"On{propertyName}Changing"))
        {
            if (symbol is IMethodSymbol { Parameters.Length: 2 }) return true;
        }
        foreach (var symbol in typeSymbol.GetMembers($"On{propertyName}Changed"))
        {
            if (symbol is IMethodSymbol { Parameters.Length: 2 }) return true;
        }
        return false;
    }

    internal static string ToPropertyName(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName)) return fieldName;

        var span = fieldName.AsSpan();
        if (span[0] == '_')
        {
            span = span.Slice(1);
        }
        else if (span.Length > 2 && span.Slice(0, 2).SequenceEqual(['m', '_']))
        {
            span = span.Slice(2);
        }

        Span<char> buffer = stackalloc char[span.Length];
        span.CopyTo(buffer);
        if (char.IsLower(buffer[0])) buffer[0] = char.ToUpperInvariant(buffer[0]);
        return buffer.ToString();
    }
}
