using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R28.</summary>
public class TestFieldContractReduced
{
    private static IEvaluateCsharpSignatures Evaluator => new FieldContractReduced();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void FieldsAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithFields(Build.Field()),
            Build.Class().WithFields(Build.Field()));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void FieldIsRemoved()
    {
        var signatures = Build.Compare(
            Build.Class().WithFields(Build.Field()),
            Build.Class());

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void FieldIsRetyped()
    {
        var signatures = Build.Compare(
            Build.Class().WithFields(Build.Field(type: "string")),
            Build.Class().WithFields(Build.Field(type: "int")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void FieldGainsReadOnly()
    {
        var signatures = Build.Compare(
            Build.Class().WithFields(Build.Field()),
            Build.Class().WithFields(new CsharpField { Name = "TestField", Type = "string", IsReadOnly = true }));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void FieldLosingReadOnlyDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Class().WithFields(new CsharpField { Name = "TestField", Type = "string", IsReadOnly = true }),
            Build.Class().WithFields(Build.Field()));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }
}
