using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S04 - directional against S05.</summary>
public class TestSwiftClassSubclassingWithdrawn
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftClassSubclassingWithdrawn();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void AccessLevelIsUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(
            BuildSwift.Compare(BuildSwift.Class(accessLevel: "open"), BuildSwift.Class(accessLevel: "open"))));
    }

    [Fact]
    public void OpenBecamePublic()
    {
        Assert.Equal(
            [BuildSwift.DefaultTypeName],
            Evaluator.FindDifferences(
                BuildSwift.Compare(BuildSwift.Class(accessLevel: "open"), BuildSwift.Class())));
    }

    [Fact]
    public void PublicBecomingOpenDoesNotFire()
    {
        Assert.Empty(Evaluator.FindDifferences(
            BuildSwift.Compare(BuildSwift.Class(), BuildSwift.Class(accessLevel: "open"))));
    }
}
