using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;
using Winterborn.Tools.EasySemVer.Process;
using Winterborn.Tools.EasySemVer.Providers;

namespace Test.Vb;

/// <summary>
/// VB-08 - a VB unit is named for VB in the baseline, even though its model is C#'s (VB-01).
/// <para>
/// The rename was free exactly once, before any repository had a VB baseline. These assert it
/// happened and that it round-trips, because a write that no read understands would classify every
/// VB unit as removed on the following run.
/// </para>
/// </summary>
public class TestVbBaseline
{
    private readonly ILanguageProvider _provider =
        LanguageProviders.Find(LanguageProviders.Create(new ProcessRunner()), "vb")!;

    private static IPackageableUnit UnitWithSignature()
    {
        var project = new CsharpProject("Widgets");
        project.Classes.Add(new CsharpClass { Name = "Widgets.Gadget" });

        return new Winterborn.Tools.EasySemVer.DataObject.PackageableUnit
        {
            LanguageId = "vb",
            UnitId = "Widgets",
            DisplayName = "Widgets.vbproj",
            RelativePath = "src/Widgets/Widgets.vbproj",
            UnitKind = "vbproj",
            Signature = project
        };
    }

    [Fact]
    public void AVbSignatureIsWrittenUnderItsOwnName()
    {
        var element = this._provider.WriteSignature(UnitWithSignature());

        Assert.Equal("VisualBasicProject", element.Name.LocalName);
    }

    [Fact]
    public void AVbSignatureRoundTrips()
    {
        var written = this._provider.WriteSignature(UnitWithSignature());

        var read = Assert.IsType<CsharpProject>(this._provider.ReadSignature(written));

        Assert.Equal("Widgets", read.Name);
        Assert.Equal(["Widgets.Gadget"], read.Classes.Select(c => c.Name));
    }

    /// <summary>
    /// Reading must not rename the caller's element. The baseline is parsed once and handed to a
    /// provider per unit; mutating it in place would leave the document describing something it no
    /// longer says.
    /// </summary>
    [Fact]
    public void ReadingASignatureDoesNotRenameTheElementItWasGiven()
    {
        var written = this._provider.WriteSignature(UnitWithSignature());

        this._provider.ReadSignature(written);

        Assert.Equal("VisualBasicProject", written.Name.LocalName);
    }

    /// <summary>C# is untouched by this: renaming its element would re-seed every existing baseline.</summary>
    [Fact]
    public void CsharpKeepsItsOwnElementName()
    {
        var csharp = LanguageProviders.Find(LanguageProviders.Create(new ProcessRunner()), "csharp")!;
        var unit = UnitWithSignature();

        Assert.Equal("CsharpProject", csharp.WriteSignature(unit).Name.LocalName);
    }
}
