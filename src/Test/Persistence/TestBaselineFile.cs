using System.Xml.Linq;
using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
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
        Assert.Equal(Language.Csharp, unit.Language);
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

    [Fact]
    public void RootCarriesTheFormatVersion()
    {
        var document = BaselineFile.BuildDocument([], Providers);

        Assert.Equal("EasySemVer", document.Root!.Name.LocalName);
        // Hardcoded rather than read from MagicValues: bumping the format is a deliberate act that
        // invalidates every baseline in the wild, so it should have to be made twice.
        Assert.Equal("3", document.Root.Attribute("formatVersion")!.Value);
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

    /// <summary>BAS-05 / PER-04 - an unreadable file on disk degrades to an empty baseline.</summary>
    [Fact]
    public void UnreadableFileOnDiskDegradesToEmpty()
    {
        var folderRoot = Directory.CreateTempSubdirectory("easysemver-baseline").FullName;
        try
        {
            File.WriteAllText(BaselineFile.GetPath(folderRoot), "this is not xml at all");

            Assert.Empty(BaselineFile.Read(folderRoot, Providers));
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
