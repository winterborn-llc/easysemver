using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

internal static class Events
{
    internal static ICsharpEvent? Find(ICsharpType type, string name)
    {
        foreach (var candidate in type.Events)
        {
            if (candidate.Name != name)
            {
                continue;
            }

            return candidate;
        }

        return null;
    }
}
