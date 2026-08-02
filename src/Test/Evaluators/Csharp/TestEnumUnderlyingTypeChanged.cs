using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R25.</summary>
public class TestEnumUnderlyingTypeChanged
{
    private static IEvaluateCsharpSignatures Evaluator => new EnumUnderlyingTypeChanged();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void UnderlyingTypeIsUnchanged()
    {
        var signatures = Build.Compare(Build.Enum(), Build.Enum());

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void UnderlyingTypeChanged()
    {
        var signatures = Build.Compare(
            Build.Enum(underlyingType: "int"),
            Build.Enum(underlyingType: "byte"));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }
}
