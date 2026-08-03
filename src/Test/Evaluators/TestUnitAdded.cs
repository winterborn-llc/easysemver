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

        Assert.Empty(Evaluator.FindDifferences(units));
    }

    [Fact]
    public void CsharpUnitIsAdded()
    {
        var units = new UnitsToCompare(
            older: [Units.Csharp("Widgets")],
            newer: [Units.Csharp("Widgets"), Units.Csharp("Gadgets")]);

        // The rule yields the unit itself, so the report can name what appeared (§20 O-04).
        Assert.Equal(
            ["Gadgets"],
            Evaluator.FindDifferences(units).Select(unit => unit.UnitId));
    }

    /// <summary>An empty baseline makes a first run Minor (BAS-05).</summary>
    [Fact]
    public void FirstRunHasNoBaselineAtAll()
    {
        var units = new UnitsToCompare(
            older: [],
            newer: [Units.Csharp("Widgets")]);

        Assert.NotEmpty(Evaluator.FindDifferences(units));
    }

    [Fact]
    public void RemovingAUnitDoesNotFire()
    {
        var units = new UnitsToCompare(
            older: [Units.Csharp("Widgets"), Units.Csharp("Gadgets")],
            newer: [Units.Csharp("Widgets")]);

        Assert.Empty(Evaluator.FindDifferences(units));
    }
}
