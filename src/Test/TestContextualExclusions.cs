using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Providers;

namespace Test;

/// <summary>
/// FLD-06 - a language declares what to skip, and the sibling marker that proves it is really that
/// directory. This is the `Packages` post-mortem written down as a mechanism: that entry was wrong
/// because the name alone did not identify the thing, and `vendor`, `target`, `venv` and `blib` are
/// all in the same position.
/// </summary>
public class TestContextualExclusions : IDisposable
{
    private readonly string _folderRoot =
        Directory.CreateTempSubdirectory("easysemver-exclusions").FullName;

    private readonly IReadOnlyList<ILanguageProvider> _providers =
        LanguageProviders.Create(new ProcessRunner());

    public TestContextualExclusions()
    {
        var declared = new List<DirectoryExclusion>();
        foreach (var provider in this._providers)
        {
            declared.AddRange(provider.DirectoryExclusions);
        }

        DirectoryExclusions.BeginRun([], declared);
    }

    public void Dispose()
    {
        Directory.Delete(this._folderRoot, recursive: true);
        GC.SuppressFinalize(this);
    }

    private DirectoryInfo Make(string relativePath, params string[] siblingFiles)
    {
        var directory = Directory.CreateDirectory(Path.Combine(this._folderRoot, relativePath));
        foreach (var file in siblingFiles)
        {
            File.WriteAllText(Path.Combine(directory.Parent!.FullName, file), string.Empty);
        }

        return directory;
    }

    /// <summary>
    /// The trap this design exists to avoid. Interface mapping is fixed at the implementing type,
    /// so a provider declaring exclusions without a virtual to override would be silently ignored -
    /// every test below would still pass on the *global* list while the declared ones did nothing.
    /// </summary>
    [Fact]
    public void DeclaredExclusionsActuallyReachTheProvider()
    {
        var go = LanguageProviders.Find(this._providers, "go")!;

        Assert.Contains(go.DirectoryExclusions, e => e.DirectoryName == "vendor");
    }

    [Theory]
    [InlineData("svc/vendor", "go.mod")]
    [InlineData("app/vendor", "composer.json")]
    [InlineData("crate/target", "Cargo.toml")]
    [InlineData("mod/target", "pom.xml")]
    [InlineData("pkg/venv", "pyproject.toml")]
    [InlineData("dist/blib", "Makefile.PL")]
    public void ADirectoryIsSkippedWhenItsMarkerSitsBesideIt(string path, string marker)
    {
        Assert.True(DirectoryExclusions.IsExcluded(this.Make(path, marker)));
    }

    /// <summary>
    /// The whole point. Every one of these names is somebody's ordinary source directory, and
    /// without its marker it is exactly that.
    /// </summary>
    [Theory]
    [InlineData("docs/vendor")]
    [InlineData("game/target")]
    [InlineData("shop/venv")]
    [InlineData("book/blib")]
    public void TheSameNameIsKeptWhenNothingVouchesForIt(string path)
    {
        Assert.False(DirectoryExclusions.IsExcluded(this.Make(path)));
    }

    /// <summary>A name that cannot mean anything else needs no corroboration.</summary>
    [Theory]
    [InlineData("anywhere/__pycache__")]
    [InlineData("anywhere/site-packages")]
    [InlineData("anywhere/node_modules")]
    public void UnconditionalNamesAreSkippedWithNoMarker(string path)
    {
        Assert.True(DirectoryExclusions.IsExcluded(this.Make(path)));
    }

    /// <summary>
    /// CLI-12 outranks every rule, declared or global. A team that really does keep code in a
    /// directory called `vendor` beside a composer.json can say so and be believed.
    /// </summary>
    [Fact]
    public void DoNotExcludeBeatsADeclaredExclusion()
    {
        var directory = this.Make("app/vendor", "composer.json");

        DirectoryExclusions.BeginRun(["vendor"], [DirectoryExclusion.Beside("vendor", "composer.json")]);

        Assert.False(DirectoryExclusions.IsExcluded(directory));
    }

    /// <summary>
    /// FLD-04's frozen list still applies to everyone. Freezing it rather than distributing it is
    /// what keeps this change from altering any existing repository's discovery.
    /// </summary>
    [Theory]
    [InlineData("src/Widgets/bin")]
    [InlineData("src/Widgets/obj")]
    [InlineData("Pods")]
    public void TheGlobalListIsUnchanged(string path)
    {
        Assert.True(DirectoryExclusions.IsExcluded(this.Make(path)));
    }
}
