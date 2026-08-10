using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S06 - a class gained final, so existing subclasses stop compiling.</summary>
public class ClassMadeFinal : IEvaluateSwiftSignatures
{
    public string Rule => "ClassMadeFinal";

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
