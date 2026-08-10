using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Evaluation;

/// <summary>One unit as the baseline recorded it, alongside the same unit as it is now (NCL-03).</summary>
[DebuggerDisplay("{Newer.LanguageId} {Newer.UnitId}")]
internal class UnitPair(IPackageableUnit older, IPackageableUnit newer)
{
    internal IPackageableUnit Older { get; } = older;

    internal IPackageableUnit Newer { get; } = newer;
}
