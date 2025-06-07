namespace UIToolkitBinding.Unity.Tests
{
    [UITKDataSourceObject]
    internal partial class Base
    {
        [UITKBindableField]
        int baseData;
    }

    [UITKDataSourceObject]
    internal partial class Sub : Base
    {
        [UITKBindableField]
        int subData;
    }
}
