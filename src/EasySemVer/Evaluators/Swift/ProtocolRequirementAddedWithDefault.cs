using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S21 - a protocol gained a requirement that an extension already satisfies.</summary>
public class ProtocolRequirementAddedWithDefault : IEvaluateSwiftSignatures
{
    public string Rule => "ProtocolRequirementAddedWithDefault";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "was added as a protocol requirement with a default";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        return SwiftProtocolRequirements.GetAddedRequirements(
            signatures, withDefaultImplementation: true);
    }
}
