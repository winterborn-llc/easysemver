using Microsoft.Build.Framework;
using Yamamari.Library.AutoVersion.Extensions;
using Yamamari.Library.AutoVersion.SignatureStructure;

namespace Yamamari.Library.AutoVersion.SignatureEvaluation;

public static class CompareSignatures
{
    private static readonly IEvaluateSignatures[] Evaluators =
    [
        new MethodsContinueToExist(),
        new MethodInputParameterOverrideRemoved(),
        new MethodReturnType(),
        new MethodOverrideAdded(),
        new ProjectClassAdded(),
        new ProjectClassesContinueToExist(),
        new ProjectsContinueToExist(),
        new PropertyEditabilityEnhanced(),
        new PropertyEditabilityReduced(),
        new PropertiesContinueToExist(),
        new PropertyReadabilityEnhanced(),
        new PropertyReadabilityReduced(),
        new PropertyType(),
        new ProjectAdded()
    ];
    
    public static VersionType GetChangeType(ITask? task, Signature? oldSignature, Signature? newSignature)
    {
        if (oldSignature is null || newSignature is null)
        {
            return VersionType.Minor;
        }
        
        var changeType = VersionType.Patch;
        var signatures = new Signatures(oldSignature, newSignature);
        foreach(var evaluator in Evaluators)
        {
            if (!evaluator.AreDifferencesPresent(signatures))
            {
                continue;
            }
            
            task?.LogInfo($"Yay differences: {evaluator.GetType().Name}");
            if (changeType == VersionType.Patch && evaluator.EvaluationImpact == VersionType.Minor)
            {
                changeType = evaluator.EvaluationImpact;
                continue;
            }
            
            if (changeType == VersionType.Patch && evaluator.EvaluationImpact == VersionType.Major)
            {
                changeType = evaluator.EvaluationImpact;
                continue;
            }
            
            if (changeType == VersionType.Minor && evaluator.EvaluationImpact == VersionType.Major)
            {
                changeType = evaluator.EvaluationImpact;
                continue;
            }
            
            if (changeType == VersionType.Major)
            {
                return VersionType.Major;
            }
        }
        
        return changeType;
    }
}