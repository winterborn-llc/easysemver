using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R04.</summary>
public class TestMethodOverrideAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new MethodOverrideAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void OverloadsAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method(overrides: Build.Override(Build.Parameter()))),
            Build.Class().WithMethods(Build.Method(overrides: Build.Override(Build.Parameter()))));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void OverloadIsAdded()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method(overrides: Build.Override(Build.Parameter()))),
            Build.Class().WithMethods(Build.Method(
                overrides:
                [
                    Build.Override(Build.Parameter()),
                    Build.Override(Build.Parameter(), Build.Parameter("count", "int"))
                ])));

        Assert.Equal(
            ["Test.TestType.TestMethod([string input], [int count])"],
            Evaluator.FindDifferences(signatures));
    }
}
