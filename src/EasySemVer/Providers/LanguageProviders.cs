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
        return
        [
            new CsharpLanguageProvider(),
            new SwiftLanguageProvider(runProcess)
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
