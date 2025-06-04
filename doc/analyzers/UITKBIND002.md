# UITKBIND002-Error The parent of nested UITKDataSource object type must be partial

Parents of nested types assigned `[UITKDataSourceObject]` smust use the partial modifier.

The following sample generates UITKBIND002:

```cs
public class Container
{
    [UITKDataSourceObject]
    partial class Nested
    {
        [UITKBindableField] int data;
    }
}
```

## Typical fix

Add the `partial` keyword:

```cs
public partial class Container
{
    [UITKDataSourceObject]
    partial class Nested
    {
        [UITKBindableField] int data;
    }
}
```
