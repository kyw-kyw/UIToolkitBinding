# UITKBIND004-Warning Unnecessary UITKDataSourceObject attribute

Static types cannot be used as a data source, so `[UITKDataSourceObject]` is not necessary

The following sample generates UITKBIND004:

```cs
[UITKDataSourceObject]
public static partial class Hoge
{
}
```

## Typical fix

Remove `[UITKDataSourceObject]`:

```cs
public static partial class Hoge
{
}
```

An automated code fix is available for this.
