using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R06.</summary>
public class TestProjectClassesContinueToExist
{
    private static IEvaluateCsharpSignatures Evaluator => new ProjectClassesContinueToExist();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ClassesAreUnchanged()
    {
        var signatures = Build.Compare(Build.Project(Build.Class()), Build.Project(Build.Class()));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void ClassIsRemoved()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class(), Build.Class("Test.Another")),
            Build.Project(Build.Class()));

        Assert.Equal(["Test.Another"], Evaluator.FindDifferences(signatures));
    }

    /// <summary>SIG-04 - identity is the namespace-qualified name, so a move is a remove + add.</summary>
    [Fact]
    public void ClassMovedToAnotherNamespace()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class("Old.Thing")),
            Build.Project(Build.Class("New.Thing")));

        // The finding names the old name, which is the one that disappeared.
        Assert.Equal(["Old.Thing"], Evaluator.FindDifferences(signatures));
    }
}
