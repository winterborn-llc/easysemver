using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R08 - a property became writable.</summary>
public class TestPropertyEditabilityEnhanced
{
    private static IEvaluateCsharpSignatures Evaluator => new PropertyEditabilityEnhanced();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void AccessorIsUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(BuildComparison(true, true)));
    }

    [Fact]
    public void AccessorChangesInTheFiringDirection()
    {
        Assert.Equal(
            ["Test.TestType.TestProperty"],
            Evaluator.FindDifferences(BuildComparison(false, true)));
    }

    [Fact]
    public void TheOppositeDirectionDoesNotFire()
    {
        Assert.Empty(Evaluator.FindDifferences(BuildComparison(true, false)));
    }

    private static ICsharpSignaturesToCompare BuildComparison(bool wasPresent, bool isPresent)
    {
        return Build.Compare(
            Build.Class().WithProperties(Build.Property(isWritable: wasPresent)),
            Build.Class().WithProperties(Build.Property(isWritable: isPresent)));
    }
}
