using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R05 - the unit gained a public class the baseline did not have.</summary>
public class ProjectClassAdded : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var newerClass in signatures.Newer.Classes)
        {
            var olderClass = CsharpSignaturesToCompare.FindClass(signatures.Older, newerClass.Name);
            if (olderClass != null)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
