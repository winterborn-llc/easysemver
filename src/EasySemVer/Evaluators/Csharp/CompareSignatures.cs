using Winterborn.Library.EasySemVer.DataObject;
using Winterborn.Library.EasySemVer.Evaluation.Csharp;
using Winterborn.Library.EasySemVer.Interfaces;
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

    internal static IReadOnlyList<ChangeFinding> GetFindings(
        IPackageableUnit unit,
        ICsharpProject? older,
        ICsharpProject? newer)
    {
        // CLS-04: fail safe towards additive. Unlike the run-level case this one has a unit to
        // name, so it is reported as an ordinary finding rather than as an unexplained verdict.
        if (older is null || newer is null)
        {
            return
            [
                new ChangeFinding
                {
                    Language = unit.Language,
                    UnitId = unit.UnitId,
                    RuleName = nameof(CompareSignatures),
                    RuleId = "CLS-04",
                    Symbol = unit.UnitId,
                    Description = "has no comparable baseline signature, so it is treated as additive",
                    Impact = VersionType.Minor
                }
            ];
        }

        var signatures = new CsharpSignaturesToCompare(older, newer);
        var findings = new List<ChangeFinding>();
        foreach (var evaluator in Evaluators)
        {
            foreach (var symbol in evaluator.FindDifferences(signatures))
            {
                findings.Add(new ChangeFinding
                {
                    Language = unit.Language,
                    UnitId = unit.UnitId,
                    RuleName = evaluator.GetType().Name,
                    RuleId = evaluator.RuleId,
                    Symbol = symbol,
                    Description = evaluator.ChangeDescription,
                    Impact = evaluator.EvaluationImpact
                });
            }
        }

        return findings;
    }
}
