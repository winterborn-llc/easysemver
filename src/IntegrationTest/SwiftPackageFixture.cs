namespace IntegrationTest;

/// <summary>
/// Copies src/TestFixtures/SwiftPackage into a temporary directory and renames its manifest into
/// place. The manifest is checked in as Package.swift.template so this repository does not become
/// a Swift tree in its own right - see the comment in that file.
/// </summary>
internal class SwiftPackageFixture : IDisposable
{
    internal string FolderRoot { get; }

    internal string PackageDirectory => Path.Combine(this.FolderRoot, "SwiftPackage");

    internal string PodspecPath => Path.Combine(this.PackageDirectory, "Widgets.podspec");

    internal SwiftPackageFixture()
    {
        this.FolderRoot = Directory.CreateTempSubdirectory("easysemver-swift-live").FullName;
        CopyDirectory(GetSourceDirectory(), this.PackageDirectory);
        File.Move(
            Path.Combine(this.PackageDirectory, "Package.swift.template"),
            Path.Combine(this.PackageDirectory, "Package.swift"));
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
            var candidate = Path.Combine(directory.FullName, "TestFixtures", "SwiftPackage");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate src/TestFixtures/SwiftPackage");
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
