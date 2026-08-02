using System.Xml;
using Winterborn.Library.EasySemVer.Interfaces;
using Version = Winterborn.Library.EasySemVer.DataObject.Version;

namespace Winterborn.Library.EasySemVer.CodeReader.Swift;

/// <summary>
/// MVR-03 - the Info.plist CFBundleShortVersionString row. CFBundleVersion is deliberately left
/// alone: it is a build counter, usually a bare integer VER-01 cannot parse (MVR-06, §20 O-01).
/// </summary>
internal class InfoPlistVersionSource(string plistPath, string relativePath) : IVersionSource
{
    private const string ShortVersionKey = "CFBundleShortVersionString";

    internal static bool HasShortVersionString(string plistText)
    {
        return plistText.Contains(ShortVersionKey, StringComparison.Ordinal);
    }

    public string Kind => "info-plist";

    public string Location { get; } = relativePath;

    public bool IsWritable => true;

    public Version? Read()
    {
        var document = Load(plistPath);
        var value = FindValueElement(document)?.InnerText;
        if (value == null || value.Length < 1)
        {
            return null;
        }

        if (Version.TryParse(value, out var version))
        {
            return version;
        }

        // A plist that interpolates a build setting, e.g. $(MARKETING_VERSION), is read-skipped;
        // the setting itself is a version source in its own right.
        Log.WriteLine($"Skipping unparseable {ShortVersionKey} '{value}' in {this.Location}");
        return null;
    }

    public void Write(Version version)
    {
        var document = Load(plistPath);
        var value = FindValueElement(document);
        if (value == null)
        {
            return;
        }

        // Only a literal is replaced; a plist pointing at a build setting keeps pointing at it.
        if (!Version.TryParse(value.InnerText, out _))
        {
            return;
        }

        value.InnerText = version;
        document.Save(plistPath);
    }

    private static XmlDocument Load(string path)
    {
        var document = new XmlDocument
        {
            // Apple's plists carry a DOCTYPE pointing at apple.com; resolving it would make the
            // run depend on network access.
            XmlResolver = null
        };
        document.Load(path);
        return document;
    }

    /// <summary>A plist dict is a flat key/value sequence: the value is the element after the key.</summary>
    private static XmlNode? FindValueElement(XmlDocument document)
    {
        var keys = document.GetElementsByTagName("key");
        for (var i = 0; i < keys.Count; i++)
        {
            if (keys[i]?.InnerText != ShortVersionKey)
            {
                continue;
            }

            return keys[i]?.NextSibling;
        }

        return null;
    }
}
