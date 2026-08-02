using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S37 - a property's type changed.</summary>
public class SwiftPropertyTypeChanged : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var propertyPair in SwiftMembers.GetPairedProperties(signatures))
        {
            if (propertyPair.Older.Type == propertyPair.Newer.Type)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
