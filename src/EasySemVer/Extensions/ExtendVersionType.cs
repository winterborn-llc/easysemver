using Winterborn.Library.EasySemVer.DataObject;

namespace Winterborn.Library.EasySemVer.Extensions;

internal static class ExtendVersionType
{
    /// <summary>
    /// ML-05 / CLS-03 - the run's verdict is the highest impact anything reported, Major beating
    /// Minor beating Patch. The enum happens to be declared most-significant-first, so "highest
    /// impact" is the lower ordinal; that is an implementation detail and stays in here.
    /// </summary>
    internal static VersionType GetHigherImpact(this VersionType current, VersionType candidate)
    {
        return candidate < current ? candidate : current;
    }
}
