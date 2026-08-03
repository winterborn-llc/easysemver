using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R15 - a type that exists on both sides gained a method.</summary>
public class MethodAdded : IEvaluateCsharpSignatures
{
    public string RuleId => "R15";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "was added";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var classPair in signatures.ClassHistory)
        {
            foreach (var newMethodName in classPair.Newer.Methods.Keys)
            {
                if (classPair.Older.Methods.Contains(newMethodName))
                {
                    continue;
                }

                yield return $"{classPair.Newer.Name}.{newMethodName}";
            }
        }
    }
}
