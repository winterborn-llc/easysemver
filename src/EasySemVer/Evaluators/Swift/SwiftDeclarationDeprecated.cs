using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S26 - a declaration was marked deprecated. Nothing stops compiling, so on its own this is a Patch; anything else that changed alongside it will out-rank it (CLS-03).</summary>
public class SwiftDeclarationDeprecated : IEvaluateSwiftSignatures
{
    public string RuleId => "S26";

    public VersionType EvaluationImpact => VersionType.Patch;

    public string ChangeDescription => "was marked deprecated";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var pair in SwiftMembers.GetPairedDeclarations(signatures))
        {
            if (SwiftAvailabilityFacts.IsDeprecated(pair.Older))
            {
                continue;
            }

            if (!SwiftAvailabilityFacts.IsDeprecated(pair.Newer))
            {
                continue;
            }

            yield return pair.Newer.Name;
        }
    }
}
