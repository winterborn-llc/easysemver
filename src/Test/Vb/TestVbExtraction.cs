using Winterborn.Tools.EasySemVer.CodeReader.Vb;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test.Vb;

/// <summary>
/// VB-01 end to end over real Visual Basic source. What this is really asserting is that the C#
/// signature model describes VB correctly - because if it does, VB inherits all forty-one C# rules
/// for the price of a parse front end, and if it does not, the whole approach is wrong.
/// </summary>
public class TestVbExtraction : IDisposable
{
    private const string Source = """
        Namespace Widgets
            Public Interface IGadget
                ReadOnly Property Name As String

                Sub Move(distance As Integer)
            End Interface

            Public Structure Point
                Public X As Integer
            End Structure

            Public Enum Colour As Byte
                Red = 1
                Green = 2
            End Enum

            Public Delegate Function Callback(input As String) As Integer

            Public MustInherit Class Gadget
                Implements IGadget

                Public Const Kind As String = "gadget"

                Public Event Moved As System.EventHandler

                Public ReadOnly Property Name As String Implements IGadget.Name
                    Get
                        Return ""
                    End Get
                End Property

                Public Property Size As Integer

                Public Shared ReadOnly Property Description As String
                    Get
                        Return ""
                    End Get
                End Property

                Public MustOverride Sub Move(distance As Integer) Implements IGadget.Move

                Public Overridable Function Convert(Of T)(value As T) As T
                    Return value
                End Function

                Friend Sub Hidden()
                End Sub

                Private Sub AlsoHidden()
                End Sub

                Public NotInheritable Class Handle
                End Class
            End Class
        End Namespace
        """;

    private readonly string _folderRoot =
        Directory.CreateTempSubdirectory("easysemver-vb-extraction").FullName;

    private readonly ICsharpProject _project;

    public TestVbExtraction()
    {
        var projectPath = Path.Combine(this._folderRoot, "Widgets.vbproj");
        File.WriteAllText(projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\" />");
        File.WriteAllText(Path.Combine(this._folderRoot, "Widgets.vb"), Source);
        this._project = VbUnitBuilder.GetProjectSignature(projectPath);
    }

    public void Dispose()
    {
        Directory.Delete(this._folderRoot, recursive: true);
        GC.SuppressFinalize(this);
    }

    private ICsharpType Find(string name)
    {
        var type = this._project.Types.FirstOrDefault(t => t.Name == name);
        Assert.NotNull(type);
        return type;
    }

    /// <summary>
    /// The load-bearing assertion. VB's keywords are its own - Structure, MustInherit,
    /// NotInheritable - and they arrive here as the metadata concepts C#'s rules already compare.
    /// </summary>
    [Fact]
    public void EveryVbTypeKindIsModelledAsTheMetadataConceptItIs()
    {
        Assert.Equal("interface", this.Find("Widgets.IGadget").Kind);
        Assert.Equal("struct", this.Find("Widgets.Point").Kind);
        Assert.Equal("enum", this.Find("Widgets.Colour").Kind);
        Assert.Equal("delegate", this.Find("Widgets.Callback").Kind);
        Assert.Equal("class", this.Find("Widgets.Gadget").Kind);
    }

    /// <summary>
    /// The root namespace guard in <see cref="VbUnitBuilder"/>. Roslyn defaults VB's root namespace
    /// to the assembly name, which would make every type here "Widgets.Widgets.Gadget" and turn a
    /// project rename into the removal of its entire API.
    /// </summary>
    [Fact]
    public void TypeNamesAreNotPrefixedWithTheAssemblyName()
    {
        Assert.All(this._project.Types, type =>
            Assert.False(
                type.Name.StartsWith("Widgets.Widgets", StringComparison.Ordinal),
                $"'{type.Name}' carries the assembly name as a root namespace."));
    }

    [Fact]
    public void MustInheritIsAbstractAndNotInheritableIsSealed()
    {
        Assert.True(this.Find("Widgets.Gadget").IsAbstract);
        Assert.True(this.Find("Widgets.Gadget.Handle").IsSealed);
    }

    /// <summary>
    /// Note the capital B. Roslyn renders a type name in the language of the compilation it came
    /// from, so VB says <c>Byte</c> where C# says <c>byte</c> - and that is right, because a VB
    /// developer reads <c>Byte</c>. It is safe despite VB-01's shared model because units are keyed
    /// by (language, unit id) and a VB unit is only ever compared against its own history, so the
    /// spelling is stable for the lifetime of the unit.
    /// <para>
    /// The one case it bites is a project converted from one language to the other in place: the
    /// unit id survives, the spelling changes, and every primitive-typed member reads as retyped.
    /// That is a Major for a rewrite that was already a Major, so it is recorded rather than fixed.
    /// </para>
    /// </summary>
    [Fact]
    public void EnumCarriesMembersAndVbsOwnSpellingOfTheUnderlyingType()
    {
        var colour = (ICsharpEnum)this.Find("Widgets.Colour");

        Assert.Equal("Byte", colour.UnderlyingType);
        Assert.Equal(["Red", "Green"], colour.Members.Select(m => m.Name));
        Assert.Equal("1", colour.Members[0].Value);
    }

    [Fact]
    public void ImplementedInterfaceIsRecorded()
    {
        Assert.Contains("Widgets.IGadget", this.Find("Widgets.Gadget").ImplementedInterfaces);
    }

    /// <summary>
    /// SWE-02's C# equivalent: VB's Friend is C#'s internal, and neither is API. A rule firing on a
    /// Friend member would report a breaking change nobody outside the assembly could observe.
    /// </summary>
    [Fact]
    public void FriendAndPrivateMembersAreNotApi()
    {
        var gadget = this.Find("Widgets.Gadget");

        Assert.DoesNotContain("Hidden", gadget.Methods.Keys);
        Assert.DoesNotContain("AlsoHidden", gadget.Methods.Keys);
    }

    [Fact]
    public void SharedMembersAreStatic()
    {
        Assert.True(this.Find("Widgets.Gadget").Properties["Description"].IsStatic);
        Assert.False(this.Find("Widgets.Gadget").Properties["Size"].IsStatic);
    }

    /// <summary>A VB property with no Set is a get-only property, and R09 has to be able to see it.</summary>
    [Fact]
    public void ReadOnlyPropertyIsNotWritable()
    {
        Assert.False(this.Find("Widgets.Gadget").Properties["Name"].IsWritable);
        Assert.True(this.Find("Widgets.Gadget").Properties["Size"].IsWritable);
    }

    [Fact]
    public void PublicEventIsRecorded()
    {
        Assert.Contains("Moved", this.Find("Widgets.Gadget").Events.Select(e => e.Name));
    }

    [Fact]
    public void NestedPublicTypeIsRecorded()
    {
        Assert.Equal("Widgets.Gadget", this.Find("Widgets.Gadget.Handle").DeclaringType);
    }
}
