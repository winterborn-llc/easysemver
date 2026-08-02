namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

/// <summary>
/// One overload: its ordered parameter list plus the facets that belong to the overload rather
/// than to the method name. The return type lives here so a change on the second overload cannot
/// hide behind the first (CSX-04, closes G-14).
/// </summary>
public interface ICsharpMethodOverride
{
    public IReadOnlyList<ICsharpMethodParameter> Parameters { get; }

    public string ReturnType { get; }

    public bool IsStatic { get; }

    public bool IsVirtual { get; }

    public bool IsAbstract { get; }

    public bool IsOverride { get; }

    public bool IsSealed { get; }

    /// <summary>For an interface requirement: whether the interface supplies a body (R21).</summary>
    public bool HasDefaultImplementation { get; }

    public IReadOnlyList<ICsharpGenericParameter> GenericParameters { get; }
}
