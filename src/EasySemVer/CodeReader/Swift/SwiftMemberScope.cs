using Winterborn.Tools.EasySemVer.DataObject.Swift;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// What a run of members is being read into, and the rules that apply to them there. The same
/// "func" means different things in three places - a requirement inside a protocol, a method
/// inside a struct, a default implementation inside an extension - and this is what carries the
/// difference down to where each member is built.
/// </summary>
internal class SwiftMemberScope
{
    /// <summary>The name members are qualified by, which is the type's for a folded extension.</summary>
    internal required string OwnerPath { get; init; }

    /// <summary>
    /// SWE-02 - the access level a member with no modifier of its own has. Internal almost
    /// everywhere, but a protocol's requirements take the protocol's, and an extension written
    /// "public extension" gives its members that.
    /// </summary>
    internal required string DefaultAccess { get; init; }

    /// <summary>
    /// The owning type's own access level. An enum case cannot carry a modifier and is as visible
    /// as the enum is, which is not the same rule as the one its methods follow.
    /// </summary>
    internal string OwnerAccess { get; init; } = "internal";

    internal string ExtensionConstraints { get; init; } = string.Empty;

    /// <summary>
    /// S21 - a member reached through an extension of a protocol is available to every conformer
    /// without them writing anything, which is what a default implementation is.
    /// </summary>
    internal bool ProvidesDefaultImplementations { get; init; }

    internal static SwiftMemberScope ForType(SwiftType owner, string access)
    {
        return new SwiftMemberScope
        {
            OwnerPath = owner.Name,
            OwnerAccess = access,

            // A protocol's requirements cannot carry a modifier, so they are as public as it is.
            DefaultAccess = owner is SwiftProtocol ? access : "internal"
        };
    }
}
