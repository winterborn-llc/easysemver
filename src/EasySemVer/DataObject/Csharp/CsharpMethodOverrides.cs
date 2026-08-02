using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.DataObject.Csharp;

internal class CsharpMethodOverrides : List<ICsharpMethodOverride>, ICsharpMethodOverrides
{
    public CsharpMethodOverrides()
    {
    }

    internal CsharpMethodOverrides(IEnumerable<ICsharpMethodOverride> items)
        : base(items)
    {
    }
    
    public static implicit operator CsharpMethodOverrides(
        ICsharpMethodOverride[] items) => new(items);

    public bool Contains(string methodSignature)
    {
        foreach(var signature in this)
        {
            if (signature.GetMethodSignature() == methodSignature)
            {
                return true;
            }
        }

        return false;
    }
}