using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R37.</summary>
public class TestParameterModifierChanged
{
    private static IEvaluateCsharpSignatures Evaluator => new ParameterModifierChanged();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void ModifiersAreUnchanged()
    {
        Assert.Empty(Evaluator.FindDifferences(Compare(Build.Parameter(), Build.Parameter())));
    }

    [Fact]
    public void ParameterGainsRef()
    {
        var newer = new CsharpMethodParameter
        {
            ParameterName = "input",
            ParameterType = "string",
            RefKind = "Ref"
        };

        Assert.Equal(
            ["Test.TestType.TestMethod([string input])"],
            Evaluator.FindDifferences(Compare(Build.Parameter(), newer)));
    }

    [Fact]
    public void ParameterGainsParams()
    {
        var newer = new CsharpMethodParameter
        {
            ParameterName = "input",
            ParameterType = "string",
            IsParams = true
        };

        Assert.NotEmpty(Evaluator.FindDifferences(Compare(Build.Parameter(), newer)));
    }

    private static ICsharpSignaturesToCompare Compare(
        CsharpMethodParameter older,
        CsharpMethodParameter newer)
    {
        return Build.Compare(
            Build.Class().WithMethods(Build.Method(overrides: Build.Override(older))),
            Build.Class().WithMethods(Build.Method(overrides: Build.Override(newer))));
    }
}
