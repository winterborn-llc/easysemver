using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R36.</summary>
public class TestMemberOverridabilityReduced
{
    private static IEvaluateCsharpSignatures Evaluator => new MemberOverridabilityReduced();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void MemberIsUnchanged()
    {
        var signatures = Compare(
            new CsharpMethodOverride { IsVirtual = true },
            new CsharpMethodOverride { IsVirtual = true });

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void MemberLosesVirtual()
    {
        var signatures = Compare(new CsharpMethodOverride { IsVirtual = true }, new CsharpMethodOverride());

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void MemberGainsAbstract()
    {
        var signatures = Compare(new CsharpMethodOverride(), new CsharpMethodOverride { IsAbstract = true });

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void MemberGainsSealed()
    {
        var signatures = Compare(new CsharpMethodOverride(), new CsharpMethodOverride { IsSealed = true });

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void MemberGainingVirtualDoesNotFire()
    {
        var signatures = Compare(new CsharpMethodOverride(), new CsharpMethodOverride { IsVirtual = true });

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    private static ICsharpSignaturesToCompare Compare(
        CsharpMethodOverride older,
        CsharpMethodOverride newer)
    {
        return Build.Compare(
            Build.Class().WithMethods(Build.Method(overrides: older)),
            Build.Class().WithMethods(Build.Method(overrides: newer)));
    }
}
