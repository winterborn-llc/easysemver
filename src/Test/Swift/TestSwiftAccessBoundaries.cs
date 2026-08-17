using Winterborn.Tools.EasySemVer.CodeReader.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Test.Swift;

/// <summary>
/// SWE-02 - where the public surface stops. Swift's default is `internal`, so most of a module is
/// out of scope, and the reader has to recognise what it is leaving out as well as what it is
/// taking: a type it does not model is not thereby a type from another module.
/// <para>
/// The extension case here came out of running the reader over a real application target. Every
/// `extension AppSettings: SomeInternalProtocol {}` in it was recorded as an extension of a
/// foreign type, because the reader had dropped `AppSettings` for being internal and then failed
/// to recognise the name. An app with no public API at all had a baseline full of entries.
/// </para>
/// </summary>
public class TestSwiftAccessBoundaries
{
    private static ISwiftModule Read(string source)
    {
        return SwiftSourceReader.Read("Widgets", [source]);
    }

    [Fact]
    public void AnExtensionOfATypeTheModuleKeepsToItselfIsNotRecorded()
    {
        var module = Read(
            """
            struct AppSettings { }
            protocol SettingsReading { }
            extension AppSettings: SettingsReading { }
            """);

        Assert.Empty(module.Extensions);
        Assert.Empty(module.Types);
    }

    [Fact]
    public void AConformanceToAProtocolTheModuleKeepsToItselfIsNotRecorded()
    {
        var module = Read(
            """
            protocol Hidden { }
            public protocol Shown { }
            public struct Gadget: Hidden, Shown { }
            """);

        var gadget = Assert.Single(module.Types, t => t.Name == "Gadget");
        Assert.Equal(["Shown"], gadget.Conformances);
    }

    /// <summary>An extension of a genuinely foreign type is still its own entity (SWM-02).</summary>
    [Fact]
    public void AnExtensionOfAForeignTypeIsStillRecorded()
    {
        var module = Read(
            """
            public protocol Shown { }
            extension String: Shown {
                public func widgetize() -> String { self }
            }
            """);

        var extension0 = Assert.Single(module.Extensions);
        Assert.Equal("String", extension0.ExtendedType);
        Assert.Equal(["Shown"], extension0.AddedConformances);
        Assert.Single(extension0.Functions, f => f.Name == "String.widgetize()");
    }

    /// <summary>
    /// A public member of an internal type is not public: nobody outside the module can name the
    /// type to reach it. The whole subtree stops, nested types included.
    /// </summary>
    [Fact]
    public void NothingInsideATypeTheModuleKeepsToItselfIsPublic()
    {
        var module = Read(
            """
            struct Hidden {
                public func reachable() { }
                public struct Nested {
                    public var value: Int = 0
                }
            }
            """);

        Assert.Empty(module.Types);
    }

    /// <summary>
    /// "public extension" makes its members public without each of them saying so, which is the
    /// one place other than a protocol where a member's access is not the default.
    /// </summary>
    [Fact]
    public void MembersOfAPublicExtensionArePublicWithoutSayingSo()
    {
        var module = Read(
            """
            public extension String {
                func widgetize() -> String { self }
                internal func hidden() { }
            }
            """);

        var extension0 = Assert.Single(module.Extensions);
        Assert.Single(extension0.Functions, f => f.Name == "String.widgetize()");
        Assert.DoesNotContain(extension0.Functions, f => f.Name == "String.hidden()");
    }

    /// <summary>
    /// A plain extension of a foreign type carries only the members that say they are public,
    /// and is not recorded at all when none of them do.
    /// </summary>
    [Fact]
    public void AnExtensionThatAddsNothingPublicIsNotRecorded()
    {
        var module = Read(
            """
            extension String {
                func helper() -> String { self }
            }
            """);

        Assert.Empty(module.Extensions);
    }

    /// <summary>
    /// A protocol's requirements cannot carry an access modifier: they are as visible as the
    /// protocol, and an enum's cases are as visible as the enum, for the same reason.
    /// </summary>
    [Fact]
    public void RequirementsAndCasesInheritTheirTypesAccess()
    {
        var module = Read(
            """
            public protocol Movable {
                func move()
            }

            public enum Colour {
                case red
            }

            protocol Hidden {
                func vanish()
            }
            """);

        Assert.Single(((ISwiftProtocol)module.Types.First(t => t.Name == "Movable")).Functions);
        Assert.Single(((ISwiftEnum)module.Types.First(t => t.Name == "Colour")).Cases);
        Assert.DoesNotContain(module.Types, t => t.Name == "Hidden");
    }
}
