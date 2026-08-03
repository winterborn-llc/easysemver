using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S03 - a type changed kind: struct to class, enum to struct, and so on.</summary>
public class SwiftTypeKindChanged : IEvaluateSwiftSignatures
{
    public string RuleId => "S03";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "changed kind";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            if (typePair.Older.Kind == typePair.Newer.Kind)
            {
                continue;
            }

            yield return typePair.Newer.Name;
        }
    }
}
