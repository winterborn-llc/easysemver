using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R17, which is directional: only the breaking direction fires (CLS-06).</summary>
public class TestMethodInputParameterMadeRequired
{
    private static IEvaluateCsharpSignatures Evaluator => new MethodInputParameterMadeRequired();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void RequirednessIsUnchanged()
    {
        Assert.False(Evaluator.AreDifferencesPresent(BuildComparison(false, false)));
    }

    [Fact]
    public void OptionalParameterMadeRequired()
    {
        Assert.True(Evaluator.AreDifferencesPresent(BuildComparison(false, true)));
    }

    [Fact]
    public void RequiredParameterMadeOptionalIsNotBreaking()
    {
        Assert.False(Evaluator.AreDifferencesPresent(BuildComparison(true, false)));
    }

    private static ICsharpSignaturesToCompare BuildComparison(bool wasRequired, bool isRequired)
    {
        return Build.Compare(
            Build.Class().WithMethods(Build.Method(
                overrides: Build.Override(
                    Build.Parameter(),
                    Build.Parameter("count", "int", wasRequired)))),
            Build.Class().WithMethods(Build.Method(
                overrides: Build.Override(
                    Build.Parameter(),
                    Build.Parameter("count", "int", isRequired)))));
    }
}
