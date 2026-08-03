using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R32 - directional against R33.</summary>
public class TestTypeInheritanceRestricted
{
    private static IEvaluateCsharpSignatures Evaluator => new TypeInheritanceRestricted();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void TypeIsUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(Build.Compare(Build.Class(), Build.Class())));
    }

    [Fact]
    public void TypeGainsSealed()
    {
        var signatures = Build.Compare(
            Build.Class(),
            new CsharpClass { Name = Build.DefaultTypeName, IsSealed = true });

        Assert.Equal([Build.DefaultTypeName], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void TypeGainsAbstract()
    {
        var signatures = Build.Compare(
            Build.Class(),
            new CsharpClass { Name = Build.DefaultTypeName, IsAbstract = true });

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void TypeGainsStatic()
    {
        var signatures = Build.Compare(
            Build.Class(),
            new CsharpClass { Name = Build.DefaultTypeName, IsStatic = true });

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void BaseClassChanged()
    {
        var signatures = Build.Compare(
            new CsharpClass { Name = Build.DefaultTypeName, BaseType = "Test.Base" },
            new CsharpClass { Name = Build.DefaultTypeName, BaseType = "Test.OtherBase" });

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void LosingSealedDoesNotFire()
    {
        var signatures = Build.Compare(
            new CsharpClass { Name = Build.DefaultTypeName, IsSealed = true },
            Build.Class());

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
