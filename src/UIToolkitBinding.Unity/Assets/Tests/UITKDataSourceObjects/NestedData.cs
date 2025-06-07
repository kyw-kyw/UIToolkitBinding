namespace UIToolkitBinding.Unity.Tests
{
    internal partial class Container
    {
        [UITKDataSourceObject]
        public partial class Nest
        {
            [UITKBindableField] int value;
        }
    }
}
