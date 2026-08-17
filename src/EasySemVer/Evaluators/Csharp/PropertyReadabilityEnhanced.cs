using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>R11 - a write-only property gained a getter.</summary>
public class PropertyReadabilityEnhanced : IEvaluateCsharpSignatures
{
    public string Rule => "PropertyReadabilityEnhanced";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "became readable";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var pair in Properties.GetPaired(signatures))
        {
            if (pair.Older.IsReadable)
            {
                continue;
            }

            if (!pair.Newer.IsReadable)
            {
                continue;
            }

            yield return $"{pair.DeclaringType.Name}.{pair.Newer.Name}";
        }
    }
}
