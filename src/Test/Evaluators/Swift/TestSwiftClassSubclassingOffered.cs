using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;
using Test.Swift;

namespace Test.Evaluators.Swift;

/// <summary>S05 - directional against S04.</summary>
public class TestSwiftClassSubclassingOffered
{
    private static IEvaluateSwiftSignatures Evaluator => new SwiftClassSubclassingOffered();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void AccessLevelIsUnchanged()
    {
        Assert.False(Evaluator.AreDifferencesPresent(
            BuildSwift.Compare(BuildSwift.Class(), BuildSwift.Class())));
    }

    [Fact]
    public void PublicBecameOpen()
    {
        Assert.True(Evaluator.AreDifferencesPresent(
            BuildSwift.Compare(BuildSwift.Class(), BuildSwift.Class(accessLevel: "open"))));
    }

    [Fact]
    public void OpenBecomingPublicDoesNotFire()
    {
        Assert.False(Evaluator.AreDifferencesPresent(
            BuildSwift.Compare(BuildSwift.Class(accessLevel: "open"), BuildSwift.Class())));
    }
}
