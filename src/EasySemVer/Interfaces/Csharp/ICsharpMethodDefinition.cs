namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

/// <summary>
/// One method symbol as extraction sees it, before overloads are grouped by name (SIG-07).
/// Purely an extraction-time intermediate; it is never persisted.
/// </summary>
public interface ICsharpMethodDefinition
{
    public string Name { get; }

    public string Type { get; }

    public ICsharpMethodOverride Inputs { get; }
}
