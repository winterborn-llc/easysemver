using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R05.</summary>
public class TestProjectClassAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new ProjectClassAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ClassesAreUnchanged()
    {
        var signatures = Build.Compare(Build.Project(Build.Class()), Build.Project(Build.Class()));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void ClassIsAdded()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class()),
            Build.Project(Build.Class(), Build.Class("Test.Another")));

        Assert.Equal(["Test.Another"], Evaluator.FindDifferences(signatures));
    }

    /// <summary>A new interface is R19's business, not this rule's.</summary>
    [Fact]
    public void AddedInterfaceIsNotAClass()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class()),
            Build.Project(Build.Class(), Build.Interface("Test.IThing")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
