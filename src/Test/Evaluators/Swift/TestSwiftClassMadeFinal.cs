using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
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
        Assert.False(Evaluator.AreDifferencesPresent(Compare(true, true)));
    }

    [Fact]
    public void FlagChangesInTheFiringDirection()
    {
        Assert.True(Evaluator.AreDifferencesPresent(Compare(false, true)));
    }

    [Fact]
    public void TheOppositeDirectionDoesNotFire()
    {
        Assert.False(Evaluator.AreDifferencesPresent(Compare(true, false)));
    }

    private static ISwiftSignaturesToCompare Compare(bool wasSet, bool isSet)
    {
        return BuildSwift.Compare(
            new SwiftStruct { Name = BuildSwift.DefaultTypeName, IsFinal = wasSet },
            new SwiftStruct { Name = BuildSwift.DefaultTypeName, IsFinal = isSet });
    }
}
