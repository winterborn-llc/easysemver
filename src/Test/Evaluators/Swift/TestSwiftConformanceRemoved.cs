using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S09 - directional against S10.</summary>
public class TestSwiftConformanceRemoved
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftConformanceRemoved();

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
