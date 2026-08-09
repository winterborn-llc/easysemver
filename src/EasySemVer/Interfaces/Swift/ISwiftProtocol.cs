namespace Winterborn.Tools.EasySemVer.Interfaces.Swift;

public interface ISwiftProtocol : ISwiftType
{
    public IReadOnlyList<string> AssociatedTypes { get; }
}
