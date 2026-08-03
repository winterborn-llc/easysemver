using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R26.</summary>
public class TestDelegateSignatureChanged
{
    private static IEvaluateCsharpSignatures Evaluator => new DelegateSignatureChanged();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void SignatureIsUnchanged()
    {
        var signatures = Build.Compare(
            Build.Delegate().WithParameters(Build.Parameter()),
            Build.Delegate().WithParameters(Build.Parameter()));

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void ReturnTypeChanged()
    {
        var signatures = Build.Compare(
            Build.Delegate(returns: "void"),
            Build.Delegate(returns: "int"));

        Assert.Equal(["Test.TestType"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void ParametersChanged()
    {
        var signatures = Build.Compare(
            Build.Delegate().WithParameters(Build.Parameter()),
            Build.Delegate().WithParameters(Build.Parameter(), Build.Parameter("count", "int")));

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }
}
