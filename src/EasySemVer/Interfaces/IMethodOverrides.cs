namespace Winterborn.Library.EasySemVer.Interfaces;

public interface IMethodOverrides : IList<IMethodOverride>
{
    public bool Contains(string methodSignature);
}