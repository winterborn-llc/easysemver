using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R13.</summary>
public class TestPropertyType
{
    private static IEvaluateCsharpSignatures Evaluator => new PropertyType();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void PropertyTypeIsUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property(type: "string")),
            Build.Class().WithProperties(Build.Property(type: "string")));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void PropertyTypeChanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property(type: "string")),
            Build.Class().WithProperties(Build.Property(type: "int")));

        Assert.Equal(["Test.TestType.TestProperty"], Evaluator.FindDifferences(signatures));
    }
}
