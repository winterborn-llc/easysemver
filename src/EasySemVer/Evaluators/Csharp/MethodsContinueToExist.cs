using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R01 - a method the baseline recorded is gone from a type that still exists.</summary>
public class MethodsContinueToExist : IEvaluateCsharpSignatures
{
    public string RuleId => "R01";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "was removed";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var classPair in signatures.ClassHistory)
        {
            foreach (var oldMethodName in classPair.Older.Methods.Keys)
            {
                if (classPair.Newer.Methods.Contains(oldMethodName))
                {
                    continue;
                }

                yield return $"{classPair.Older.Name}.{oldMethodName}";
            }
        }
    }
}
