using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R35 - a public type started implementing another interface.</summary>
public class ImplementedInterfaceAdded : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            foreach (var newerInterface in typePair.Newer.ImplementedInterfaces)
            {
                if (typePair.Older.ImplementedInterfaces.Contains(newerInterface))
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }
}
