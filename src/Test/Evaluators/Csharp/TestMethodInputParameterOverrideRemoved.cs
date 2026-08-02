using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

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

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
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

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
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

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }
}
