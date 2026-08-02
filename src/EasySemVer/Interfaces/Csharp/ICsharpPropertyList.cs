namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpPropertyList : IList<ICsharpProperty>
{
    public bool Contains(string name);
    
    public ICsharpProperty this[string name] { get; }
    
    public string[] Keys {get;}
}