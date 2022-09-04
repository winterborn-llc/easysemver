using Yamamari.Library.VersionCounter;

namespace Test;

public class TestParts
{
    [Fact]
    public void AssertPathRequired()
    {
        Assert.Throws
            <InvalidProgramException>
            (() => IncrementFileVersion.GetFilePath(Array.Empty<string>()));
    }
    
    [Fact]
    public void AssertPathToSomewhere()
    {
        Assert.Throws
            <FileNotFoundException>
            (() => IncrementFileVersion.GetFilePath(new []{ "HelloWorld.csv" }));
    }
    
    [Fact]
    public void ValidateVersionList()
    {
        var version = new Version();
        var list = new VersionList(version);
        Assert.Equal(2, list.List.Count);
        Assert.Equal("0.0", list.ToString());
        list.Increment();
        Assert.Equal("0.1", list.ToString());
        list.Increment(true);
        Assert.Equal("1.0", list.ToString());
    }
    
    [Fact]
    public void ValidateLimits()
    {
        var version = new Version($"1.0.{int.MaxValue}");
        var list = new VersionList(version);
        Assert.Equal(3, list.List.Count);
        Assert.Equal($"1.0.{int.MaxValue}", list.ToString());
        list.Increment();
        Assert.Equal("1.1.0", list.ToString());
    }
    
    [Fact]
    public void ValidateLimitsSignificant()
    {
        var version = new Version($"1.0.{int.MaxValue}");
        var list = new VersionList(version);
        Assert.Equal(3, list.List.Count);
        Assert.Equal($"1.0.{int.MaxValue}", list.ToString());
        list.Increment(true);
        Assert.Equal("1.1.0", list.ToString());
    }
}