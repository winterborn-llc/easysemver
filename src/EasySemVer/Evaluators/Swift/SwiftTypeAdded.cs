using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S02 - a public type appeared.</summary>
public class SwiftTypeAdded : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        foreach (var newerType in signatures.Newer.Types)
        {
            if (SwiftSignaturesToCompare.FindType(signatures.Older, newerType.Name) != null)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
