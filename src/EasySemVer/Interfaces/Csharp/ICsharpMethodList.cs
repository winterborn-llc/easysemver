namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpMethodList : IList<ICsharpMethod>
{
    public bool Contains(string name);
    
    public ICsharpMethod this[string name] { get; }
    
    public string[] Keys { get; }
}