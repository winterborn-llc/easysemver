using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluators;

public class ProjectsContinueToExist : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ISignaturesToCompare signatures)
    {
        var oldSignature = signatures.Older;
        var newSignature = signatures.Newer;
        foreach (var oldProject in oldSignature)
        {
            var newProject = newSignature.FirstOrDefault(p => p.Name == oldProject.Name);
            if (newProject != null)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}