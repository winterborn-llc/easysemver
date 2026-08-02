using System.Collections;
using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

public class CsharpMethodOverrides : List<CsharpMethodOverride>, ICsharpMethodOverrides
{
    public bool Contains(string methodSignature)
    {
        foreach (var candidate in (List<CsharpMethodOverride>)this)
        {
            if (candidate.GetMethodSignature() != methodSignature)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    IEnumerator<ICsharpMethodOverride> IEnumerable<ICsharpMethodOverride>.GetEnumerator()
    {
        foreach (var methodOverride in (List<CsharpMethodOverride>)this)
        {
            yield return methodOverride;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
