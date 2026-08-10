using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.DataObject.Swift;
using Winterborn.Tools.EasySemVer.Evaluation.Swift;
using Winterborn.Tools.EasySemVer.Interfaces.Swift;

namespace Winterborn.Tools.EasySemVer.Evaluators.Swift;

/// <summary>S08 - a superclass was changed or removed.</summary>
public class SuperclassChanged : IEvaluateSwiftSignatures
{
    public string Rule => "SuperclassChanged";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "changed or lost its superclass";

    public IEnumerable<string> FindDifferences(ISwiftSignaturesToCompare signatures)
    {
        foreach (var typePair in signatures.TypeHistory)
        {
            if (typePair.Older.Superclass == typePair.Newer.Superclass)
            {
                continue;
            }

            yield return typePair.Newer.Name;
        }
    }
}
