namespace Winterborn.Library.EasySemVer.CodeReader.Swift;

/// <summary>
/// The small amount of reading the symbol graph forces on us. This is not a Swift parser (D-02):
/// it works on the toolchain's own reassembled declaration text to recover two things the graph
/// does not state as fields - which parameters carry a default, and a parameter's type as written.
/// </summary>
internal static class SwiftDeclarationText
{
    /// <summary>
    /// The indices of parameters that declare a default value. The per-parameter fragments omit
    /// the default; only the full declaration carries it, as trailing " = 0" text.
    /// </summary>
    internal static HashSet<int> GetParametersWithDefaults(string declaration)
    {
        var withDefaults = new HashSet<int>();
        var parameterList = GetParameterList(declaration);
        if (parameterList.Length < 1)
        {
            return withDefaults;
        }

        var index = 0;
        foreach (var parameter in SplitTopLevel(parameterList))
        {
            if (parameter.Contains(" = ", StringComparison.Ordinal))
            {
                withDefaults.Add(index);
            }

            index++;
        }

        return withDefaults;
    }

    /// <summary>The parameter's type, from its own fragments: everything after "name: ".</summary>
    internal static string GetParameterType(string parameterDeclaration)
    {
        var separator = parameterDeclaration.IndexOf(": ", StringComparison.Ordinal);
        return separator < 0
            ? parameterDeclaration.Trim()
            : parameterDeclaration[(separator + 2)..].Trim();
    }

    /// <summary>The text between the first top-level "(" and its matching ")".</summary>
    private static string GetParameterList(string declaration)
    {
        var start = declaration.IndexOf('(');
        if (start < 0)
        {
            return string.Empty;
        }

        var depth = 0;
        for (var i = start; i < declaration.Length; i++)
        {
            switch (declaration[i])
            {
                case '(':
                    depth++;
                    break;
                case ')':
                    depth--;
                    if (depth == 0)
                    {
                        return declaration[(start + 1)..i];
                    }

                    break;
            }
        }

        return string.Empty;
    }

    /// <summary>Splits on commas that are not inside brackets, so tuple and closure types survive.</summary>
    private static List<string> SplitTopLevel(string parameterList)
    {
        var parameters = new List<string>();
        var depth = 0;
        var start = 0;
        for (var i = 0; i < parameterList.Length; i++)
        {
            switch (parameterList[i])
            {
                case '(' or '[' or '<':
                    depth++;
                    break;
                case ')' or ']' or '>':
                    depth--;
                    break;
                case ',' when depth == 0:
                    parameters.Add(parameterList[start..i]);
                    start = i + 1;
                    break;
            }
        }

        if (start < parameterList.Length)
        {
            parameters.Add(parameterList[start..]);
        }

        return parameters;
    }
}
