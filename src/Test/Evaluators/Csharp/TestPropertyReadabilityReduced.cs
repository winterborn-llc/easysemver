using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R12 - a property stopped being readable.</summary>
public class TestPropertyReadabilityReduced
{
    private static IEvaluateCsharpSignatures Evaluator => new PropertyReadabilityReduced();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void AccessorIsUnchanged()
    {
        Assert.False(Evaluator.AreDifferencesPresent(BuildComparison(false, false)));
    }

    [Fact]
    public void AccessorChangesInTheFiringDirection()
    {
        Assert.True(Evaluator.AreDifferencesPresent(BuildComparison(true, false)));
    }

    [Fact]
    public void TheOppositeDirectionDoesNotFire()
    {
        Assert.False(Evaluator.AreDifferencesPresent(BuildComparison(false, true)));
    }

    private static ICsharpSignaturesToCompare BuildComparison(bool wasPresent, bool isPresent)
    {
        return Build.Compare(
            Build.Class().WithProperties(Build.Property(isReadable: wasPresent)),
            Build.Class().WithProperties(Build.Property(isReadable: isPresent)));
    }
}
