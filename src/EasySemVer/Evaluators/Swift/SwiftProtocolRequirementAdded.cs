using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S20 - a protocol gained a requirement with no default implementation, so every conformer stops compiling.</summary>
public class SwiftProtocolRequirementAdded : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "was added as a protocol requirement with no default";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        return SwiftProtocolRequirements.GetAddedRequirements(
            signatures, withDefaultImplementation: false);
    }
}
