namespace UIToolkitBinding;

internal static class AccessibilityExtensions
{
    public static string ToKeyword(this DeclaredAccessibility accessibility)
    {
        return accessibility switch
        {
            DeclaredAccessibility.Protected => "protected",
            DeclaredAccessibility.Internal => "internal",
            DeclaredAccessibility.ProtectedInternal => "protected internal",
            DeclaredAccessibility.Private => "private",
            DeclaredAccessibility.PrivateProtected => "private protected",
            _ => "public"
        };
    }

    public static string ToSetterAccessorDeclaration(this SetterAccessibility setAccessor)
    {
        return setAccessor switch
        {
            SetterAccessibility.Public => "set",
            SetterAccessibility.Protected => "protected set",
            SetterAccessibility.Internal => "internal set",
            SetterAccessibility.ProtectedInternal => "protected internal set",
            SetterAccessibility.Private => "private set",
            SetterAccessibility.PrivateProtected => "private protected set",
            _ => throw new ArgumentOutOfRangeException($"SetAccessor: {nameof(setAccessor)}"),
        };
    }
}
