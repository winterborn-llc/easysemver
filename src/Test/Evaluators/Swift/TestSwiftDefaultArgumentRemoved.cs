using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S31 - directional against S32.</summary>
public class TestSwiftDefaultArgumentRemoved
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftDefaultArgumentRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void DefaultsAreUnchanged()
    {
        Assert.False(Evaluator.AreDifferencesPresent(Compare(true, true)));
    }

    [Fact]
    public void DefaultIsRemoved()
    {
        Assert.True(Evaluator.AreDifferencesPresent(Compare(true, false)));
    }

    [Fact]
    public void AddingADefaultDoesNotFire()
    {
        Assert.False(Evaluator.AreDifferencesPresent(Compare(false, true)));
    }

    private static ISwiftSignaturesToCompare Compare(bool hadDefault, bool hasDefault)
    {
        return BuildSwift.Compare(
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()
                .WithParameters(BuildSwift.Parameter(hasDefault: hadDefault))),
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()
                .WithParameters(BuildSwift.Parameter(hasDefault: hasDefault))));
    }
}
