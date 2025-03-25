using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion.SignatureEvaluation;

public class PropertiesContinueToExist : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;
    
    public bool AreDifferencesPresent(Signatures signatures)
    {
        var classes = signatures.GetClassesInBoth();
        foreach (var classPair in classes)
        {
            var oldClass = classPair.Older;
            var newClass = classPair.Newer;
            foreach (var oldPropertyName in oldClass.Properties.Keys)
            {
                if (newClass.Properties.ContainsKey(oldPropertyName))
                {
                    continue;
                }
                
                return true;
            }
        }
        
        return false;
    }
}