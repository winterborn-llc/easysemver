using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R16.</summary>
public class TestPropertyAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new PropertyAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void PropertiesAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property("One")),
            Build.Class().WithProperties(Build.Property("One")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void PropertyIsAddedToExistingType()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property("One")),
            Build.Class().WithProperties(Build.Property("One"), Build.Property("Two")));

        Assert.Equal(["Test.TestType.Two"], Evaluator.FindDifferences(signatures));
    }

    /// <summary>CLS-02 - members of a brand-new type are R05's concern, not this rule's.</summary>
    [Fact]
    public void PropertyOnBrandNewTypeIsNotCounted()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class().WithProperties(Build.Property("One"))),
            Build.Project(
                Build.Class().WithProperties(Build.Property("One")),
                Build.Class("Test.BrandNew").WithProperties(Build.Property("Two"))));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
