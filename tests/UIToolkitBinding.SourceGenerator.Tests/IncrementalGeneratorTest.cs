namespace UIToolkitBinding.SourceGenerator.Tests;

public class IncrementalGeneratorTest
{
    [Fact]
    public void CheckReasons()
    {
        var step1 = """
[UITKDataSourceObject]
public partial class DataSource
{
    [UITKBindableField] int data;
}
""";
        var step2 = """
[UITKDataSourceObject]
public partial class DataSource
{
    [UITKBindableField] int data;
    int data2;
}
""";
        var step3 = """
[UITKDataSourceObject]
public partial class DataSource
{
    [UITKBindableField] int data;
    [UITKBindableField] int data2;
}
""";

        var reasons = SourceGeneratorRunner.GetIncrementalGeneratorTrackedStepsReasons("UIToolkitBindingSourceGenerator.SyntaxProvider.", step1, step2, step3);
        VerifySourceOutputReasonIsCached(reasons[1]);
        VerifySourceOutputReasonIsNotCached(reasons[2]);
    }

    static void VerifySourceOutputReasonIsCached((string Key, string Reasons)[] reasons)
    {
        var reason = reasons.FirstOrDefault(x => x.Key == "SourceOutput").Reasons;
        Assert.Equal("Cached", reason);
    }

    static void VerifySourceOutputReasonIsNotCached((string Key, string Reasons)[] reasons)
    {
        var reason = reasons.FirstOrDefault(x => x.Key == "SourceOutput").Reasons;
        Assert.NotEqual("Cached", reason);
    }
}
