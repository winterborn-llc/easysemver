using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R20 - an interface gained a requirement with no default implementation. Every existing
/// implementer stops compiling, so this is breaking.
/// </summary>
public class InterfaceRequirementAdded : IEvaluateCsharpSignatures
{
    public string RuleId => "R20";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "was added as an interface requirement with no default";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        return InterfaceRequirements.GetAddedRequirements(
            signatures, withDefaultImplementation: false);
    }
}
