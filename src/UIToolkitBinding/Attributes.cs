namespace UIToolkitBinding;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class UITKDataSourceObjectAttribute : Attribute;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class UITKBindableFieldAttribute : Attribute
{
    public UITKBindableFieldAttribute()
        : this(DeclaredAccessibility.Public, SetterAccessibility.Public)
    {
    }

    public UITKBindableFieldAttribute(SetterAccessibility setterAccessibility)
        : this(DeclaredAccessibility.Public, setterAccessibility)
    {
    }

    public UITKBindableFieldAttribute(DeclaredAccessibility declaredAccessibility, SetterAccessibility setterAccessibility)
    {
        DeclaredAccessibility = declaredAccessibility;
        SetterAccessibility = setterAccessibility;
    }

    public DeclaredAccessibility DeclaredAccessibility { get; }
    public SetterAccessibility SetterAccessibility { get; }
}
