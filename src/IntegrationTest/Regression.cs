using Winterborn.Library.EasySemVer;
using Winterborn.Library.EasySemVer.CodeReader;
using Winterborn.Library.EasySemVer.CodeReader.Csharp;
using Winterborn.Library.EasySemVer.Extensions;
using Xunit;
using Version = Winterborn.Library.EasySemVer.DataObject.Version;

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
        Program.Main();
        
        // Get the current version from disk
        // Run the increment process
        // Get the updated version from disk, which should be a patch since nothing changed

        var baselineFile = GetTestFile();
        var previous = new Version(baselineFile.Version);
        Program.Main();
        
        var updatedFile = GetTestFile();
        var current = new Version(updatedFile.Version);
        Assert.Equal(previous.Patch + 1, current.Patch);
    }

    private static CsProjFile GetTestFile()
    {
        var solutionDirectory = Environment.CurrentDirectory.GetSolutionDirectory();
        var testFile = CsProjFile.GetSolutionProjectFiles(solutionDirectory).FirstOrDefault(p => p.ProjectName == "Test.csproj");
        if (testFile == null)
        {
            Assert.Fail("Unable to load the test project file.");
        }
        
        return testFile;
    }
}