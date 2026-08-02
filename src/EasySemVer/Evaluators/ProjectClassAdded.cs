using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluators;

public class ProjectClassAdded : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ISignaturesToCompare signatures)
    {
        var oldSignature = signatures.Older;
        var newSignature = signatures.Newer;
        foreach (var newProject in newSignature)
        {
            var oldProject = oldSignature.FirstOrDefault(p => p.Name == newProject.Name);
            if (oldProject == null)
            {
                continue;
            }

            foreach (var newClass in newProject.Classes)
            {
                var oldClass = oldProject.Classes.FirstOrDefault(p => p.Name == newClass.Name);
                if (oldClass != null)
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }
}