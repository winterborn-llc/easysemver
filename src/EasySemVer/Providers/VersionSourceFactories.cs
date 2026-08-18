using Winterborn.Tools.EasySemVer.CodeReader.Csharp;
using Winterborn.Tools.EasySemVer.CodeReader.Swift;
using Winterborn.Tools.EasySemVer.CodeReader.Vb;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// The registration point for version conventions, and the only file a new one has to touch
/// outside its own class - the same bargain <see cref="LanguageProviders"/> makes for languages.
/// <para>
/// This exists because the alternative was a private GetVersionSources on every provider, each
/// with its own hand-rolled glob-probe-add loop; there were three of them for two languages, and
/// C# had no loop at all, so teaching it a second convention meant restructuring the provider
/// rather than adding a class.
/// </para>
/// </summary>
internal static class VersionSourceFactories
{
    internal static IReadOnlyList<IDiscoverVersionSources> Create(IRunProcess runProcess)
    {
        return
        [
            new CsProjVersionSources(),
            new VbProjVersionSources(),
            new GitTagVersionSources(runProcess),
            new PodspecVersionSources(),
            new SwiftVersionFileSources(),
            new MarketingVersionSources(),
            new BuildCounterVersionSources(),
            new InfoPlistVersionSources()
        ];
    }

    /// <summary>
    /// Every source of every convention that applies to this scope, in registration order so that
    /// a unit's sources are ordered the same way on every machine (BAS-04).
    /// </summary>
    internal static IVersionSource[] For(
        IReadOnlyList<IDiscoverVersionSources> factories,
        string languageId,
        VersionSourceScope scope)
    {
        var sources = new List<IVersionSource>();
        foreach (var factory in factories)
        {
            if (!string.Equals(factory.LanguageId, languageId, StringComparison.Ordinal))
            {
                continue;
            }

            if (!factory.UnitKinds.Contains(scope.UnitKind))
            {
                continue;
            }

            sources.AddRange(factory.Discover(scope));
        }

        return sources.ToArray();
    }
}
