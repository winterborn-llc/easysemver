using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S27 - directional against S28.</summary>
public class TestSwiftObjCExposureRemoved
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftObjCExposureRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ExposureIsUnchanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithObjC(),
            BuildSwift.Struct().WithObjC());

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void ExposureIsRemoved()
    {
        var signatures = BuildSwift.Compare(BuildSwift.Struct().WithObjC(), BuildSwift.Struct());

        Assert.Equal([BuildSwift.DefaultTypeName], Evaluator.FindDifferences(signatures));
    }

    /// <summary>A custom ObjC name is part of the contract, so changing it is breaking too.</summary>
    [Fact]
    public void CustomObjCNameChanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithObjC("@objc(WBGadget)"),
            BuildSwift.Struct().WithObjC("@objc(WBWidget)"));

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void AddingExposureDoesNotFire()
    {
        var signatures = BuildSwift.Compare(BuildSwift.Struct(), BuildSwift.Struct().WithObjC());

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
