using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R39 - directional against R40.</summary>
public class TestGenericConstraintTightened
{
    private static IEvaluateCsharpSignatures Evaluator => new GenericConstraintTightened();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void GenericsAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithGenerics(Build.Generic("T", "class")),
            Build.Class().WithGenerics(Build.Generic("T", "class")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void ParameterCountChanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithGenerics(Build.Generic()),
            Build.Class().WithGenerics(Build.Generic(), Build.Generic("TOther")));

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void ConstraintIsAdded()
    {
        var signatures = Build.Compare(
            Build.Class().WithGenerics(Build.Generic("T", "class")),
            Build.Class().WithGenerics(Build.Generic("T", "class, new()")));

        Assert.Equal(["Test.TestType"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void ConstraintOnAMethodIsAdded()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method(
                overrides: Build.Override().WithGenerics(Build.Generic("T")))),
            Build.Class().WithMethods(Build.Method(
                overrides: Build.Override().WithGenerics(Build.Generic("T", "class")))));

        // A method-level constraint is reported against the overload, not the type.
        Assert.Equal(["Test.TestType.TestMethod()"], Evaluator.FindDifferences(signatures));
    }

    /// <summary>
    /// The other half of the crosswise pairing that majored PluginArchitecture on an unchanged
    /// tree: the non-generic overload matched the generic one, and its empty constraint list read
    /// as `where T : class` having been added.
    /// </summary>
    [Fact]
    public void OverloadsDifferingOnlyInGenericArityDoNotPairCrosswise()
    {
        var signatures = Build.Compare(MakeInstance(), MakeInstance());

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    /// <summary>
    /// `public static T MakeInstance&lt;T&gt;(this Type type)` alongside
    /// `public static object MakeInstance(this Type type)`, in that declaration order.
    /// </summary>
    private static CsharpClass MakeInstance()
    {
        return Build.Class().WithMethods(Build.Method(
            name: "MakeInstance",
            returns: "T",
            overrides:
            [
                Build.Override(Build.Parameter("type", "Type")).WithGenerics(Build.Generic("T", "class")),
                new(Build.Parameter("type", "Type")) { ReturnType = "object" }
            ]));
    }

    [Fact]
    public void RemovingAConstraintDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Class().WithGenerics(Build.Generic("T", "class, new()")),
            Build.Class().WithGenerics(Build.Generic("T", "class")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
