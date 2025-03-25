using Xunit;
using Yamamari.Library.AutoVersion;
using Version = Yamamari.Library.AutoVersion.Version;

namespace IntegrationTest;

public class Regression
{
    /// <summary>
    /// This test will fail the first time it is run after a major or minor change.
    /// </summary>
    [Fact]
    public void TestProgramInvocation()
    {
        var autoVersion = GetAutoVersion();
        var testFile = GetTestFile();
        
        // Set the baseline and ensure the file exists.
        autoVersion.Execute();
        
        // Get the current version from disk
        // Run the increment process
        // Get the updated version from disk
        
        var previous = new Version(testFile.Version);
        autoVersion.Execute();

        var newTestFile = GetTestFile();
        var current = new Version(newTestFile.Version);
        
        // Confirm it's just the patch that's updated
        Assert.Equal(previous.Patch + 1, current.Patch);
    }

    private static AutoVersion GetAutoVersion()
    {
        var autoVersion = new AutoVersion(Environment.CurrentDirectory);
        return autoVersion;
    }

    private static CsProjFile GetTestFile()
    {
        var autoVersion = GetAutoVersion();
        var testFile = autoVersion.CsProjFiles.FirstOrDefault(p => p.ProjectName == "Test.csproj");
        if (testFile == null)
        {
            Assert.Fail("Unable to load the test project file.");
        }
        
        return testFile;
    }
}