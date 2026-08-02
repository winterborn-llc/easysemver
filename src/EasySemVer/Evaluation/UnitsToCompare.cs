using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluation;

/// <inheritdoc cref="IUnitsToCompare"/>
internal class UnitsToCompare(
    IReadOnlyList<IPackageableUnit> older,
    IReadOnlyList<IPackageableUnit> newer) : IUnitsToCompare
{
    public IReadOnlyList<IPackageableUnit> Older { get; } = older;

    public IReadOnlyList<IPackageableUnit> Newer { get; } = newer;
}
