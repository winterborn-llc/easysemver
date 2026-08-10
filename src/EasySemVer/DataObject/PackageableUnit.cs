using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.DataObject;

/// <inheritdoc cref="IPackageableUnit"/>
[DebuggerDisplay("{LanguageId} {UnitId}")]
public class PackageableUnit : IPackageableUnit
{
    public string LanguageId { get; init; } = string.Empty;

    public string UnitId { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string RelativePath { get; init; } = string.Empty;

    public string UnitKind { get; init; } = string.Empty;

    public IReadOnlyList<IVersionSource> VersionSources { get; init; } = [];

    public object? Signature { get; set; }

    /// <inheritdoc/>
    public bool HasPublicApiSurface { get; set; } = true;

    /// <summary>The identity used for pairing and for baseline ordering (ML-03, BAS-04).</summary>
    public static string GetSortKey(IPackageableUnit unit)
    {
        return $"{unit.LanguageId} {unit.UnitId}";
    }
}
