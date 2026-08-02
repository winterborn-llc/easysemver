namespace IntegrationTest;

/// <summary>
/// Copies src/TestFixtures/XcodeProject into a temporary directory and renames the project
/// bundle into place. It is checked in as <c>App.xcodeproj.template</c> so this repository does
/// not itself become an Xcode tree that every run has to build - see that fixture's README.
/// </summary>
internal class XcodeProjectFixture : IDisposable
{
    private const string TemplateName = "App.xcodeproj.template";

    private const string ProjectName = "App.xcodeproj";

    internal string FolderRoot { get; }

    internal string ProjectFilePath =>
        Path.Combine(this.FolderRoot, ProjectName, "project.pbxproj");

    internal XcodeProjectFixture()
    {
        this.FolderRoot = Directory.CreateTempSubdirectory("easysemver-xcode-live").FullName;
        CopyDirectory(GetSourceDirectory(), this.FolderRoot);
        Directory.Move(
            Path.Combine(this.FolderRoot, TemplateName),
            Path.Combine(this.FolderRoot, ProjectName));
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(this.FolderRoot, recursive: true);
        }
        catch (IOException)
        {
            // A build server that still has a file handle open is not a test failure.
        }

        GC.SuppressFinalize(this);
    }

    private static string GetSourceDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, "TestFixtures", "XcodeProject");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate src/TestFixtures/XcodeProject");
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.GetFiles(source))
        {
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
        }

        foreach (var directory in Directory.GetDirectories(source))
        {
            CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)));
        }
    }
}
