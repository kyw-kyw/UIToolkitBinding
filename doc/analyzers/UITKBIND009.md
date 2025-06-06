# UITKBIND009-Error Field conflicts with generated property

The field conflicts with generated property.

The following sample generates UITKBIND009:

```cs
[UITKDataSourceObject]
public partial class DataSource
{
    [UITKBindableField] int Value;
}
```

## Typical fix

Field names should use `lowerCamel`, `_lowerCamel` or `m_lowerCamel` pattern to avoid conflict with generated property.

```cs
[UITKDataSourceObject]
public partial class DataSource
{
    [UITKBindableField] int value;
}
```
