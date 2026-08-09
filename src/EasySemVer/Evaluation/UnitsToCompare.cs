using Winterborn.Tools.EasySemVer.Interfaces;

namespace Winterborn.Tools.EasySemVer.Evaluation;

/// <inheritdoc cref="IUnitsToCompare"/>
internal class UnitsToCompare(
    IReadOnlyList<IPackageableUnit> older,
    IReadOnlyList<IPackageableUnit> newer) : IUnitsToCompare
{
    public IReadOnlyList<IPackageableUnit> Older { get; } = older;

    public IReadOnlyList<IPackageableUnit> Newer { get; } = newer;
}
