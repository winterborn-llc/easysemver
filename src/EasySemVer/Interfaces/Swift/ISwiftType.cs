namespace Winterborn.Tools.EasySemVer.Interfaces.Swift;

/// <summary>
/// A public nominal type in a Swift module. Each kind keeps its own concept - a protocol is not
/// "a class with a flag" - but they share a member surface, so the member rules are written once.
/// </summary>
public interface ISwiftType : ISwiftDeclaration
{
    /// <summary>"class" | "struct" | "enum" | "protocol" | "actor". A change here is S03.</summary>
    public string Kind { get; }

    public bool IsFinal { get; }

    public bool IsFrozen { get; }

    /// <summary>The superclass, or empty. Changing or losing it is S08.</summary>
    public string Superclass { get; }

    /// <summary>Protocols this type conforms to - or, for a protocol, inherits from.</summary>
    public IReadOnlyList<string> Conformances { get; }

    public IReadOnlyList<ISwiftGenericParameter> GenericParameters { get; }

    public IReadOnlyList<ISwiftInitializer> Initializers { get; }

    public IReadOnlyList<ISwiftFunction> Functions { get; }

    public IReadOnlyList<ISwiftProperty> Properties { get; }

    public IReadOnlyList<ISwiftSubscript> Subscripts { get; }
}
