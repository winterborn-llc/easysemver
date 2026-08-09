using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Evaluators;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Providers;

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

    /// <summary>
    /// UNI-04 - a unit with no public API surface is not compared, so gutting one classifies as
    /// Patch. This is the whole point: renaming a test method is not a breaking change to anybody.
    /// </summary>
    [Fact]
    public void AUnitWithNoApiSurfaceIsNotCompared()
    {
        var wasFull = new CsharpProject("Tests")
        {
            Classes = [new CsharpClass { Name = "Tests.WidgetTests" }]
        };

        var older = new[] { Units.Csharp("Tests", wasFull) };
        var newer = new[] { Units.Csharp("Tests", new CsharpProject("Tests"), hasPublicApiSurface: false) };

        var report = ChangeClassifier.Classify(older, newer, Providers);

        Assert.Equal(VersionType.Patch, report.ChangeType);
        Assert.Empty(report.Findings);
    }

    /// <summary>
    /// UNI-04's upgrade path. A baseline written before it still lists the unit in full, and the
    /// unit is no longer on the comparable side - which reads as a removal, and Major, unless it is
    /// dropped from the older side too. That would be this requirement causing the exact
    /// misclassification it exists to prevent, once, on everybody's next release.
    /// </summary>
    [Fact]
    public void ALegacyBaselineEntryForASurfacelessUnitIsNotARemoval()
    {
        var older = new[]
        {
            Units.Csharp("Widgets", new CsharpProject("Widgets")),
            Units.Csharp("Tests", new CsharpProject("Tests"))
        };

        IPackageableUnit[] newer =
        [
            Units.Csharp("Widgets", new CsharpProject("Widgets")),
            Units.Csharp("Tests", hasPublicApiSurface: false)
        ];

        var report = ChangeClassifier.Classify(older, newer, Providers);

        Assert.Equal(VersionType.Patch, report.ChangeType);
        Assert.Empty(report.Findings);
    }

    /// <summary>
    /// UNI-04 - and it does not go the other way either. A surfaceless unit appearing for the first
    /// time is not an addition, so adding a test project to a repository does not bump its minor.
    /// </summary>
    [Fact]
    public void AddingAUnitWithNoApiSurfaceIsNotAnAddition()
    {
        var older = new[] { Units.Csharp("Widgets", new CsharpProject("Widgets")) };
        IPackageableUnit[] newer =
        [
            Units.Csharp("Widgets", new CsharpProject("Widgets")),
            Units.Csharp("Tests", hasPublicApiSurface: false)
        ];

        var report = ChangeClassifier.Classify(older, newer, Providers);

        Assert.Equal(VersionType.Patch, report.ChangeType);
        Assert.Empty(report.Findings);
    }

    /// <summary>
    /// UNI-04 is per unit, not per run: a real change beside a surfaceless one is still classified.
    /// A filter that dropped the wrong side, or too much of it, would pass every test above and
    /// silently stop versioning the repository.
    /// </summary>
    [Fact]
    public void RealChangesAreStillClassifiedAlongsideASurfacelessUnit()
    {
        var older = new[]
        {
            Units.Csharp("Widgets", new CsharpProject("Widgets")),
            Units.Csharp("Tests", new CsharpProject("Tests"))
        };

        IPackageableUnit[] newer =
        [
            Units.Csharp("Tests", hasPublicApiSurface: false),
            Units.Swift("Sources/Gadgets:Gadgets")
        ];

        var report = ChangeClassifier.Classify(older, newer, Providers);

        // Widgets really did go, and Gadgets really did arrive.
        Assert.Equal(VersionType.Major, report.ChangeType);
        Assert.Equal(
            [nameof(UnitAdded), nameof(UnitRemoved)],
            report.Findings.Select(finding => finding.RuleName).Order());
    }
}
