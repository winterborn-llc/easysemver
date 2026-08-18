using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// Perl distributions, marked by a Makefile.PL, a Build.PL or a dist.ini (LNG-01, version-sync).
/// <para>
/// Three manifests because three toolchains, and a distribution carrying two of them is still one
/// package - which is what LNG-05's one-unit-per-directory rule is for. The version is read from
/// <c>our $VERSION</c> in the distribution's .pm files, and from dist.ini's own <c>version =</c>
/// where Dist::Zilla owns it. META.json and META.yml are deliberately ignored: they are generated
/// from those, and writing them would be writing the output rather than the input.
/// </para>
/// <para>
/// Every .pm carrying a literal $VERSION is written, per MVR-05, because keeping them in step by
/// hand is exactly the chore this removes.
/// </para>
/// <para>
/// Perl will not graduate to Full. Only perl can parse Perl - source filters and prototypes let a
/// program change its own grammar as it is read - so there is no static reader that is correct,
/// and @EXPORT_OK plus a list of sub names is not an API worth classifying against (LNG-02).
/// </para>
/// </summary>
internal class PerlLanguageProvider(
    IReadOnlyList<IDiscoverVersionSources> versionSources)
    : ManifestLanguageProvider(versionSources)
{
    internal const string PerlLanguageId = "perl";

    internal const string PerlUnitKind = "perl-distribution";

    /// <summary>FLD-06. `blib` is what a Perl build copies lib into, vouched for by the manifest.</summary>
    public override IReadOnlyList<DirectoryExclusion> DirectoryExclusions =>
    [
        DirectoryExclusion.Beside("blib", "Makefile.PL", "Build.PL", "dist.ini")
    ];

    public override string LanguageId => PerlLanguageId;

    protected override string UnitKind => PerlUnitKind;

    protected override string ManifestFileName => "Makefile.PL";

    protected override IReadOnlyList<string> ManifestFileNames =>
        ["Makefile.PL", "Build.PL", "dist.ini"];
}
