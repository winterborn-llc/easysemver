using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S02 - a public type appeared.</summary>
public class SwiftTypeAdded : IEvaluateSwiftSignatures
{
    public string RuleId => "S02";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "was added";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var newerType in signatures.Newer.Types)
        {
            if (SwiftSignaturesToCompare.FindType(signatures.Older, newerType.Name) != null)
            {
                continue;
            }

            yield return newerType.Name;
        }
    }
}
