using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R18 - the rule that closes the largest half of G-15.</summary>
public class TestTypeRemoved
{
    private static IEvaluateCsharpSignatures Evaluator => new TypeRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void TypesAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Interface(), Build.Enum("Test.Colour")),
            Build.Project(Build.Interface(), Build.Enum("Test.Colour")));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Theory]
    [MemberData(nameof(EveryNonClassKind))]
    public void TypeIsRemoved(CsharpType removed)
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class(), removed),
            Build.Project(Build.Class()));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    /// <summary>A removed class is R06's concern; this rule must not double-count it.</summary>
    [Fact]
    public void RemovedClassIsNotCounted()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class(), Build.Class("Test.Another")),
            Build.Project(Build.Class()));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    /// <summary>Pairing is by (name, kind), so a struct that became a class reads as removed.</summary>
    [Fact]
    public void TypeKindChanged()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Struct("Test.Point")),
            Build.Project(Build.Class("Test.Point")));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    public static TheoryData<CsharpType> EveryNonClassKind() =>
    [
        Build.Interface("Test.IThing"),
        Build.Struct("Test.Point"),
        Build.Record("Test.Money"),
        Build.Enum("Test.Colour"),
        Build.Delegate("Test.Callback")
    ];
}
