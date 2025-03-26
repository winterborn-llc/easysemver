namespace Yamamari.Library.AutoVersion;

internal static class MagicValues
{
    internal const string AutoVersionPropertyName = "LatestAutoVersionSignature";
    
    internal static readonly string[] VersionPropertyNames = 
        { "AssemblyVersion", "PackageVersion", "FileVersion" };
}