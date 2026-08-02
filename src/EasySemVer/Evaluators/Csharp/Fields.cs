using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

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
