namespace Winterborn.Library.EasySemVer.Interfaces;

public interface IPropertyList : IList<IProperty>
{
    public bool Contains(string name);
    
    public IProperty this[string name] { get; }
    
    public string[] Keys {get;}
}