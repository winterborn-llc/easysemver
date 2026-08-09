using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R40 - directional against R39.</summary>
public class TestGenericConstraintLoosened
{
    private static IEvaluateCsharpSignatures Evaluator => new GenericConstraintLoosened();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
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
    public void ConstraintIsRemoved()
    {
        var signatures = Build.Compare(
            Build.Class().WithGenerics(Build.Generic("T", "class, new()")),
            Build.Class().WithGenerics(Build.Generic("T", "class")));

        Assert.Equal(["Test.TestType"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void AddingAConstraintDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Class().WithGenerics(Build.Generic("T", "class")),
            Build.Class().WithGenerics(Build.Generic("T", "class, new()")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    /// <summary>A parameter-count change is R39's alone; this rule must stay out of it.</summary>
    [Fact]
    public void ParameterCountChangeDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Class().WithGenerics(Build.Generic("T", "class")),
            Build.Class().WithGenerics(Build.Generic("T", "class"), Build.Generic("TOther")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
