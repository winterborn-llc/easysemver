using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>
/// R42 - the rule that closes the last warning row in the CLS scenario matrix. Before CSX-03
/// captured init separately from set, this change classified as Patch.
/// </summary>
public class TestPropertySetterBecameInitOnly
{
    private static IEvaluateCsharpSignatures Evaluator => new PropertySetterBecameInitOnly();

    private static CsharpProperty Settable => Build.Property();

    private static CsharpProperty InitOnly => new()
    {
        Name = "TestProperty",
        Type = "string",
        IsReadable = true,
        IsWritable = true,
        IsInitOnly = true
    };

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void SetterIsUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Settable),
            Build.Class().WithProperties(Settable));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void SetBecomesInit()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Settable),
            Build.Class().WithProperties(InitOnly));

        Assert.Equal(["Test.TestType.TestProperty"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void InitBecomingSetDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(InitOnly),
            Build.Class().WithProperties(Settable));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }
}
