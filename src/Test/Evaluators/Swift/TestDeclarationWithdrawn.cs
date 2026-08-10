using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S25.</summary>
public class TestSwiftDeclarationWithdrawn
{
    private static IEvaluateSwiftSignatures Evaluator => new DeclarationWithdrawn();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void AvailabilityIsUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(
            BuildSwift.Compare(BuildSwift.Struct(), BuildSwift.Struct())));
    }

    [Fact]
    public void DeclarationBecameUnavailable()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct(),
            BuildSwift.Struct().WithAvailability(BuildSwift.Available(isUnavailable: true)));

        Assert.Equal([BuildSwift.DefaultTypeName], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void DeclarationGainedAnObsoletedVersion()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct(),
            BuildSwift.Struct().WithAvailability(BuildSwift.Available(obsoleted: "2.0")));

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void DeprecationAloneDoesNotFire()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct(),
            BuildSwift.Struct().WithAvailability(BuildSwift.Available(isDeprecated: true)));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
