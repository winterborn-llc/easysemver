namespace Winterborn.Library.EasySemVer.Interfaces.Swift;

/// <summary>A func, whether a method, a protocol requirement, or a global (SWM-01).</summary>
public interface ISwiftFunction : ISwiftDeclaration
{
    public IReadOnlyList<ISwiftParameter> Parameters { get; }

    public string ReturnType { get; }

    public bool IsStatic { get; }

    public bool IsMutating { get; }

    public bool IsAsync { get; }

    public bool Throws { get; }

    public bool IsFinal { get; }

    public IReadOnlyList<ISwiftGenericParameter> GenericParameters { get; }

    /// <summary>For a protocol requirement: whether an extension supplies a body (S21).</summary>
    public bool HasDefaultImplementation { get; }

    /// <summary>The where clause of the extension this member came from, if any (SWM-02).</summary>
    public string ExtensionConstraints { get; }
}
