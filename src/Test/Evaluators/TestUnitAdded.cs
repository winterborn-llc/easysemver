using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Test.Evaluators;

/// <summary>
/// "A unit was added", tested through every language that owns a copy of it (TST-M2). Each
/// language declares its own rule, so each is exercised here rather than one standing in for all:
/// the day a language overrides the shared diffing instead of inheriting it, these are what say
/// whether it still agrees with everyone else.
/// </summary>
public class TestUnitAdded
{
    public static TheoryData<IEvaluateUnitExistence> Evaluators =>
    [
        new Winterborn.Tools.EasySemVer.Evaluators.Csharp.UnitAdded(),
        new Winterborn.Tools.EasySemVer.Evaluators.Swift.UnitAdded()
    ];

    [Theory]
    [MemberData(nameof(Evaluators))]
    public void ChangeTypeIsExpected(IEvaluateUnitExistence evaluator)
    {
        Assert.Equal(VersionType.Minor, evaluator.EvaluationImpact);
    }

    /// <summary>The published half of the key, which a class rename must not be able to move.</summary>
    [Theory]
    [MemberData(nameof(Evaluators))]
    public void RuleIsNamedTheSameInEveryLanguage(IEvaluateUnitExistence evaluator)
    {
        Assert.Equal("UnitAdded", evaluator.Rule);
    }

    [Theory]
    [MemberData(nameof(Evaluators))]
    public void UnitsAreUnchanged(IEvaluateUnitExistence evaluator)
    {
        var units = new UnitsToCompare(
            older: [Units.Csharp("Widgets"), Units.Swift("Sources/Gadgets:Gadgets")],
            newer: [Units.Csharp("Widgets"), Units.Swift("Sources/Gadgets:Gadgets")]);

        Assert.Empty(evaluator.FindDifferences(units));
    }

    [Theory]
    [MemberData(nameof(Evaluators))]
    public void UnitIsAdded(IEvaluateUnitExistence evaluator)
    {
        var units = new UnitsToCompare(
            older: [Units.Csharp("Widgets")],
            newer: [Units.Csharp("Widgets"), Units.Csharp("Gadgets")]);

        // The rule yields the unit itself, so the report can name what appeared (§20 O-04).
        Assert.Equal(
            ["Gadgets"],
            evaluator.FindDifferences(units).Select(unit => unit.UnitId));
    }

    /// <summary>An empty baseline makes a first run Minor (BAS-05).</summary>
    [Theory]
    [MemberData(nameof(Evaluators))]
    public void FirstRunHasNoBaselineAtAll(IEvaluateUnitExistence evaluator)
    {
        var units = new UnitsToCompare(
            older: [],
            newer: [Units.Csharp("Widgets")]);

        Assert.NotEmpty(evaluator.FindDifferences(units));
    }

    [Theory]
    [MemberData(nameof(Evaluators))]
    public void RemovingAUnitDoesNotFire(IEvaluateUnitExistence evaluator)
    {
        var units = new UnitsToCompare(
            older: [Units.Csharp("Widgets"), Units.Csharp("Gadgets")],
            newer: [Units.Csharp("Widgets")]);

        Assert.Empty(evaluator.FindDifferences(units));
    }
}
