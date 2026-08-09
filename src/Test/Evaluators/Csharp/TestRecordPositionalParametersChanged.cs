using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R27.</summary>
public class TestRecordPositionalParametersChanged
{
    private static IEvaluateCsharpSignatures Evaluator => new RecordPositionalParametersChanged();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void PositionalParametersAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Record().WithPositional(Build.Parameter("Amount", "decimal")),
            Build.Record().WithPositional(Build.Parameter("Amount", "decimal")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void PositionalParameterIsAdded()
    {
        var signatures = Build.Compare(
            Build.Record().WithPositional(Build.Parameter("Amount", "decimal")),
            Build.Record().WithPositional(
                Build.Parameter("Amount", "decimal"),
                Build.Parameter("Currency", "string")));

        Assert.Equal(["Test.TestType"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void PositionalParameterIsRetyped()
    {
        var signatures = Build.Compare(
            Build.Record().WithPositional(Build.Parameter("Amount", "decimal")),
            Build.Record().WithPositional(Build.Parameter("Amount", "double")));

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }
}
