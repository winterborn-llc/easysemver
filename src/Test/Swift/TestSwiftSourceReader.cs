using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Test.Swift;

/// <summary>
/// TST-M5 - extraction tested by feeding checked-in Swift source through the reader, which is the
/// whole of extraction now that no toolchain is involved. The fixture covers a struct, class,
/// actor, enum with associated values, enum with a raw value type, protocol with and without
/// default implementations, extensions on in-module and external types, generics with constraints,
/// async/throws, mutating/inout/variadic, @frozen, @available and @objc.
/// </summary>
public class TestSwiftSourceReader
{
    private static ISwiftModule Module => Fixtures.WidgetsModule;

    private static ISwiftType Find(string name)
    {
        var type = Module.Types.FirstOrDefault(t => t.Name == name);
        Assert.NotNull(type);
        return type;
    }

    [Fact]
    public void EveryTypeKindIsModelledAsItself()
    {
        Assert.Equal("struct", Find("Point").Kind);
        Assert.Equal("class", Find("Gadget").Kind);
        Assert.Equal("actor", Find("Counter").Kind);
        Assert.Equal("enum", Find("Colour").Kind);
        Assert.Equal("protocol", Find("Movable").Kind);
    }

    /// <summary>SWE-02 - only public and open declarations enter the signature.</summary>
    [Fact]
    public void EveryDeclarationIsPublicOrOpen()
    {
        foreach (var type in Module.Types)
        {
            Assert.Contains(type.AccessLevel, (string[])["public", "open"]);
        }
    }

    [Fact]
    public void OpenIsDistinguishedFromPublic()
    {
        Assert.Equal("open", Find("Gadget").AccessLevel);
        Assert.Equal("public", Find("Point").AccessLevel);
    }

    [Fact]
    public void FrozenIsCaptured()
    {
        Assert.True(Find("Frozen").IsFrozen);
        Assert.False(Find("Point").IsFrozen);
    }

    [Fact]
    public void ArgumentLabelsArePartOfTheIdentity()
    {
        var move = Find("Gadget").Functions.FirstOrDefault(f => f.Name == "Gadget.move(to:animated:)");

        Assert.NotNull(move);
        Assert.Equal(["to", "animated"], move.Parameters.Select(p => p.Label));
        Assert.Equal("point", move.Parameters[0].InternalName);
        Assert.Equal("Point", move.Parameters[0].Type);
        Assert.True(move.Throws);
    }

    [Fact]
    public void AsyncAndThrowsAndStaticAreCaptured()
    {
        var make = Find("Gadget").Functions.First(f => f.Name == "Gadget.make()");

        Assert.True(make.IsAsync);
        Assert.True(make.Throws);
        Assert.True(make.IsStatic);
        Assert.Equal("Gadget", make.ReturnType);
    }

    [Fact]
    public void MutatingAndInoutAndVariadicAndDefaultsAreCaptured()
    {
        var add = Find("Mutator").Functions.First(f => f.Name.StartsWith("Mutator.add"));

        Assert.True(add.IsMutating);
        Assert.True(add.Parameters[0].HasDefault);
        Assert.False(add.Parameters[1].HasDefault);
        Assert.True(add.Parameters[1].IsInout);
        Assert.Equal("inout", add.Parameters[1].Ownership);
        Assert.Equal("Int", add.Parameters[1].Type);
        Assert.True(add.Parameters[2].IsVariadic);
        Assert.Equal("Mutator.add(_:into:extra:)", add.Name);
    }

    [Fact]
    public void EnumCasesAndAssociatedValuesAreCaptured()
    {
        var colour = (ISwiftEnum)Find("Colour");

        Assert.Equal(["Colour.green(shade:_:)", "Colour.red"], colour.Cases.Select(c => c.Name));
        var green = colour.Cases.First(c => c.Name.StartsWith("Colour.green"));
        Assert.Equal(2, green.AssociatedValues.Count);
        Assert.Equal("shade", green.AssociatedValues[0].Label);
        Assert.Equal("Int", green.AssociatedValues[0].Type);
        Assert.Equal("Double", green.AssociatedValues[1].Type);
    }

    /// <summary>
    /// An enum's first inheritance entry is its raw value type when it is one, and a conformance
    /// otherwise. "Colour" has no inheritance clause at all and must not acquire one.
    /// </summary>
    [Fact]
    public void RawValueTypesAreToldFromConformances()
    {
        Assert.Equal("String", ((ISwiftEnum)Find("Size")).RawValueType);
        Assert.Empty(((ISwiftEnum)Find("Size")).Conformances);
        Assert.Equal(string.Empty, ((ISwiftEnum)Find("Colour")).RawValueType);
    }

    [Fact]
    public void ProtocolRequirementsAndAssociatedTypesAreCaptured()
    {
        var movable = (ISwiftProtocol)Find("Movable");

        Assert.Equal(["Distance"], movable.AssociatedTypes);
        Assert.Contains(movable.Functions, f => f.Name == "Movable.move(to:animated:)");
        Assert.Contains(movable.Properties, p => p.Name == "Movable.speed");
    }

    /// <summary>
    /// S20/S21 turn on this distinction. A requirement an extension supplies a body for is
    /// defaulted; one nobody has written a body for is not.
    /// </summary>
    [Fact]
    public void DefaultImplementationsAreDistinguished()
    {
        var defaulted = (ISwiftProtocol)Find("Defaulted");

        var required = defaulted.Functions.First(f => f.Name == "Defaulted.required1()");
        Assert.False(required.HasDefaultImplementation);

        var withDefault = defaulted.Functions.First(f => f.Name == "Defaulted.withDefault()");
        Assert.True(withDefault.HasDefaultImplementation);

        // Defaulting a requirement does not add a second member with the same name.
        Assert.Single(defaulted.Functions, f => f.Name == "Defaulted.withDefault()");
    }

    /// <summary>SWM-02 - members an in-module extension adds are folded into the type.</summary>
    [Fact]
    public void InModuleExtensionMembersAreFoldedIntoTheirType()
    {
        Assert.Contains(Find("Movable").Functions, f => f.Name == "Movable.describe()");
    }

    /// <summary>SWM-02 - an extension on a foreign type is its own entity.</summary>
    [Fact]
    public void ExtensionOnAnExternalTypeIsItsOwnEntity()
    {
        var extension = Assert.Single(Module.Extensions);

        Assert.Equal("String", extension.ExtendedType);
        Assert.Contains(extension.Functions, f => f.Name == "String.widgetize()");
    }

    [Fact]
    public void GenericsAndConstraintsAreCaptured()
    {
        var topLevel = Module.GlobalFunctions.First(f => f.Name == "topLevel(_:)");

        var parameter = Assert.Single(topLevel.GenericParameters);
        Assert.Equal("T", parameter.Name);
        Assert.Equal("conformance Equatable", parameter.Constraints);
    }

    [Fact]
    public void GlobalsAndAliasesAndOperatorsAreCaptured()
    {
        Assert.Contains(Module.GlobalVariables, v => v.Name == "globalThing");
        Assert.Contains(Module.TypeAliases, a => a.Name == "Alias");

        var declared = Module.Operators.First(o => o.Name == "<~>(_:_:)");
        Assert.Equal("infix", declared.OperatorKind);
        Assert.Equal("AdditionPrecedence", declared.PrecedenceGroup);
    }

    [Fact]
    public void AvailabilityIsCaptured()
    {
        var introduced = Assert.Single(Find("NewThing").Availability);
        Assert.Equal("macOS", introduced.Domain);
        Assert.Equal("12.0", introduced.Introduced);

        var deprecated = Assert.Single(Find("OldThing").Availability);
        Assert.True(deprecated.IsDeprecated);
        Assert.Equal("NewThing", deprecated.RenamedTo);
    }

    [Fact]
    public void ObjCExposureIsCaptured()
    {
        var ping = Find("Gadget").Functions.First(f => f.Name == "Gadget.ping()");

        Assert.Equal("@objc", ping.ObjCExposure);
    }

    /// <summary>
    /// SWM-03 - the first entry of a class's inheritance clause is its superclass unless this
    /// module declares it as a protocol, which "Movable" is and "NSObject" is not.
    /// </summary>
    [Fact]
    public void SuperclassAndConformancesAreCaptured()
    {
        var gadget = Find("Gadget");

        Assert.Equal("NSObject", gadget.Superclass);
        Assert.Equal(["Movable"], gadget.Conformances);
    }

    [Fact]
    public void SettabilityIsCaptured()
    {
        Assert.True(Find("Point").Properties.First(p => p.Name == "Point.x").IsSettable);
        Assert.False(Find("Gadget").Properties.First(p => p.Name == "Gadget.speed").IsSettable);
        Assert.False(Find("Frozen").Properties.First(p => p.Name == "Frozen.v").IsSettable);
        Assert.True(Find("Gadget").Properties.First(p => p.Name == "Gadget.name").IsSettable);
    }

    [Fact]
    public void SubscriptsAndInitializersAreCaptured()
    {
        Assert.Contains(Find("Gadget").Subscripts, s => s.Name == "Gadget.subscript(_:)");
        Assert.Contains(Find("Point").Initializers, i => i.Name == "Point.init(x:y:)");
        Assert.True(Find("Point").Initializers.First().Parameters[1].HasDefault);
    }

    /// <summary>
    /// A conformance's synthesised members - "==", "hashValue", a memberwise init - are the
    /// compiler's, not this module's. Source never mentions them, which is the point: they cannot
    /// reach the baseline and a toolchain upgrade cannot look like an API change.
    /// </summary>
    [Fact]
    public void SynthesizedConformanceMembersAreExcluded()
    {
        Assert.DoesNotContain(Find("Size").Properties, p => p.Name.EndsWith("hashValue"));
        Assert.DoesNotContain(Find("Point").Functions, f => f.Name.Contains("!="));
    }

    /// <summary>SWE-04 - nothing toolchain-version-dependent reaches the model.</summary>
    [Fact]
    public void NoMangledNamesReachTheModel()
    {
        var module = (SwiftModule)Module;
        var xml = Winterborn.Tools.EasySemVer.Extensions.ExtendObject
            .SerializeToElement(module)
            .ToString();

        Assert.DoesNotContain("::SYNTHESIZED::", xml);
        Assert.DoesNotContain("s:7Widgets", xml);
    }

    /// <summary>
    /// A nested typealias is part of the owning type's surface and is recorded as a get-only
    /// property, which is how it diffs.
    /// </summary>
    [Fact]
    public void NestedTypeAliasesBelongToTheirType()
    {
        var distance = Find("Gadget").Properties.First(p => p.Name == "Gadget.Distance");

        Assert.Equal("Int", distance.Type);
    }
}
