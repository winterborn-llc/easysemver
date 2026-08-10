using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S21 - directional against S20.</summary>
public class TestSwiftProtocolRequirementAddedWithDefault
{
    private static IEvaluateSwiftSignatures Evaluator => new ProtocolRequirementAddedWithDefault();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ProtocolIsUnchanged()
    {
        var signatures = BuildSwift.Compare(BuildSwift.Protocol(), BuildSwift.Protocol());

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void RequirementWithDefaultIsAdded()
    {
        var defaulted = BuildSwift.Function();
        defaulted.HasDefaultImplementation = true;

        var signatures = BuildSwift.Compare(BuildSwift.Protocol(), BuildSwift.Protocol().WithFunctions(defaulted));

        Assert.Equal(["TestType.move()"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void RequirementWithoutDefaultDoesNotFire()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Protocol(),
            BuildSwift.Protocol().WithFunctions(BuildSwift.Function()));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
