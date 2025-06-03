using Microsoft.CodeAnalysis;

namespace UIToolkitBinding;

internal static class ISymbolExtensions
{
    public static bool ContainsAttribute(this ISymbol symbol, string displayString, SymbolDisplayFormat? format = null)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString(format) == displayString) return true;
        }
        return false;
    }
}
