using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S32 - directional against S31.</summary>
public class TestSwiftDefaultArgumentAdded
{
    private static IEvaluateSwiftSignatures Evaluator => new DefaultArgumentAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void DefaultsAreUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(false, false)));
    }

    [Fact]
    public void DefaultIsAdded()
    {
        Assert.Equal(["TestType.move() (to)"], Evaluator.FindDifferences(Compare(false, true)));
    }

    [Fact]
    public void RemovingADefaultDoesNotFire()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(true, false)));
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
