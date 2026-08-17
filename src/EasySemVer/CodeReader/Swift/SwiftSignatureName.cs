using System.Text;
using Winterborn.Tools.EasySemVer.DataObject.Swift;

namespace Winterborn.Tools.EasySemVer.CodeReader.Swift;

/// <summary>
/// SWE-03 - the identity a declaration is matched by from one run to the next. It is the Swift
/// name including argument labels, qualified by the types it is nested in: "Gadget.move(to:)" and
/// "Gadget.move(toward:)" are two different members, because to a caller they are.
/// </summary>
internal static class SwiftSignatureName
{
    internal static string Qualify(string ownerPath, string name)
    {
        return ownerPath.Length < 1 ? name : $"{ownerPath}.{name}";
    }

    /// <summary>
    /// The name plus its argument labels. An operator takes its arguments positionally whatever
    /// its parameters are called, so every label of one is written as the omitted label.
    /// </summary>
    internal static string ForCallable(
        string name,
        IReadOnlyList<SwiftParameter> parameters,
        bool hasParameterList)
    {
        if (!hasParameterList)
        {
            return name;
        }

        var isOperator = !SwiftText.IsIdentifier(name);
        var labels = new StringBuilder(name);
        labels.Append('(');
        foreach (var parameter in parameters)
        {
            labels.Append(isOperator ? "_" : GetLabel(parameter));
            labels.Append(':');
        }

        labels.Append(')');
        return labels.ToString();
    }

    private static string GetLabel(SwiftParameter parameter)
    {
        return parameter.Label.Length < 1 ? "_" : parameter.Label;
    }
}
