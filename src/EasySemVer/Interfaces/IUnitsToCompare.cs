namespace Winterborn.Tools.EasySemVer.Interfaces;

/// <summary>
/// The two unit lists a run compares: last run's baseline and this run's discovery. This is the
/// only comparison context the neutral existence rules (§7) ever see.
/// </summary>
public interface IUnitsToCompare
{
    public IReadOnlyList<IPackageableUnit> Older { get; }

    public IReadOnlyList<IPackageableUnit> Newer { get; }
}
