using System.Text.Json;
using Winterborn.Tools.EasySemVer.DataObject.Swift;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// Turns the toolchain's symbol-graph JSON into an <see cref="SwiftModule"/> (SWE-01). The graph
/// is the only source of Swift signatures - there is no hand-rolled Swift parser anywhere in this
/// tool (D-02) - and only the fields modelled here are ever persisted, so nothing
/// toolchain-version-dependent reaches the baseline (SWE-04).
/// </summary>
internal static class SymbolGraphReader
{
    /// <summary>
    /// Protocol conformances make the compiler emit inherited members onto every conforming type.
    /// They are not this module's declarations, and including them would make a Swift version
    /// bump look like an API change.
    /// </summary>
    private const string SynthesizedMarker = "::SYNTHESIZED::";

    /// <summary>
    /// Reads every graph file belonging to one module. A module emits one graph for itself plus
    /// one per foreign module it extends (SWM-02).
    /// </summary>
    internal static SwiftModule Read(string moduleName, IEnumerable<string> graphJsonDocuments)
    {
        var module = new SwiftModule(moduleName);
        var symbols = new List<SymbolGraphSymbol>();
        var relationships = new List<SymbolGraphRelationship>();

        foreach (var json in graphJsonDocuments)
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (GetGraphModuleName(root) != moduleName)
            {
                continue;
            }

            ReadSymbols(root, symbols);
            ReadRelationships(root, relationships);
        }

        Assemble(module, symbols, relationships);
        module.SortForPersistence();
        return module;
    }

    internal static string GetGraphModuleName(JsonElement root)
    {
        return root.TryGetProperty("module", out var moduleElement)
               && moduleElement.TryGetProperty("name", out var nameElement)
            ? nameElement.GetString() ?? string.Empty
            : string.Empty;
    }

    private static void ReadSymbols(JsonElement root, List<SymbolGraphSymbol> symbols)
    {
        if (!root.TryGetProperty("symbols", out var symbolsElement))
        {
            return;
        }

        foreach (var element in symbolsElement.EnumerateArray())
        {
            var symbol = SymbolGraphSymbol.Read(element);

            // SWE-02: do not trust the minimum-access-level flag alone.
            if (symbol.AccessLevel is not (SwiftAccessLevels.Public or SwiftAccessLevels.Open))
            {
                continue;
            }

            if (symbol.Usr.Contains(SynthesizedMarker, StringComparison.Ordinal))
            {
                continue;
            }

            symbols.Add(symbol);
        }
    }

    private static void ReadRelationships(JsonElement root, List<SymbolGraphRelationship> relationships)
    {
        if (!root.TryGetProperty("relationships", out var relationshipsElement))
        {
            return;
        }

        foreach (var element in relationshipsElement.EnumerateArray())
        {
            relationships.Add(SymbolGraphRelationship.Read(element));
        }
    }

    private static void Assemble(
        SwiftModule module,
        List<SymbolGraphSymbol> symbols,
        List<SymbolGraphRelationship> relationships)
    {
        var byUsr = new Dictionary<string, SymbolGraphSymbol>();
        foreach (var symbol in symbols)
        {
            byUsr[symbol.Usr] = symbol;
        }

        var types = new Dictionary<string, SwiftType>();
        var extensions = new Dictionary<string, SwiftExtension>();
        BuildTypes(module, symbols, types);
        BuildExtensions(module, symbols, relationships, byUsr, extensions);
        ApplyRelationships(symbols, relationships, byUsr, types);
        PlaceMembers(module, symbols, relationships, byUsr, types, extensions);
    }

    private static void BuildTypes(
        SwiftModule module,
        List<SymbolGraphSymbol> symbols,
        Dictionary<string, SwiftType> types)
    {
        foreach (var symbol in symbols)
        {
            var type = SwiftSymbolFactory.CreateType(symbol);
            if (type == null)
            {
                continue;
            }

            module.Add(type);
            types[symbol.Usr] = type;
        }
    }

    private static void BuildExtensions(
        SwiftModule module,
        List<SymbolGraphSymbol> symbols,
        List<SymbolGraphRelationship> relationships,
        Dictionary<string, SymbolGraphSymbol> byUsr,
        Dictionary<string, SwiftExtension> extensions)
    {
        foreach (var symbol in symbols)
        {
            if (symbol.Kind != SymbolGraphKinds.Extension)
            {
                continue;
            }

            var extension = new SwiftExtension
            {
                ExtendedType = GetExtendedTypeName(symbol, relationships, byUsr),
                Constraints = symbol.SwiftExtensionConstraints
            };

            module.Extensions.Add(extension);
            extensions[symbol.Usr] = extension;
        }
    }

    private static string GetExtendedTypeName(
        SymbolGraphSymbol symbol,
        List<SymbolGraphRelationship> relationships,
        Dictionary<string, SymbolGraphSymbol> byUsr)
    {
        foreach (var relationship in relationships)
        {
            if (relationship.Kind != SymbolGraphRelationshipKinds.ExtensionTo)
            {
                continue;
            }

            if (relationship.Source != symbol.Usr)
            {
                continue;
            }

            if (byUsr.TryGetValue(relationship.Target, out var target))
            {
                return target.Path;
            }

            // SWE-03: the fallback is the readable name the toolchain supplies for a symbol it
            // did not emit, which is exactly what we want for a foreign type.
            return relationship.TargetFallback.Length > 0
                ? relationship.TargetFallback
                : symbol.Path;
        }

        return symbol.Path;
    }

    private static void ApplyRelationships(
        List<SymbolGraphSymbol> symbols,
        List<SymbolGraphRelationship> relationships,
        Dictionary<string, SymbolGraphSymbol> byUsr,
        Dictionary<string, SwiftType> types)
    {
        foreach (var relationship in relationships)
        {
            switch (relationship.Kind)
            {
                case SymbolGraphRelationshipKinds.ConformsTo
                    when types.TryGetValue(relationship.Source, out var conformer):
                    conformer.Conformances.Add(ResolveName(relationship, byUsr));
                    break;

                case SymbolGraphRelationshipKinds.InheritsFrom
                    when types.TryGetValue(relationship.Source, out var subclass):
                    subclass.Superclass = ResolveName(relationship, byUsr);
                    break;
            }
        }

        // S21: a requirement is defaulted when an extension supplies a body for it. The graph
        // states that directly, so we do not have to guess from names.
        var defaulted = new HashSet<string>();
        foreach (var relationship in relationships)
        {
            if (relationship.Kind != SymbolGraphRelationshipKinds.DefaultImplementationOf)
            {
                continue;
            }

            defaulted.Add(relationship.Target);
        }

        foreach (var symbol in symbols)
        {
            symbol.HasDefaultImplementation = defaulted.Contains(symbol.Usr);
        }
    }

    private static string ResolveName(
        SymbolGraphRelationship relationship,
        Dictionary<string, SymbolGraphSymbol> byUsr)
    {
        if (byUsr.TryGetValue(relationship.Target, out var target))
        {
            return target.Path;
        }

        return relationship.TargetFallback.Length > 0
            ? relationship.TargetFallback
            : relationship.Target;
    }

    private static void PlaceMembers(
        SwiftModule module,
        List<SymbolGraphSymbol> symbols,
        List<SymbolGraphRelationship> relationships,
        Dictionary<string, SymbolGraphSymbol> byUsr,
        Dictionary<string, SwiftType> types,
        Dictionary<string, SwiftExtension> extensions)
    {
        var owners = new Dictionary<string, string>();
        foreach (var relationship in relationships)
        {
            if (relationship.Kind is not (SymbolGraphRelationshipKinds.MemberOf
                or SymbolGraphRelationshipKinds.RequirementOf
                or SymbolGraphRelationshipKinds.OptionalRequirementOf))
            {
                continue;
            }

            owners[relationship.Source] = relationship.Target;
        }

        foreach (var symbol in symbols)
        {
            if (symbol.Kind == SymbolGraphKinds.Extension)
            {
                continue;
            }

            if (types.ContainsKey(symbol.Usr))
            {
                continue;
            }

            var ownerUsr = owners.GetValueOrDefault(symbol.Usr, string.Empty);
            if (ownerUsr.Length > 0 && types.TryGetValue(ownerUsr, out var owningType))
            {
                // SWM-02: members an extension adds to a type declared in this module are folded
                // into that type, because that is how a Swift developer reads them.
                SwiftSymbolFactory.AddMember(owningType, symbol, byUsr);
                continue;
            }

            if (ownerUsr.Length > 0 && extensions.TryGetValue(ownerUsr, out var owningExtension))
            {
                SwiftSymbolFactory.AddMember(owningExtension, symbol);
                continue;
            }

            if (ownerUsr.Length > 0)
            {
                // A member of something we deliberately did not model - an internal type, or a
                // foreign type reached without an extension block. Not this module's surface.
                continue;
            }

            SwiftSymbolFactory.AddGlobal(module, symbol);
        }
    }
}
