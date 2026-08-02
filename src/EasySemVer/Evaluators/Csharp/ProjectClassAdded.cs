using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

public class ProjectClassAdded : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
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