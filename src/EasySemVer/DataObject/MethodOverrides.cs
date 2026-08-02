using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.DataObject;

internal class MethodOverrides : List<IMethodOverride>, IMethodOverrides
{
    public MethodOverrides()
    {
    }

    internal MethodOverrides(IEnumerable<IMethodOverride> items)
        : base(items)
    {
    }
    
    public static implicit operator MethodOverrides(
        IMethodOverride[] items) => new(items);

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