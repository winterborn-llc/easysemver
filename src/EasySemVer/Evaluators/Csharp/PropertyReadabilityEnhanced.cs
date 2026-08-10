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
        foreach (var classPair in signatures.ClassHistory)
        {
            var oldClass = classPair.Older;
            var newClass = classPair.Newer;
            foreach (var oldPropertyName in oldClass.Properties.Keys)
            {
                var oldProperty = oldClass.Properties[oldPropertyName];
                if (oldProperty.IsReadable)
                {
                    continue;
                }

                if (!newClass.Properties.Contains(oldPropertyName))
                {
                    continue;
                }

                var newProperty = newClass.Properties[oldPropertyName];
                if (!newProperty.IsReadable)
                {
                    continue;
                }

                yield return $"{newClass.Name}.{oldPropertyName}";
            }
        }
    }
}
