namespace Winterborn.Tools.EasySemVer.Interfaces.Swift;

public interface ISwiftSubscript : ISwiftDeclaration
{
    public IReadOnlyList<ISwiftParameter> Parameters { get; }

    public string ReturnType { get; }

    public bool IsSettable { get; }

    public bool IsStatic { get; }
}
