using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

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

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void MemberIsAdded()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red")),
            Build.Enum().WithMembers(Build.EnumMember("Red"), Build.EnumMember("Green", "1")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void RemovingAMemberDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red"), Build.EnumMember("Green", "1")),
            Build.Enum().WithMembers(Build.EnumMember("Red")));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }
}
