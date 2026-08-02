using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.DataObject;

public class PropertyList : List<IProperty>, IPropertyList
{
    private readonly Dictionary<string,IProperty> _map = new();

    public new void Add(IProperty property)
    {
        this._map.Add(property.Name, property);
        base.Add(property);
    }
    
    public bool Contains(string name)
    {
        return this._map.ContainsKey(name);
    }

    public IProperty this[string name] => this._map[name];

    public string[] Keys => this._map.Keys.ToArray();
}