namespace Yamamari.Library.AutoVersion.Extensions;

internal static class ExtendVersion
{
    internal static Version GetNextIncrement(this Version version, VersionType versionType)
    {
        var versionList = new VersionList(version);
        versionList.Increment(versionType);
        var next = versionList.ToVersion();
        return next;
    }
}