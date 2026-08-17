using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>R08 - a read-only property gained a setter.</summary>
public class PropertyEditabilityEnhanced : IEvaluateCsharpSignatures
{
    public string Rule => "PropertyEditabilityEnhanced";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "became writable";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var pair in Properties.GetPaired(signatures))
        {
            if (pair.Older.IsWritable)
            {
                continue;
            }

            if (!pair.Newer.IsWritable)
            {
                continue;
            }

            yield return $"{pair.DeclaringType.Name}.{pair.Newer.Name}";
        }
    }
}
