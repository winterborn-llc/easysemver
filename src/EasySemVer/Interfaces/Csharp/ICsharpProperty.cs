namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpProperty
{
    public string Name { get; }

    public string Type { get; }

    public bool IsReadable { get; }

    public bool IsWritable { get; }

    /// <summary>
    /// True when the setter is <c>init</c> rather than <c>set</c>. Recording this separately is
    /// what makes set-to-init detectable at all (R42, CSX-03).
    /// </summary>
    public bool IsInitOnly { get; }

    public bool IsStatic { get; }

    public bool IsRequired { get; }

    /// <summary>For an interface requirement: whether the interface supplies a body (R21).</summary>
    public bool HasDefaultImplementation { get; }
}
