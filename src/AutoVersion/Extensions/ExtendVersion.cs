using System;

namespace Yamamari.Library.VersionCounter.Extensions;

internal static class ExtendVersion
{
    internal static Version GetNextIncrement(this Version version, bool isSignificant = false)
    {
        var versionList = new VersionList(version);
        versionList.Increment(isSignificant);
        var next = versionList.ToVersion();
        return next;
    }
}