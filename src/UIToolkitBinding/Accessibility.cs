namespace UIToolkitBinding;

#if SOURCE_GENERATOR
internal
#else
public
#endif
enum DeclaredAccessibility : byte
{
    /// <summary>access modifier is <see langword="public"/></summary>
    Public = 0,
    /// <summary>access modifier is <see langword="protected"/></summary>
    Protected = 1,
    /// <summary>access modifier is <see langword="internal"/></summary>
    Internal = 2,
    /// <summary>access modifier is <see langword="protected"/> <see langword="internal"/></summary>
    ProtectedInternal = 3,
    /// <summary>access modifier is <see langword="private"/></summary>
    Private = 4,
    /// <summary>access modifier is <see langword="private"/> <see langword="protected"/></summary>
    PrivateProtected = 5
}

#if SOURCE_GENERATOR
internal
#else
public
#endif
enum SetterAccessibility : byte
{
    /// <summary>access modifier is <see langword="public"/></summary>
    Public = 0,
    /// <summary>access modifier is <see langword="protected"/></summary>
    Protected = 1,
    /// <summary>access modifier is <see langword="internal"/></summary>
    Internal = 2,
    /// <summary>access modifier is <see langword="protected"/> <see langword="internal"/></summary>
    ProtectedInternal = 3,
    /// <summary>access modifier is <see langword="private"/></summary>
    Private = 4,
    /// <summary>access modifier is <see langword="private"/> <see langword="protected"/></summary>
    PrivateProtected = 5
}
