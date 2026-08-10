using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S24 - directional against S23.</summary>
public class TestSwiftEffectRemoved
{
    private static IEvaluateSwiftSignatures Evaluator => new EffectRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void FlagIsUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(false, false)));
    }

    [Fact]
    public void FlagChangesInTheFiringDirection()
    {
        Assert.Equal(["TestType.move()"], Evaluator.FindDifferences(Compare(true, false)));
    }

    [Fact]
    public void TheOppositeDirectionDoesNotFire()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(false, true)));
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
