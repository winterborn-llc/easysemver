using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Swift;
using Winterborn.Library.EasySemVer.Evaluation.Swift;
using Winterborn.Library.EasySemVer.Interfaces.Swift;

namespace Winterborn.Library.EasySemVer.Evaluators.Swift;

/// <summary>S06 - a class gained final, so existing subclasses stop compiling.</summary>
public class SwiftClassMadeFinal : IEvaluateSwiftSignatures
{
    public string RuleId => "S06";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "became final";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            if (typePair.Older.IsFinal || !typePair.Newer.IsFinal)
            {
                continue;
            }

            yield return typePair.Newer.Name;
        }
    }
}
