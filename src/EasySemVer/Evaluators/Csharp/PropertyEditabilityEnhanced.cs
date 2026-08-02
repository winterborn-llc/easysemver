using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

public class PropertyEditabilityEnhanced : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        var classes = signatures.ClassHistory;
        foreach (var classPair in classes)
        {
            var oldClass = classPair.Older;
            var newClass = classPair.Newer;
            foreach (var oldPropertyName in oldClass.Properties.Keys)
            {
                var oldProperty = oldClass.Properties[oldPropertyName];
                if (!newClass.Properties.Contains(oldPropertyName))
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