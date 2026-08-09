using System.Text;
using System.Text.Json;
using Winterborn.Tools.EasySemVer.DataObject.Swift;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// One entry from the symbol graph's <c>symbols</c> array, reduced to the facets EasySemVer
/// models. Everything the toolchain reports that we do not model - source locations, doc
/// comments, mangled names - is dropped here rather than carried around (SWE-04).
/// </summary>
[DebuggerDisplay("{Kind} {Path}")]
internal class SymbolGraphSymbol
{
    /// <summary>The mangled precise identifier. Used to join relationships, never as identity (SWE-03).</summary>
    internal string Usr { get; private init; } = string.Empty;

    internal string Kind { get; private init; } = string.Empty;

    /// <summary>Path components joined with dots - the identity key (SWE-03).</summary>
    internal string Path { get; private init; } = string.Empty;

    /// <summary>The last path component: the declaration's own name, with argument labels.</summary>
    internal string LocalName { get; private init; } = string.Empty;

    internal string AccessLevel { get; private init; } = string.Empty;

    /// <summary>The declaration as written, reassembled from its fragments.</summary>
    internal string Declaration { get; private init; } = string.Empty;

    internal IReadOnlyList<string> Keywords { get; private init; } = [];

    internal IReadOnlyList<string> Attributes { get; private init; } = [];

    internal List<SwiftParameter> Parameters { get; private init; } = [];

    internal string ReturnType { get; private init; } = string.Empty;

    internal List<SwiftGenericParameter> GenericParameters { get; private init; } = [];

    internal List<SwiftAvailability> Availability { get; private init; } = [];

    internal bool IsFromExtension { get; private init; }

    internal string SwiftExtensionConstraints { get; private init; } = string.Empty;

    /// <summary>Filled in from the graph's relationships, not from the symbol itself.</summary>
    internal bool HasDefaultImplementation { get; set; }

    internal static SymbolGraphSymbol Read(JsonElement element)
    {
        var fragments = element.GetOrNull("declarationFragments");
        var pathComponents = ReadPathComponents(element);
        var swiftExtension = element.GetOrNull("swiftExtension");

        return new SymbolGraphSymbol
        {
            Usr = element.GetOrNull("identifier")?.GetStringOrEmpty("precise") ?? string.Empty,
            Kind = element.GetOrNull("kind")?.GetStringOrEmpty("identifier") ?? string.Empty,
            Path = string.Join('.', pathComponents),
            LocalName = pathComponents.Count > 0 ? pathComponents[^1] : string.Empty,
            AccessLevel = element.GetStringOrEmpty("accessLevel"),
            Declaration = JoinFragments(fragments),
            Keywords = ReadFragmentsOfKind(fragments, "keyword"),
            Attributes = ReadFragmentsOfKind(fragments, "attribute"),
            Parameters = ReadParameters(element, fragments),
            ReturnType = ReadReturnType(element),
            GenericParameters = ReadGenericParameters(element),
            Availability = ReadAvailability(element),
            IsFromExtension = swiftExtension != null,
            SwiftExtensionConstraints = ReadConstraints(swiftExtension?.GetOrNull("constraints"))
        };
    }

    internal bool HasKeyword(string keyword)
    {
        return this.Keywords.Contains(keyword);
    }

    /// <summary>
    /// SWM-04 - "@objc" or "@objc(CustomName)" exactly as written, since a custom name is part of
    /// the contract an Objective-C client sees.
    /// </summary>
    internal string GetObjCExposure()
    {
        foreach (var attribute in this.Attributes)
        {
            if (attribute.StartsWith("@objc", StringComparison.Ordinal))
            {
                return attribute;
            }
        }

        return string.Empty;
    }

    private static List<string> ReadPathComponents(JsonElement element)
    {
        var components = new List<string>();
        var pathComponents = element.GetOrNull("pathComponents");
        if (pathComponents == null)
        {
            return components;
        }

        foreach (var component in pathComponents.Value.EnumerateArray())
        {
            components.Add(component.GetString() ?? string.Empty);
        }

        return components;
    }

    private static string JoinFragments(JsonElement? fragments)
    {
        if (fragments == null)
        {
            return string.Empty;
        }

        var text = new StringBuilder();
        foreach (var fragment in fragments.Value.EnumerateArray())
        {
            text.Append(fragment.GetStringOrEmpty("spelling"));
        }

        return text.ToString();
    }

    private static List<string> ReadFragmentsOfKind(JsonElement? fragments, string kind)
    {
        var spellings = new List<string>();
        if (fragments == null)
        {
            return spellings;
        }

        foreach (var fragment in fragments.Value.EnumerateArray())
        {
            if (fragment.GetStringOrEmpty("kind") != kind)
            {
                continue;
            }

            spellings.Add(fragment.GetStringOrEmpty("spelling"));
        }

        return spellings;
    }

    private static List<SwiftParameter> ReadParameters(JsonElement element, JsonElement? fragments)
    {
        var parameters = new List<SwiftParameter>();
        var signature = element.GetOrNull("functionSignature");
        var declared = signature?.GetOrNull("parameters");
        if (declared == null)
        {
            return parameters;
        }

        // Default values are absent from the per-parameter fragments but present in the
        // declaration text, so the declaration's parameter list is what we read them from.
        var defaults = SwiftDeclarationText.GetParametersWithDefaults(JoinFragments(fragments));

        var index = 0;
        foreach (var parameter in declared.Value.EnumerateArray())
        {
            var parameterFragments = parameter.GetOrNull("declarationFragments");
            parameters.Add(new SwiftParameter
            {
                Label = parameter.GetStringOrEmpty("name"),
                InternalName = parameter.GetStringOrEmpty("internalName"),
                Type = SwiftDeclarationText.GetParameterType(JoinFragments(parameterFragments)),
                HasDefault = defaults.Contains(index),
                IsInout = ReadFragmentsOfKind(parameterFragments, "keyword").Contains("inout"),
                IsVariadic = JoinFragments(parameterFragments).EndsWith("...", StringComparison.Ordinal),
                Ownership = ReadOwnership(ReadFragmentsOfKind(parameterFragments, "keyword"))
            });
            index++;
        }

        return parameters;
    }

    private static string ReadOwnership(IReadOnlyList<string> keywords)
    {
        string[] ownershipKeywords = ["inout", "borrowing", "consuming", "__owned", "__shared"];
        foreach (var keyword in ownershipKeywords)
        {
            if (keywords.Contains(keyword))
            {
                return keyword;
            }
        }

        return string.Empty;
    }

    private static string ReadReturnType(JsonElement element)
    {
        var returns = element.GetOrNull("functionSignature")?.GetOrNull("returns");
        if (returns == null)
        {
            return string.Empty;
        }

        var text = new StringBuilder();
        foreach (var fragment in returns.Value.EnumerateArray())
        {
            text.Append(fragment.GetStringOrEmpty("spelling"));
        }

        return text.ToString();
    }

    private static List<SwiftGenericParameter> ReadGenericParameters(JsonElement element)
    {
        var parameters = new List<SwiftGenericParameter>();
        var generics = element.GetOrNull("swiftGenerics");
        var declared = generics?.GetOrNull("parameters");
        if (declared == null)
        {
            return parameters;
        }

        foreach (var parameter in declared.Value.EnumerateArray())
        {
            var name = parameter.GetStringOrEmpty("name");
            parameters.Add(new SwiftGenericParameter
            {
                Name = name,
                Constraints = ReadConstraintsFor(generics?.GetOrNull("constraints"), name)
            });
        }

        return parameters;
    }

    private static string ReadConstraintsFor(JsonElement? constraints, string parameterName)
    {
        if (constraints == null)
        {
            return string.Empty;
        }

        var matching = new List<string>();
        foreach (var constraint in constraints.Value.EnumerateArray())
        {
            if (constraint.GetStringOrEmpty("lhs") != parameterName)
            {
                continue;
            }

            matching.Add($"{constraint.GetStringOrEmpty("kind")} {constraint.GetStringOrEmpty("rhs")}");
        }

        matching.Sort(StringComparer.Ordinal);
        return string.Join(", ", matching);
    }

    private static string ReadConstraints(JsonElement? constraints)
    {
        if (constraints == null)
        {
            return string.Empty;
        }

        var rendered = new List<string>();
        foreach (var constraint in constraints.Value.EnumerateArray())
        {
            rendered.Add(
                $"{constraint.GetStringOrEmpty("lhs")} {constraint.GetStringOrEmpty("kind")} " +
                $"{constraint.GetStringOrEmpty("rhs")}");
        }

        rendered.Sort(StringComparer.Ordinal);
        return string.Join(", ", rendered);
    }

    private static List<SwiftAvailability> ReadAvailability(JsonElement element)
    {
        var availability = new List<SwiftAvailability>();
        var declared = element.GetOrNull("availability");
        if (declared == null)
        {
            return availability;
        }

        foreach (var clause in declared.Value.EnumerateArray())
        {
            availability.Add(new SwiftAvailability
            {
                Domain = clause.GetStringOrEmpty("domain"),
                Introduced = ReadVersion(clause.GetOrNull("introduced")),
                Deprecated = ReadVersion(clause.GetOrNull("deprecated")),
                Obsoleted = ReadVersion(clause.GetOrNull("obsoleted")),
                IsDeprecated = clause.GetOrNull("deprecated") != null
                               || IsTrue(clause, "isUnconditionallyDeprecated"),
                IsUnavailable = IsTrue(clause, "isUnconditionallyUnavailable"),
                RenamedTo = clause.GetStringOrEmpty("renamed")
            });
        }

        return availability;
    }

    private static bool IsTrue(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value)
               && value.ValueKind == JsonValueKind.True;
    }

    private static string ReadVersion(JsonElement? version)
    {
        if (version == null || version.Value.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        var parts = new List<string>();
        foreach (var part in (string[])["major", "minor", "patch"])
        {
            if (!version.Value.TryGetProperty(part, out var value))
            {
                break;
            }

            parts.Add(value.GetInt32().ToString());
        }

        return string.Join('.', parts);
    }
}
