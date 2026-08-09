using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluators.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

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

        Assert.Empty(Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void EventIsAdded()
    {
        var signatures = Build.Compare(Build.Class(), Build.Class().WithEvents(Build.Event()));

        Assert.Equal(["Test.TestType.TestEvent"], Evaluator.FindDifferences(signatures));
    }
}
