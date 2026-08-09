using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R10.</summary>
public class TestPropertiesContinueToExist
{
    private static IEvaluateCsharpSignatures Evaluator => new PropertiesContinueToExist();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void PropertiesAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property("One"), Build.Property("Two")),
            Build.Class().WithProperties(Build.Property("One"), Build.Property("Two")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void PropertyIsRemoved()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property("One"), Build.Property("Two")),
            Build.Class().WithProperties(Build.Property("One")));

        Assert.Equal(["Test.TestType.Two"], Evaluator.FindDifferences(signatures));
    }
}
