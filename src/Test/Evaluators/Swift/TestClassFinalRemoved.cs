using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S07 - directional against S06.</summary>
public class TestSwiftClassFinalRemoved
{
    private static IEvaluateSwiftSignatures Evaluator => new ClassFinalRemoved();

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
            new SwiftStruct { Name = BuildSwift.DefaultTypeName, IsFinal = wasSet },
            new SwiftStruct { Name = BuildSwift.DefaultTypeName, IsFinal = isSet });
    }
}
