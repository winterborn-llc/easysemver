using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S01 - a public type is gone from the module.</summary>
public class TypeRemoved : IEvaluateSwiftSignatures
{
    public string Rule => "TypeRemoved";

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
