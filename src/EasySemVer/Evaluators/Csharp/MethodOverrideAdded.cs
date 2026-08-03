using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>R04 - an existing method gained an overload.</summary>
internal class MethodOverrideAdded : IEvaluateCsharpSignatures
{
    public string RuleId => "R04";

    public VersionType EvaluationImpact => VersionType.Minor;

    public string ChangeDescription => "was added as an overload";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var classPair in signatures.ClassHistory)
        {
            var oldClass = classPair.Older;
            var newClass = classPair.Newer;
            foreach (var newMethodName in newClass.Methods.Keys)
            {
                if (!oldClass.Methods.Contains(newMethodName))
                {
                    continue;
                }

                var newMethod = newClass.Methods[newMethodName];
                var oldMethod = oldClass.Methods[newMethodName];
                foreach (var newOverride in newMethod.Overrides)
                {
                    var newOverrideSignature = newOverride.GetMethodSignature();
                    if (oldMethod.Overrides.Contains(newOverrideSignature))
                    {
                        continue;
                    }

                    yield return $"{newClass.Name}.{newMethodName}({newOverrideSignature})";
                }
            }
        }
    }
}
