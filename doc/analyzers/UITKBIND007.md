# UITKBIND007-Warning DontCreatePropertyAttribute should be given

If a field with `[UITKBindableField]` is public or has `[SerializeField]`, `[DontCreateProperty]` should be given. By giving the `[DontCreateProperty]`, the binding will go through the property rather than the field.

The following sample generates UITKBIND007:

```cs
[UITKDataSourceObject]
public class DataSource
{
    [UITKBindableField, SerializeField] 
    int data;

    [UITKBindableField] 
    public int value;
}
```

## Typical fix

Add `[DontCreateProperty]` to the target field:

```cs
[UITKDataSourceObject]
public class DataSource
{
    [UITKBindableField, SerializeField, DontCreateProperty] 
    int data;

    [UITKBindableField, DontCreateProperty] 
    public int value;
}
```

An automated code fix is available for this.
