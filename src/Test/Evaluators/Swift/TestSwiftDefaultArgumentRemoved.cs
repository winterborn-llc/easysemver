using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
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
        Assert.Empty(Evaluator.FindDifferences(Compare(true, true)));
    }

    [Fact]
    public void DefaultIsRemoved()
    {
        // The symbol names the function and the parameter that lost its default.
        Assert.Equal(["TestType.move() (to)"], Evaluator.FindDifferences(Compare(true, false)));
    }

    [Fact]
    public void AddingADefaultDoesNotFire()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(false, true)));
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
