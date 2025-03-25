using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion.SignatureEvaluation;

public interface IEvaluateSignatures
{
    public VersionType EvaluationImpact { get; }
    
    public bool AreDifferencesPresent(Signatures signatures);
}