using System.Collections;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.DataObject.Csharp;

/// <inheritdoc cref="CsharpMethodList"/>
public class CsharpPropertyList : List<CsharpProperty>, ICsharpPropertyList
{
    public bool Contains(string name)
    {
        return Find(this, name) != null;
    }

    public ICsharpProperty this[string name] =>
        Find(this, name)
        ?? throw new KeyNotFoundException($"No property named '{name}' is present.");

    public string[] Keys
    {
        get
        {
            var keys = new List<string>();
            foreach (var property in (List<CsharpProperty>)this)
            {
                keys.Add(property.Name);
            }

            return keys.ToArray();
        }
    }

    private static CsharpProperty? Find(List<CsharpProperty> properties, string name)
    {
        foreach (var property in properties)
        {
            if (property.Name != name)
            {
                continue;
            }

            return property;
        }

        return null;
    }

    IEnumerator<ICsharpProperty> IEnumerable<ICsharpProperty>.GetEnumerator()
    {
        foreach (var property in (List<CsharpProperty>)this)
        {
            yield return property;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
