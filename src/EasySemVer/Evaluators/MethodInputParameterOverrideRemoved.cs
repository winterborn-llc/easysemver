using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluators;

internal class MethodInputParameterOverrideRemoved : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;
    
    public bool AreDifferencesPresent(ISignaturesToCompare signatures)
    {
        var classes = signatures.ClassHistory;
        foreach (var classPair in classes)
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
                    
                    return true;
                }
            }
        }

        return false;
    }
    
    private static bool DoesOldOverrideExistInNew(
        IMethodOverride oldOverride, 
        IMethod newMethod)
    {
        foreach (var newOverride in newMethod.Overrides)
        {
            if (newOverride.Count != oldOverride.Count)
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
        IMethodOverride oldOverride,
        IMethodOverride newOverride)
    {
        for (var i = 0; i < oldOverride.Count; i++)
        {
            var oldParam = oldOverride[i];
            var newParam = newOverride[i];
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