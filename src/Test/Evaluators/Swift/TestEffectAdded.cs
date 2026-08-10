using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S23 - directional against S24.</summary>
public class TestSwiftEffectAdded
{
    private static IEvaluateSwiftSignatures Evaluator => new EffectAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void FlagIsUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(true, true)));
    }

    [Fact]
    public void FlagChangesInTheFiringDirection()
    {
        // The rule names what it found, which is what a dry run reports (§20 O-04).
        Assert.Equal(["TestType.move()"], Evaluator.FindDifferences(Compare(false, true)));
    }

    [Fact]
    public void TheOppositeDirectionDoesNotFire()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(true, false)));
    }

    private static ISwiftSignaturesToCompare Compare(bool wasSet, bool isSet)
    {
        return BuildSwift.Compare(
            BuildSwift.Struct().WithFunctions(
                new SwiftFunction { Name = "TestType.move()", Throws = wasSet }),
            BuildSwift.Struct().WithFunctions(
                new SwiftFunction { Name = "TestType.move()", Throws = isSet }));
    }
}
