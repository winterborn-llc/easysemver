namespace Winterborn.Library.EasySemVer.Interfaces.Csharp;

public interface ICsharpMethodOverrides : IList<ICsharpMethodOverride>
{
    public bool Contains(string methodSignature);
}