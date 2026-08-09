using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R34 - directional against R35.</summary>
public class TestImplementedInterfaceRemoved
{
    private static IEvaluateCsharpSignatures Evaluator => new ImplementedInterfaceRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void InterfacesAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithInterfaces("Test.IThing"),
            Build.Class().WithInterfaces("Test.IThing"));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void InterfaceIsRemoved()
    {
        var signatures = Build.Compare(
            Build.Class().WithInterfaces("Test.IThing", "Test.IOther"),
            Build.Class().WithInterfaces("Test.IThing"));

        // The symbol names the type and the interface it dropped.
        Assert.Equal(["Test.TestType (Test.IOther)"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void AddingAnInterfaceDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Class().WithInterfaces("Test.IThing"),
            Build.Class().WithInterfaces("Test.IThing", "Test.IOther"));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
