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
                if (newProperty.IsReadable)
                {
                    continue;
                }

                if (!oldProperty.IsReadable)
                {
                    continue;
                }

                yield return $"{newClass.Name}.{oldPropertyName}";
            }
        }
    }
}
