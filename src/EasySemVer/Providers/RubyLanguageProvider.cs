using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// Ruby gems, marked by a .gemspec (LNG-01, version-sync).
/// <para>
/// The version is read from two places, because gems use two conventions: a literal
/// <c>spec.version = "1.2.3"</c> in the gemspec, and the far more common
/// <c>spec.version = Widgets::VERSION</c> pointing at a <c>VERSION</c> constant in
/// <c>lib/widgets/version.rb</c>. The second is not a literal and is untouchable (MVR-04), so the
/// constant itself is what gets written.
/// </para>
/// <para>
/// Ruby stays version-sync for a reason that will not change soon: <c>private</c> is a method call,
/// not a declaration, and a class's surface can be assembled at load time. There is no honest static
/// answer to what a gem's public API is (LNG-02).
/// </para>
/// </summary>
internal class RubyLanguageProvider(
    IReadOnlyList<IDiscoverVersionSources> versionSources)
    : ManifestLanguageProvider(versionSources)
{
    internal const string RubyLanguageId = "ruby";

    internal const string RubyUnitKind = "gem";

    public override string LanguageId => RubyLanguageId;

    protected override string UnitKind => RubyUnitKind;

    protected override string ManifestFileName => "*.gemspec";
}
