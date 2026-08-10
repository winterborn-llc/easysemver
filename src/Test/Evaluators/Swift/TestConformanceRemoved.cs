using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S09 - directional against S10.</summary>
public class TestSwiftConformanceRemoved
{
    private static IEvaluateSwiftSignatures Evaluator => new ConformanceRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ConformancesAreUnchanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithConformances("Equatable"),
            BuildSwift.Struct().WithConformances("Equatable"));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void ConformanceIsRemoved()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithConformances("Equatable", "Hashable"),
            BuildSwift.Struct().WithConformances("Equatable"));

        // The symbol names the type and the conformance it dropped.
        Assert.Equal(["TestType (Hashable)"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void AddingAConformanceDoesNotFire()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithConformances("Equatable"),
            BuildSwift.Struct().WithConformances("Equatable", "Hashable"));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
