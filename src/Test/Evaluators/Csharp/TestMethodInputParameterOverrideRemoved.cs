using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R02.</summary>
public class TestMethodInputParameterOverrideRemoved
{
    private static IEvaluateCsharpSignatures Evaluator => new MethodInputParameterOverrideRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
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
    public void OverloadIsRemoved()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method(
                overrides:
                [
                    Build.Override(Build.Parameter()),
                    Build.Override(Build.Parameter(), Build.Parameter("count", "int"))
                ])),
            Build.Class().WithMethods(Build.Method(overrides: Build.Override(Build.Parameter()))));

        // An overload-level finding names the overload, not just the method (SIG-09).
        Assert.Equal(
            ["Test.TestType.TestMethod([string input], [int count])"],
            Evaluator.FindDifferences(signatures));
    }

    /// <summary>
    /// CLS-06 - the matcher is deliberately requiredness-blind, so a requiredness-only change is
    /// not reported as a removed overload. R17 handles that, directionally.
    /// </summary>
    [Fact]
    public void RequirednessAloneIsNotARemoval()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method(
                overrides: Build.Override(Build.Parameter(isRequired: true)))),
            Build.Class().WithMethods(Build.Method(
                overrides: Build.Override(Build.Parameter(isRequired: false)))));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
