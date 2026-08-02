using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>The half S20 and S21 share, so the two directions cannot drift apart.</summary>
internal static class SwiftProtocolRequirements
{
    internal static bool WasRequirementAdded(
        ISwiftSignaturesToCompare signatures,
        bool withDefaultImplementation)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            if (typePair.Newer.Kind != SwiftTypeKinds.Protocol)
            {
                continue;
            }

            foreach (var function in typePair.Newer.Functions)
            {
                if (SwiftMembers.FindFunction(typePair.Older, function.Name) != null)
                {
                    continue;
                }

                if (function.HasDefaultImplementation != withDefaultImplementation)
                {
                    continue;
                }

                return true;
            }

            foreach (var property in typePair.Newer.Properties)
            {
                if (SwiftMembers.FindProperty(typePair.Older, property.Name) != null)
                {
                    continue;
                }

                if (property.HasDefaultImplementation != withDefaultImplementation)
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }
}
