using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S17 - directional against S16.</summary>
public class TestSwiftMemberAdded
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftMemberAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void MembersAreUnchanged()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()),
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void MemberIsAdded()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Struct(),
            BuildSwift.Struct().WithFunctions(BuildSwift.Function()));

        Assert.Equal(["TestType.move()"], Evaluator.FindDifferences(signatures));
    }

    /// <summary>NCL-03 - members of a brand-new type are S02's concern, not this rule's.</summary>
    [Fact]
    public void MemberOnABrandNewTypeIsNotCounted()
    {
        var signatures = BuildSwift.Compare(
            BuildSwift.Module(BuildSwift.Struct("Point")),
            BuildSwift.Module(
                BuildSwift.Struct("Point"),
                BuildSwift.Struct("Gadget").WithFunctions(BuildSwift.Function("Gadget.move()"))));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
