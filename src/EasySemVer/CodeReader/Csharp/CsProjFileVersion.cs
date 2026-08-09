using System.Xml;
using Winterborn.Tools.EasySemVer.Settings;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.CodeReader.Csharp;

/// <summary>
/// VER-06 - a project's version is the highest of the first occurrence of each of
/// AssemblyVersion, PackageVersion and FileVersion, or 0.0.0 if it declares none.
/// </summary>
[DebuggerDisplay("{Version}")]
internal class CsProjFileVersion
{
    internal Version Version { get; }

    internal CsProjFileVersion(string inputXml)
    {
        this.Version = GetVersionFromXml(inputXml);
    }

    private static Version GetVersionFromXml(string inputXml)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(inputXml);
        var version = new Version();
        foreach (var propertyPreference in MagicValues.VersionPropertyNames)
        {
            var elements = xmlDoc.GetElementsByTagName(propertyPreference);
            var value = elements[0]?.InnerText;
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            // MVR-03: a value nobody can parse is skipped with a warning, not a failed run.
            if (!Version.TryParse(value, out var specificVersion))
            {
                Log.WriteLine($"Skipping unparseable {propertyPreference} '{value}'");
                continue;
            }

            if (specificVersion < version)
            {
                continue;
            }

            version = specificVersion;
        }

        return version;
    }
}
