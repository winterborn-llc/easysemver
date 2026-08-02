namespace Winterborn.Library.EasySemVer.Interfaces.Swift;

/// <summary>
/// An extension on a type declared in another module (SWM-02). Extensions on the module's own
/// types are folded into the type itself, because that is how a Swift developer reads them.
/// </summary>
public interface ISwiftExtension
{
    public string ExtendedType { get; }

    public string Constraints { get; }

    public IReadOnlyList<string> AddedConformances { get; }

    public IReadOnlyList<ISwiftFunction> Functions { get; }

    public IReadOnlyList<ISwiftProperty> Properties { get; }

    public IReadOnlyList<ISwiftSubscript> Subscripts { get; }

    /// <summary>Identity: the extended type plus the constraints that narrow it.</summary>
    public string Key { get; }
}
