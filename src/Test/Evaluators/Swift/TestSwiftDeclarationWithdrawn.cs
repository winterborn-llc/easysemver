using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S25.</summary>
public class TestSwiftDeclarationWithdrawn
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftDeclarationWithdrawn();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void AvailabilityIsUnchanged()
    {
        Assert.False(Evaluator.AreDifferencesPresent(
            BuildSwift.Compare(BuildSwift.Struct(), BuildSwift.Struct())));
    }

    [Fact]
    public void DeclarationBecameUnavailable()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct(),
            BuildSwift.Struct().WithAvailability(BuildSwift.Available(isUnavailable: true)));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void DeclarationGainedAnObsoletedVersion()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct(),
            BuildSwift.Struct().WithAvailability(BuildSwift.Available(obsoleted: "2.0")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void DeprecationAloneDoesNotFire()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct(),
            BuildSwift.Struct().WithAvailability(BuildSwift.Available(isDeprecated: true)));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }
}
