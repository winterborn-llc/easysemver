using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S24 - directional against S23.</summary>
public class TestSwiftEffectRemoved
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftEffectRemoved();

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
            BuildSwift.Struct().WithFunctions(
                new SwiftFunction { Name = "TestType.move()", Throws = wasSet }),
            BuildSwift.Struct().WithFunctions(
                new SwiftFunction { Name = "TestType.move()", Throws = isSet }));
    }
}
