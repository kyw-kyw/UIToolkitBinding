# UITKBIND010-Error Conflicts between generated properties

Conflicts between generated properties

The following sample generates UITKBIND0010:

```cs
[UITKDataSourceObject]
public partial class DataSource
{
    [UITKBindableField] int value;
    [UITKBindableField] int _value;
}
```

## Typical fix

Rename the field so that the generated property name does not match the conflicting property name:

```cs
[UITKDataSourceObject]
public partial class DataSource
{
    [UITKBindableField] int value;
    [UITKBindableField] int value1;
}
```
