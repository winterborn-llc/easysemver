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
        foreach (var classPair in signatures.ClassHistory)
        {
            var oldClass = classPair.Older;
            var newClass = classPair.Newer;
            foreach (var oldPropertyName in oldClass.Properties.Keys)
            {
                var oldProperty = oldClass.Properties[oldPropertyName];
                if (!newClass.Properties.Contains(oldPropertyName))
                {
                    continue;
                }

                var newProperty = newClass.Properties[oldPropertyName];
                if (oldProperty.IsWritable)
                {
                    continue;
                }

                if (!newProperty.IsWritable)
                {
                    continue;
                }

                yield return $"{newClass.Name}.{oldPropertyName}";
            }
        }
    }
}
