using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S01 - a public type is gone from the module.</summary>
public class SwiftTypeRemoved : IEvaluateSwiftSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "was removed";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var olderType in signatures.Older.Types)
        {
            if (SwiftSignaturesToCompare.FindType(signatures.Newer, olderType.Name) != null)
            {
                continue;
            }

            yield return olderType.Name;
        }
    }
}
