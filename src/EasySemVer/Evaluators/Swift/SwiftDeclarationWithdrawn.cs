using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S25 - a declaration became unavailable or gained an obsoleted availability.</summary>
public class SwiftDeclarationWithdrawn : IEvaluateSwiftSignatures
{
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
