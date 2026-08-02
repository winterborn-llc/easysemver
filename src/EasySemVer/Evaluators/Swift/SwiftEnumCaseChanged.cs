using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S19 - an enum case was removed or renamed, or its associated values or raw value changed.</summary>
public class SwiftEnumCaseChanged : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var enumPair in SwiftEnums.GetPaired(signatures))
        {
            foreach (var olderCase in enumPair.Older.Cases)
            {
                var newerCase = SwiftMembers.FindCase(enumPair.Newer, olderCase.Name);
                if (newerCase == null)
                {
                    return true;
                }

                if (newerCase.RawValue != olderCase.RawValue)
                {
                    return true;
                }

                if (!SwiftEnums.AreAssociatedValuesTheSame(olderCase, newerCase))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
