using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>R12 - a property lost its getter.</summary>
public class PropertyReadabilityReduced : IEvaluateCsharpSignatures
{
    public string Rule => "PropertyReadabilityReduced";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "is no longer readable";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var pair in Properties.GetPaired(signatures))
        {
            if (pair.Newer.IsReadable)
            {
                continue;
            }

            if (!pair.Older.IsReadable)
            {
                continue;
            }

            yield return $"{pair.DeclaringType.Name}.{pair.Newer.Name}";
        }
    }
}
