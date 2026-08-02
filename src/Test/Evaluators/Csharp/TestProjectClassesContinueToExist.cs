using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

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

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void ClassIsRemoved()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class(), Build.Class("Test.Another")),
            Build.Project(Build.Class()));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    /// <summary>SIG-04 - identity is the namespace-qualified name, so a move is a remove + add.</summary>
    [Fact]
    public void ClassMovedToAnotherNamespace()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class("Old.Thing")),
            Build.Project(Build.Class("New.Thing")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }
}
