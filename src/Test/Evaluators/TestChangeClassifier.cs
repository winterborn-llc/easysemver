using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Evaluators;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Process;
using Winterborn.Library.EasySemVer.Providers;

namespace Test.Evaluators;

/// <summary>
/// ML-05 aggregation, including the mixed case TST-M2 asks for: a Swift unit removed while a C#
/// unit is added must come out Major, because Major beats Minor. Since §20 O-04 the classifier
/// also carries the findings behind that verdict, so the tests check both.
/// </summary>
public class TestChangeClassifier
{
    private static IReadOnlyList<ILanguageProvider> Providers =>
        LanguageProviders.Create(new ProcessRunner());

    [Fact]
    public void NothingChangedIsPatch()
    {
        var unit = Units.Csharp("Widgets", new CsharpProject("Widgets"));
        var baseline = Units.Csharp("Widgets", new CsharpProject("Widgets"));

        var report = ChangeClassifier.Classify([baseline], [unit], Providers);

        Assert.Equal(VersionType.Patch, report.ChangeType);
        Assert.Empty(report.Findings);
    }

    [Fact]
    public void SwiftUnitRemovedAndCsharpUnitAddedIsMajor()
    {
        var older = new[] { Units.Swift("Sources/Gadgets:Gadgets") };
        var newer = new[] { Units.Csharp("Widgets") };

        var report = ChangeClassifier.Classify(older, newer, Providers);

        Assert.Equal(VersionType.Major, report.ChangeType);
        Assert.Equal(2, report.Findings.Count);
        Assert.Equal(1, report.Count(VersionType.Major));
        Assert.Equal(1, report.Count(VersionType.Minor));
    }

    [Fact]
    public void OnlyAddingAUnitIsMinor()
    {
        var older = new[] { Units.Csharp("Widgets", new CsharpProject("Widgets")) };
        IPackageableUnit[] newer =
        [
            Units.Csharp("Widgets", new CsharpProject("Widgets")),
            Units.Swift("Sources/Gadgets:Gadgets")
        ];

        var report = ChangeClassifier.Classify(older, newer, Providers);

        Assert.Equal(VersionType.Minor, report.ChangeType);
        var finding = Assert.Single(report.Findings);
        Assert.Equal(nameof(UnitAdded), finding.RuleName);
        Assert.Equal("Sources/Gadgets", finding.Symbol);
        Assert.Equal("was added", finding.Description);
    }

    /// <summary>
    /// NCL-03 - a removed unit is never double-counted as "everything inside it was removed", so
    /// the C# rules never see it at all.
    /// </summary>
    [Fact]
    public void LanguageRulesOnlySeePairedUnits()
    {
        var removedProject = new CsharpProject("Gadgets")
        {
            Classes = [new CsharpClass { Name = "Gadgets.Gadget" }]
        };

        var older = new[]
        {
            Units.Csharp("Widgets", new CsharpProject("Widgets")),
            Units.Csharp("Gadgets", removedProject)
        };
        var newer = new[] { Units.Csharp("Widgets", new CsharpProject("Widgets")) };

        var report = ChangeClassifier.Classify(older, newer, Providers);

        // Major from UnitRemoved alone; the class inside it is not separately counted, so the one
        // finding names the unit and never Gadgets.Gadget.
        Assert.Equal(VersionType.Major, report.ChangeType);
        var finding = Assert.Single(report.Findings);
        Assert.Equal(nameof(UnitRemoved), finding.RuleName);
        Assert.Equal("Gadgets", finding.UnitId);
    }

    /// <summary>
    /// BAS-04 - the same input has to produce the same report, so findings are sorted by unit and
    /// then by symbol rather than left in discovery or rule-registration order.
    /// </summary>
    [Fact]
    public void FindingsAreOrderedByUnitThenSymbol()
    {
        IPackageableUnit[] older = [Units.Csharp("Widgets", new CsharpProject("Widgets"))];
        IPackageableUnit[] newer =
        [
            Units.Swift("Sources/Zebra:Zebra"),
            Units.Csharp("Widgets", new CsharpProject("Widgets")),
            Units.Csharp("Alpha")
        ];

        var report = ChangeClassifier.Classify(older, newer, Providers);

        Assert.Equal(
            ["Csharp Alpha", "Swift Sources/Zebra:Zebra"],
            report.Findings.Select(finding => $"{finding.Language} {finding.UnitId}"));
    }

    /// <summary>NCL-04 / CLS-04 - a null signature list fails safe towards additive.</summary>
    [Fact]
    public void NullSignatureIsMinor()
    {
        var report = ChangeClassifier.Classify(null, [], Providers);

        Assert.Equal(VersionType.Minor, report.ChangeType);
        Assert.Empty(report.Findings);
    }
}
