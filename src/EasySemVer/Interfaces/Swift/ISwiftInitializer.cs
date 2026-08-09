namespace Winterborn.Tools.EasySemVer.Interfaces.Swift;

public interface ISwiftInitializer : ISwiftDeclaration
{
    public IReadOnlyList<ISwiftParameter> Parameters { get; }

    public bool IsFailable { get; }

    public bool IsRequired { get; }

    public bool IsConvenience { get; }

    public bool IsAsync { get; }

    public bool Throws { get; }
}
