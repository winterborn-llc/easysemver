using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R22.</summary>
public class TestEnumMemberRemoved
{
    private static IEvaluateCsharpSignatures Evaluator => new EnumMemberRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void MembersAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red"), Build.EnumMember("Green", "1")),
            Build.Enum().WithMembers(Build.EnumMember("Red"), Build.EnumMember("Green", "1")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void MemberIsRemoved()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red"), Build.EnumMember("Green", "1")),
            Build.Enum().WithMembers(Build.EnumMember("Red")));

        Assert.Equal(["Test.TestType.Green"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void MemberIsRenamed()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red")),
            Build.Enum().WithMembers(Build.EnumMember("Crimson")));

        // The finding names the old member, which is the one callers can no longer reach.
        Assert.Equal(["Test.TestType.Red"], Evaluator.FindDifferences(signatures));
    }
}
