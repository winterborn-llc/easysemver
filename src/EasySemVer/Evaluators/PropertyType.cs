using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluators;

[DebuggerDisplay("{EvaluationImpact}")]
public class PropertyType : IEvaluateSignatures
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