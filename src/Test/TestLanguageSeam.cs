using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Providers;

namespace Test;

/// <summary>
/// Acceptance criterion 8, made enforceable: adding a language must not require editing anything
/// under Interfaces/, Evaluation/, Persistence/ or the neutral Evaluators/, beyond one
/// registration line. The seam is easy to breach by accident - one convenient cast to
/// <c>CsharpProject</c> in the classifier and it is gone - so this asserts it structurally
/// rather than trusting review.
/// </summary>
public class TestLanguageSeam
{
    private static readonly string[] NeutralFolders =
    [
        "Interfaces",
        "Evaluation",
        "Persistence",
        "Evaluators"
    ];

    /// <summary>Language names may appear in prose; what must not appear is a reference in code.</summary>
    private static bool IsCode(string line)
    {
        var trimmed = line.TrimStart();
        return !trimmed.StartsWith("///", StringComparison.Ordinal)
               && !trimmed.StartsWith("//", StringComparison.Ordinal)
               && !trimmed.StartsWith('*');
    }

    [Fact]
    public void TheNeutralCoreNamesNoLanguageType()
    {
        var offences = new List<string>();
        foreach (var folder in NeutralFolders)
        {
            var directory = Path.Combine(GetSourceDirectory(), folder);

            // Files directly in the folder are neutral; the per-language subfolders are not.
            foreach (var file in Directory.GetFiles(directory, "*.cs", SearchOption.TopDirectoryOnly))
            {
                var lineNumber = 0;
                foreach (var line in File.ReadAllLines(file))
                {
                    lineNumber++;
                    if (!IsCode(line))
                    {
                        continue;
                    }

                    foreach (var languageName in (string[])["Csharp", "Swift", "Xcode"])
                    {
                        if (!line.Contains(languageName, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        offences.Add($"{folder}/{Path.GetFileName(file)}:{lineNumber}: {line.Trim()}");
                    }
                }
            }
        }

        Assert.Empty(offences);
    }

    /// <summary>Every registered language has exactly one provider, and every provider is reachable.</summary>
    [Fact]
    public void EveryLanguageHasExactlyOneProvider()
    {
        var providers = LanguageProviders.Create(new ProcessRunner());

        foreach (var language in Enum.GetValues<Language>())
        {
            var matching = providers.Where(p => p.Language == language).ToArray();
            Assert.Single(matching);
            Assert.NotNull(LanguageProviders.Find(providers, language));
        }

        Assert.Equal(Enum.GetValues<Language>().Length, providers.Count);
    }

    /// <summary>
    /// UNI-04 - every language answers the test-code question for itself, and none inherits the
    /// interface's default.
    /// <para>
    /// The default exists so that adding the member broke no implementer, and so that an unfamiliar
    /// language behaves as it did before the question was asked. That makes forgetting it silent:
    /// the run stays green and a language's test code quietly keeps a vote on the version. This is
    /// the only thing that would say so, which is why it reads the declaration rather than calling
    /// the method - an override that happens to return false for the fixture at hand still counts
    /// as having thought about it.
    /// </para>
    /// </summary>
    [Fact]
    public void EveryProviderDecidesForItselfWhatIsTestCode()
    {
        foreach (var provider in LanguageProviders.Create(new ProcessRunner()))
        {
            var declared = provider.GetType().GetMethod(
                nameof(ILanguageProvider.IsTestCode),
                [typeof(IPackageableUnit)]);

            Assert.True(
                declared?.DeclaringType == provider.GetType(),
                $"{provider.Language} inherits ILanguageProvider.IsTestCode's default. Its test "
                + "code will be compared like production code and vote on the version (UNI-04).");
        }
    }

    /// <summary>
    /// The neutral core reaches a signature only as <see cref="object"/>: if it could see the
    /// concrete type it would eventually be tempted to look inside it (ML-01).
    /// </summary>
    [Fact]
    public void SignatureIsOpaqueToTheCore()
    {
        Assert.Equal(typeof(object), typeof(IPackageableUnit).GetProperty(nameof(IPackageableUnit.Signature))!.PropertyType);
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
