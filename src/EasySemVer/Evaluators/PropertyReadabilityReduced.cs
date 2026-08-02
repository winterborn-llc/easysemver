using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluators;

public class PropertyReadabilityReduced : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ISignaturesToCompare signatures)
    {
        var classes = signatures.ClassHistory;
        foreach (var classPair in classes)
        {
            var oldClass = classPair.Older;
            var newClass = classPair.Newer;
            foreach (var oldPropertyName in oldClass.Properties.Keys)
            {
                var oldProperty = classPair.Older.Properties[oldPropertyName];
                if (!newClass.Properties.Contains(oldPropertyName))
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