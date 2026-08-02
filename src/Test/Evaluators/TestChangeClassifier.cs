using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Process;
using Winterborn.Library.EasySemVer.Providers;

namespace Test.Evaluators;

/// <summary>
/// ML-05 aggregation, including the mixed case TST-M2 asks for: a Swift unit removed while a C#
/// unit is added must come out Major, because Major beats Minor.
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

        Assert.Equal(VersionType.Patch, ChangeClassifier.Classify([baseline], [unit], Providers));
    }

    [Fact]
    public void SwiftUnitRemovedAndCsharpUnitAddedIsMajor()
    {
        var older = new[] { Units.Swift("Sources/Gadgets:Gadgets") };
        var newer = new[] { Units.Csharp("Widgets") };

        Assert.Equal(VersionType.Major, ChangeClassifier.Classify(older, newer, Providers));
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

        Assert.Equal(VersionType.Minor, ChangeClassifier.Classify(older, newer, Providers));
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

        // Major from UnitRemoved alone; the class inside it is not separately counted.
        Assert.Equal(VersionType.Major, ChangeClassifier.Classify(older, newer, Providers));
    }

    /// <summary>NCL-04 / CLS-04 - a null signature list fails safe towards additive.</summary>
    [Fact]
    public void NullSignatureIsMinor()
    {
        Assert.Equal(VersionType.Minor, ChangeClassifier.Classify(null, [], Providers));
    }
}
