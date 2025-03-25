using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion.SignatureEvaluation;

public class MethodsContinueToExist : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;
    
    public bool AreDifferencesPresent(Signatures signatures)
    {
        var classes = signatures.GetClassesInBoth();
        foreach (var classPair in classes)
        {
            var oldClass = classPair.Older;
            var newClass = classPair.Newer;
            if (DoAllMethodsStillExist(oldClass, newClass))
            {
                continue;
            }
            
            return true;
        }

        return false;
    }

    internal static bool DoAllMethodsStillExist(ProjectClass oldClass, ProjectClass newClass)
    {
        foreach (var oldMethodName in oldClass.Methods.Keys)
        {
            if (newClass.Methods.ContainsKey(oldMethodName))
            {
                continue;
            }

            return false;
        }
        
        return true;
    }
}