using Winterborn.Tools.EasySemVer.Evaluators.Csharp;

namespace Test.Evaluators.Csharp;

/// <summary>RUL-11 - the enum-member traversal, on the same terms as <see cref="TestPropertyPairing"/>.</summary>
public class TestEnumMemberPairing
{
    [Fact]
    public void NeitherSideHasMembers()
    {
        var signatures = Build.Compare(Build.Enum(), Build.Enum());

        Assert.Empty(EnumMembers.GetPairedMembers(signatures));
    }

    [Fact]
    public void OnlyTheOlderSideHasMembers()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red")),
            Build.Enum());

        Assert.Empty(EnumMembers.GetPairedMembers(signatures));
    }

    [Fact]
    public void OnlyTheNewerSideHasMembers()
    {
        var signatures = Build.Compare(
            Build.Enum(),
            Build.Enum().WithMembers(Build.EnumMember("Red")));

        Assert.Empty(EnumMembers.GetPairedMembers(signatures));
    }

    [Fact]
    public void AMemberOnOneSideOnlyIsNotPaired()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Kept"), Build.EnumMember("Dropped")),
            Build.Enum().WithMembers(Build.EnumMember("Kept"), Build.EnumMember("Introduced")));

        var paired = EnumMembers.GetPairedMembers(signatures).ToList();

        Assert.Single(paired);
        Assert.Equal("Kept", paired[0].Newer.Name);
    }

    [Fact]
    public void AMemberOnBothSidesPairsTheTwoDeclarations()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("Red", "1")),
            Build.Enum().WithMembers(Build.EnumMember("Red", "2")));

        var paired = EnumMembers.GetPairedMembers(signatures).ToList();

        Assert.Single(paired);
        Assert.Equal("1", paired[0].Older.Value);
        Assert.Equal("2", paired[0].Newer.Value);
    }

    [Fact]
    public void PairsComeOutInTheOlderSidesOrder()
    {
        var signatures = Build.Compare(
            Build.Enum().WithMembers(Build.EnumMember("First"), Build.EnumMember("Second")),
            Build.Enum().WithMembers(Build.EnumMember("Second"), Build.EnumMember("First")));

        var paired = EnumMembers.GetPairedMembers(signatures).ToList();

        Assert.Equal(["First", "Second"], paired.Select(p => p.Newer.Name));
    }

    [Fact]
    public void ATypeThatIsNotAnEnumIsNotTraversed()
    {
        // GetPairedEnums filters on the newer side's kind, so a class never reaches here even
        // though ClassHistory pairs types of every kind (CLS-02).
        var signatures = Build.Compare(
            Build.Class().WithProperties(Build.Property()),
            Build.Class().WithProperties(Build.Property()));

        Assert.Empty(EnumMembers.GetPairedMembers(signatures));
    }
}
