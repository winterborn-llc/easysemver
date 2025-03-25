using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion.SignatureEvaluation;

public class ProjectAdded : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(Signatures signatures)
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