# UITKBIND003-Error InvalidSetAccessor

The accessibility modifier of the set accessor must be more restrictive than the property

The following sample generates UITKBIND003:

```cs
[UITKDataSourceObject]
public partial class DataSource
{
    [UITKBindableField(DeclaredAccessibility.Internal, SetterAccessibility.Public)] 
    int data;
}
```

## Typical fix

`SetterAccessibility` should be lower than `DeclaredAccessibility`:

```cs
[UITKDataSourceObject]
public partial class DataSource
{
    [UITKBindableField(DeclaredAccessibility.Internal, SetterAccessibility.Private)] 
    int data;
}
```
