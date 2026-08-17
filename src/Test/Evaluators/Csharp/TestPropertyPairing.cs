using Winterborn.Tools.EasySemVer.Evaluators.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>
/// RUL-11 - the traversal seven rules now share. Pairing is where an inverted argument or an
/// off-by-one hides silently: every rule downstream still fires, just on the wrong pair, and each
/// rule's own test passes because it built both sides itself.
/// </summary>
public class TestPropertyPairing
{
    [Fact]
    public void NeitherSideHasProperties()
    {
        var signatures = Build.Compare(Build.Class(), Build.Class());

        Assert.Empty(Properties.GetPaired(signatures));
    }

    [Fact]
    public void OnlyTheOlderSideHasProperties()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property()),
            Build.Class());

        Assert.Empty(Properties.GetPaired(signatures));
    }

    [Fact]
    public void OnlyTheNewerSideHasProperties()
    {
        var signatures = Build.Compare(
            Build.Class(),
            Build.Class().WithProperties(Build.Property()));

        Assert.Empty(Properties.GetPaired(signatures));
    }

    [Fact]
    public void APropertyOnOneSideOnlyIsNotPaired()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property("Kept"), Build.Property("Dropped")),
            Build.Class().WithProperties(Build.Property("Kept"), Build.Property("Introduced")));

        var paired = Properties.GetPaired(signatures).ToList();

        Assert.Single(paired);
        Assert.Equal("Kept", paired[0].Newer.Name);
    }

    [Fact]
    public void APropertyOnBothSidesPairsTheTwoDeclarations()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property(type: "string")),
            Build.Class().WithProperties(Build.Property(type: "int")));

        var paired = Properties.GetPaired(signatures).ToList();

        Assert.Single(paired);

        // The whole point of the helper: older is genuinely the older declaration. Transposing
        // these would invert every directional rule at once and no rule test would notice.
        Assert.Equal("string", paired[0].Older.Type);
        Assert.Equal("int", paired[0].Newer.Type);
    }

    [Fact]
    public void PairsComeOutInTheOlderSidesOrder()
    {
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property("First"), Build.Property("Second")),
            Build.Class().WithProperties(Build.Property("Second"), Build.Property("First")));

        var paired = Properties.GetPaired(signatures).ToList();

        Assert.Equal(["First", "Second"], paired.Select(p => p.Newer.Name));
    }

    [Fact]
    public void DeclaringTypeIsTheNewerType()
    {
        var signatures = Build.Compare(
            Build.Class("Test.Renamed").WithProperties(Build.Property()),
            Build.Class("Test.Renamed").WithProperties(Build.Property()));

        var paired = Properties.GetPaired(signatures).ToList();

        Assert.Single(paired);
        Assert.Same(signatures.ClassHistory.First().Newer, paired[0].DeclaringType);
    }

    [Fact]
    public void ADuplicatedNamePairsAgainstTheFirstMatch()
    {
        // Roslyn cannot produce this, but a hand-edited or future baseline can, and a silent
        // wrong answer is worse than a documented one. First-match is what the list's own
        // indexer does, so the helper agrees with every other lookup in the codebase.
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property(type: "string")),
            Build.Class().WithProperties(
                Build.Property(type: "int"),
                Build.Property(type: "long")));

        var paired = Properties.GetPaired(signatures).ToList();

        Assert.Single(paired);
        Assert.Equal("int", paired[0].Newer.Type);
    }
}
