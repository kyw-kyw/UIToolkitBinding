# UITKBIND008-Warning Direct field reference to [UITKBindableField] backing field

Fields annotated with `[UITKBindableField]` should not be referenced directly because they cannot notify change value.

The following sample generates UITKBIND008:

```cs
[UITKDataSourceObject]
public partial class DataSource
{
    [UITKBindableField] 
    int data;

    public void SetData(int value)
    {
        data = value;
    }
}
```

## Typical fix

Use the generated property instead:

```cs
[UITKDataSourceObject]
public partial class DataSource
{
    [UITKBindableField] 
    int data;

    public void SetData(int value)
    {
        Data = value;
    }
}
```

An automated code fix is available for this.
