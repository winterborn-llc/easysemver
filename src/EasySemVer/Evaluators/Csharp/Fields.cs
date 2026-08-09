using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

internal static class Fields
{
    internal static ICsharpField? Find(ICsharpType type, string name)
    {
        foreach (var field in type.Fields)
        {
            if (field.Name != name)
            {
                continue;
            }

            return field;
        }

        return null;
    }
}
