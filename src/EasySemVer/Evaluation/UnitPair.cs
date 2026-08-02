using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluation;

/// <summary>One unit as the baseline recorded it, alongside the same unit as it is now (NCL-03).</summary>
[DebuggerDisplay("{Newer.Language} {Newer.UnitId}")]
internal class UnitPair(IPackageableUnit older, IPackageableUnit newer)
{
    internal IPackageableUnit Older { get; } = older;

    internal IPackageableUnit Newer { get; } = newer;
}
