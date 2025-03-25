using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion.SignatureEvaluation;

public class PropertyEditabilityEnhanced : IEvaluateSignatures
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
                if (!newClass.Properties.ContainsKey(oldPropertyName))
                {
                    continue;
                }

                var newProperty = newClass.Properties[oldPropertyName];
                if (oldProperty.IsWritable)
                {
                    continue;
                }

                if (!newProperty.IsWritable)
                {
                    continue;
                }
                    
                return true;
            }
        }

        return false;
    }
}