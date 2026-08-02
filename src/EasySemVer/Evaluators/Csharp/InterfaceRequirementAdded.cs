using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// R20 - an interface gained a requirement with no default implementation. Every existing
/// implementer stops compiling, so this is breaking.
/// </summary>
public class InterfaceRequirementAdded : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        return InterfaceRequirements.WasRequirementAdded(signatures, withDefaultImplementation: false);
    }
}
