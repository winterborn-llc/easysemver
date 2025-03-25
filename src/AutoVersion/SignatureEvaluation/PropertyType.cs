using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion.SignatureEvaluation;

public class PropertyType : IEvaluateSignatures
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
                
                var newProperty = newClass.Properties[oldPropertyName];
                if (oldProperty.Type == newProperty.Type)
                {
                    continue;
                }
                    
                return true;
            }
        }

        return false;
    }
}