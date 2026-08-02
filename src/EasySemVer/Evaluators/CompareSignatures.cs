using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation;
using Winterborn.Library.EasySemVer.Interfaces;

namespace Winterborn.Library.EasySemVer.Evaluators;

internal static class CompareSignatures
{
    private static readonly IEvaluateSignatures[] Evaluators =
    [
        new MethodsContinueToExist(),
        new MethodAdded(),
        new MethodInputParameterOverrideRemoved(),
        new MethodInputParameterMadeRequired(),
        new MethodReturnType(),
        new MethodOverrideAdded(),
        new ProjectClassAdded(),
        new ProjectClassesContinueToExist(),
        new ProjectsContinueToExist(),
        new PropertyEditabilityEnhanced(),
        new PropertyEditabilityReduced(),
        new PropertiesContinueToExist(),
        new PropertyAdded(),
        new PropertyReadabilityEnhanced(),
        new PropertyReadabilityReduced(),
        new PropertyType(),
        new ProjectAdded()
    ];
    
    internal static VersionType GetChangeType(ISignaturesToCompare? signatures)
    {
        var changeType = CalculateChangeType(signatures?.Older, signatures?.Newer);
        Log.WriteLine($"Change Type: {changeType.ToString()}");
        return changeType;
    }
    
    private static VersionType CalculateChangeType(
        ISolution? oldSignature, 
        ISolution? newSignature)
    {
        if (oldSignature is null || newSignature is null)
        {
            return VersionType.Minor;
        }
        
        var changeType = VersionType.Patch;
        var signatures = new SignaturesToCompare("", oldSignature, newSignature);
        foreach(var evaluator in Evaluators)
        {
            if (!evaluator.AreDifferencesPresent(signatures))
            {
                continue;
            }
            
            Log.WriteLine($"Yay differences: {evaluator.GetType().Name}");
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