using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Providers;

namespace Test.Swift;

/// <summary>
/// The property this whole reader exists for, made enforceable: reading Swift runs no processes.
/// <para>
/// It used to run four - <c>swift package dump-package</c> and <c>xcodebuild -list</c> to discover
/// targets, <c>swift build</c> and <c>xcodebuild build</c> to extract signatures - and the first
/// two resolved the project's package dependencies before they would answer, so a versioning run
/// needed a toolchain, a network and credentials for every private dependency. Nothing about
/// behaviour would notice one of them coming back: every other test would pass and the run would
/// merely need Xcode again. This is what would notice.
/// </para>
/// <para>
/// Git is deliberately not covered. Reading the highest semver tag is a version source (MVR-07),
/// not signature extraction, and it is optional in a way a compile never was.
/// </para>
/// </summary>
public class TestSwiftNeedsNoToolchain
{
    /// <summary>The one file allowed to shell out, and the tool it is allowed to shell out to.</summary>
    private const string GitTagSource = "GitTagVersionSource";

    private static readonly string[] Toolchains = ["xcodebuild", "swiftc", "IRunProcess"];

    /// <summary>Language names may appear in prose; what must not appear is a reference in code.</summary>
    private static bool IsCode(string line)
    {
        var trimmed = line.TrimStart();
        return !trimmed.StartsWith("///", StringComparison.Ordinal)
               && !trimmed.StartsWith("//", StringComparison.Ordinal)
               && !trimmed.StartsWith('*');
    }

    [Fact]
    public void NothingThatReadsSwiftCanRunAProcess()
    {
        var offences = new List<string>();
        var directory = Path.Combine(GetSourceDirectory(), "CodeReader", "Swift");

        foreach (var file in Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (name.StartsWith(GitTagSource, StringComparison.Ordinal))
            {
                continue;
            }

            var lineNumber = 0;
            foreach (var line in File.ReadAllLines(file))
            {
                lineNumber++;
                if (!IsCode(line))
                {
                    continue;
                }

                foreach (var toolchain in Toolchains)
                {
                    if (!line.Contains(toolchain, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    offences.Add($"{Path.GetFileName(file)}:{lineNumber}: {line.Trim()}");
                }
            }
        }

        Assert.Empty(offences);
    }

    /// <summary>
    /// The provider is where a process runner would have to be handed in, so its absence from the
    /// constructor is the structural half of the same assertion.
    /// </summary>
    [Fact]
    public void TheSwiftProviderIsNotGivenAProcessRunner()
    {
        var parameters = typeof(SwiftLanguageProvider)
            .GetConstructors()
            .SelectMany(c => c.GetParameters())
            .Select(p => p.ParameterType);

        Assert.DoesNotContain(typeof(IRunProcess), parameters);
    }

    /// <summary>
    /// And the end of it: a whole package discovered and extracted with no PATH to find a
    /// toolchain on. This is the assertion a user would make.
    /// </summary>
    [Fact]
    public void APackageIsReadWithNoToolchainOnThePath()
    {
        using var fixture = new SwiftPackageFixture();
        var previousPath = Environment.GetEnvironmentVariable("PATH");
        try
        {
            Environment.SetEnvironmentVariable("PATH", string.Empty);

            var provider = new SwiftLanguageProvider(
                VersionSourceFactories.Create(new ProcessRunner()));
            var units = provider.Discover(fixture.FolderRoot);
            var widgets = units.First(u => u.DisplayName == "Widgets");
            provider.Extract(widgets);

            Assert.NotNull(widgets.Signature);
            Assert.Contains(units, u => u.DisplayName == "WidgetsTests");
            Assert.True(provider.IsTestCode(units.First(u => u.DisplayName == "WidgetsTests")));
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", previousPath);
        }
    }

    private static string GetSourceDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, "EasySemVer");
            if (Directory.Exists(Path.Combine(candidate, "Interfaces")))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate src/EasySemVer");
    }
}
