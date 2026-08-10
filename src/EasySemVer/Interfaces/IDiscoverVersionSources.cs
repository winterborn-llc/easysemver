using Winterborn.Tools.EasySemVer.DataObject;

namespace Winterborn.Tools.EasySemVer.Interfaces;

/// <summary>
/// Finds one kind of version location inside a unit (MVR-03). One implementation per convention -
/// a csproj property group, a podspec, an Xcode build setting, a package.json - registered
/// alongside the others, so teaching EasySemVer a new place a version lives costs a class and a
/// registration line and touches no provider.
/// <para>
/// Sources are discovered, never created (MVR-04): an implementation returns nothing when the
/// value it wraps is not already on disk, which is why every one of them probes the file rather
/// than trusting the filename.
/// </para>
/// </summary>
public interface IDiscoverVersionSources
{
    /// <inheritdoc cref="IPackageableUnit.LanguageId"/>
    public string LanguageId { get; }

    /// <summary>
    /// The <see cref="IPackageableUnit.UnitKind"/> values this convention applies to. A podspec
    /// belongs to a SwiftPM package and an Info.plist to an Xcode target, and neither should be
    /// hunted for where it cannot be.
    /// </summary>
    public IReadOnlyList<string> UnitKinds { get; }

    public IEnumerable<IVersionSource> Discover(VersionSourceScope scope);
}
