using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S10 - directional against S09.</summary>
public class TestSwiftConformanceAdded
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftConformanceAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
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
    public void ConformanceIsAdded()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithConformances("Equatable"),
            BuildSwift.Struct().WithConformances("Equatable", "Hashable"));

        Assert.Equal(["TestType (Hashable)"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void RemovingAConformanceDoesNotFire()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithConformances("Equatable", "Hashable"),
            BuildSwift.Struct().WithConformances("Equatable"));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
