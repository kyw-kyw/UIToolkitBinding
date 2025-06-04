# UITKBIND005-Warning Unnecessary UITKBindableField attribute

Since static fields cannot bind value, there is no need to assign `[UITKBindableField]` to static fields.
Or,You do not need to assign `[UITKBindableField]` to a class that does not have any `[UITKDataSourceObject]` assigned.

The following sample generates UITKBIND005:

```cs
[UITKDataSourceObject]
public partial class DataSource
{
    // static fields cannot bind value
    [UITKBindableField] 
    static int value;
}

public class OtherDataSource
{
    // OtherDataSource does not have [UITKDataSourceObject].
    [UITKBindableField] 
    int value;
}
```

## Typical fix

- If it's a static field, remove `[UITKBindableField]`.
- If a class does not have `[UITKDataSourceObject]`, give it attributes or remove `[UITKBindableField]` from the field.

```cs
[UITKDataSourceObject]
public partial class DataSource
{
    static int value;
}

// Remove [UITKBindableField]
public class OtherDataSource
{
    int value;
}
// Or Add [UITKDataSourceObject]
[UITKDataSourceObject]
public class OtherDataSource
{
    [UITKBindableField] 
    int value;
}

```

An automated code fix is available for this.
