using Winterborn.Tools.EasySemVer.DataObject;
using Winterborn.Tools.EasySemVer.Extensions;
using Winterborn.Tools.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Tools.EasySemVer.Evaluators.Csharp;

/// <summary>R17 - a parameter that used to be optional now has to be passed.</summary>
internal class MethodInputParameterMadeRequired : IEvaluateCsharpSignatures
{
    public string RuleId => "R17";

    public VersionType EvaluationImpact => VersionType.Major;

    public string ChangeDescription => "made an optional parameter required";

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
                    if (!IsAnyParameterMadeRequired(oldOverride, newMethod))
                    {
                        continue;
                    }

                    yield return
                        $"{oldClass.Name}.{oldMethodName}({oldOverride.GetMethodSignature()})";
                }
            }
        }
    }

    private static bool IsAnyParameterMadeRequired(
        ICsharpMethodOverride oldOverride,
        ICsharpMethod newMethod)
    {
        foreach (var newOverride in newMethod.Overrides)
        {
            if (newOverride.Parameters.Count != oldOverride.Parameters.Count)
            {
                continue;
            }

            if (!DoesNewOverrideMatchOld(oldOverride, newOverride))
            {
                continue;
            }

            if (!DidRequirednessTighten(oldOverride, newOverride))
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

    private static bool DidRequirednessTighten(
        ICsharpMethodOverride oldOverride,
        ICsharpMethodOverride newOverride)
    {
        for (var i = 0; i < oldOverride.Parameters.Count; i++)
        {
            var oldParam = oldOverride.Parameters[i];
            var newParam = newOverride.Parameters[i];
            if (oldParam.IsRequired)
            {
                continue;
            }

            if (!newParam.IsRequired)
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
