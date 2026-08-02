using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Providers;

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

    internal static ILanguageProvider? Find(IReadOnlyList<ILanguageProvider> providers, Language language)
    {
        foreach (var provider in providers)
        {
            if (provider.Language != language)
            {
                continue;
            }

            return provider;
        }

        return null;
    }
}
