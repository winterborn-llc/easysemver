using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

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

    /// <summary>
    /// Two overloads differing only in generic arity - PluginArchitecture's
    /// `MakeInstance&lt;T&gt;(Type)` and `MakeInstance(Type)` - used to pair crosswise, because the
    /// matcher keyed on the parameter list alone and both have `(Type type)`. The non-generic one
    /// matched the generic one, its `object` return read as a change to `T`, and an untouched
    /// repository bumped a major on every single run.
    /// </summary>
    [Fact]
    public void OverloadsDifferingOnlyInGenericArityDoNotPairCrosswise()
    {
        Assert.Empty(Evaluator.FindDifferences(Build.Compare(MakeInstance(), MakeInstance())));
    }

    /// <summary>
    /// The pairing is arity-aware, not arity-blind: the generic overload is still compared, and a
    /// real change to it is still Major.
    /// </summary>
    [Fact]
    public void ReturnTypeChangedOnTheGenericOverloadOnly()
    {
        Assert.Equal(
            ["Test.TestType.MakeInstance([Type type])"],
            Evaluator.FindDifferences(Build.Compare(MakeInstance(), MakeInstance("TOther"))));
    }

    /// <summary>
    /// `public static T MakeInstance&lt;T&gt;(this Type type)` alongside
    /// `public static object MakeInstance(this Type type)`, in that declaration order. The generic
    /// overload's return type is a parameter so a test can change that one alone.
    /// </summary>
    private static CsharpClass MakeInstance(string genericReturns = "T")
    {
        return Build.Class().WithMethods(Build.Method(
            name: "MakeInstance",
            returns: "T",
            overrides:
            [
                new CsharpMethodOverride(Build.Parameter("type", "Type")) { ReturnType = genericReturns }
                    .WithGenerics(Build.Generic("T", "class")),
                new(Build.Parameter("type", "Type")) { ReturnType = "object" }
            ]));
    }
}
