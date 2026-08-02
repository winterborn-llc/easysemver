using Winterborn.Library.EasySemVer.DataObject;

namespace Winterborn.Library.EasySemVer.Interfaces;

/// <summary>
/// One independently shippable module of code - a csproj, a SwiftPM target, an Xcode target
/// (UNI-01). This is the whole of the neutral vocabulary for "some code we version": the core
/// knows a unit's identity, its language, and where its versions live, and nothing at all about
/// what is inside it.
/// </summary>
public interface IPackageableUnit
{
    public Language Language { get; }

    /// <summary>
    /// Stable across machines and checkouts, and free of absolute paths (ML-03). Renaming a unit
    /// therefore reads as remove + add.
    /// </summary>
    public string UnitId { get; }

    public string DisplayName { get; }

    /// <summary>Folder-root-relative, forward slashes, so the baseline stays portable (BAS-04).</summary>
    public string RelativePath { get; }

    /// <summary>"csproj" | "swiftpm-target" | "xcode-target".</summary>
    public string UnitKind { get; }

    public IReadOnlyList<IVersionSource> VersionSources { get; }

    /// <summary>
    /// The language's own native signature graph - an <c>ICsharpProject</c>, an <c>ISwiftModule</c>.
    /// Opaque here on purpose: only the owning provider ever looks inside it (ML-01).
    /// </summary>
    public object? Signature { get; set; }
}
