using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S06 - directional against S07.</summary>
public class TestSwiftClassMadeFinal
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftClassMadeFinal();

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
        Assert.Equal(
            [BuildSwift.DefaultTypeName],
            Evaluator.FindDifferences(Compare(false, true)));
    }

    [Fact]
    public void TheOppositeDirectionDoesNotFire()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(true, false)));
    }

    private static ISwiftSignaturesToCompare Compare(bool wasSet, bool isSet)
    {
        return BuildSwift.Compare(
            new SwiftStruct { Name = BuildSwift.DefaultTypeName, IsFinal = wasSet },
            new SwiftStruct { Name = BuildSwift.DefaultTypeName, IsFinal = isSet });
    }
}
