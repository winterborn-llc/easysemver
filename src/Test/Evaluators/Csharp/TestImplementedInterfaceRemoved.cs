using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

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

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void InterfaceIsRemoved()
    {
        var signatures = Build.Compare(
            Build.Class().WithInterfaces("Test.IThing", "Test.IOther"),
            Build.Class().WithInterfaces("Test.IThing"));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void AddingAnInterfaceDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Class().WithInterfaces("Test.IThing"),
            Build.Class().WithInterfaces("Test.IThing", "Test.IOther"));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }
}
