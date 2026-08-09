using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R21 - directional against R20.</summary>
public class TestInterfaceRequirementAddedWithDefault
{
    private static IEvaluateCsharpSignatures Evaluator => new InterfaceRequirementAddedWithDefault();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void InterfaceIsUnchanged()
    {
        var signatures = Build.Compare(
            Build.Interface().WithMethods(Build.Method("One")),
            Build.Interface().WithMethods(Build.Method("One")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void RequirementWithDefaultIsAdded()
    {
        var signatures = Build.Compare(
            Build.Interface().WithMethods(Build.Method("One")),
            Build.Interface().WithMethods(
                Build.Method("One"),
                Build.Method("Two", overrides: new CsharpMethodOverride { HasDefaultImplementation = true })));

        Assert.Equal(["Test.TestType.Two"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void RequirementWithoutDefaultDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Interface().WithMethods(Build.Method("One")),
            Build.Interface().WithMethods(Build.Method("One"), Build.Method("Two")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
