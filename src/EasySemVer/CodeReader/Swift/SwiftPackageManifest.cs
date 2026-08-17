namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// SWD-01 - a package's targets, read from the text of its Package.swift.
/// <para>
/// A manifest is executable Swift, and the toolchain's own answer used to be asked for by running
/// <c>swift package dump-package</c>. That compiles the manifest and resolves the package's
/// dependency graph before it will answer, which needs a toolchain, a network, and credentials for
/// every private dependency - three things a versioning run has no business requiring. What is
/// actually wanted from it is a list of names, and those are literals in the file.
/// </para>
/// <para>
/// The cost is that a target whose name is computed rather than written is not seen. That is rare,
/// it is visible in the log rather than silent, and it fails towards discovering nothing rather
/// than towards discovering something wrong.
/// </para>
/// </summary>
internal static class SwiftPackageManifest
{
    /// <summary>
    /// SWD-04 - the target kinds that are first-party source, and therefore units. Macros, system
    /// libraries, binaries and plugins are not: nothing in them is this package's API surface.
    /// </summary>
    private static readonly (string Call, bool IsTest)[] SourceTargetCalls =
    [
        (".target(", false),
        (".executableTarget(", false),
        (".testTarget(", true)
    ];

    internal static IReadOnlyList<SwiftPackageTarget> Read(string manifestText)
    {
        // Blanked for structure only: a brace or a comma inside a comment or a string must not be
        // read as one, while the names themselves are read back out of the original text.
        var structure = SwiftSourceText.Blank(manifestText);
        var targets = new List<SwiftPackageTarget>();

        foreach (var call in SourceTargetCalls)
        {
            var index = 0;
            while (true)
            {
                index = structure.IndexOf(call.Call, index, StringComparison.Ordinal);
                if (index < 0)
                {
                    break;
                }

                var open = index + call.Call.Length - 1;
                var close = SwiftText.FindMatching(structure, open);
                if (close < 0)
                {
                    break;
                }

                var target = ReadTarget(manifestText, structure, open + 1, close, call.IsTest);
                if (target != null)
                {
                    targets.Add(target);
                }

                index = close;
            }
        }

        targets.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
        return targets;
    }

    private static SwiftPackageTarget? ReadTarget(
        string manifestText,
        string structure,
        int start,
        int end,
        bool isTest)
    {
        var arguments = ReadArguments(manifestText, structure, start, end);
        var name = GetString(arguments, "name");
        if (name.Length < 1)
        {
            Log.WriteLine(
                "Skipping a Package.swift target whose name is not written as a literal; "
                + "EasySemVer reads the manifest as text and cannot evaluate it.");
            return null;
        }

        return new SwiftPackageTarget
        {
            Name = name,
            IsTest = isTest,
            Path = GetString(arguments, "path"),
            Sources = GetStrings(arguments, "sources"),
            Excluded = GetStrings(arguments, "exclude")
        };
    }

    /// <summary>
    /// The call's arguments, keyed by label. Split on the blanked copy so that a comma inside a
    /// nested call or a string does not end an argument, then sliced out of the original.
    /// </summary>
    private static Dictionary<string, string> ReadArguments(
        string manifestText,
        string structure,
        int start,
        int end)
    {
        var arguments = new Dictionary<string, string>(StringComparer.Ordinal);
        var depth = 0;
        var argumentStart = start;
        for (var index = start; index <= end; index++)
        {
            if (index == end || (structure[index] == ',' && depth < 1))
            {
                Add(arguments, manifestText[argumentStart..index]);
                argumentStart = index + 1;
                continue;
            }

            switch (structure[index])
            {
                case '(' or '[' or '{':
                    depth++;
                    break;

                case ')' or ']' or '}':
                    depth--;
                    break;
            }
        }

        return arguments;
    }

    private static void Add(Dictionary<string, string> arguments, string argument)
    {
        var separator = argument.IndexOf(':');
        if (separator < 0)
        {
            return;
        }

        var label = argument[..separator].Trim();
        if (label.Length < 1)
        {
            return;
        }

        arguments[label] = argument[(separator + 1)..].Trim();
    }

    private static string GetString(Dictionary<string, string> arguments, string label)
    {
        return arguments.TryGetValue(label, out var value) ? Unquote(value) : string.Empty;
    }

    private static IReadOnlyList<string> GetStrings(Dictionary<string, string> arguments, string label)
    {
        if (!arguments.TryGetValue(label, out var value)
            || !value.StartsWith('[')
            || !value.EndsWith(']'))
        {
            return [];
        }

        var values = new List<string>();
        foreach (var element in SwiftText.SplitTopLevel(value[1..^1], ','))
        {
            var unquoted = Unquote(element);
            if (unquoted.Length < 1)
            {
                continue;
            }

            values.Add(unquoted);
        }

        return values;
    }

    /// <summary>A literal, or nothing at all: an expression is not something this can evaluate.</summary>
    private static string Unquote(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length > 1 && trimmed.StartsWith('"') && trimmed.EndsWith('"')
            ? trimmed[1..^1]
            : string.Empty;
    }
}
