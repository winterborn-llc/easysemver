using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R24.</summary>
public class TestEnumMemberValueChanged
{
    private static IEvaluateCsharpSignatures Evaluator => new EnumMemberValueChanged();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ValuesAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red", "1")),
            Build.Enum().WithMembers(Build.EnumMember("Red", "1")));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void ValueChanged()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red", "1")),
            Build.Enum().WithMembers(Build.EnumMember("Red", "7")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }
}
