using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion.SignatureEvaluation;

public class ProjectsContinueToExist : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(Signatures signatures)
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