using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

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

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void MemberIsRemoved()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red"), Build.EnumMember("Green", "1")),
            Build.Enum().WithMembers(Build.EnumMember("Red")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void MemberIsRenamed()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red")),
            Build.Enum().WithMembers(Build.EnumMember("Crimson")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }
}
