using Xunit;
using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.Extensions;
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
        // Set the baseline and ensure the file exists.
        var autoVersion = GetAutoVersion();
        autoVersion.Execute();
        
        // Get the current version from disk
        // Run the increment process
        // Get the updated version from disk, which should be a patch since nothing changed

        var baselineFile = GetTestFile();
        var previous = new Version(baselineFile.Version);
        autoVersion.Execute();

        autoVersion.GetHashCode();
        
        var updatedFile = GetTestFile();
        var current = new Version(updatedFile.Version);
        Assert.Equal(previous.Patch + 1, current.Patch);
    }

    private static AutoVersion GetAutoVersion()
    {
        var autoVersion = new AutoVersion();
        return autoVersion;
    }

    private static CsProjFile GetTestFile()
    {
        var solutionDirectory = Environment.CurrentDirectory.GetSolutionDirectory();
        var testFile = AutoVersion.GetProjectFiles(solutionDirectory).FirstOrDefault(p => p.ProjectName == "Test.csproj");
        if (testFile == null)
        {
            Assert.Fail("Unable to load the test project file.");
        }
        
        return testFile;
    }
}