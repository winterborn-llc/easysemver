using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Extensions;
using Winterborn.Library.EasySemVer.Interfaces.Csharp;

namespace Winterborn.Library.EasySemVer.Evaluators.Csharp;

/// <summary>
/// The C# rule list and its aggregation. Runs over one paired unit at a time (NCL-03); unit
/// existence is the neutral core's concern, so the old R07/R14 are gone from here.
/// </summary>
internal static class CompareSignatures
{
    private static readonly IEvaluateCsharpSignatures[] Evaluators =
    [
        new MethodsContinueToExist(),
        new MethodAdded(),
        new MethodInputParameterOverrideRemoved(),
        new MethodInputParameterMadeRequired(),
        new MethodReturnType(),
        new MethodOverrideAdded(),
        new ProjectClassAdded(),
        new ProjectClassesContinueToExist(),
        new PropertyEditabilityEnhanced(),
        new PropertyEditabilityReduced(),
        new PropertiesContinueToExist(),
        new PropertyAdded(),
        new PropertyReadabilityEnhanced(),
        new PropertyReadabilityReduced(),
        new PropertyType(),

        // CSX-05: the fidelity expansion. Everything below here was invisible under G-15.
        new TypeRemoved(),
        new TypeAdded(),
        new InterfaceRequirementAdded(),
        new InterfaceRequirementAddedWithDefault(),
        new EnumMemberRemoved(),
        new EnumMemberAdded(),
        new EnumMemberValueChanged(),
        new EnumUnderlyingTypeChanged(),
        new DelegateSignatureChanged(),
        new RecordPositionalParametersChanged(),
        new FieldContractReduced(),
        new FieldAdded(),
        new EventContractReduced(),
        new EventAdded(),
        new TypeInheritanceRestricted(),
        new TypeInheritanceRelaxed(),
        new ImplementedInterfaceRemoved(),
        new ImplementedInterfaceAdded(),
        new MemberOverridabilityReduced(),
        new ParameterModifierChanged(),
        new MemberStaticnessChanged(),
        new GenericConstraintTightened(),
        new GenericConstraintLoosened(),
        new NestedTypeRemoved(),
        new NestedTypeAdded(),
        new PropertySetterBecameInitOnly()
    ];

    internal static VersionType GetChangeType(
        string unitId,
        ICsharpProject? older,
        ICsharpProject? newer)
    {
        if (older is null || newer is null)
        {
            return VersionType.Minor;
        }

        var signatures = new CsharpSignaturesToCompare(older, newer);
        var changeType = VersionType.Patch;
        foreach (var evaluator in Evaluators)
        {
            if (!evaluator.AreDifferencesPresent(signatures))
            {
                continue;
            }

            Log.WriteLine($"{evaluator.GetType().Name}: {evaluator.EvaluationImpact} in {unitId}");
            changeType = changeType.GetHigherImpact(evaluator.EvaluationImpact);
        }

        return changeType;
    }
}
