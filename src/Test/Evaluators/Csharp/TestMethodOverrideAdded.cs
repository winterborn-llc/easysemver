using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

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

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
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

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }
}
