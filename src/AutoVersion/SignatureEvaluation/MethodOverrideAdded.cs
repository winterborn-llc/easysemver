using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion.SignatureEvaluation;

public class MethodOverrideAdded : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(Signatures signatures)
    {
        var classes = signatures.GetClassesInBoth();
        foreach (var classPair in classes)
        {
            var oldClass = classPair.Older;
            var newClass = classPair.Newer;
            foreach (var newMethodName in newClass.Methods.Keys)
            {
                if (!oldClass.Methods.ContainsKey(newMethodName))
                {
                    continue;
                }
             
                var newMethod = newClass.Methods[newMethodName];   
                var oldMethod = oldClass.Methods[newMethodName];
                foreach(var newOverride in newMethod.Overrides)
                {
                    if (!IsTheNewOverrideInTheOldMethod(newOverride, oldMethod))
                    {
                        continue;
                    }
                    
                    return true;
                }
            }
        }
        
        return false;
    }
    
    private static bool IsTheNewOverrideInTheOldMethod(MethodOverride newOverride,
        Method oldMethod)
    {
        var newOverrideSignature = newOverride.MethodSignature;
        var oldOverride =
            oldMethod.Overrides.FirstOrDefault(o => o.MethodSignature == newOverrideSignature);
        if (oldOverride != null)
        {
            return false;
        }
        
        return true;
    }
}