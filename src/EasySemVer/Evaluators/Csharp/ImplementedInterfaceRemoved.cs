using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R34 - a public type stopped implementing an interface, so every cast to it fails.</summary>
public class ImplementedInterfaceRemoved : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            foreach (var olderInterface in typePair.Older.ImplementedInterfaces)
            {
                if (typePair.Newer.ImplementedInterfaces.Contains(olderInterface))
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }
}
