using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion.SignatureEvaluation;

public class MethodReturnType : IEvaluateSignatures
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
                if (!newClass.Methods.ContainsKey(oldMethodName))
                {
                    continue;
                }
                
                var newMethod = newClass.Methods[oldMethodName];
                if (oldMethod.MethodType == newMethod.MethodType)
                {
                    continue;
                }
                    
                return true;
            }
        }

        return false;
    }
}