using UIToolkitBinding;

var data = new DataSource(0);
data.propertyChanged += (s, e) =>
{
    Console.WriteLine($"{e.propertyName} Changed");
};

data.Data1 = 1;
data.Data2 = 1;

var sub = new Sub();
sub.propertyChanged += (s, e) => Console.WriteLine(e.propertyName);
sub.SubValue = 1;
sub.BaseValue = 1;


[UITKDataSourceObject]
public partial class DataSource(int id)
{
    [UITKBindableField] int id = id;
    [UITKBindableField] int data1;
    [UITKBindableField(SetterAccessibility.Internal)] int data2;

    partial void OnData2Changing(int oldValue, int newValue)
    {
        Console.WriteLine($"OnData2Changing: {oldValue} -> {newValue}");
    }

    partial void OnData2Changed(int oldValue, int newValue)
    {
        Console.WriteLine($"OnData2Changed: {oldValue} -> {newValue}");
    }
}


[UITKDataSourceObject]
public partial class Base
{
    [UITKBindableField] int baseValue;
}

[UITKDataSourceObject]
public partial class Sub : Base
{
    [UITKBindableField] int subValue;
}
