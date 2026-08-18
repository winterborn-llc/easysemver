using Winterborn.Tools.EasySemVer.Interfaces;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.Providers;

/// <summary>
/// Reading and writing a unit's versions, shared by every provider (MVR-03, MVR-05).
/// <para>
/// These two loops were written out character-for-character in C#, VB, Swift and the version-sync
/// base - four copies of the same walk over <see cref="IPackageableUnit.VersionSources"/>. Unlike a
/// classification rule, there is nothing here a language could legitimately disagree about: a source
/// either yields a version or it does not, and a source either is writable or it is not. The
/// per-language decisions all happen earlier, when the sources are discovered.
/// </para>
/// <para>
/// It is a static helper rather than a base class because the Full-tier providers are not otherwise
/// related, and giving three shipped providers a common ancestor to share eight lines would be a
/// larger change than the duplication costs.
/// </para>
/// </summary>
internal static class UnitVersions
{
    /// <summary>MVR-03 - every version this unit's sources can offer. Unreadable ones are skipped.</summary>
    internal static IReadOnlyList<Version> Read(IPackageableUnit unit)
    {
        var versions = new List<Version>();
        foreach (var source in unit.VersionSources)
        {
            var version = source.Read();
            if (version == null)
            {
                continue;
            }

            versions.Add(version);
        }

        return versions;
    }

    /// <summary>
    /// MVR-05 - the one new version into every writable location, returning what was touched so the
    /// run can report it (REP-10). A source that is not writable is skipped rather than asked, which
    /// is what keeps a read-only convention - a git tag without <c>--tag</c> - out of the file list.
    /// </summary>
    internal static IReadOnlyList<string> Write(IPackageableUnit unit, Version version)
    {
        var written = new List<string>();
        foreach (var source in unit.VersionSources)
        {
            if (!source.IsWritable)
            {
                continue;
            }

            source.Write(version);
            Log.WriteLine($"Wrote {version} to {source.Location}");
            written.Add(source.Location);
        }

        return written;
    }
}
