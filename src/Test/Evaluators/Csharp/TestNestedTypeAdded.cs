using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R41, addition half.</summary>
public class TestNestedTypeAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new NestedTypeAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void NestedTypesAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class(), Build.Class("Test.TestType.Inner").Nested(Build.DefaultTypeName)),
            Build.Project(Build.Class(), Build.Class("Test.TestType.Inner").Nested(Build.DefaultTypeName)));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void NestedTypeIsAdded()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class()),
            Build.Project(Build.Class(), Build.Enum("Test.TestType.Mode").Nested(Build.DefaultTypeName)));

        Assert.Equal(["Test.TestType.Mode"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void TopLevelTypeAdditionDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class()),
            Build.Project(Build.Class(), Build.Class("Test.Another")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
