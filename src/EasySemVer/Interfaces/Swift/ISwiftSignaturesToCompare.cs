namespace Winterborn.Library.EasySemVer.Interfaces.Swift;

/// <summary>
/// The comparison context for exactly one paired Swift module. Swift's equivalent of
/// ICsharpSignaturesToCompare, and deliberately sharing nothing with it (ML-01).
/// </summary>
public interface ISwiftSignaturesToCompare
{
    public ISwiftModule Older { get; }

    public ISwiftModule Newer { get; }

    /// <summary>Types present on both sides; member rules see only these.</summary>
    public ISwiftTypeHistory[] TypeHistory { get; }
}
