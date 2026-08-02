namespace Winterborn.Library.EasySemVer.Interfaces.Swift;

public interface ISwiftParameter
{
    /// <summary>The argument label as callers write it, or "_" when there is none.</summary>
    public string Label { get; }

    public string InternalName { get; }

    public string Type { get; }

    /// <summary>Removing a default is breaking (S31); adding one is not (S32).</summary>
    public bool HasDefault { get; }

    public bool IsInout { get; }

    public bool IsVariadic { get; }

    /// <summary>"", "borrowing", "consuming", or "inout" - the parameter's ownership (S33).</summary>
    public string Ownership { get; }
}
