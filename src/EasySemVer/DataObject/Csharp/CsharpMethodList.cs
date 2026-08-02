using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

public class CsharpMethodList : List<ICsharpMethod>, ICsharpMethodList
{
    private readonly Dictionary<string,ICsharpMethod> _map = new();

    public new void Add(ICsharpMethod method)
    {
        this._map.Add(method.MethodName, method);
        base.Add(method);
    }
    
    public bool Contains(string name)
    {
        return this._map.ContainsKey(name);
    }

    public ICsharpMethod this[string name] => this._map[name];

    public string[] Keys => this._map.Keys.ToArray();
}