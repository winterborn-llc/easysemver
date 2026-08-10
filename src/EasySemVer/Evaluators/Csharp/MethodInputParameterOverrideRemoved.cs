using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Extensions;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>R02 - an overload the baseline recorded has no match in the new signature.</summary>
internal class MethodInputParameterOverrideRemoved : IEvaluateCsharpSignatures
{
    public string Rule => "MethodInputParameterOverrideRemoved";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "no longer has a matching overload";

    public IEnumerable<string> FindDifferences(ICsharpSignaturesToCompare signatures)
    {
        foreach (var classPair in signatures.ClassHistory)
        {
            var oldClass = classPair.Older;
            var newClass = classPair.Newer;
            foreach (var oldMethodName in oldClass.Methods.Keys)
            {
                var oldMethod = oldClass.Methods[oldMethodName];
                if (!newClass.Methods.Contains(oldMethod.MethodName))
                {
                    continue;
                }

                var newMethod = newClass.Methods[oldMethodName];
                foreach (var oldOverride in oldMethod.Overrides)
                {
                    if (DoesOldOverrideExistInNew(oldOverride, newMethod))
                    {
                        continue;
                    }

                    yield return
                        $"{oldClass.Name}.{oldMethodName}({oldOverride.GetMethodSignature()})";
                }
            }
        }
    }

    private static bool DoesOldOverrideExistInNew(
        ICsharpMethodOverride oldOverride,
        ICsharpMethod newMethod)
    {
        foreach (var newOverride in newMethod.Overrides)
        {
            if (newOverride.Parameters.Count != oldOverride.Parameters.Count)
            {
                continue;
            }

            var isFound = DoesNewOverrideMatchOld(oldOverride, newOverride);
            if (!isFound)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool DoesNewOverrideMatchOld(
        ICsharpMethodOverride oldOverride,
        ICsharpMethodOverride newOverride)
    {
        for (var i = 0; i < oldOverride.Parameters.Count; i++)
        {
            var oldParam = oldOverride.Parameters[i];
            var newParam = newOverride.Parameters[i];
            if (oldParam.ParameterName != newParam.ParameterName)
            {
                return false;
            }

            if (oldParam.ParameterType != newParam.ParameterType)
            {
                return false;
            }
        }

        return true;
    }
}
