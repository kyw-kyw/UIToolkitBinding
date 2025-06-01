using UIToolkitBinding;

var data = new DataSource(0);
data.propertyChanged += (s, e) =>
{
    Console.WriteLine($"{e.propertyName} Changed");
};

data.Data1 = 1;
data.Data2 = 1;

[UITKDataSourceObject]
public partial class DataSource(int id)
{
    [UITKBindableField] int id = id;
    [UITKBindableField] int data1;
    [UITKBindableField(SetterAccessibility.Internal)] int data2;

    partial void OnData2Changing(int currentValue, int newValue)
    {
        Console.WriteLine($"OnData2Changing: {currentValue} -> {currentValue}");
    }

    partial void OnData2Changed(int oldValue, int currentValue)
    {
        Console.WriteLine($"OnData2Changed: {oldValue} -> {currentValue}");
    }
}
