using Winterborn.Tools.EasySemVer.CodeReader.Csharp;
using Winterborn.Tools.EasySemVer.DataObject.Csharp;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Test;

/// <summary>
/// CSX-01…CSX-04 end to end over real source. This is the coverage G-15 never had: before this
/// pass, removing a public interface or renaming an enum member classified as Patch.
/// </summary>
public class TestCsharpExtraction : IDisposable
{
    private const string Source = """
        namespace Widgets;

        public interface IGadget
        {
            string Name { get; }

            void Move(int distance);

            int Weigh() => 0;
        }

        public struct Point
        {
            public int X;

            public readonly int Y;
        }

        public record Money(decimal Amount, string Currency);

        public record struct Ratio(int Numerator, int Denominator);

        public enum Colour : byte
        {
            Red = 1,
            Green = 2
        }

        public delegate int Callback(string input, ref int count);

        public abstract class Gadget : System.Object, IGadget
        {
            public const string Kind = "gadget";

            public static readonly string Family = "widgets";

            public event System.EventHandler? Moved;

            public string Name { get; init; } = "";

            public required int Size { get; set; }

            public static string Description => "";

            public abstract void Move(int distance);

            public virtual T Convert<T>(T value) where T : class, new() => value;

            public void Take(params string[] values) { }

            public void Emit(out int result) { result = 0; }

            public enum Mode
            {
                Fast
            }

            public sealed class Handle
            {
            }
        }
        """;

    private readonly string _folderRoot =
        Directory.CreateTempSubdirectory("easysemver-extraction").FullName;

    private readonly ICsharpProject _project;

    public TestCsharpExtraction()
    {
        var projectPath = Path.Combine(this._folderRoot, "Widgets.csproj");
        File.WriteAllText(projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\" />");
        File.WriteAllText(Path.Combine(this._folderRoot, "Widgets.cs"), Source);
        this._project = CsharpUnitBuilder.GetProjectSignature(projectPath);
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

    [Fact]
    public void EveryTypeKindIsModelledAsItself()
    {
        Assert.Equal("interface", this.Find("Widgets.IGadget").Kind);
        Assert.Equal("struct", this.Find("Widgets.Point").Kind);
        Assert.Equal("record", this.Find("Widgets.Money").Kind);
        Assert.Equal("record", this.Find("Widgets.Ratio").Kind);
        Assert.Equal("enum", this.Find("Widgets.Colour").Kind);
        Assert.Equal("delegate", this.Find("Widgets.Callback").Kind);
        Assert.Equal("class", this.Find("Widgets.Gadget").Kind);
    }

    [Fact]
    public void RecordStructIsAValueType()
    {
        Assert.False(((ICsharpRecord)this.Find("Widgets.Money")).IsValueType);
        Assert.True(((ICsharpRecord)this.Find("Widgets.Ratio")).IsValueType);
    }

    [Fact]
    public void RecordCarriesItsPositionalParameters()
    {
        var money = (ICsharpRecord)this.Find("Widgets.Money");

        Assert.Equal(["Amount", "Currency"], money.PositionalParameters.Select(p => p.ParameterName));
        Assert.Equal("decimal", money.PositionalParameters[0].ParameterType);
    }

    [Fact]
    public void EnumCarriesMembersAndUnderlyingType()
    {
        var colour = (ICsharpEnum)this.Find("Widgets.Colour");

        Assert.Equal("byte", colour.UnderlyingType);
        Assert.Equal(["Red", "Green"], colour.Members.Select(m => m.Name));
        Assert.Equal("1", colour.Members[0].Value);
    }

    [Fact]
    public void DelegateCarriesItsSignature()
    {
        var callback = (ICsharpDelegate)this.Find("Widgets.Callback");

        Assert.Equal("int", callback.ReturnType);
        Assert.Equal(["input", "count"], callback.Parameters.Select(p => p.ParameterName));
        Assert.Equal("Ref", callback.Parameters[1].RefKind);
    }

    [Fact]
    public void FieldsAreCaptured()
    {
        var point = this.Find("Widgets.Point");
        Assert.Equal(["X", "Y"], point.Fields.Select(f => f.Name).Order());
        Assert.True(point.Fields.First(f => f.Name == "Y").IsReadOnly);

        var gadget = this.Find("Widgets.Gadget");
        Assert.True(gadget.Fields.First(f => f.Name == "Kind").IsConstant);
        Assert.True(gadget.Fields.First(f => f.Name == "Family").IsStatic);
    }

    [Fact]
    public void EventsAreCaptured()
    {
        var moved = Assert.Single(this.Find("Widgets.Gadget").Events);

        Assert.Equal("Moved", moved.Name);
        Assert.Contains("EventHandler", moved.HandlerType);
    }

    [Fact]
    public void PropertyModifiersAreCaptured()
    {
        var gadget = this.Find("Widgets.Gadget");

        Assert.True(gadget.Properties["Name"].IsInitOnly);
        Assert.False(gadget.Properties["Size"].IsInitOnly);
        Assert.True(gadget.Properties["Size"].IsRequired);
        Assert.True(gadget.Properties["Description"].IsStatic);
    }

    [Fact]
    public void MemberModifiersAreCaptured()
    {
        var gadget = this.Find("Widgets.Gadget");

        Assert.True(gadget.Methods["Move"].Overrides.First().IsAbstract);
        Assert.True(gadget.Methods["Convert"].Overrides.First().IsVirtual);
        Assert.True(gadget.Methods["Take"].Overrides.First().Parameters[0].IsParams);
        Assert.Equal("Out", gadget.Methods["Emit"].Overrides.First().Parameters[0].RefKind);
    }

    [Fact]
    public void GenericConstraintsAreCaptured()
    {
        var convert = this.Find("Widgets.Gadget").Methods["Convert"].Overrides.First();

        var parameter = Assert.Single(convert.GenericParameters);
        Assert.Equal("T", parameter.Name);
        Assert.Equal("class, new()", parameter.Constraints);
    }

    [Fact]
    public void TypeFacetsAreCaptured()
    {
        var gadget = this.Find("Widgets.Gadget");

        Assert.True(gadget.IsAbstract);
        Assert.Equal(["Widgets.IGadget"], gadget.ImplementedInterfaces);

        // Deriving from object is universal and cannot change, so it is not recorded.
        Assert.Equal(string.Empty, gadget.BaseType);
    }

    [Fact]
    public void NestedTypesAreCapturedWithTheirDeclaringType()
    {
        var mode = this.Find("Widgets.Gadget.Mode");
        Assert.Equal("enum", mode.Kind);
        Assert.Equal("Widgets.Gadget", mode.DeclaringType);

        var handle = this.Find("Widgets.Gadget.Handle");
        Assert.Equal("Widgets.Gadget", handle.DeclaringType);
        Assert.True(handle.IsSealed);
    }

    [Fact]
    public void InterfaceDefaultImplementationsAreDistinguished()
    {
        var gadget = this.Find("Widgets.IGadget");

        Assert.False(gadget.Methods["Move"].Overrides.First().HasDefaultImplementation);
        Assert.True(gadget.Methods["Weigh"].Overrides.First().HasDefaultImplementation);
    }

    [Fact]
    public void PerOverloadReturnTypeIsRecorded()
    {
        var convert = this.Find("Widgets.Gadget").Methods["Convert"].Overrides.First();

        Assert.Equal("T", convert.ReturnType);
    }
}
