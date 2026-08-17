using System.Xml.Linq;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Persistence;
using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Providers;

namespace Test.Persistence;

/// <summary>
/// TST-M4 - baseline v2 round-trips. This is the test that would have caught G-01: the old graph
/// could not be handed to XmlSerializer at all.
/// </summary>
public class TestBaselineFile
{
    private static IReadOnlyList<ILanguageProvider> Providers =>
        LanguageProviders.Create(new ProcessRunner());

    private static IPackageableUnit BuildPopulatedUnit()
    {
        var project = new CsharpProject("Widgets")
        {
            Classes =
            [
                new CsharpClass
                {
                    Name = "Widgets.Gadget",
                    Properties =
                    {
                        new CsharpProperty
                        {
                            Name = "Name",
                            Type = "string",
                            IsReadable = true,
                            IsWritable = true
                        }
                    },
                    Methods =
                    {
                        new CsharpMethod
                        {
                            MethodName = "Move",
                            MethodType = "void",
                            Overrides =
                            {
                                new CsharpMethodOverride(
                                    new CsharpMethodParameter
                                    {
                                        ParameterName = "distance",
                                        ParameterType = "System.Int32",
                                        IsRequired = true
                                    })
                            }
                        }
                    }
                }
            ]
        };

        return Units.Csharp("Widgets", project);
    }

    [Fact]
    public void PopulatedUnitArrayRoundTripsToIdenticalXml()
    {
        var units = new[] { BuildPopulatedUnit() };

        var first = BaselineFile.BuildDocument(units, Providers).ToString();
        var readBack = BaselineFile.ReadDocument(XDocument.Parse(first), Providers);
        var second = BaselineFile.BuildDocument(readBack, Providers).ToString();

        Assert.Equal(first, second);
    }

    [Fact]
    public void RoundTripPreservesTheSignatureItself()
    {
        var units = new[] { BuildPopulatedUnit() };
        var document = BaselineFile.BuildDocument(units, Providers);

        var readBack = BaselineFile.ReadDocument(document, Providers);

        var unit = Assert.Single(readBack);
        Assert.Equal(CsharpLanguageProvider.CsharpLanguageId, unit.LanguageId);
        Assert.Equal("Widgets", unit.UnitId);
        Assert.Equal("csproj", unit.UnitKind);
        var project = Assert.IsType<CsharpProject>(unit.Signature);
        var projectClass = Assert.Single(project.Classes);
        Assert.Equal("Widgets.Gadget", projectClass.Name);
        Assert.True(projectClass.Methods.Contains("Move"));
        Assert.Equal("void", projectClass.Methods["Move"].MethodType);
        Assert.True(projectClass.Properties.Contains("Name"));
        Assert.True(projectClass.Properties["Name"].IsWritable);
    }

    /// <summary>BAS-01 - the document is a flat array of units, sorted by (Language, UnitId).</summary>
    [Fact]
    public void UnitsAreWrittenSortedByLanguageAndId()
    {
        IPackageableUnit[] units =
        [
            Units.Csharp("Widgets", new CsharpProject("Widgets")),
            Units.Csharp("Gadgets", new CsharpProject("Gadgets"))
        ];

        var document = BaselineFile.BuildDocument(units, Providers);

        var ids = document.Root!
            .Elements("Unit")
            .Select(e => e.Attribute("unitId")!.Value)
            .ToArray();
        Assert.Equal(["Gadgets", "Widgets"], ids);
    }

    /// <summary>
    /// UNI-04 - the baseline is signature history, and a unit with no API surface has none. It was
    /// never extracted, so writing it would persist an empty graph that the next run reads back as
    /// "everything in it was removed" - a Major, every release, forever.
    /// </summary>
    [Fact]
    public void AUnitWithNoApiSurfaceIsNotWritten()
    {
        IPackageableUnit[] units =
        [
            Units.Csharp("Widgets", new CsharpProject("Widgets")),
            Units.Csharp("Tests", new CsharpProject("Tests"), hasPublicApiSurface: false)
        ];

        var document = BaselineFile.BuildDocument(units, Providers);

        var ids = document.Root!
            .Elements("Unit")
            .Select(e => e.Attribute("unitId")!.Value)
            .ToArray();
        Assert.Equal(["Widgets"], ids);
    }

    [Fact]
    public void RootCarriesTheFormatVersion()
    {
        var document = BaselineFile.BuildDocument([], Providers);

        Assert.Equal("EasySemVer", document.Root!.Name.LocalName);
        // Hardcoded rather than read from MagicValues: bumping the format is a deliberate act that
        // invalidates every baseline in the wild, so it should have to be made twice.
        Assert.Equal("4", document.Root.Attribute("formatVersion")!.Value);
    }

    /// <summary>
    /// BAS-07 - a unit whose signature was written by a generation its provider no longer speaks
    /// is dropped, and everything around it is kept. This is the whole reason the per-unit version
    /// exists rather than another bump of the file's: Swift changing how it words a signature must
    /// not re-seed a repository's C# history, and must not re-seed anything at all in a repository
    /// with no Swift in it.
    /// </summary>
    [Fact]
    public void AUnitWrittenByAnotherSignatureGenerationIsDroppedAndTheRestIsKept()
    {
        var baseline = XDocument.Parse(
            """
            <EasySemVer formatVersion="4">
               <Unit language="swift" unitId="Pkg:Widgets" unitKind="swiftpm-target" path="Pkg" signatureVersion="1">
                  <SwiftModule name="Widgets" />
               </Unit>
               <Unit language="swift" unitId="Pkg:Gears" unitKind="swiftpm-target" path="Pkg" signatureVersion="2">
                  <SwiftModule name="Gears" />
               </Unit>
               <Unit language="csharp" unitId="Widgets" unitKind="csproj" path="Widgets.csproj">
                  <CsharpProject name="Widgets" />
               </Unit>
            </EasySemVer>
            """);

        var units = BaselineFile.ReadDocument(baseline, Providers);

        // The C# unit predates signature versions entirely and is read as the first generation,
        // which is still what the C# provider writes - so it survives untouched.
        Assert.Equal(["Pkg:Gears", "Widgets"], units.Select(u => u.UnitId).Order());
    }

    /// <summary>BAS-03 - an unknown or absent format version is unreadable, never guessed at.</summary>
    [Theory]
    [InlineData("<EasySemVer formatVersion=\"1\" />")]
    // A version-2 baseline holds the metadata types extraction used to record before SIG-03; a
    // version-3 run never produces them, so diffing the two would read their absence as Major.
    [InlineData("<EasySemVer formatVersion=\"2\" />")]
    [InlineData("<EasySemVer />")]
    [InlineData("<Solution />")]
    public void UnusableBaselineIsRejected(string xml)
    {
        Assert.ThrowsAny<Exception>(
            () => BaselineFile.ReadDocument(XDocument.Parse(xml), Providers));
    }

    /// <summary>
    /// BAS-05 / PER-04 - an unreadable file on disk fails the run rather than degrading to an
    /// empty baseline, which would publish a verdict with no history behind it.
    /// </summary>
    [Theory]
    [InlineData("this is not xml at all")]
    [InlineData("<EasySemVer formatVersion=\"3\" />")]
    public void UnreadableFileOnDiskFailsTheRun(string content)
    {
        var folderRoot = Directory.CreateTempSubdirectory("easysemver-baseline").FullName;
        try
        {
            File.WriteAllText(BaselineFile.GetPath(folderRoot), content);

            var exception = Assert.Throws<InvalidDataException>(
                () => BaselineFile.Read(folderRoot, Providers));

            // The path, so the file can be found, and the way out, so nobody has to guess at it.
            Assert.Contains(BaselineFile.GetPath(folderRoot), exception.Message);
            Assert.Contains("delete it", exception.Message);
        }
        finally
        {
            Directory.Delete(folderRoot, recursive: true);
        }
    }

    [Fact]
    public void MissingFileIsAnEmptyBaseline()
    {
        var folderRoot = Directory.CreateTempSubdirectory("easysemver-baseline").FullName;
        try
        {
            Assert.Empty(BaselineFile.Read(folderRoot, Providers));
        }
        finally
        {
            Directory.Delete(folderRoot, recursive: true);
        }
    }

    /// <summary>BAS-06 - written via a temporary file, leaving no debris behind.</summary>
    [Fact]
    public void WriteLeavesOnlyTheBaselineBehind()
    {
        var folderRoot = Directory.CreateTempSubdirectory("easysemver-baseline").FullName;
        try
        {
            BaselineFile.Write(folderRoot, [BuildPopulatedUnit()], Providers);

            var written = Directory.GetFiles(folderRoot).Select(f => Path.GetFileName(f)!).ToArray();
            Assert.Equal(["EasySemVer.xml"], written);
            Assert.DoesNotContain(folderRoot, File.ReadAllText(BaselineFile.GetPath(folderRoot)));
        }
        finally
        {
            Directory.Delete(folderRoot, recursive: true);
        }
    }
}
