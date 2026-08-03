using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R09 - a property lost its setter.</summary>
public class PropertyEditabilityReduced : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "is no longer writable";

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
                if (newProperty.IsWritable)
                {
                    continue;
                }

                if (!oldProperty.IsWritable)
                {
                    continue;
                }

                yield return $"{newClass.Name}.{oldPropertyName}";
            }
        }
    }
}
