using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S28 - directional against S27.</summary>
public class TestSwiftObjCExposureAdded
{
    private static IEvaluateSwiftSignatures Evaluator => new ObjCExposureAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ExposureIsUnchanged()
    {
        var signatures = BuildSwift.Compare(BuildSwift.Struct(), BuildSwift.Struct());

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void ExposureIsAdded()
    {
        var signatures = BuildSwift.Compare(BuildSwift.Struct(), BuildSwift.Struct().WithObjC());

        Assert.Equal([BuildSwift.DefaultTypeName], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void RemovingExposureDoesNotFire()
    {
        var signatures = BuildSwift.Compare(BuildSwift.Struct().WithObjC(), BuildSwift.Struct());

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
