using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R23.</summary>
public class TestEnumMemberAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new EnumMemberAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void MembersAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red")),
            Build.Enum().WithMembers(Build.EnumMember("Red")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void MemberIsAdded()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red")),
            Build.Enum().WithMembers(Build.EnumMember("Red"), Build.EnumMember("Green", "1")));

        Assert.Equal(["Test.TestType.Green"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void RemovingAMemberDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red"), Build.EnumMember("Green", "1")),
            Build.Enum().WithMembers(Build.EnumMember("Red")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
