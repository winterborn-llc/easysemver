using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S20 - a protocol gained a requirement with no default implementation, so every conformer stops compiling.</summary>
public class SwiftProtocolRequirementAdded : IEvaluateSwiftSignatures
{
    public string RuleId => "S20";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "was added as a protocol requirement with no default";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        return SwiftProtocolRequirements.GetAddedRequirements(
            signatures, withDefaultImplementation: false);
    }
}
