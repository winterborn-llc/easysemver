using Version = Winterborn.Library.EasySemVer.DataObject.Version;

namespace Winterborn.Library.EasySemVer.Interfaces;

/// <summary>
/// One place a version number already lives inside a packageable unit (MVR-03): a csproj property
/// group, an Xcode build setting, a podspec, a git tag. Sources are discovered, never created
/// (MVR-04) - a source exists only because the value it wraps already existed on disk.
/// </summary>
public interface IVersionSource
{
    /// <summary>What kind of location this is, for the run log. e.g. "csproj", "podspec".</summary>
    public string Kind { get; }

    /// <summary>Folder-root-relative path of the file this source reads and writes.</summary>
    public string Location { get; }

    /// <summary>False for sources EasySemVer reads for seeding but must not modify (e.g. git tags).</summary>
    public bool IsWritable { get; }

    /// <summary>The version currently recorded here, or null if absent or unparseable (MVR-03).</summary>
    public Version? Read();

    /// <summary>Replaces every occurrence of the version at this location. A no-op when not writable.</summary>
    public void Write(Version version);
}
