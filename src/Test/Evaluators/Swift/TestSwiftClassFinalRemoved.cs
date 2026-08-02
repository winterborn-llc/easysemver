using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S07 - directional against S06.</summary>
public class TestSwiftClassFinalRemoved
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftClassFinalRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void FlagIsUnchanged()
    {
        Assert.False(Evaluator.AreDifferencesPresent(Compare(false, false)));
    }

    [Fact]
    public void FlagChangesInTheFiringDirection()
    {
        Assert.True(Evaluator.AreDifferencesPresent(Compare(true, false)));
    }

    [Fact]
    public void TheOppositeDirectionDoesNotFire()
    {
        Assert.False(Evaluator.AreDifferencesPresent(Compare(false, true)));
    }

    private static ISwiftSignaturesToCompare Compare(bool wasSet, bool isSet)
    {
        return BuildSwift.Compare(
            new SwiftStruct { Name = BuildSwift.DefaultTypeName, IsFinal = wasSet },
            new SwiftStruct { Name = BuildSwift.DefaultTypeName, IsFinal = isSet });
    }
}
