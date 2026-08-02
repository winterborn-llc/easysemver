using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R06 - a public class the baseline recorded is gone, renamed, or moved namespace.</summary>
public class ProjectClassesContinueToExist : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        foreach (var olderClass in signatures.Older.Classes)
        {
            var newerClass = CsharpSignaturesToCompare.FindType(
                signatures.Newer, olderClass.Name, olderClass.Kind);
            if (newerClass != null)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
