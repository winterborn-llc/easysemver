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
    /// FLD-07 - the names that used to be global now belong to a language and need its evidence.
    /// The common case is unchanged: MSBuild puts bin and obj beside the project file, CocoaPods
    /// puts Pods beside the Podfile, Xcode's build sits beside the .xcodeproj.
    /// </summary>
    [Theory]
    [InlineData("src/Widgets/bin", "Widgets.csproj")]
    [InlineData("src/Widgets/obj", "Widgets.csproj")]
    [InlineData("src/Widgets/bin", "Widgets.vbproj")]
    [InlineData("app/Pods", "Podfile")]
    [InlineData("app/Carthage", "Cartfile")]
    [InlineData("app/build", "App.xcodeproj")]
    [InlineData("svc/build", "CMakeLists.txt")]
    [InlineData("mod/build", "build.gradle.kts")]
    public void TheMigratedNamesStillSkipWhenVouchedFor(string path, string marker)
    {
        Assert.True(DirectoryExclusions.IsExcluded(this.Make(path, marker)));
    }

    /// <summary>
    /// And the behaviour that actually changed. Each of these was skipped in every repository
    /// before; now, with nothing identifying it as build output, it is walked. Anything found
    /// surfaces as a new unit - loud and reviewable - where the global rule hid first-party code
    /// silently, which is the failure `Packages` was removed for.
    /// </summary>
    [Theory]
    [InlineData("tools/bin")]
    [InlineData("scripts/build")]
    [InlineData("music/Pods")]
    [InlineData("gear/Carthage")]
    public void TheMigratedNamesAreKeptWhenNothingVouchesForThem(string path)
    {
        Assert.False(DirectoryExclusions.IsExcluded(this.Make(path)));
    }

    /// <summary>
    /// The two that survived as unconditional, because neither name can mean anything else. They
    /// are owned by a language now rather than by a shared list, but they still need no marker.
    /// </summary>
    [Theory]
    [InlineData("anywhere/node_modules")]
    [InlineData("anywhere/DerivedData")]
    public void TheUnambiguousNamesStayUnconditional(string path)
    {
        Assert.True(DirectoryExclusions.IsExcluded(this.Make(path)));
    }

    /// <summary>
    /// FLD-07 - the global list is now empty, and the bar for putting anything back in it is that
    /// the name cannot mean anything else in any language. This is what would notice a regression
    /// to the old shape.
    /// </summary>
    [Fact]
    public void NothingIsExcludedGloballyByNameAnyMore()
    {
        Assert.Empty(Winterborn.Tools.EasySemVer.Settings.MagicValues.ExcludedDirectoryNames);
    }

    /// <summary>The leading-dot rule is a convention rather than an ecosystem, and stays global.</summary>
    [Theory]
    [InlineData("app/.build")]
    [InlineData("app/.git")]
    [InlineData("app/.venv")]
    public void TheLeadingDotRuleIsStillGlobal(string path)
    {
        Assert.True(DirectoryExclusions.IsExcluded(this.Make(path)));
    }
}
