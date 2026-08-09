namespace Winterborn.Tools.EasySemVer.Interfaces.Csharp;

/// <summary>
/// REN-04 - the comparison context for exactly one paired C# unit. It knows nothing about file
/// paths, project enumeration, or saving, which is what kills the working-directory dependency
/// the old context carried (CLS-07, G-13).
/// </summary>
public interface ICsharpSignaturesToCompare
{
    public ICsharpProject Older { get; }

    public ICsharpProject Newer { get; }

    /// <summary>Classes present on both sides (CLS-02); member rules see only these.</summary>
    public ICsharpClassHistory[] ClassHistory { get; }
}
