using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R21 - an interface gained a requirement that carries a default implementation. Existing
/// implementers keep compiling, so this is additive.
/// </summary>
public class InterfaceRequirementAddedWithDefault : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        return InterfaceRequirements.WasRequirementAdded(signatures, withDefaultImplementation: true);
    }
}
