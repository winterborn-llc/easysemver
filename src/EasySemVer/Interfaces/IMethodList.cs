namespace Winterborn.Library.EasySemVer.Interfaces;

public interface IMethodList : IList<IMethod>
{
    public bool Contains(string name);
    
    public IMethod this[string name] { get; }
    
    public string[] Keys { get; }
}