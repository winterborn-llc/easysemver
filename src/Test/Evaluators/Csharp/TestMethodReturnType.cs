using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R03, including the per-overload return type that closes G-14 (CSX-04).</summary>
public class TestMethodReturnType
{
    private static IEvaluateCsharpSignatures Evaluator => new MethodReturnType();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ReturnTypesAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method(returns: "string")),
            Build.Class().WithMethods(Build.Method(returns: "string")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void ReturnTypeChanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method(returns: "string")),
            Build.Class().WithMethods(Build.Method(returns: "int")));

        Assert.Equal(["Test.TestType.TestMethod"], Evaluator.FindDifferences(signatures));
    }

    /// <summary>
    /// G-14 - a return-type change on the second overload used to be invisible, because the type
    /// was recorded once per method name.
    /// </summary>
    [Fact]
    public void ReturnTypeChangedOnSecondOverloadOnly()
    {
        var older = Build.Class().WithMethods(Build.Method(
            overrides:
            [
                new() { ReturnType = "string" },
                new(Build.Parameter()) { ReturnType = "string" }
            ]));
        var newer = Build.Class().WithMethods(Build.Method(
            overrides:
            [
                new() { ReturnType = "string" },
                new(Build.Parameter()) { ReturnType = "int" }
            ]));

        // The finding names the overload that changed, not the method as a whole.
        Assert.Equal(
            ["Test.TestType.TestMethod([string input])"],
            Evaluator.FindDifferences(Build.Compare(older, newer)));
    }
}
