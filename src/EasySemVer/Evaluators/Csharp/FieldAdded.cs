using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R29 - a public field appeared on an existing type.</summary>
public class FieldAdded : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.ClassHistory)
        {
            foreach (var newerField in typePair.Newer.Fields)
            {
                if (Fields.Find(typePair.Older, newerField.Name) != null)
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }
}
