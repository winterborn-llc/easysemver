using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S14 - directional against S15.</summary>
public class TestSwiftFrozenRemoved
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftFrozenRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void FlagIsUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(false, false)));
    }

    [Fact]
    public void FlagChangesInTheFiringDirection()
    {
        Assert.Equal(
            [BuildSwift.DefaultTypeName],
            Evaluator.FindDifferences(Compare(true, false)));
    }

    [Fact]
    public void TheOppositeDirectionDoesNotFire()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(false, true)));
    }

    private static ISwiftSignaturesToCompare Compare(bool wasSet, bool isSet)
    {
        return BuildSwift.Compare(
            new SwiftStruct { Name = BuildSwift.DefaultTypeName, IsFrozen = wasSet },
            new SwiftStruct { Name = BuildSwift.DefaultTypeName, IsFrozen = isSet });
    }
}
