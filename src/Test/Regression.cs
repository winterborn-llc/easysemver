using Yamamari.Library.AutoVersion;
using Version = Yamamari.Library.AutoVersion.Version;

namespace Test;

public class Regression
{
    /// <summary>
    /// This test will fail the first time it is run after a major or minor change.
    /// </summary>
    [Fact]
    public void TestProgramInvocation()
    {
        var autoVersion = new AutoVersion(Environment.CurrentDirectory);
        var testFile = autoVersion.CsProjFiles.FirstOrDefault(p => p.ProjectName == "Test.csproj");
        if (testFile == null)
        {
            Assert.Fail("Unable to load the test project file.");
        }

        var previous = new Version(testFile.Version);
        autoVersion.Execute();
        var current = new Version(testFile.Version);
        Assert.Equal(previous.Patch + 1, current.Patch);
        
        var proveItFile = new CsProjFile(testFile.ProjectFilePath);
        Assert.Equal(current, proveItFile.Version);
    }
}