using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S20 - directional against S21.</summary>
public class TestSwiftProtocolRequirementAdded
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftProtocolRequirementAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ProtocolIsUnchanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Protocol().WithFunctions(BuildSwift.Function()),
            BuildSwift.Protocol().WithFunctions(BuildSwift.Function()));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void RequirementWithoutDefaultIsAdded()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Protocol(),
            BuildSwift.Protocol().WithFunctions(BuildSwift.Function()));

        Assert.Equal(["TestType.move()"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void RequirementWithDefaultDoesNotFire()
    {
        var defaulted = BuildSwift.Function();
        defaulted.HasDefaultImplementation = true;

        var signatures = BuildSwift.Compare(BuildSwift.Protocol(), BuildSwift.Protocol().WithFunctions(defaulted));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    /// <summary>A member added to a struct is S17's concern, and only Minor.</summary>
    [Fact]
    public void MemberAddedToAStructDoesNotFire()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct(),
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
