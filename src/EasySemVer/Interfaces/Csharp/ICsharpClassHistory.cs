namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpClassHistory
{
    public ICsharpClass Older { get; }

    public ICsharpClass Newer { get; }
}
