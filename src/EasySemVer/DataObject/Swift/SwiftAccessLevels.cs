namespace Winterborn.Library.EasySemVer.DataObject.Swift;

/// <summary>
/// The only two access levels that reach the signature (SWE-02). "open" additionally permits
/// subclassing and overriding outside the module, which is what S04/S05 turn on.
/// </summary>
internal static class SwiftAccessLevels
{
    internal const string Public = "public";
    internal const string Open = "open";
}
