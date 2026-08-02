namespace Winterborn.Library.EasySemVer.Interfaces.Swift;

public interface ISwiftProperty : ISwiftDeclaration
{
    public string Type { get; }

    /// <summary>Losing the setter is breaking (S35); gaining one is not (S36).</summary>
    public bool IsSettable { get; }

    public bool IsStatic { get; }

    public bool IsMutating { get; }

    public bool IsAsync { get; }

    public bool Throws { get; }

    public bool HasDefaultImplementation { get; }
}
