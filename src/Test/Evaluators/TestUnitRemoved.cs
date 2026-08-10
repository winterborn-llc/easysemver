using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Test.Evaluators;

/// <summary>
/// "A unit was removed", tested through every language that owns a copy of it (TST-M2). See
/// <see cref="TestUnitAdded"/> for why each language's own rule is exercised rather than one
/// standing in for all.
/// </summary>
public class TestUnitRemoved
{
    public static TheoryData<IEvaluateUnitExistence> Evaluators =>
    [
        new Winterborn.Tools.EasySemVer.Evaluators.Csharp.UnitRemoved(),
        new Winterborn.Tools.EasySemVer.Evaluators.Swift.UnitRemoved()
    ];

    [Theory]
    [MemberData(nameof(Evaluators))]
    public void ChangeTypeIsExpected(IEvaluateUnitExistence evaluator)
    {
        Assert.Equal(VersionType.Major, evaluator.EvaluationImpact);
    }

    /// <summary>The published half of the key, which a class rename must not be able to move.</summary>
    [Theory]
    [MemberData(nameof(Evaluators))]
    public void RuleIsNamedTheSameInEveryLanguage(IEvaluateUnitExistence evaluator)
    {
        Assert.Equal("UnitRemoved", evaluator.Rule);
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
    public void CsharpUnitIsRemoved(IEvaluateUnitExistence evaluator)
    {
        var units = new UnitsToCompare(
            older: [Units.Csharp("Widgets"), Units.Csharp("Gadgets")],
            newer: [Units.Csharp("Widgets")]);

        // The rule yields the baseline's unit, which is the one that disappeared.
        Assert.Equal(
            ["Gadgets"],
            evaluator.FindDifferences(units).Select(unit => unit.UnitId));
    }

    [Theory]
    [MemberData(nameof(Evaluators))]
    public void SwiftUnitIsRemoved(IEvaluateUnitExistence evaluator)
    {
        var units = new UnitsToCompare(
            older: [Units.Swift("Sources/Gadgets:Gadgets")],
            newer: []);

        Assert.NotEmpty(evaluator.FindDifferences(units));
    }

    /// <summary>
    /// Identity is (LanguageId, UnitId), so two units that merely share a name across languages
    /// are not each other (ML-03). The core hands a provider only its own language, so this can no
    /// longer happen in a run - it stays asserted because the pairing is what guarantees that,
    /// and the pairing is shared by every language.
    /// </summary>
    [Theory]
    [MemberData(nameof(Evaluators))]
    public void SameNameInAnotherLanguageIsNotTheSameUnit(IEvaluateUnitExistence evaluator)
    {
        var units = new UnitsToCompare(
            older: [Units.Csharp("Widgets")],
            newer: [Units.Swift("Widgets")]);

        Assert.NotEmpty(evaluator.FindDifferences(units));
    }
}
