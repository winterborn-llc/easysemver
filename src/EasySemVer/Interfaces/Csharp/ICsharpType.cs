namespace Winterborn.Tools.EasySemVer.Interfaces.Csharp;

/// <summary>
/// What every C# type kind has in common. Each kind keeps its own concept - CSX-01 is explicit
/// that an interface is not "a class with a flag" - but they share a member surface, so the
/// member rules can be written once and apply everywhere.
/// </summary>
public interface ICsharpType
{
    /// <summary>Namespace-qualified name, "global::" stripped; nested types read Outer.Inner (SIG-04).</summary>
    public string Name { get; }

    /// <summary>"class" | "interface" | "struct" | "record" | "enum" | "delegate".</summary>
    public string Kind { get; }

    /// <summary>The containing type's name, or empty for a namespace-level type (R41).</summary>
    public string DeclaringType { get; }

    public bool IsStatic { get; }

    public bool IsAbstract { get; }

    public bool IsSealed { get; }

    /// <summary>The declared base type, or empty. Changing it is breaking (R32).</summary>
    public string BaseType { get; }

    public IReadOnlyList<string> ImplementedInterfaces { get; }

    public IReadOnlyList<ICsharpGenericParameter> GenericParameters { get; }

    public ICsharpMethodList Methods { get; }

    public ICsharpPropertyList Properties { get; }

    public IReadOnlyList<ICsharpField> Fields { get; }

    public IReadOnlyList<ICsharpEvent> Events { get; }
}
