# 14 — Language Expansion

The requirements for languages added after C# and Swift. Doc 12 built the seam and asserted that
adding a language costs one provider and one registration line; this document is that claim being
cashed in, one language at a time, and the record of what each one cost.

Open decisions raised while implementing are in
[language-questions.md](../language-questions.md) at the repository root, not here — this document
carries what was decided, that file carries what is still worth asking.

## 1. Support tiers

**LNG-01 — Every language declares a tier, and the tier is published.** ✅ required
A language is supported at exactly one of two tiers, and the README, the run log and this document
SHALL all name it:

| Tier | Discovers units | Seeds and stamps versions | Reads the API | Votes on the change type |
|---|:--:|:--:|:--:|:--:|
| **Full** | ✅ | ✅ | ✅ | ✅ |
| **Version-sync** | ✅ | ✅ | ❌ | ❌ |

Version-sync is not a degraded Full; it is UNI-04 applied to a whole language — the same mechanism
that lets a test project carry a version without its members being a contract. Its units are
discovered, seed MVR-03 and are written by MVR-05, and are absent from extraction, classification
and the baseline.

**LNG-02 — A language ships at Version-sync until its reader is trustworthy.** ✅ required
No language SHALL ship at Full on a reader that has not been tested against real declarations of
every kind it claims to model. A reader that under-reports public surface silently converts a
breaking change into a Patch, on a run nobody is watching — which is G-23 and G-24's shared failure
mode, and the reason both cost a major version to undo.

ℹ️ The asymmetry is the whole argument. Version-sync is *visibly* incomplete: it says so per unit in
the log and in this table. A bad reader is *invisibly* wrong, and the tool's entire job is to be
trusted with a number nobody checks.

**LNG-03 — A Version-sync language SHALL say so at runtime.** ✅ required
Each such unit SHALL be logged as versioned without its API being read, so a build log cannot be
mistaken for "no breaking changes found". The existing UNI-04 line already says exactly this and is
reused rather than restated.

## 2. Current tiers

| Language | Tier | Units | Version sources | Notes |
|---|---|---|---|---|
| C# | **Full** | `.csproj` | `AssemblyVersion`, `PackageVersion`, `FileVersion` | Roslyn, doc 05 |
| VB.NET | **Full** | `.vbproj` | as C# | §3 — shares C#'s model |
| Swift | **Full** | SwiftPM target, Xcode target | 7 sources, MVR-03 | doc 12 |

## 3. VB.NET

**VB-01 — VB reuses C#'s signature model, rules and evaluators.** ✅ required
A VB unit's signature SHALL be a `CsharpProject`, and VB SHALL classify with `CompareSignatures` —
all forty-one C# rules, unmodified.

This is a deliberate, bounded exception to ML-01, and it is bounded by a fact rather than by taste:
**VB and C# compile to one metadata format.** Roslyn produces the same `INamedTypeSymbol` graph from
both, and the two languages break compatibility in the same ways — a removed public member, a
retyped property, a narrowed accessor, a sealed class. The "native topology" ML-01 protects is, for
these two languages, genuinely the same topology: the CLR's.

The cost is named rather than hidden: VB signatures live in types spelled `Csharp*` and persist into
`<CsharpProject>` elements. That is one language described in another's vocabulary, which ML-01
forbids, and it is permitted **here and nowhere else**. A language that does not compile to CLR
metadata gets its own topology no matter how similar it looks — the test is the metadata, not the
resemblance.

ℹ️ The alternative was measured before it was rejected. A parallel `Interfaces/Vb/`, `DataObject/Vb/`
and `Evaluators/Vb/` would have duplicated roughly forty rule classes and forty test classes that
could never legitimately disagree, because any disagreement would be one of them being wrong about
the metadata both languages emit.

**VB-02 — The language-specific half is a parse front end.** ✅ required
`VbUnitBuilder` SHALL supply Visual Basic syntax trees and a `VisualBasicCompilation`, and SHALL
share everything downstream via `CsharpUnitBuilder.AppendCompilation`. Extraction below the
compilation SHALL remain language-neutral symbol code.

ℹ️ This is the whole of VB support: one file of about sixty lines, plus a provider. It is what
doc 12's acceptance criterion 8 predicted, and the first time it has been tested by a language the
seam was not designed around.

**VB-03 — The root namespace is explicitly emptied.** ✅ required
`VisualBasicCompilationOptions` SHALL be built `.WithRootNamespace(string.Empty)`.

Roslyn defaults VB's root namespace to the **assembly name**, so every type would be recorded as
`Widgets.Widgets.Gadget`, and renaming a project would read as the removal of its entire API and the
addition of another. C# has no equivalent default. Asserted by
`TestVbExtraction.TypeNamesAreNotPrefixedWithTheAssemblyName`.

**VB-04 — VB owns its own unit-existence rules.** ✅ required
`Evaluators/Vb/UnitAdded` and `UnitRemoved`, subclassing the shared bases per ML-04. The signature
model is shared; anything keyed by language is not, because the report's key is `(language, rule)`
and `"vb"/"UnitAdded"` is a different finding from `"csharp"/"UnitAdded"`.

**VB-05 — Test-code signals are MSBuild's, as for C#.** ✅ required
`IsTestCode` SHALL read the `.vbproj` with `CsProjTestProject` — an explicit `<IsTestProject>`, or a
`PackageReference` to Microsoft.NET.Test.Sdk, xunit, NUnit or MSTest (UNI-04, G-23). Declared on the
provider rather than inherited, so `TestLanguageSeam` can see VB answered.

**VB-06 — Primitive spellings are VB's own, and that is not a defect.** ℹ️
`Public Enum Colour As Byte` records `Byte` where C# records `byte`: Roslyn renders a type in its
own compilation's language. It is stable — a VB unit is only ever compared against its own history —
and it is what a VB developer reads.

The one case it bites is a project converted in place from C# to VB under the same project name:
every primitive-typed member reads as retyped, and the run is Major. The rewrite was already a
Major, so this is recorded rather than fixed.

### Fix carried by this work

**VB-07 — The global namespace is omitted, not string-stripped.** ✅ required — *corrects a latent C# defect*
`GetFullyQualifiedName` SHALL obtain names with `SymbolDisplayGlobalNamespaceStyle.Omitted`.

It previously cut the string at `::`, which handled C#'s `global::Widgets.Gadget` and silently let
VB's `Global.Widgets.Gadget` through — every VB type would have entered the baseline under a
`Global.` prefix that no rule and no reader would match. Stripping a leading `Global.` instead would
corrupt a C# type genuinely declared in a namespace named `Global`. Asking Roslyn to omit it needs no
per-language knowledge and is why this is a fix rather than a VB special case.

ℹ️ C# output is unchanged — the full C# extraction suite passes untouched — so no baseline re-seeds.
