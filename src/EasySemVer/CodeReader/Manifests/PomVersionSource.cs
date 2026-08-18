using System.Xml.Linq;
using Winterborn.Tools.EasySemVer.Interfaces;
using Version = Winterborn.Tools.EasySemVer.DataObject.Version;

namespace Winterborn.Tools.EasySemVer.CodeReader.Manifests;

/// <summary>
/// MVR-03 for a Maven pom.xml. This is the one manifest in the version-sync set that is read as XML
/// rather than matched with a pattern, because it is the one where a pattern is genuinely unsafe:
/// a pom mentions <c>&lt;version&gt;</c> for its parent and for every dependency, and rewriting one
/// of those would pin somebody else's artifact to this repository's number.
/// <para>
/// Only <c>/project/version</c> is read or written - the project's own, a direct child of the root.
/// A module that inherits its version from a parent has no such element, so it is read-skipped and
/// write-skipped (MVR-04): the parent is where that version lives, and the parent is its own unit.
/// </para>
/// </summary>
internal class PomVersionSource(string pomPath, string relativePath) : IVersionSource
{
    private const string ProjectVersionElement = "version";

    /// <summary>MVR-04 - probe before constructing, so a child module contributes no source.</summary>
    internal static bool HasOwnVersion(string pomText)
    {
        return FindOwnVersionElement(Parse(pomText)) != null;
    }

    private static XDocument? Parse(string pomText)
    {
        try
        {
            return XDocument.Parse(pomText, LoadOptions.PreserveWhitespace);
        }
        catch (System.Xml.XmlException)
        {
            return null;
        }
    }

    /// <summary>
    /// The root's direct <c>version</c> child, in whatever namespace the pom declares. Maven poms
    /// are conventionally in the Maven POM namespace but are valid without one, and matching on
    /// local name rather than the qualified name handles both without hard-coding a URI that a
    /// future schema version would change.
    /// </summary>
    private static XElement? FindOwnVersionElement(XDocument? document)
    {
        var root = document?.Root;
        if (root == null)
        {
            return null;
        }

        foreach (var child in root.Elements())
        {
            if (child.Name.LocalName != ProjectVersionElement)
            {
                continue;
            }

            return child;
        }

        return null;
    }

    public string Kind => "pom";

    public string Location { get; } = relativePath;

    public bool IsWritable => true;

    public Version? Read()
    {
        var element = FindOwnVersionElement(Parse(File.ReadAllText(pomPath)));
        if (element == null)
        {
            return null;
        }

        var text = element.Value.Trim();
        if (Version.TryParse(text, out var version))
        {
            return version;
        }

        // A Maven version is routinely "1.2.3-SNAPSHOT", which is not three numbers. Skipping with
        // a warning is MVR-03; failing the run over somebody's snapshot build would not be.
        Log.WriteLine($"Skipping unparseable version '{text}' in {this.Location}");
        return null;
    }

    public void Write(Version version)
    {
        var document = Parse(File.ReadAllText(pomPath));
        var element = FindOwnVersionElement(document);
        if (element == null || document == null)
        {
            return;
        }

        element.Value = version.ToString();

        // PreserveWhitespace on both ends: a pom is a file people read and review, and reflowing it
        // to XDocument's default indentation would produce a diff nobody asked for around the one
        // line that changed.
        using var writer = System.Xml.XmlWriter.Create(
            pomPath,
            new System.Xml.XmlWriterSettings { OmitXmlDeclaration = document.Declaration == null });
        document.Save(writer);
    }
}
