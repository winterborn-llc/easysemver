using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.DataObject.Csharp;
using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

internal class MethodOverrideAdded : IEvaluateCsharpSignatures
{
    public VersionType EvaluationImpact => VersionType.Minor;

    public bool AreDifferencesPresent(ICsharpSignaturesToCompare signatures)
    {
        var classes = signatures.ClassHistory;
        foreach (var classPair in classes)
        {
            var oldClass = classPair.Older;
            var newClass = classPair.Newer;
            foreach (var newMethodName in newClass.Methods.Keys)
            {
                if (!oldClass.Methods.Contains(newMethodName))
                {
                    continue;
                }
                
                var newMethod = newClass.Methods[newMethodName];   
                var oldMethod = oldClass.Methods[newMethodName];
                foreach(var newOverride in newMethod.Overrides)
                {
                    var newOverrideSignature = newOverride.GetMethodSignature();
                    if (oldMethod.Overrides.Contains(newOverrideSignature))
                    {
                        continue;
                    }
                    
                    return true;
                }
            }
        }
        
        return false;
    }
}