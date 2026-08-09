using System.Xml;
using System.Xml.Linq;

namespace Winterborn.Tools.EasySemVer.CodeReader.Csharp;

/// <summary>
/// UNI-04 - whether one .csproj is a test project, decided from the project file alone.
/// <para>
/// A test project is versioned like any other unit but contributes no API surface: renaming a
/// <c>[Fact]</c> method is not a breaking change to anything, and counting it as one cut this
/// repository's own v17.0.0 off the back of two renamed test methods.
/// </para>
/// <para>
/// The signals are MSBuild's own, not a convention this tool invented. There is no name matching -
/// a project called <c>Tests</c> that is not one, and one called <c>Fixtures</c> that is, are both
/// commonplace, and a heuristic on the filename would get each of them wrong.
/// </para>
/// </summary>
internal static class CsProjTestProject
{
    private const string IsTestProjectPropertyName = "IsTestProject";

    private const string PackageReferenceElementName = "PackageReference";

    /// <summary>
    /// Matched as prefixes, so that <c>xunit.v3</c>, <c>NUnit3TestAdapter</c> and
    /// <c>MSTest.TestFramework</c> are all recognised without listing every package each framework
    /// has ever shipped. <c>Microsoft.NET.Test.Sdk</c> is the one every runnable test project needs
    /// whichever framework it uses; the rest catch a project that has the framework but not yet the
    /// SDK reference.
    /// </summary>
    private static readonly string[] TestPackagePrefixes =
    [
        "Microsoft.NET.Test.Sdk",
        "xunit",
        "NUnit",
        "MSTest"
    ];

    /// <summary>
    /// Reads the project from disk. An unreadable file is not this type's problem - the version
    /// source and the signature builder each report on it in their own terms - so it answers false
    /// and leaves the unit with the surface it would have had before UNI-04 existed.
    /// </summary>
    internal static bool Read(string projectFilePath)
    {
        try
        {
            return IsTestProject(File.ReadAllText(projectFilePath));
        }
        catch (IOException e)
        {
            Log.WriteLine($"Could not read {projectFilePath} to check whether it is a test project: {e.Message}");
            return false;
        }
        catch (UnauthorizedAccessException e)
        {
            Log.WriteLine($"Could not read {projectFilePath} to check whether it is a test project: {e.Message}");
            return false;
        }
    }

    internal static bool IsTestProject(string projectXml)
    {
        XDocument document;
        try
        {
            document = XDocument.Parse(projectXml);
        }
        catch (XmlException)
        {
            return false;
        }

        // An explicit property wins in both directions, and is the escape hatch: a library that
        // references a test framework on purpose - assertion helpers, a testing extension package -
        // declares `<IsTestProject>false</IsTestProject>` and keeps its surface. Nothing here is a
        // setting this tool defines; it is the property `dotnet test` already reads.
        var declared = FindDeclaredIsTestProject(document);
        if (declared != null)
        {
            return declared.Value;
        }

        return HasTestPackageReference(document);
    }

    /// <summary>
    /// Element names are matched on their local name so that a legacy non-SDK project, whose
    /// elements sit in the 2003 MSBuild namespace, reads the same as an SDK-style one.
    /// </summary>
    private static bool? FindDeclaredIsTestProject(XDocument document)
    {
        foreach (var element in document.Descendants())
        {
            if (element.Name.LocalName != IsTestProjectPropertyName)
            {
                continue;
            }

            if (bool.TryParse(element.Value.Trim(), out var declared))
            {
                return declared;
            }
        }

        return null;
    }

    private static bool HasTestPackageReference(XDocument document)
    {
        foreach (var element in document.Descendants())
        {
            if (element.Name.LocalName != PackageReferenceElementName)
            {
                continue;
            }

            var include = element.Attribute("Include")?.Value ?? string.Empty;
            foreach (var prefix in TestPackagePrefixes)
            {
                if (include.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
