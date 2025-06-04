# UITKBIND001-Error UITKDataSource object type must be partial

Types assigned `[UITKDataSourceObject]` must use the partial modifier.

The following sample generates UITKBIND001:

```cs
[UITKDataSourceObject]
public class DataSource
{
    [UITKBindableField] int data;
}
```

## Typical fix

Add the `partial` keyword:

```cs
[UITKDataSourceObject]
public partial class DataSource
{
    [UITKBindableField] int data;
}
```

An automated code fix is available for this.
