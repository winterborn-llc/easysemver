using System.Text.RegularExpressions;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.CodeReader.Manifests;

/// <summary>
/// MVR-03 for a manifest that carries its package's version as a literal.
/// <para>
/// This is one class taking its language, unit kind and pattern as arguments, rather than the
/// one-class-per-convention the other version sources use. The convention here really is a single
/// one - "a literal version assignment in this package's own manifest" - and package.json,
/// Cargo.toml, pubspec.yaml and gradle.properties differ only in the pattern that finds it. Seven
/// classes differing by one field each would be a worse description of that than seven arguments.
/// </para>
/// </summary>
internal class ManifestVersionSources(
    string languageId,
    string unitKind,
    string kind,
    Regex assignment) : IDiscoverVersionSources
{
    public string LanguageId => languageId;

    public IReadOnlyList<string> UnitKinds => [unitKind];

    public IEnumerable<IVersionSource> Discover(VersionSourceScope scope)
    {
        // MVR-04: probe rather than trust the filename. A manifest with no literal version - one
        // that computes it, or simply omits it as most composer.json files do - yields no source,
        // and so is read-skipped and write-skipped rather than being given a version it never had.
        if (!File.Exists(scope.UnitPath))
        {
            yield break;
        }

        string text;
        try
        {
            text = File.ReadAllText(scope.UnitPath);
        }
        catch (IOException e)
        {
            Log.WriteLine($"Could not read {scope.UnitPath}: {e.Message}");
            yield break;
        }

        if (!ManifestVersionSource.HasLiteralVersion(text, assignment))
        {
            yield break;
        }

        yield return new ManifestVersionSource(
            scope.UnitPath,
            FolderScanner.GetRelativePath(scope.FolderRoot, scope.UnitPath),
            kind,
            assignment);
    }
}
