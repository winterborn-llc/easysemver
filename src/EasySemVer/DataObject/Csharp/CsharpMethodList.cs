using System.Collections;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

/// <summary>
/// A plain <see cref="List{T}"/> of concrete methods so XmlSerializer can round-trip it, with the
/// by-name lookups the rules use layered on top. Lookups scan rather than caching a dictionary:
/// a cache built in <c>Add</c> would silently go stale when the serializer populates the list
/// through the base class.
/// </summary>
public class CsharpMethodList : List<CsharpMethod>, ICsharpMethodList
{
    public bool Contains(string name)
    {
        return Find(this, name) != null;
    }

    public ICsharpMethod this[string name] =>
        Find(this, name)
        ?? throw new KeyNotFoundException($"No method named '{name}' is present.");

    public string[] Keys
    {
        get
        {
            var keys = new List<string>();
            foreach (var method in (List<CsharpMethod>)this)
            {
                keys.Add(method.MethodName);
            }

            return keys.ToArray();
        }
    }

    private static CsharpMethod? Find(List<CsharpMethod> methods, string name)
    {
        foreach (var method in methods)
        {
            if (method.MethodName != name)
            {
                continue;
            }

            return method;
        }

        return null;
    }

    IEnumerator<ICsharpMethod> IEnumerable<ICsharpMethod>.GetEnumerator()
    {
        foreach (var method in (List<CsharpMethod>)this)
        {
            yield return method;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
