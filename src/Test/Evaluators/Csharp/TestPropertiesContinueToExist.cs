using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R10.</summary>
public class TestPropertiesContinueToExist
{
    private static IEvaluateCsharpSignatures Evaluator => new PropertiesContinueToExist();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void PropertiesAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property("One"), Build.Property("Two")),
            Build.Class().WithProperties(Build.Property("One"), Build.Property("Two")));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void PropertyIsRemoved()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property("One"), Build.Property("Two")),
            Build.Class().WithProperties(Build.Property("One")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }
}
