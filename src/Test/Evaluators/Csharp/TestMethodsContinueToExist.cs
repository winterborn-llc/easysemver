using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R01.</summary>
public class TestMethodsContinueToExist
{
    private static IEvaluateCsharpSignatures Evaluator => new MethodsContinueToExist();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void MethodsAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method("One"), Build.Method("Two")),
            Build.Class().WithMethods(Build.Method("One"), Build.Method("Two")));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void MethodNoLongerExists()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method("One"), Build.Method("Two")),
            Build.Class().WithMethods(Build.Method("One"), Build.Method("Three")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }
}
