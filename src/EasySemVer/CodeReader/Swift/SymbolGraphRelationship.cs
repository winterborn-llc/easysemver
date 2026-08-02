using System.Text.Json;

namespace Winterborn.Library.EasySemVer.CodeReader.Swift;

[DebuggerDisplay("{Kind}")]
internal class SymbolGraphRelationship
{
    internal string Kind { get; private init; } = string.Empty;

    internal string Source { get; private init; } = string.Empty;

    internal string Target { get; private init; } = string.Empty;

    /// <summary>The readable name of a target the toolchain did not emit a symbol for.</summary>
    internal string TargetFallback { get; private init; } = string.Empty;

    internal static SymbolGraphRelationship Read(JsonElement element)
    {
        return new SymbolGraphRelationship
        {
            Kind = element.GetStringOrEmpty("kind"),
            Source = element.GetStringOrEmpty("source"),
            Target = element.GetStringOrEmpty("target"),
            TargetFallback = element.GetStringOrEmpty("targetFallback")
        };
    }
}
