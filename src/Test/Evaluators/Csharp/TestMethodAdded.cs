using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R15.</summary>
public class TestMethodAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new MethodAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void MethodsAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method("One")),
            Build.Class().WithMethods(Build.Method("One")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void MethodIsAddedToExistingType()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method("One")),
            Build.Class().WithMethods(Build.Method("One"), Build.Method("Two")));

        Assert.Equal(["Test.TestType.Two"], Evaluator.FindDifferences(signatures));
    }

    /// <summary>CLS-02 - members of a brand-new type are R05's concern, not this rule's.</summary>
    [Fact]
    public void MethodOnBrandNewTypeIsNotCounted()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class().WithMethods(Build.Method("One"))),
            Build.Project(
                Build.Class().WithMethods(Build.Method("One")),
                Build.Class("Test.BrandNew").WithMethods(Build.Method("Two"))));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
