namespace Winterborn.Tools.EasySemVer.Interfaces.Swift;

public interface ISwiftEnumCase : ISwiftDeclaration
{
    /// <summary>The case's associated values, label and type in order.</summary>
    public IReadOnlyList<ISwiftParameter> AssociatedValues { get; }

    public string RawValue { get; }
}
