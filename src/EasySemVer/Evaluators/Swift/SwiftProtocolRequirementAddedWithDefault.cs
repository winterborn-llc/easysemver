using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S21 - a protocol gained a requirement that an extension already satisfies.</summary>
public class SwiftProtocolRequirementAddedWithDefault : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "was added as a protocol requirement with a default";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        return SwiftProtocolRequirements.GetAddedRequirements(
            signatures, withDefaultImplementation: true);
    }
}
