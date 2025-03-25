using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion.SignatureEvaluation;

public class PropertyReadabilityReduced : IEvaluateSignatures
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
                var oldProperty = classPair.Older.Properties[oldPropertyName];
                if (!newClass.Properties.ContainsKey(oldPropertyName))
                {
                    continue;
                }
                
                var newProperty = classPair.Newer.Properties[oldPropertyName];
                if (newProperty.IsReadable)
                {
                    continue;
                }

                if (!oldProperty.IsReadable)
                {
                    continue;
                }
                    
                return true;
            }
        }

        return false;
    }
}