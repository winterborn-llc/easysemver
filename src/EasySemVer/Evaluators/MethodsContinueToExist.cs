using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluators;

public class MethodsContinueToExist : IEvaluateSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;
    
    public bool AreDifferencesPresent(ISignaturesToCompare signatures)
    {
        var classes = signatures.ClassHistory;
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

    internal static bool DoAllMethodsStillExist(
        IProjectClass oldClass, 
        IProjectClass newClass)
    {
        foreach (var oldMethodName in oldClass.Methods.Keys)
        {
            if (newClass.Methods.Contains(oldMethodName))
            {
                continue;
            }

            return false;
        }
        
        return true;
    }
}