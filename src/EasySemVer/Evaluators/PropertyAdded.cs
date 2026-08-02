using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluators;

public class PropertyAdded : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ISignaturesToCompare signatures)
    {
        var classes = signatures.ClassHistory;
        foreach (var classPair in classes)
        {
            var oldClass = classPair.Older;
            var newClass = classPair.Newer;
            foreach (var newPropertyName in newClass.Properties.Keys)
            {
                if (oldClass.Properties.Contains(newPropertyName))
                {
                    continue;
                }

                return true;
            }
        }

        return false;
    }
}
