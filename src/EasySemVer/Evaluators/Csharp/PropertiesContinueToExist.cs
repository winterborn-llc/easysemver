using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>R10 - a property the baseline recorded is gone from a type that still exists.</summary>
public class PropertiesContinueToExist : IEvaluateCsharpSignatures
{
    public string Rule => "PropertiesContinueToExist";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "was removed";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var classPair in signatures.ClassHistory)
        {
            foreach (var oldPropertyName in classPair.Older.Properties.Keys)
            {
                if (classPair.Newer.Properties.Contains(oldPropertyName))
                {
                    continue;
                }

                yield return $"{classPair.Older.Name}.{oldPropertyName}";
            }
        }
    }
}
