using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

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
        Assert.False(Evaluator.AreDifferencesPresent(BuildComparison(true, true)));
    }

    [Fact]
    public void AccessorChangesInTheFiringDirection()
    {
        Assert.True(Evaluator.AreDifferencesPresent(BuildComparison(false, true)));
    }

    [Fact]
    public void TheOppositeDirectionDoesNotFire()
    {
        Assert.False(Evaluator.AreDifferencesPresent(BuildComparison(true, false)));
    }

    private static ICsharpSignaturesToCompare BuildComparison(bool wasPresent, bool isPresent)
    {
        return Build.Compare(
            Build.Class().WithProperties(Build.Property(isWritable: wasPresent)),
            Build.Class().WithProperties(Build.Property(isWritable: isPresent)));
    }
}
