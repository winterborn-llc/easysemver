using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// The registration point, and the only file a new language has to touch outside its own folders
/// (ML-02, acceptance criterion 8).
/// </summary>
internal static class LanguageProviders
{
    internal static IReadOnlyList<ILanguageProvider> Create(IRunProcess runProcess)
    {
        var versionSources = VersionSourceFactories.Create(runProcess);
        return
        [
            // Full tier (LNG-01): discovered, read, classified, stamped.
            new CsharpLanguageProvider(versionSources),
            new VbLanguageProvider(versionSources),
            new SwiftLanguageProvider(versionSources),

            // Version-sync tier (LNG-01): discovered, seeded and stamped, never read and so never
            // voting on the change type. Each is one file, because ManifestLanguageProvider holds
            // everything except the language's id, unit kind and manifest name.
            new JavascriptLanguageProvider(versionSources),
            new RustLanguageProvider(versionSources),
            new PythonLanguageProvider(versionSources),
            new DartLanguageProvider(versionSources),
            new PhpLanguageProvider(versionSources),
            new JavaLanguageProvider(versionSources),
            new CppLanguageProvider(versionSources),
            new RubyLanguageProvider(versionSources),
            new PerlLanguageProvider(versionSources),
            new GradleLanguageProvider(versionSources)
        ];
    }

    internal static ILanguageProvider? Find(
        IReadOnlyList<ILanguageProvider> providers,
        string languageId)
    {
        foreach (var provider in providers)
        {
            if (!string.Equals(provider.LanguageId, languageId, StringComparison.Ordinal))
            {
                continue;
            }

            return provider;
        }

        return null;
    }
}
