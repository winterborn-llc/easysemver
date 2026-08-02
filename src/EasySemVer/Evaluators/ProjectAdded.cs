using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluators;

public class ProjectAdded : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ISignaturesToCompare signatures)
    {
        var oldSignature = signatures.Older;
        var newSignature = signatures.Newer;
        foreach (var newProject in newSignature)
        {
            var oldProject = oldSignature.FirstOrDefault(p => p.Name == newProject.Name);
            if (oldProject != null)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}