using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// Python distributions, marked by a pyproject.toml (LNG-01, version-sync).
/// <para>
/// PEP 621 put the version under [project], and Poetry puts it under [tool.poetry]; both are matched.
/// A project using dynamic versioning - `dynamic = ["version"]`, or setuptools-scm - has no literal
/// to find, so it is left alone (MVR-04), which is right: its version comes from a tag.
/// </para>
/// </summary>
internal class PythonLanguageProvider(
    IReadOnlyList<IDiscoverVersionSources> versionSources)
    : ManifestLanguageProvider(versionSources)
{
    internal const string PythonLanguageId = "python";

    internal const string PythonUnitKind = "python-project";

    public override string LanguageId => PythonLanguageId;

    protected override string UnitKind => PythonUnitKind;

    protected override string ManifestFileName => "pyproject.toml";
}
