using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S29 - directional against S30.</summary>
public class TestSwiftMutatingAdded
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftMutatingAdded();

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
            BuildSwift.Struct().WithFunctions(
                new SwiftFunction { Name = "TestType.move()", IsMutating = wasSet }),
            BuildSwift.Struct().WithFunctions(
                new SwiftFunction { Name = "TestType.move()", IsMutating = isSet }));
    }
}
