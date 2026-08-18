using System.Text.RegularExpressions;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Evaluation;
using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.CodeReader.Manifests;

/// <summary>
/// MVR-03 for the ecosystems that keep the version <em>beside</em> the manifest rather than in it.
/// Ruby is the archetype: the gemspec says <c>spec.version = Widgets::VERSION</c>, which is not a
/// literal and so is untouchable (MVR-04), while the number itself lives in
/// <c>lib/widgets/version.rb</c>. Perl does the same with <c>our $VERSION</c> in a .pm.
/// <para>
/// Every matching file under the unit's directory yields its own source, because MVR-05 writes every
/// existing location and these ecosystems routinely keep several in step by hand. Reading takes the
/// highest of them (MVR-03), which is the right answer when they have drifted.
/// </para>
/// </summary>
internal class NearbyVersionSources(
    string languageId,
    string unitKind,
    string kind,
    string searchPattern,
    Regex assignment) : IDiscoverVersionSources
{
    public string LanguageId => languageId;

    public IReadOnlyList<string> UnitKinds => [unitKind];

    public IEnumerable<IVersionSource> Discover(VersionSourceScope scope)
    {
        var directory = Path.GetDirectoryName(scope.UnitPath);
        if (directory == null || !Directory.Exists(directory))
        {
            yield break;
        }

        foreach (var path in FolderScanner.FindFiles(directory, searchPattern))
        {
            string text;
            try
            {
                text = File.ReadAllText(path);
            }
            catch (IOException e)
            {
                Log.WriteLine($"Could not read {path}: {e.Message}");
                continue;
            }

            if (!ManifestVersionSource.HasLiteralVersion(text, assignment))
            {
                continue;
            }

            yield return new ManifestVersionSource(
                path,
                FolderScanner.GetRelativePath(scope.FolderRoot, path),
                kind,
                assignment);
        }
    }
}
