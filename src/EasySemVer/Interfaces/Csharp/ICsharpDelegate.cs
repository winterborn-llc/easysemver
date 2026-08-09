namespace Winterborn.Tools.EasySemVer.Interfaces.Csharp;

public interface ICsharpDelegate : ICsharpType
{
    public string ReturnType { get; }

    public IReadOnlyList<ICsharpMethodParameter> Parameters { get; }
}
