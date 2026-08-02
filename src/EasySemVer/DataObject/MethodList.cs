using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.DataObject;

public class MethodList : List<IMethod>, IMethodList
{
    private readonly Dictionary<string,IMethod> _map = new();

    public new void Add(IMethod method)
    {
        this._map.Add(method.MethodName, method);
        base.Add(method);
    }
    
    public bool Contains(string name)
    {
        return this._map.ContainsKey(name);
    }

    public IMethod this[string name] => this._map[name];

    public string[] Keys => this._map.Keys.ToArray();
}