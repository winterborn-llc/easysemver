# 07 — Change Classification

How (baseline, current) signature pairs map to a SemVer change type.
Sources: [`CompareSignatures.cs`](../src/EasySemVer/Evaluators/CompareSignatures.cs),
[`SignaturesToCompare.cs`](../src/EasySemVer/Evaluation/SignaturesToCompare.cs), the rule
classes in [`src/EasySemVer/Evaluators/`](../src/EasySemVer/Evaluators), and their tests in
[`src/Test/TestSignatureEvaluators/`](../src/Test/TestSignatureEvaluators).

## Architecture

**CLS-01 — Rule-object model.** ✅
Classification SHALL be expressed as independent rule objects implementing
[`IEvaluateSignatures`](../src/EasySemVer/Interfaces/IEvaluateSignatures.cs):

```csharp
VersionType EvaluationImpact { get; }                    // Major | Minor
bool AreDifferencesPresent(ISignaturesToCompare s);      // does this kind of change exist?
```

Rules are registered in a static list in `CompareSignatures` (17 today). Adding a detection
capability means adding a rule class + registration + a test class (TST-01) — no changes to
the aggregation logic.

**CLS-02 — Pairing (`ClassHistory`).** ✅
Before member-level rules run, the comparison context SHALL pair up entities that exist on
*both* sides: projects matched by name, then classes matched by name within matched projects.
Member-level rules (methods/properties) operate **only on paired classes**; added/removed
projects and classes are the exclusive concern of the existence rules (R05–R07, R14). This
layering prevents double-counting a removed class as also "removing all its methods."

**CLS-03 — Aggregation.** ✅
The result SHALL be the highest impact among all firing rules — `Major > Minor > Patch` —
with **Patch as the default** when no rule fires. Every rule is evaluated (no short-circuit
requirement); evaluation order must not affect the outcome.

**CLS-04 — Defensive null handling.** ✅
If either signature is `null`, the result SHALL be **Minor** (fail-safe: assume additive).
In practice a missing baseline is an *empty* (not null) solution, and first runs classify
Minor via R14 (see PER-03).

## The rules

| ID | Rule (class) | Fires when… | Impact |
|----|--------------|-------------|:------:|
| R01 | [`MethodsContinueToExist`](../src/EasySemVer/Evaluators/MethodsContinueToExist.cs) | a method name present in the old paired class is absent from the new one | **Major** |
| R02 | [`MethodInputParameterOverrideRemoved`](../src/EasySemVer/Evaluators/MethodInputParameterOverrideRemoved.cs) | an old overload has no new overload with the same parameter count, names, and types, in order (requiredness deliberately ignored — a requiredness-only change is not a *removed* overload; direction-sensitive handling is R17's job) | **Major** |
| R03 | [`MethodReturnType`](../src/EasySemVer/Evaluators/MethodReturnType.cs) | a paired method name's recorded return type differs | **Major** |
| R04 | [`MethodOverrideAdded`](../src/EasySemVer/Evaluators/MethodOverrideAdded.cs) | a method that exists on both sides gains an overload whose canonical signature string (SIG-09, requiredness included) is absent from the old side | **Minor** |
| R05 | [`ProjectClassAdded`](../src/EasySemVer/Evaluators/ProjectClassAdded.cs) | a paired project contains a class name absent from the old side | **Minor** |
| R06 | [`ProjectClassesContinueToExist`](../src/EasySemVer/Evaluators/ProjectClassesContinueToExist.cs) | a class in an old paired project is absent from the new side | **Major** |
| R07 | [`ProjectsContinueToExist`](../src/EasySemVer/Evaluators/ProjectsContinueToExist.cs) | a project in the old signature is absent from the new one | **Major** |
| R08 | [`PropertyEditabilityEnhanced`](../src/EasySemVer/Evaluators/PropertyEditabilityEnhanced.cs) | a paired property goes not-writable → writable | **Minor** |
| R09 | [`PropertyEditabilityReduced`](../src/EasySemVer/Evaluators/PropertyEditabilityReduced.cs) | a paired property goes writable → not-writable | **Major** |
| R10 | [`PropertiesContinueToExist`](../src/EasySemVer/Evaluators/PropertiesContinueToExist.cs) | a property name in an old paired class is absent from the new one | **Major** |
| R11 | [`PropertyReadabilityEnhanced`](../src/EasySemVer/Evaluators/PropertyReadabilityEnhanced.cs) | a paired property goes not-readable → readable | **Minor** |
| R12 | [`PropertyReadabilityReduced`](../src/EasySemVer/Evaluators/PropertyReadabilityReduced.cs) | a paired property goes readable → not-readable | **Major** |
| R13 | [`PropertyType`](../src/EasySemVer/Evaluators/PropertyType.cs) | a paired property's type name differs | **Major** |
| R14 | [`ProjectAdded`](../src/EasySemVer/Evaluators/ProjectAdded.cs) | the new signature contains a project name absent from the old one | **Minor** |
| R15 | [`MethodAdded`](../src/EasySemVer/Evaluators/MethodAdded.cs) | a paired class contains a method name absent from the old side | **Minor** |
| R16 | [`PropertyAdded`](../src/EasySemVer/Evaluators/PropertyAdded.cs) | a paired class contains a property name absent from the old side | **Minor** |
| R17 | [`MethodInputParameterMadeRequired`](../src/EasySemVer/Evaluators/MethodInputParameterMadeRequired.cs) | for overloads matched on parameter count, names, and types (R02's matcher), a parameter is `IsRequired = false` in the old side and `IsRequired = true` in the new one | **Major** |

R15/R16 are the additive complements of R01/R10 and, per CLS-02, only inspect `ClassHistory`
pairs — members of a brand-new class are R05's concern, not theirs.
R17 is deliberately **directional**: only optional→required fires. The reverse
(required→optional) is non-breaking and stays Minor via R04.

All 17 rules are covered by dedicated test classes asserting impact, a no-change negative
case, and a positive case ([11-testing.md](11-testing.md)).

## Scenario → outcome matrix

The observable contract, as produced by the rules above. ⚠️ rows are where the current rule
set diverges from SemVer intent (details in [99-known-gaps.md](99-known-gaps.md)).

| Scenario | Outcome | Via |
|----------|:-------:|-----|
| First run (no baseline) | Minor | R14 |
| Nothing changed / implementation-only change | Patch | default |
| Project added | Minor | R14 |
| Project removed or renamed | Major | R07 (+R14 on rename) |
| Class added to existing project | Minor | R05 |
| Class removed, renamed, or moved to another namespace | Major | R06 (+R05) |
| New method added to existing class | Minor | R15 |
| New property added to existing class | Minor | R16 |
| Overload added to an existing method | Minor | R04 |
| Method removed (all overloads) | Major | R01 |
| One overload removed | Major | R02 |
| Parameter added/removed/renamed/retyped on an existing overload | Major | R02 (+R04) |
| Return type changed | Major | R03 (first-overload caveat, G-14) |
| Optional parameter made required | Major | R17 (R04 also fires; Major wins) |
| Required parameter made optional | Minor | R04 |
| Property type changed | Major | R13 |
| Property setter removed | Major | R09 |
| Property setter added | Minor | R08 |
| Property `set` changed to `init` | Patch ⚠️ | not distinguished — both count as writable (SIG-05) |
| Property getter removed / added | Major / Minor | R12 / R11 |
| Any change to public interfaces, structs, enums, delegates, events, fields | Patch ⚠️ G-15 | *out-of-scope surface* |

**CLS-05 — Additions to existing classes SHALL be Minor.** ✅
Per SemVer, adding new public functionality (a brand-new method or property on an existing
class) is a Minor change. R15 (`MethodAdded`) and R16 (`PropertyAdded`) detect this as the
additive complements of R01/R10 — R04 could not, since it requires the method name to already
exist, nor could R08/R11, which require the property to already exist. (Was gap **G-07**.)

**CLS-06 — Tightening a contract SHALL be Major.** ✅
Making an optional parameter required is breaking and classifies Major via R17
(`MethodInputParameterMadeRequired`). R02's matcher is intentionally left requiredness-blind:
folding `IsRequired` into it would classify the non-breaking reverse (required→optional) as a
removed overload, i.e. Major. R17 instead reuses R02's count/names/types matcher and fires
only in the breaking direction, so required→optional stays Minor via R04.
(Was gap **G-08**.)

**CLS-07 — Rebuild context cleanly.** ⚠️
`CompareSignatures.CalculateChangeType` reconstructs a `SignaturesToCompare` with an empty
solution path, silently re-discovering project files from the current working directory
(DSC-07). Classification SHOULD depend only on the two signatures it is given; the rebuilt
context happens to produce identical `ClassHistory`, but the cwd dependency and duplicate
I/O are accidental. (Gap **G-13**.)
