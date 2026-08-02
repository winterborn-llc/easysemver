using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

public class MethodsContinueToExist : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Major;
    
    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
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
        ICsharpType oldClass,
        ICsharpType newClass)
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