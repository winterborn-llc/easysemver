using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluators;

public class PropertyReadabilityEnhanced : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ISignaturesToCompare signatures)
    {
        var classes = signatures.ClassHistory;
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
                
                if (!newClass.Properties.Contains(oldPropertyName))
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