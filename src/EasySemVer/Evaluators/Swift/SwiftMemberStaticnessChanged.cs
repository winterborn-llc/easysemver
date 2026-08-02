using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S34 - a member moved between static and instance, in either direction.</summary>
public class SwiftMemberStaticnessChanged : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var functionPair in SwiftMembers.GetPairedFunctions(signatures))
        {
            if (functionPair.Older.IsStatic != functionPair.Newer.IsStatic)
            {
                return true;
            }
        }

        foreach (var propertyPair in SwiftMembers.GetPairedProperties(signatures))
        {
            if (propertyPair.Older.IsStatic != propertyPair.Newer.IsStatic)
            {
                return true;
            }
        }

        return false;
    }
}
