namespace Winterborn.Tools.EasySemVer.DataObject;

/// <summary>
/// Where a <see cref="Interfaces.IDiscoverVersionSources"/> is being asked to look.
/// </summary>
/// <param name="FolderRoot">
/// Absolute. Needed both for the sources that are scoped to the whole run rather than to a unit -
/// a git tag is one - and for turning an absolute hit back into the folder-root-relative path the
/// baseline and the report both require (BAS-04).
/// </param>
/// <param name="UnitPath">
/// Absolute path to the thing the unit is: the .csproj file, the directory holding a
/// Package.swift, the .xcodeproj bundle. Whether that is a file or a directory is the convention's
/// business, which is why <see cref="UnitKind"/> comes with it.
/// </param>
/// <param name="UnitKind">The discovering provider's own <see cref="IPackageableUnit.UnitKind"/>.</param>
public sealed record VersionSourceScope(string FolderRoot, string UnitPath, string UnitKind);
