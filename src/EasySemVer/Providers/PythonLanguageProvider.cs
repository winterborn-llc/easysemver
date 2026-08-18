using Winterborn.Tools.EasySemVer.DataObject;
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

    /// <summary>
    /// FLD-06. `__pycache__` and `site-packages` mean one thing wherever they appear; `venv` does
    /// not, so it needs the pyproject beside it. `.venv`, `.tox` and `.eggs` are already covered by
    /// the leading-dot rule.
    /// </summary>
    public override IReadOnlyList<DirectoryExclusion> DirectoryExclusions =>
    [
        DirectoryExclusion.Always("__pycache__"),
        DirectoryExclusion.Always("site-packages"),
        DirectoryExclusion.Beside("venv", "pyproject.toml", "setup.py", "setup.cfg")
    ];

    public override string LanguageId => PythonLanguageId;

    protected override string UnitKind => PythonUnitKind;

    protected override string ManifestFileName => "pyproject.toml";
}
