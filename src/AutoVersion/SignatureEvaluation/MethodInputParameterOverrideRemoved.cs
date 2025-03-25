using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion.SignatureEvaluation;

public class MethodInputParameterOverrideRemoved : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;

    public bool AreDifferencesPresent(Signatures signatures)
    {
        var classes = signatures.GetClassesInBoth();
        foreach (var classPair in classes)
        {
            var oldClass = classPair.Older;
            var newClass = classPair.Newer;
            foreach (var oldMethodName in oldClass.Methods.Keys)
            {
                var oldMethod = oldClass.Methods[oldMethodName];
                if (!newClass.Methods.ContainsKey(oldMethod.MethodName))
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
    
    private static bool DoesOldOverrideExistInNew(MethodOverride oldOverride, Method newMethod)
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
        MethodOverride oldOverride,
        MethodOverride newOverride)
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