using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S25 - a declaration became unavailable or gained an obsoleted availability.</summary>
public class DeclarationWithdrawn : IEvaluateSwiftSignatures
{
    public string Rule => "DeclarationWithdrawn";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "became unavailable or obsoleted";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var pair in SwiftMembers.GetPairedDeclarations(signatures))
        {
            if (SwiftAvailabilityFacts.IsWithdrawn(pair.Older))
            {
                continue;
            }

            if (!SwiftAvailabilityFacts.IsWithdrawn(pair.Newer))
            {
                continue;
            }

            yield return pair.Newer.Name;
        }
    }
}
