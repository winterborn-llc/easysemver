using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R41, removal half.</summary>
public class TestNestedTypeRemoved
{
    private static IEvaluateCsharpSignatures Evaluator => new NestedTypeRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void NestedTypesAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class(), Build.Class("Test.TestType.Inner").Nested(Build.DefaultTypeName)),
            Build.Project(Build.Class(), Build.Class("Test.TestType.Inner").Nested(Build.DefaultTypeName)));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void NestedTypeIsRemoved()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class(), Build.Class("Test.TestType.Inner").Nested(Build.DefaultTypeName)),
            Build.Project(Build.Class()));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    /// <summary>A namespace-level type is R06/R18's concern, not this rule's.</summary>
    [Fact]
    public void TopLevelTypeRemovalDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class(), Build.Class("Test.Another")),
            Build.Project(Build.Class()));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }
}
