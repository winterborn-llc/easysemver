using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

internal class MethodInputParameterOverrideRemoved : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;
    
    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
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