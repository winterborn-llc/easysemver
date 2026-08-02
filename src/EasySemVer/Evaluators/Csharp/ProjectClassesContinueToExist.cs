using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

public class ProjectClassesContinueToExist : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        var oldSignature = signatures.Older;
        var newSignature = signatures.Newer;
        foreach (var oldProject in oldSignature)
        {
            var newProject = newSignature.FirstOrDefault(p => p.Name == oldProject.Name);
            if (newProject == null)
            {
                continue;
            }

            foreach (var oldClass in oldProject.Classes)
            {
                var newClass = newProject?.Classes.FirstOrDefault(c => c.Name == oldClass.Name);
                if (newClass != null)
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }
}