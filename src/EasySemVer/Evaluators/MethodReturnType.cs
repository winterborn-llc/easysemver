using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluators;

internal class MethodReturnType : IEvaluateSignatures
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
                if (!newClass.Methods.Contains(oldMethodName))
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