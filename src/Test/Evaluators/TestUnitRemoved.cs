using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Test.Evaluators;

/// <summary>NCL-01, tested across languages (TST-M2).</summary>
public class TestUnitRemoved
{
    private static IEvaluateUnitExistence Evaluator => new UnitRemoved();

    [Fact]
    public void ChangeTypeIsExpected()
    {
        Assert.Equal(VersionType.Major, Evaluator.EvaluationImpact);
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
    public void CsharpUnitIsRemoved()
    {
        var units = new UnitsToCompare(
            older: [Units.Csharp("Widgets"), Units.Csharp("Gadgets")],
            newer: [Units.Csharp("Widgets")]);

        Assert.True(Evaluator.AreDifferencesPresent(units));
    }

    [Fact]
    public void SwiftUnitIsRemoved()
    {
        var units = new UnitsToCompare(
            older: [Units.Swift("Sources/Gadgets:Gadgets")],
            newer: []);

        Assert.True(Evaluator.AreDifferencesPresent(units));
    }

    /// <summary>
    /// Identity is (Language, UnitId), so two units that merely share a name across languages are
    /// not each other (ML-03).
    /// </summary>
    [Fact]
    public void SameNameInAnotherLanguageIsNotTheSameUnit()
    {
        var units = new UnitsToCompare(
            older: [Units.Csharp("Widgets")],
            newer: [Units.Swift("Widgets")]);

        Assert.True(Evaluator.AreDifferencesPresent(units));
    }
}
