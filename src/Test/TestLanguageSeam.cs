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

    /// <summary>
    /// Every registered language has exactly one provider, and every provider is reachable by its
    /// own id. There is no enum to enumerate any more - the ids are whatever the providers say
    /// they are - so this asserts against the registry itself rather than against a list that a
    /// new language would have to be added to.
    /// </summary>
    [Fact]
    public void EveryLanguageHasExactlyOneProvider()
    {
        var providers = LanguageProviders.Create(new ProcessRunner());

        foreach (var provider in providers)
        {
            var matching = providers
                .Where(p => p.LanguageId == provider.LanguageId)
                .ToArray();

            Assert.Single(matching);
            Assert.Same(provider, LanguageProviders.Find(providers, provider.LanguageId));
        }
    }

    /// <summary>
    /// A language id is persisted in the baseline and published in the report, so it is a
    /// contract before it is a label. Case and whitespace are the two ways a new provider would
    /// break it without noticing: "CSharp" reads back as a different language from "csharp" and
    /// silently loses every unit the old baseline recorded.
    /// </summary>
    [Fact]
    public void EveryLanguageIdIsLowerCaseAndBare()
    {
        foreach (var provider in LanguageProviders.Create(new ProcessRunner()))
        {
            var id = provider.LanguageId;

            Assert.False(string.IsNullOrWhiteSpace(id));
            Assert.Equal(id.ToLowerInvariant(), id);
            Assert.Equal(id.Trim(), id);
        }
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

            // A version-sync provider (LNG-01) answers at its tier rather than per language, and
            // ManifestLanguageProvider is where that answer is written down: a unit with no API
            // surface at all has nothing for test code to be excluded from, so the question is
            // settled for every language in the tier at once. Requiring six identical overrides
            // would be ceremony that reads like coverage.
            //
            // What must still never happen is a provider silently inheriting the *interface's*
            // default, which is what this has always been guarding against.
            var answeredByTheTier = declared?.DeclaringType == typeof(ManifestLanguageProvider);

            Assert.True(
                declared?.DeclaringType == provider.GetType() || answeredByTheTier,
                $"{provider.LanguageId} inherits ILanguageProvider.IsTestCode's default. Its test "
                + "code will be compared like production code and vote on the version (UNI-04).");
        }
    }

    /// <summary>
    /// A version convention is registered against a language and a set of unit kinds, and a typo
    /// in either is silent: the factory simply never fires, the unit reports no version location,
    /// and the run seeds from 0.0.0 without complaining. This is what would say so.
    /// </summary>
    [Fact]
    public void EveryVersionConventionIsRegisteredAgainstARealLanguageAndUnitKind()
    {
        var providers = LanguageProviders.Create(new ProcessRunner());
        var languages = providers.Select(p => p.LanguageId).ToHashSet(StringComparer.Ordinal);

        foreach (var factory in VersionSourceFactories.Create(new ProcessRunner()))
        {
            Assert.Contains(factory.LanguageId, languages);
            Assert.NotEmpty(factory.UnitKinds);
            Assert.DoesNotContain(string.Empty, factory.UnitKinds);
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
