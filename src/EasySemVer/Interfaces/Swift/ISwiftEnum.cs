namespace Winterborn.Library.EasySemVer.Interfaces.Swift;

public interface ISwiftEnum : ISwiftType
{
    public string RawValueType { get; }

    /// <summary>
    /// Adding a case here is Major, not Minor: a client switching exhaustively stops compiling
    /// (S18, SCL-01).
    /// </summary>
    public IReadOnlyList<ISwiftEnumCase> Cases { get; }
}
