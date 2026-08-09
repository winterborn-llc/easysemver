using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

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

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void ValueChanged()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red", "1")),
            Build.Enum().WithMembers(Build.EnumMember("Red", "7")));

        Assert.Equal(["Test.TestType.Red"], Evaluator.FindDifferences(signatures));
    }
}
