using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Test.Evaluators;

/// <summary>NCL-02, tested across languages (TST-M2).</summary>
public class TestUnitAdded
{
    private static IEvaluateUnitExistence Evaluator => new UnitAdded();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Minor, Evaluator.EvaluationImpact);
    }

    [Fact]
    public void UnitsAreUnchanged()
    {
        var units = new UnitsToCompare(
            older: [Units.Csharp("Widgets"), Units.Swift("Sources/Gadgets:Gadgets")],
            newer: [Units.Csharp("Widgets"), Units.Swift("Sources/Gadgets:Gadgets")]);

        Assert.False(Evaluator.AreDifferencesPresent(units));
    }

    [Fact]
    public void CsharpUnitIsAdded()
    {
        var units = new UnitsToCompare(
            older: [Units.Csharp("Widgets")],
            newer: [Units.Csharp("Widgets"), Units.Csharp("Gadgets")]);

        Assert.True(Evaluator.AreDifferencesPresent(units));
    }

    /// <summary>An empty baseline makes a first run Minor (BAS-05).</summary>
    [Fact]
    public void FirstRunHasNoBaselineAtAll()
    {
        var units = new UnitsToCompare(
            older: [],
            newer: [Units.Csharp("Widgets")]);

        Assert.True(Evaluator.AreDifferencesPresent(units));
    }

    [Fact]
    public void RemovingAUnitDoesNotFire()
    {
        var units = new UnitsToCompare(
            older: [Units.Csharp("Widgets"), Units.Csharp("Gadgets")],
            newer: [Units.Csharp("Widgets")]);

        Assert.False(Evaluator.AreDifferencesPresent(units));
    }
}
