using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R29.</summary>
public class TestFieldAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new FieldAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void FieldsAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithFields(Build.Field()),
            Build.Class().WithFields(Build.Field()));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void FieldIsAdded()
    {
        var signatures = Build.Compare(
            Build.Class(),
            Build.Class().WithFields(Build.Field()));

        Assert.Equal(["Test.TestType.TestField"], Evaluator.FindDifferences(signatures));
    }
}
