using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R19.</summary>
public class TestTypeAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new TypeAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void TypesAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Interface()),
            Build.Project(Build.Interface()));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Theory]
    [MemberData(nameof(TestTypeRemoved.EveryNonClassKind), MemberType = typeof(TestTypeRemoved))]
    public void TypeIsAdded(CsharpType added)
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class()),
            Build.Project(Build.Class(), added));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }

    /// <summary>An added class is R05's concern.</summary>
    [Fact]
    public void AddedClassIsNotCounted()
    {
        var signatures = Build.Compare(
            Build.Project(Build.Class()),
            Build.Project(Build.Class(), Build.Class("Test.Another")));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }
}
