using System.Diagnostics;
using Yamamari.Library.AutoVersion;
using Version = Yamamari.Library.AutoVersion.Version;

namespace Test;

public class TestVersion
{
    [Fact]
    public void CanExtractPartsFromVersionString()
    {
        var version = new Version("1.0.2");
        Assert.Equal(1, version.Major);
        Assert.Equal(0, version.Minor);
        Assert.Equal(2, version.Build);
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
}