using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R31.</summary>
public class TestEventAdded
{
    private static IEvaluateCsharpSignatures Evaluator => new EventAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void EventsAreUnchanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithEvents(Build.Event()),
            Build.Class().WithEvents(Build.Event()));

        Assert.False(Evaluator.AreDifferencesPresent(signatures));
    }

    [Fact]
    public void EventIsAdded()
    {
        var signatures = Build.Compare(Build.Class(), Build.Class().WithEvents(Build.Event()));

        Assert.True(Evaluator.AreDifferencesPresent(signatures));
    }
}
