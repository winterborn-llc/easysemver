using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R20 - directional against R21: only the undefaulted requirement is breaking.</summary>
public class TestInterfaceRequirementAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new InterfaceRequirementAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void InterfaceIsUnchanged()
    {
        var signatures = Build.Compare(
            Build.Interface().WithMethods(Build.Method("One")),
            Build.Interface().WithMethods(Build.Method("One")));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void RequirementWithoutDefaultIsAdded()
    {
        var signatures = Build.Compare(
            Build.Interface().WithMethods(Build.Method("One")),
            Build.Interface().WithMethods(Build.Method("One"), Build.Method("Two")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void RequirementWithDefaultDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Interface().WithMethods(Build.Method("One")),
            Build.Interface().WithMethods(
                Build.Method("One"),
                Build.Method("Two", overrides: new CsharpMethodOverride { HasDefaultImplementation = true })));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    /// <summary>A method added to a class is R15's concern, and only Minor.</summary>
    [Fact]
    public void MethodAddedToAClassDoesNotFire()
    {
        var signatures = Build.Compare(
            Build.Class().WithMethods(Build.Method("One")),
            Build.Class().WithMethods(Build.Method("One"), Build.Method("Two")));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void PropertyRequirementWithoutDefaultIsAdded()
    {
        var signatures = Build.Compare(
            Build.Interface(),
            Build.Interface().WithProperties(Build.Property()));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }
}
