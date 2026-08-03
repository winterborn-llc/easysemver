using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluators.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>R30.</summary>
public class TestEventContractReduced
{
    private static IEvaluateCsharpSignatures Evaluator => new EventContractReduced();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
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
    public void EventIsRemoved()
    {
        var signatures = Build.Compare(Build.Class().WithEvents(Build.Event()), Build.Class());

        Assert.Equal(["Test.TestType.TestEvent"], Evaluator.FindDifferences(signatures));
    }

    [Fact]
    public void HandlerTypeChanged()
    {
        var signatures = Build.Compare(
            Build.Class().WithEvents(Build.Event(handler: "System.EventHandler")),
            Build.Class().WithEvents(Build.Event(handler: "System.Action")));

        Assert.NotEmpty(Evaluator.FindDifferences(signatures));
    }
}
