using System.Collections;
using Winterborn.Tools.EasySemVer.Extensions;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.DataObject.Csharp;

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
