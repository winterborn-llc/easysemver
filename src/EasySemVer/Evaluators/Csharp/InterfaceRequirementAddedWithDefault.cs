using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R21 - an interface gained a requirement that carries a default implementation. Existing
/// implementers keep compiling, so this is additive.
/// </summary>
public class InterfaceRequirementAddedWithDefault : IEvaluateCsharpSignatures
{
    public string RuleId => "R21";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "was added as an interface requirement with a default";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        return InterfaceRequirements.GetAddedRequirements(
            signatures, withDefaultImplementation: true);
    }
}
