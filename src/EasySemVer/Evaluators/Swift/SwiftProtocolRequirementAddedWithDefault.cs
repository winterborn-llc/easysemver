using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S21 - a protocol gained a requirement that an extension already satisfies.</summary>
public class SwiftProtocolRequirementAddedWithDefault : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ISwiftSignaturesToCompare signatures)
    {
        return SwiftProtocolRequirements.WasRequirementAdded(signatures, withDefaultImplementation: true);
    }
}
