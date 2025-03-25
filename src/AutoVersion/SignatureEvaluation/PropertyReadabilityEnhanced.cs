using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion.SignatureEvaluation;

public class PropertyReadabilityEnhanced : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(Signatures signatures)
    {
        var classes = signatures.GetClassesInBoth();
        foreach (var classPair in classes)
        {
            var oldClass = classPair.Older;
            var newClass = classPair.Newer;
            foreach (var oldPropertyName in oldClass.Properties.Keys)
            {
                var oldProperty = oldClass.Properties[oldPropertyName];
                if (oldProperty.IsReadable)
                {
                    continue;
                }
                
                if (!newClass.Properties.ContainsKey(oldPropertyName))
                {
                    continue;
                }
                
                var newProperty = newClass.Properties[oldPropertyName];
                if (!newProperty.IsReadable)
                {
                    continue;
                }
                    
                return true;
            }
        }

        return false;
    }
}