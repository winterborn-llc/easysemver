using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R33 - directional against R32.</summary>
public class TestTypeInheritanceRelaxed
{
    private static IEvaluateCsharpSignatures Evaluator => new TypeInheritanceRelaxed();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void TypeIsUnchanged()
    {
        Assert.False(Evaluator.AreDifferencesPresent(Build.Compare(Build.Class(), Build.Class())));
    }

    [Fact]
    public void TypeLosesSealed()
    {
        var signatures = Build.Compare(
            new CsharpClass { Name = Build.DefaultTypeName, IsSealed = true },
            Build.Class());

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void TypeLosesAbstract()
    {
        var signatures = Build.Compare(
            new CsharpClass { Name = Build.DefaultTypeName, IsAbstract = true },
            Build.Class());

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void GainingSealedDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Class(),
            new CsharpClass { Name = Build.DefaultTypeName, IsSealed = true });

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }
}
