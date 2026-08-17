using Winterborn.Tools.EasySemVer.DataObject;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Test;

public class TestVersion
{
    [Fact]
    public void CanExtractPartsFromVersionString()
    {
        var version = new Version("1.0.2");
        Assert.Equal(1, version.Major);
        Assert.Equal(0, version.Minor);
        Assert.Equal(2, version.Patch);
    }

    [Theory]
    [InlineData("1.0.2", "1.0.3", VersionType.Patch)]
    [InlineData("1.0.2", "1.1.0", VersionType.Minor)]
    [InlineData("1.1.1", "2.0.0", VersionType.Major)]
    [InlineData("1.0.2147483647", "1.1.0", VersionType.Patch)]
    [InlineData("1.2147483647.123", "2.0.0", VersionType.Minor)]
    public void VerifyIncrementingLogic(string given, string expected, VersionType incrementer)
    {
        var version = new Version(given);
        version.Increment(incrementer);
        Assert.Equal(expected, version);
    }

    /// <summary>
    /// Opt-in segment ceilings: a segment past its cap carries into the one above. The result is
    /// always the higher version, so a run never publishes a number below the one before it.
    /// </summary>
    [Theory]
    [InlineData("1.0.255", VersionType.Patch, "1.1.0")]
    [InlineData("1.0.254", VersionType.Patch, "1.0.255")]
    [InlineData("1.255.0", VersionType.Minor, "2.0.0")]
    [InlineData("1.255.255", VersionType.Patch, "2.0.0")]
    [InlineData("1.0.300", VersionType.Patch, "1.1.0")]
    public void SegmentsCarryWhenTheyPassTheirCeiling(
        string given,
        VersionType incrementer,
        string expected)
    {
        var version = new Version(given);

        version.Increment(incrementer, maximumMinor: 255, maximumPatch: 255);

        Assert.Equal(expected, version.ToString());
    }

    /// <summary>
    /// The ceilings are independent: capping patch alone lets minor climb past 255 unchecked, which
    /// is what a target that constrains one segment and not the other needs.
    /// </summary>
    [Fact]
    public void AnUncappedSegmentNeverCarries()
    {
        var version = new Version("1.255.255");

        version.Increment(VersionType.Patch, maximumMinor: null, maximumPatch: 255);

        Assert.Equal("1.256.0", version.ToString());
    }

    /// <summary>No ceiling is the default, so nobody who has not asked for this sees it.</summary>
    [Fact]
    public void WithoutCeilingsSegmentsClimbFreely()
    {
        var version = new Version("1.0.255");

        version.Increment(VersionType.Patch);

        Assert.Equal("1.0.256", version.ToString());
    }

    /// <summary>
    /// MVR-02 (was G-11) - short versions normalise to three segments on parse instead of
    /// crashing Increment. A two-segment MARKETING_VERSION is routine input now.
    /// </summary>
    [Theory]
    [InlineData("1.0", "1.0.0")]
    [InlineData("1", "1.0.0")]
    [InlineData("", "0.0.0")]
    [InlineData(null, "0.0.0")]
    public void ShortVersionsNormaliseToThreeSegments(string? given, string expected)
    {
        Assert.Equal(expected, new Version(given).ToString());
    }

    [Theory]
    [InlineData("1.0", VersionType.Patch, "1.0.1")]
    [InlineData("1.2", VersionType.Minor, "1.3.0")]
    [InlineData("1", VersionType.Major, "2.0.0")]
    [InlineData("", VersionType.Patch, "0.0.1")]
    public void ShortVersionsCanBeIncremented(string given, VersionType incrementer, string expected)
    {
        var version = new Version(given);
        version.Increment(incrementer);
        Assert.Equal(expected, version.ToString());
    }

    [Fact]
    public void EmptyVersionComparesAsZero()
    {
        Assert.True(new Version("") == new Version("0.0.0"));
        Assert.True(new Version("0.0.1") > new Version(""));
    }

    /// <summary>MVR-03 - a value nobody can parse is skipped, never a reason to fail the run.</summary>
    [Theory]
    [InlineData("1.2.3-beta.1")]
    [InlineData("not-a-version")]
    [InlineData("1.x.3")]
    public void UnparseableVersionsAreRejectedWithoutThrowing(string given)
    {
        Assert.False(Version.TryParse(given, out var version));
        Assert.Null(version);
    }

    [Fact]
    public void ParseableVersionsComeBackFromTryParse()
    {
        Assert.True(Version.TryParse("4.5.6", out var version));
        Assert.Equal("4.5.6", version!.ToString());
    }
}
