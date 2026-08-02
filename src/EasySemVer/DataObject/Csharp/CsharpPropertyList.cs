using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

public class CsharpPropertyList : List<ICsharpProperty>, ICsharpPropertyList
{
    private readonly Dictionary<string,ICsharpProperty> _map = new();

    public new void Add(ICsharpProperty property)
    {
        this._map.Add(property.Name, property);
        base.Add(property);
    }
    
    public bool Contains(string name)
    {
        return this._map.ContainsKey(name);
    }

    public ICsharpProperty this[string name] => this._map[name];

    public string[] Keys => this._map.Keys.ToArray();
}