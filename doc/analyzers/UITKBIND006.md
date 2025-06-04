# UITKBIND006-Error Invalid Inheritance

The base class that implements `InotifyBindablePropertyChanged` must be given `[UITKDataSourceObject]`.

The following sample generates UITKBIND006:

```cs
public class BaseDataSource : InotifyBindablePropertyChanged
{
    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
}

[UITKDataSourceObject]
public partial class DerivedDataSource : BaseDataSource
{
    [UITKBindableField]
    int id;
}
```

## Typical fix

Add `[UITKDataSourceObject]` to the target base class:

```cs
[UITKDataSourceObject]
public partial class BaseDataSource : InotifyBindablePropertyChanged
{
    [UITKBindableField]
    int data;
}

[UITKDataSourceObject]
public partial class DerivedDataSource : BaseDataSource
{
    [UITKBindableField]
    int id;
}
```

An automated code fix is available for this.
