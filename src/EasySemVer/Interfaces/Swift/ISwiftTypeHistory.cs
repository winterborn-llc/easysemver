namespace Winterborn.Tools.EasySemVer.Interfaces.Swift;

/// <summary>One type as the baseline recorded it, alongside the same type now.</summary>
public interface ISwiftTypeHistory
{
    public ISwiftType Older { get; }

    public ISwiftType Newer { get; }
}
