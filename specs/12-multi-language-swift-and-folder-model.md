# 12 — Multi-Language Model, Folder Invocation, and Swift Support

**Status: forward-looking specification.** Docs 01–11 describe the system *as built* (C#-only,
solution-rooted). This document specifies the *next* system. Where it conflicts with an
existing requirement, this document wins and the older requirement is marked below as
**replaced** or **retired**; the implementer updates docs 01–11 at the end of the work
(§17).

Decisions in §1 were made by the repository owner on 2026-08-01 and are **settled** — do not
re-litigate them. Genuinely open items are collected in §20.

## 0. What changes

| Aspect | Today | After this work |
|--------|-------|-----------------|
| Entry contract | walk up to the nearest `.sln` | **the folder you are pointed at** is the root, full stop |
| Languages | C# only | C# and Swift, with a seam that makes Java/Kotlin/C++ additive |
| Model shape | one `Solution → Project → Class` graph | **per-language native topologies**, never mapped onto each other |
| Unit of packaging | `.csproj` | **packageable unit**: `.csproj`, SwiftPM target, Xcode target |
| Baseline | `EasySemVer.xml`, unserializable (G-01, blocking) | `EasySemVer.xml` v2 — a flat **array of packageable units**, each carrying its language's native signature |
| C# fidelity | public classes only (G-15) | full C# topology: interfaces, structs, records, enums, delegates, events, fields, nested types |
| Version seed | highest of `AssemblyVersion`/`PackageVersion`/`FileVersion` across `.csproj` files | highest across **every** version source in **every** unit in **every** language |

## 1. Settled decisions

**D-01 — Scope is the whole rework.** Folder-based CLI **and** the G-01 serialization fix
**and** the `ICsharp*` rename **and** Swift, in one body of work (phased per §18).

**D-02 — Swift signatures come from the Swift toolchain's symbol graph.** No hand-rolled
Swift parser, no tree-sitter, no shipped native helper.

**D-03 — Missing toolchain or failed build is fatal.** If the folder contains Swift units and
their signatures cannot be extracted, the run fails (exit 1). No skip-and-warn, no partial
baseline.

**D-04 — Swift models the full native Swift topology.** `public` and `open` declarations of
every kind: class, struct, enum (with cases and associated values), protocol (with
requirements), actor, extension members, global functions and variables, typealiases,
initializers, subscripts, operators.

**D-05 — A Swift packageable unit is a *target* (module).** SwiftPM targets and Xcode targets
alike. Not products, not whole packages.

**D-06 — Xcode is in scope for signatures too**, not just version stamping — via `xcodebuild`
with symbol-graph flags. Accepted cost: a repo containing an `.xcodeproj` pays an Xcode build
on every versioned run, and CI needs Xcode configured. (See §20 O-03 for the mitigation the
implementer should propose but not unilaterally adopt.)

**D-07 — Every Swift version location is read, and the highest wins**: Xcode
`MARKETING_VERSION` / Info.plist `CFBundleShortVersionString`, `.podspec` `s.version`, git
tags, and a generated Swift version file.

**D-08 — Naming is `ICsharpProject` / `ISwiftModule`**, with per-language subfolders under the
existing concern folders (`Interfaces/Csharp/`, `DataObject/Swift/`, …) and namespaces to
match. Spelling is `Csharp`, not `CSharp`.

**D-09 — C# comes up to full fidelity in this pass.** Gap G-15 closes here.

**D-10 — One baseline file, whose content is a flat array of packageable units.**

## 2. Terminology

| Term | Meaning |
|------|---------|
| **Folder root** | The directory handed to the CLI. The unit of versioning. Replaces "solution root" everywhere. |
| **Packageable unit** | One independently shippable module of code: a `.csproj`, a SwiftPM target, an Xcode target. The atom of add/remove detection and of version write-back. |
| **Language provider** | The pluggable per-language implementation: discovery, extraction, classification, version read/write. |
| **Native topology** | A language's own object model, named as that language's users name it. Swift has modules, structs, protocols, actors, extensions. C# has projects, classes, interfaces, delegates. Neither is expressed in the other's vocabulary. |
| **Unit signature** | The extracted native topology for one unit, as persisted in the baseline. |

## 3. Architecture — the language seam

**ML-01 — The neutral layer knows only three things.** ✅ required
The language-neutral core SHALL be expressible in terms of exactly: the **packageable unit**
(identity, language, path, version sources), the **`VersionType`** verdict, and the
**`Version`** value. Nothing else may be shared. There SHALL be no cross-language abstraction
of "type", "member", "class", "method", or "property" — Swift's topology SHALL NOT be
expressed through C# concepts and vice versa (D-04, D-08).

**ML-02 — Language provider contract.** ✅ required
Each supported language SHALL be implemented behind a single interface, roughly:

```csharp
public interface ILanguageProvider
{
    Language Language { get; }                                  // Csharp | Swift | ...
    IReadOnlyList<IPackageableUnit> Discover(string folderRoot);
    void Extract(IPackageableUnit unit);                        // fills the unit's signature
    VersionType Classify(IPackageableUnit? older, IPackageableUnit newer);
    IReadOnlyList<Version> ReadVersions(IPackageableUnit unit);
    void WriteVersion(IPackageableUnit unit, Version version);
}
```

Adding Java later SHALL require: one provider, one `Interfaces/Java/` + `DataObject/Java/` +
`Evaluators/Java/` folder set, one registration line — and **no** edits to the neutral core.
This is the acceptance test for whether the seam is right.

**ML-03 — Unit identity.** ✅ required
A unit's identity SHALL be `(Language, UnitId)` where `UnitId` is stable across machines and
checkouts and contains no absolute paths:
- C#: the `.csproj` filename without extension (preserves DSC-05).
- SwiftPM: `<package-directory-relative-path>:<target-name>`.
- Xcode: `<xcodeproj-relative-path>:<target-name>`.

Renaming a unit reads as remove + add, exactly as DSC-05 already establishes for C#.

**ML-04 — Per-language rule objects survive.** ✅ required
The rule-object model of CLS-01 SHALL be preserved *within* each language:
`IEvaluateCsharpSignatures` and `IEvaluateSwiftSignatures`, each with
`VersionType EvaluationImpact` and a `bool AreDifferencesPresent(...)` over that language's
own comparison context. Rules are registered in a per-language list. There is no
cross-language rule base type beyond, at most, a marker.

**ML-05 — Aggregation across languages.** ✅ required
The run's change type SHALL be the highest impact across the neutral unit-existence rules
(§7) and every language provider's verdict, with **Patch** as the default (preserves CLS-03,
OVR-03). Swift-only changes therefore move the C# projects' versions too, and vice versa —
one version per folder (ML-06).

**ML-06 — One version per folder.** ✅ required
Generalizes OVR-02: the entire folder shares a single version regardless of how many
languages or units it contains. Per-unit or per-language version streams are out of scope.

**ML-07 — Process execution is injectable.** ✅ required
All external tool invocations (`swift`, `xcodebuild`, `git`) SHALL go through an injectable
runner abstraction (e.g. `IRunProcess`) so extraction, failure, and timeout paths are
unit-testable without the toolchain installed.

## 4. Folder invocation and discovery

**FLD-01 — The argument is the root.** ✅ required — *replaces CLI-04, fixes G-06*
`easysemver <folder>` SHALL use `<folder>` as the folder root. The current implementation
validates the argument and then ignores it; that bug SHALL be fixed as part of this work.
Zero arguments SHALL continue to mean the current working directory (CLI-03). Two or more
arguments SHALL still fail (CLI-02).

**FLD-02 — No solution walk-up.** ✅ required — *retires CLI-05, DSC-01, DSC-02, G-09*
The tool SHALL NOT search ancestors for a `.sln`/`.slnx`, and SHALL NOT require one to exist.
A folder containing no `.sln` is a valid, ordinary input. `ExtendString.GetSolutionDirectory`
and `Program.GetSolutionDirectory` are both deleted.

**FLD-03 — Discovery is recursive from the root, once.** ✅ required — *fixes G-13*
Every provider SHALL be given the same root and enumerate it **once** per run; the resulting
unit list SHALL be passed to every downstream stage. The current four-times enumeration and
the cwd fallback inside `CompareSignatures` (CLS-07) SHALL both be eliminated: classification
receives the two signatures and nothing else.

**FLD-04 — Excluded directories.** ✅ required — *fixes G-10*
Discovery SHALL skip, at any depth:
- any directory whose name begins with `.` (covers `.git`, `.build`, `.packages`, `.swiftpm`),
- `bin`, `obj`, `build`, `DerivedData`, `Pods`, `Carthage`, `node_modules`, `Packages`
  (Xcode's local-package checkout dir).

The list lives in `MagicValues`. This is not optional politeness: with a folder root instead
of a solution root, an unexcluded `.packages/` or `.build/checkouts/` would pull **dependency
source** into the signature and make every dependency update a Major change.

**FLD-05 — Empty folder is not an error.** ✅ required
A root containing no recognizable units SHALL log that fact, classify as Patch, write an empty
baseline, and exit 0.

## 5. Packageable units

**UNI-01 — The neutral unit model.** ✅ required
```
IPackageableUnit
  Language        Language        // Csharp | Swift
  UnitId          string          // ML-03
  DisplayName     string
  RelativePath    string          // folder-root-relative, forward slashes
  UnitKind        string          // "csproj" | "swiftpm-target" | "xcode-target"
  VersionSources  IReadOnlyList<IVersionSource>   // §14
  Signature       object          // the language-native graph; opaque to the core
```
`Signature` SHALL be typed per language at the provider boundary (`ICsharpProject`,
`ISwiftModule`) and never inspected by the neutral core.

**UNI-02 — C# units.** ✅ required
One unit per `*.csproj` under the root (preserves DSC-03 semantics minus the exclusions fix).
Test projects and sample projects still participate — there is deliberately no include/exclude
configuration.

**UNI-03 — Swift units.** ✅ required (D-05)
One unit per SwiftPM **target** and per Xcode **target**. Test targets participate as units
(they carry versions and their disappearance is a real change), consistent with UNI-02.

## 6. Baseline format v2

**BAS-01 — One file, an array of units.** ✅ required (D-10)
The baseline SHALL be `<folder root>/EasySemVer.xml` (name preserved from PER-01), whose
content is a **flat array of packageable-unit entries**. Each entry carries its language, unit
id, unit kind, relative path, and that language's native signature payload. Sketch:

```xml
<EasySemVer formatVersion="2">
   <Unit language="Csharp" unitId="EasySemVer" unitKind="csproj" path="src/EasySemVer/EasySemVer.csproj">
      <CsharpProject> … classes, interfaces, enums, … </CsharpProject>
   </Unit>
   <Unit language="Swift" unitId="Sources/Widgets:Widgets" unitKind="swiftpm-target" path="Sources/Widgets">
      <SwiftModule> … structs, protocols, actors, … </SwiftModule>
   </Unit>
</EasySemVer>
```

**BAS-02 — It must actually serialize.** ✅ required — *fixes G-01, satisfies PER-05*
The persisted graph SHALL consist of **concrete, public, parameterless-constructible DTO
types** — no interface-typed members, no `List<IProject>` bases. `XmlSerializer` cannot
serialize interfaces; that is the single blocking defect in the repo today and it is fixed
here by shape, not by attribute tricks. The in-memory model may stay interface-typed; if so,
provide an explicit mapping layer to/from the DTOs. Round-tripping SHALL be verified by test:
`Serialize(Deserialize(Serialize(x))) == Serialize(x)` (TST-M4).

**BAS-03 — `formatVersion` is mandatory.** ✅ required
The root element SHALL carry `formatVersion="2"`. An unknown or absent version SHALL be
treated as unreadable → PER-04 (warn, proceed with an empty baseline). No migration from v1 is
needed or wanted: because of G-01 the v1 writer never succeeded, so no valid v1 file exists
anywhere.

**BAS-04 — Determinism.** ✅ required — *extends PER-07*
The file SHALL contain no absolute paths, timestamps, machine names, toolchain versions, or
raw tool output. Units SHALL be written sorted by `(Language, UnitId)`; every collection
inside a unit signature SHALL be written in a deterministic order (sort by the entity's
identity key). Two runs over unchanged source on two machines SHALL produce byte-identical
files. Symbol-graph JSON ordering is **not** guaranteed by the toolchain — sorting is the
implementer's job, not the tool's.

**BAS-05 — Self-healing preserved.** ✅ required
PER-03 and PER-04 carry over unchanged: missing file → empty baseline; unreadable file → warn
and proceed with an empty baseline. A first run therefore classifies Minor via NCL-02.

**BAS-06 — Write atomically, and only on success.** ✅ required — *tightens PER-06*
The baseline SHALL be written to a temporary file in the same directory and moved into place.
It SHALL NOT be written at all if any discovered unit failed extraction (SWE-05). Ordering
within persistence stays: baseline first, then version write-back.

## 7. Neutral classification (unit existence)

Unit-level existence moves **out** of the language rule sets and into the neutral core, because
"a shippable module appeared/disappeared" means the same thing in every language.

**NCL-01 — Unit removed → Major.** ✅ required — *replaces R07 for C#*
A unit present in the baseline and absent from the current run SHALL classify Major.

**NCL-02 — Unit added → Minor.** ✅ required — *replaces R14 for C#*
A unit present in the current run and absent from the baseline SHALL classify Minor.

**NCL-03 — Pair first, then delegate.** ✅ required — *generalizes CLS-02*
Units present on both sides SHALL be paired by `(Language, UnitId)` and handed to that
language's provider for classification. Language rules SHALL only ever see paired units of
their own language, so a removed unit is never double-counted as "everything in it was
removed."

**NCL-04 — Null-safety.** ✅ required
If either side is null, the result SHALL be Minor (preserves CLS-04).

`ProjectsContinueToExist` (R07) and `ProjectAdded` (R14) are deleted as C# rules and their
behavior re-homed here; their **rule IDs are retired, not reused**, and their tests move to the
neutral test set.

## 8. C# rename map

Mechanical, behavior-preserving. All existing tests SHALL still pass after this phase alone.

**REN-01 — Type renames.** ✅ required

| Today | Becomes |
|-------|---------|
| `ISolution` / `Solution` | *(deleted — the unit array in BAS-01 replaces it)* |
| `IProject` / `Project` | `ICsharpProject` / `CsharpProject` |
| `IProjectClass` / `ProjectClass` | `ICsharpClass` / `CsharpClass` |
| `IProjectClassHistory` / `ProjectClassHistory` | `ICsharpClassHistory` / `CsharpClassHistory` |
| `IMethod` / `Method` | `ICsharpMethod` / `CsharpMethod` |
| `IMethodList` / `MethodList` | `ICsharpMethodList` / `CsharpMethodList` |
| `IMethodDefinition` / `MethodDefinition` | `ICsharpMethodDefinition` / `CsharpMethodDefinition` |
| `IMethodOverride(s)` / `MethodOverride(s)` | `ICsharpMethodOverride(s)` / `CsharpMethodOverride(s)` |
| `IMethodOverrideInput` / `MethodOverrideInput` | `ICsharpMethodParameter` / `CsharpMethodParameter` |
| `IProperty` / `Property` | `ICsharpProperty` / `CsharpProperty` |
| `IPropertyList` / `PropertyList` | `ICsharpPropertyList` / `CsharpPropertyList` |
| `ISignaturesToCompare` / `SignaturesToCompare` | `ICsharpSignaturesToCompare` / `CsharpSignaturesToCompare` (scoped to one paired unit — §8 REN-04) |
| `IEvaluateSignatures` | `IEvaluateCsharpSignatures` |
| `SolutionBuilder` | `CsharpUnitBuilder` |
| `CsProjFile`, `CsProjFileVersion` | unchanged names, moved into `CodeReader/Csharp/` |

**REN-02 — Folder and namespace layout.** ✅ required (D-08)
```
src/EasySemVer/
   Interfaces/            (neutral: ILanguageProvider, IPackageableUnit, IVersionSource, IRunProcess)
   Interfaces/Csharp/     Interfaces/Swift/
   DataObject/            (neutral: Version, VersionType, PackageableUnit)
   DataObject/Csharp/     DataObject/Swift/
   CodeReader/Csharp/     CodeReader/Swift/
   Evaluators/            (neutral: unit existence rules, aggregation)
   Evaluators/Csharp/     Evaluators/Swift/
   Evaluation/            (neutral: pairing, run orchestration)
   Persistence/           (baseline DTOs + reader/writer)
   Settings/
```
Namespaces follow the folders: `Winterborn.Library.EasySemVer.Interfaces.Csharp`,
`…DataObject.Swift`, and so on. Tests mirror this:
`src/Test/Evaluators/Csharp/`, `src/Test/Evaluators/Swift/`.

**REN-03 — Evaluator classes keep their names**, moving to `Evaluators/Csharp/` (e.g.
`MethodsContinueToExist` stays `MethodsContinueToExist`). Rule IDs R01–R17 keep their meaning
and their tests, except R07/R14 which are retired per §7.

**REN-04 — Comparison context is per unit.** ✅ required
`CsharpSignaturesToCompare` SHALL carry exactly `(ICsharpProject older, ICsharpProject newer)`
plus the paired-class history for that unit. It SHALL NOT know about file paths, project
enumeration, or saving — `Save` moves to the neutral persistence layer. This is what kills the
cwd dependency in CLS-07/G-13.

**REN-05 — Delete on the way through.** ✅ required — *closes G-19*
Remove `CsProjSignature`, `ExtendIList.AddIfNew`, `ExtendDirectoryInfo.GetSubDirectory`,
`ExtendString.GetXmlNodeValue`, the commented-out `AdhocWorkspace` block in the builder,
`ExtendFileInfo.GetFileText` (only used by that block), and the `Newtonsoft.Json` attributes on
`Solution` (persistence is XML; the package isn't even directly referenced).

## 9. C# fidelity expansion

**CSX-01 — Types in scope.** ✅ required — *replaces SIG-02, closes G-15*
The C# signature SHALL include every `public` namespace-level **and public nested** type of
kind: class, interface, struct, record, record struct, enum, delegate. Each is modeled as its
own native concept (`CsharpClass`, `CsharpInterface`, `CsharpStruct`, `CsharpRecord`,
`CsharpEnum`, `CsharpDelegate`), not flattened into "class".

**CSX-02 — Members in scope.** ✅ required — *extends SIG-05…SIG-08*
Methods, properties, **fields**, **events**, **constructors**, **operators**, **indexers**,
**enum members** (name + explicit value), and **nested types**. Namespace exclusions (SIG-03)
carry over unchanged.

**CSX-03 — Modifiers captured.** ✅ required — *closes the code `TODO`s*
Type level: `static`, `abstract`, `sealed`, base class, implemented interfaces, generic
parameter count and constraints. Member level: `static`, `virtual`, `abstract`, `override`,
`sealed`, `readonly`, `required`, `init`-vs-`set`, and parameter modifiers `ref`/`out`/`in`/
`params`.

**CSX-04 — Per-overload return types.** ✅ required — *closes G-14*
Return type SHALL be recorded per overload, not per method name. R03's first-overload
limitation ends here.

**CSX-05 — New C# rules.** ✅ required
Each is a rule class in `Evaluators/Csharp/` with a dedicated test class (TST-01). R01–R17
keep their current semantics; these are additive.

| ID | Fires when… | Impact |
|----|-------------|:------:|
| R18 | a public interface / struct / record / enum / delegate is removed | Major |
| R19 | a public interface / struct / record / enum / delegate is added | Minor |
| R20 | an interface gains a requirement with **no** default implementation | Major |
| R21 | an interface gains a requirement **with** a default implementation | Minor |
| R22 | an enum member is removed or renamed | Major |
| R23 | an enum member is added | Minor |
| R24 | an enum member's explicit value changes | Major |
| R25 | an enum's underlying type changes | Major |
| R26 | a delegate's signature (parameters or return type) changes | Major |
| R27 | a record's positional parameter list changes | Major |
| R28 | a public field is removed, retyped, or gains `readonly` | Major |
| R29 | a public field is added | Minor |
| R30 | a public event is removed or its handler type changes | Major |
| R31 | a public event is added | Minor |
| R32 | a type gains `sealed`, `abstract`, or `static`, or changes base class | Major |
| R33 | a type loses `sealed` or `abstract` | Minor |
| R34 | an implemented interface is removed from a public type | Major |
| R35 | an implemented interface is added to a public type | Minor |
| R36 | a member loses `virtual`/`abstract`, or gains `abstract`/`sealed` | Major |
| R37 | a parameter's `ref`/`out`/`in`/`params` modifier changes | Major |
| R38 | a member's static-vs-instance-ness changes | Major |
| R39 | generic parameter count changes, or a constraint is added/tightened | Major |
| R40 | a generic constraint is removed or loosened | Minor |
| R41 | a public nested type is removed / added | Major / Minor |
| R42 | a property's `set` accessor changes to `init` | Major |

R42 closes the ⚠️ row in the CLS scenario matrix (`set` → `init` currently reads as Patch);
capturing `init` separately from `set` (CSX-03) is what makes it detectable.

## 10. Swift discovery

**SWD-01 — SwiftPM packages.** ✅ required
Every `Package.swift` under the root (subject to FLD-04) SHALL be a Swift package. Targets
SHALL be enumerated by running **`swift package dump-package`** in the package directory and
reading the JSON manifest — the manifest is executable Swift and SHALL NOT be text-parsed.

**SWD-02 — Xcode projects.** ✅ required (D-06)
Every `*.xcodeproj` under the root SHALL be an Xcode project. Targets SHALL be enumerated by
`xcodebuild -list -json -project <path>`. `*.xcworkspace` is used only to locate projects, not
as a unit itself.

**SWD-03 — Unit kinds and identity.** ✅ required
Per ML-03: `swiftpm-target` with id `<package-relative-dir>:<target>`, `xcode-target` with id
`<xcodeproj-relative-path>:<target>`.

**SWD-04 — Dependencies are never units.** ✅ required
Resolved SwiftPM dependencies (`.build/checkouts`, `.swiftpm`, `Packages/`, `Pods/`) and any
system/binary targets SHALL be excluded from discovery. Only first-party targets declared by a
first-party manifest are units.

## 11. Swift extraction

**SWE-01 — Symbol graph is the only source.** ✅ required (D-02)
Signatures SHALL be read from the toolchain's symbol-graph JSON. For SwiftPM:

```
swift build --package-path <pkg> -Xswiftc -emit-symbol-graph \
            -Xswiftc -emit-symbol-graph-dir -Xswiftc <tempdir>
```

or, when a built module is already available, `swift symbolgraph-extract -module-name <M>
-target <triple> -I <build-dir> -output-dir <tempdir>`. For Xcode targets, the equivalent
`xcodebuild` invocation with `OTHER_SWIFT_FLAGS` carrying the same emit flags. Extension blocks
SHALL be requested (`-emit-extension-block-symbols`) so extension members are visible.

**SWE-02 — Access level filter.** ✅ required
Only `public` and `open` declarations enter the signature (`-minimum-access-level public`
where supported, plus an explicit filter on the parsed graph — do not trust the flag alone).
`internal`, `fileprivate`, `private`, and `package` are out of scope. A declaration dropping
from public to internal therefore surfaces as a removal (Major) via S01/S16, which is correct.

**SWE-03 — Identity is declaration-derived, not USR.** ✅ required
Entity identity SHALL be built from the symbol graph's `pathComponents` plus, for functions,
the full Swift name **including argument labels** (e.g. `Widgets.Gadget.move(to:animated:)`).
The mangled `precise identifier` (USR) MAY be recorded as an attribute but SHALL NOT be the
identity key — mangling schemes change between toolchain versions and would churn the baseline
(BAS-04).

**SWE-04 — Determinism over toolchain output.** ✅ required
Only modeled fields are persisted; raw JSON is never stored. All collections are sorted by
identity key before persistence. Nothing toolchain-version-dependent (mangled names, symbol
ordering, source locations, doc comments) enters the file.

**SWE-05 — Extraction failure is fatal.** ✅ required (D-03)
If Swift units are discovered and any of the following occurs — `swift`/`xcodebuild` not found,
non-zero exit, timeout, no symbol graph produced for a discovered target — the run SHALL
fail with exit 1, and the message SHALL name the unit, the exact command executed, and the
tool's stderr. No baseline is written (BAS-06), no version is stamped, the working tree is
untouched.

**SWE-06 — Temp artifacts.** ✅ required
Symbol graphs SHALL be emitted to a temporary directory outside the folder root and cleaned up
afterwards, so extraction never dirties the user's tree or feeds its own output back into
discovery.

## 12. Swift signature model

**SWM-01 — Native shape.** ✅ required (D-04)
```
SwiftModule                      { Name, UnitKind }
├── SwiftClass / SwiftStruct / SwiftActor
│     { Name, AccessLevel, IsFinal, IsFrozen, Superclass,
│       Conformances[], GenericParameters[], Availability }
│     ├── SwiftInitializer  { Labels, Parameters[], IsFailable, IsRequired, IsConvenience, IsAsync, Throws }
│     ├── SwiftFunction     { FullName(with labels), Parameters[], ReturnType,
│     │                       IsStatic, IsMutating, IsAsync, Throws, IsFinal, GenericParameters[] }
│     ├── SwiftProperty     { Name, Type, IsSettable, IsStatic, IsMutating(setter), IsAsync, Throws }
│     ├── SwiftSubscript    { Labels, Parameters[], ReturnType, IsSettable, IsStatic }
│     └── nested types
├── SwiftEnum               { …type facets…, IsFrozen, RawValueType }
│     └── SwiftEnumCase     { Name, AssociatedValues[](label+type), RawValue }
├── SwiftProtocol           { Name, InheritedProtocols[], AssociatedTypes[] }
│     └── SwiftRequirement  { …function/property shape…, HasDefaultImplementation }
├── SwiftExtension          { ExtendedType, Constraints[], AddedConformances[], members… }
├── SwiftTypeAlias          { Name, UnderlyingType }
├── SwiftGlobalFunction, SwiftGlobalVariable
└── SwiftOperator           { Name, Kind, Precedence }

SwiftParameter { Label, InternalName, Type, HasDefault, IsInout, IsVariadic, Ownership }
```

**SWM-02 — Extensions.** ✅ required
Members added by an extension to a type **declared in the same module** SHALL be folded into
that type's member set, tagged with their originating constraint (if any) — that is how a
Swift developer reads them. Extensions on types from **other modules** SHALL be recorded as
their own `SwiftExtension` entities keyed by extended-type name plus constraints, since the
extended type is not part of this module's signature.

**SWM-03 — Availability.** ✅ required
`@available` SHALL be captured per declaration with at minimum: introduced platform/version,
`deprecated`, `obsoleted`, `unavailable`, and `renamed`. It drives S24/S25.

**SWM-04 — ObjC exposure.** ✅ required
`@objc` / `@objc(CustomName)` / `@nonobjc` / `@IBAction` exposure SHALL be captured; losing
ObjC exposure is breaking for Objective-C and KVO clients (S26).

## 13. Swift classification rules

Same discipline as CLS-01: one class per rule in `Evaluators/Swift/`, one test class each
(TST-01). Rules operate only on **paired units of the same module** (NCL-03), and within a
unit, member rules operate only on **paired types**.

| ID | Fires when… | Impact |
|----|-------------|:------:|
| S01 | a public type (class/struct/enum/protocol/actor/typealias) is removed | Major |
| S02 | a public type is added | Minor |
| S03 | a type's kind changes (struct↔class, enum↔struct, …) | Major |
| S04 | a class goes `open` → `public` (subclassing/overriding withdrawn) | Major |
| S05 | a class goes `public` → `open` | Minor |
| S06 | a class gains `final` | Major |
| S07 | a class loses `final` | Minor |
| S08 | a superclass is changed or removed | Major |
| S09 | a protocol conformance is removed from a public type | Major |
| S10 | a protocol conformance is added | Minor |
| S11 | a generic parameter count changes | Major |
| S12 | a generic constraint is added or tightened | Major |
| S13 | a generic constraint is removed or loosened | Minor |
| S14 | `@frozen` is removed from a public struct/enum | Major |
| S15 | `@frozen` is added | Minor |
| S16 | a public member (func/property/init/subscript/nested type) is removed | Major |
| S17 | a public member is added to an existing type | Minor |
| S18 | an enum case is added | **Major** — a client's exhaustive `switch` stops compiling |
| S19 | an enum case is removed, renamed, or its associated values / raw value change | Major |
| S20 | a protocol gains a requirement with **no** default implementation | Major |
| S21 | a protocol gains a requirement **with** a default implementation in an extension | Minor |
| S22 | a function's parameter labels, types, order, count, or return type change | Major |
| S23 | `throws` or `async` is added to an existing declaration | Major |
| S24 | `throws` or `async` is removed | Minor |
| S25 | a declaration becomes `unavailable` or gains an `obsoleted:` availability | Major |
| S26 | a declaration is marked `deprecated` (and nothing else changed) | Patch |
| S27 | ObjC exposure (`@objc`) is removed from a public declaration | Major |
| S28 | ObjC exposure is added | Minor |
| S29 | `mutating` is added to a member of a value type | Major |
| S30 | `mutating` is removed | Minor |
| S31 | a default argument value is removed from a parameter | Major |
| S32 | a default argument value is added to a parameter | Minor |
| S33 | a parameter's `inout` / variadic / ownership modifier changes | Major |
| S34 | a member's static-vs-instance-ness changes | Major |
| S35 | a property's setter is removed (settable → get-only) | Major |
| S36 | a property gains a setter | Minor |
| S37 | a property's type changes | Major |
| S38 | an operator declaration or its precedence group changes | Major |

**SCL-01 — S18 rationale, and the one knob worth exposing.** ℹ️
Adding a case to a public enum is source-breaking for any client that switches exhaustively —
which is every client of a package built without library evolution, i.e. the common SwiftPM
case. It is therefore **Major** by default. If a `@frozen`-vs-non-frozen distinction is later
wanted (non-frozen in a library-evolution module forces clients to write `@unknown default`,
making additions Minor), that is a follow-up, and it must be a deliberate decision, not a
silent default.

**SCL-02 — Overlapping rules are fine.** ℹ️
A renamed method fires S16 (removed) and S17 (added); Major wins per ML-05. Rules SHALL NOT be
made mutually exclusive at the cost of clarity — aggregation already handles it (CLS-03).

## 14. Version model and synchronization

**MVR-01 — Version value semantics unchanged.** ✅ required
`Version`, `VersionType`, VER-01…VER-05 carry over as-is.

**MVR-02 — Fix the version edge cases.** ✅ required — *closes G-11*
Versions with fewer than three segments SHALL be normalized to three on parse (`"1.0"` →
`1.0.0`) rather than crashing `Increment`; an empty version SHALL behave as `0.0.0` for
`ToString` and comparison. With multiple ecosystems feeding seeds, two-segment values
(`MARKETING_VERSION = 1.2`) are now routine input, not a latent edge.

**MVR-03 — Seed = highest across everything.** ✅ required (D-07) — *generalizes VER-06*
The starting version SHALL be the highest version found across **all** sources in **all**
units:

| Language | Source | Read | Write |
|----------|--------|:----:|:-----:|
| C# | `.csproj` `AssemblyVersion`, `PackageVersion`, `FileVersion` | ✅ | ✅ |
| Swift/Xcode | `MARKETING_VERSION` in build settings (`xcodebuild -showBuildSettings -json`, written back into `project.pbxproj`) | ✅ | ✅ |
| Swift/Xcode | `CFBundleShortVersionString` in `Info.plist` | ✅ | ✅ |
| Swift | `.podspec` `s.version` | ✅ | ✅ |
| Swift | git tags matching `v?MAJOR.MINOR.PATCH` | ✅ | opt-in, §20 O-02 |
| Swift | generated version file (e.g. `EasySemVerVersion.swift`) | ✅ | ✅ if it exists |

Unparseable values SHALL be skipped with a warning, never fail the run.

**MVR-04 — Never create a version property.** ✅ required — *generalizes SYN-03/OVR-04*
Write-back SHALL only update version locations that already exist. A `.podspec` without a
literal `version`, an Xcode target without `MARKETING_VERSION`, a package with no version file
— all are read-skipped and write-skipped. Opting in stays an explicit act by the consuming
team.

**MVR-05 — Write every occurrence, in every unit.** ✅ required — *generalizes SYN-01/SYN-02*
The single new version SHALL be written to every existing version location in every discovered
unit across all languages. SYN-04 (DOM rewrite, normalized formatting) and SYN-05 (no
transactionality; next run self-corrects via highest-wins) carry over.

**MVR-06 — Non-semver build counters are out of scope.** ⚠️ decision needed
`CURRENT_PROJECT_VERSION` / `CFBundleVersion` are build numbers, frequently a bare integer
that VER-01 cannot parse. Default behavior: **read-skip and write-skip them**. See §20 O-01.

## 15. Errors, logging, exit codes

**ERR-M1 — Exit codes unchanged.** ✅ required
0 on success; 1 on any unhandled failure with the exception printed (CLI-06). Swift extraction
failure is such a failure (SWE-05).

**ERR-M2 — Fix the logger while you're in here.** ✅ required — *closes G-12*
`Log.Indent`/`Outdent` semantics are currently inverted and the indent-aligned message is
computed then discarded; no call site uses it, and several paths bypass `Log` for bare
`Console.WriteLine`. With multiple languages, multiple units, and shelled-out tools, structured
indented logging becomes genuinely useful: root → per language → per unit → per firing rule.
Route everything through `Log`.

**ERR-M3 — Report the verdict legibly.** ✅ required
The run SHALL log, at minimum: the folder root, unit count per language, each firing rule with
its unit and impact, the aggregate change type, the seed version, the new version, and each
file written. The current `"Yay differences: {rule}"` line is replaced by something a build log
reader can act on.

## 16. Testing

**TST-M1 — Rule coverage.** ✅ required
Every rule in §9 and §13 SHALL have a dedicated test class asserting: declared impact, a
no-difference negative case, and a representative positive case, built from hand-constructed
signature graphs (never from live extraction). Directional rules (S04/S05, S06/S07, S23/S24,
S29/S30, S31/S32, S35/S36, R32/R33, R39/R40) SHALL additionally assert the non-firing
direction. This preserves TST-01 and is the main defense against a 60-rule set rotting.

**TST-M2 — Neutral rules.** ✅ required
Unit added/removed (NCL-01/NCL-02) tested across languages, including the mixed case (a Swift
unit removed while a C# unit is added → Major).

**TST-M3 — Discovery.** ✅ required
Fixture folder trees asserting: `.csproj` under `bin`/`obj`/`.packages` is ignored; SwiftPM
dependency checkouts under `.build` are ignored; a folder with no `.sln` works; a folder with
no units at all exits 0.

**TST-M4 — Baseline round-trip.** ✅ required
A populated multi-language unit array SHALL serialize, deserialize, and re-serialize to
byte-identical XML; unknown `formatVersion` SHALL degrade to empty-baseline; the file SHALL
contain no absolute path (assert by scanning the output for the temp root).

**TST-M5 — Swift extraction, toolchain-free.** ✅ required
Extraction SHALL be tested by feeding **checked-in symbol-graph JSON fixtures** through the
parser (via the `IRunProcess` seam, ML-07), so the unit suite runs on any machine. Assert the
`ISwiftModule` graph for a fixture covering: struct, class, actor, enum with associated values,
protocol with and without default implementations, extension on an in-module type, extension on
an external type, generics with constraints, `async`/`throws`, `@available`, `@objc`.

**TST-M6 — Swift extraction, live.** ✅ required
A small dependency-free fixture SwiftPM package (`src/TestFixtures/SwiftPackage/`) SHALL be
built and extracted for real, asserting a handful of symbols. Marked as a trait/category so it
can be skipped where Swift is absent. **No network access** — no external package
dependencies, so `swift build` never resolves.

**TST-M7 — Failure path.** ✅ required
With Swift units present and the process runner stubbed to "command not found" and to
"non-zero exit", the run SHALL fail, SHALL name the unit and command, and SHALL leave both the
baseline file and every project file byte-identical.

**TST-M8 — Integration regression restored.** ✅ required — *satisfies TST-05*
`Regression.TestProgramInvocation` SHALL pass: two consecutive runs over an unchanged tree
increment Patch by exactly 1. This is the proof that G-01 is dead. Extend it with a
multi-language fixture folder (one `.csproj` + one SwiftPM package) once §18 P4 lands.

**TST-M9 — Hygiene.** ✅ required — *closes G-18*
Delete `Experimental.cs`'s hard-coded `/Users/andrew/...` path test; remove the
`Test.csproj` content references to the non-existent `SampleCsProj.xml` / `TargetCsProj.xml`.

## 17. Documentation to update at the end

- **specs/01** — pipeline diagram, core concepts, non-goals (Swift is no longer one), the
  "future direction" section becomes "implemented in doc 12".
- **specs/02** — CLI contract: folder argument honored, no `.sln` requirement; MSBuild section
  reconciled with G-02/G-03/G-04 (either restore a `Task` class or rewrite the README around
  the packaged targets — this is a real decision, not a doc edit).
- **specs/04** — rewritten as folder discovery; DSC-01/02 retired, DSC-03/06 generalized.
- **specs/05** — becomes "C# signature extraction", with CSX-01…CSX-04 folded in.
- **specs/06** — baseline v2 (§6), PER-05 marked resolved.
- **specs/07** — C# rules R01–R42 with R07/R14 marked retired to §7; add a pointer to §13.
- **specs/08/09** — MVR-02…MVR-06.
- **specs/11** — the test requirements above.
- **specs/99** — mark G-01, G-06, G-09, G-10, G-11, G-12, G-13, G-14, G-15, G-18, G-19 resolved
  (each with the requirement that resolved it, per the file's existing convention of never
  reusing IDs); G-02/G-03/G-04/G-05/G-17 remain unless separately addressed.
- **README.md** — folder-based usage, Swift prerequisites (toolchain, Xcode), the version
  sources table, and the stale "Auto-Version"/`UsingTask`/post-build content (G-17).
- **.github/workflows/dotnet.yml** — macOS runner for the Swift suite, `AutoVersion` →
  `EasySemVer`, SDK version (G-05).

## 18. Implementation order

Each phase ends green: build clean, full unit suite passing, working tree committable.

- **P1 — Rename and relayout (§8).** Purely mechanical. All 65 existing tests still pass.
- **P2 — Neutral seam, folder CLI, baseline v2 (§3, §4, §6, §7).** C# remains the only
  provider. Ends with the integration test passing for the first time (G-01 dead). This is the
  highest-value phase — land it before touching Swift.
- **P3 — C# fidelity expansion (§9).** R18–R42 plus tests.
- **P4 — Swift via SwiftPM (§10 SWD-01, §11, §12, §13).** Fixture-driven extraction tests
  first, then live.
- **P5 — Xcode targets (§10 SWD-02, §14 Xcode rows).** Discovery, `xcodebuild` symbol graphs,
  `project.pbxproj` / Info.plist version read-write.
- **P6 — Docs, README, CI (§17).**

Commit per phase, or per coherent slice within a phase. Do not begin P4 until P2's integration
test is green — Swift work built on an unwritable baseline cannot be verified.

## 19. Acceptance criteria

1. `dotnet build` clean (the `NU5129` packaging warning may remain if G-04 is deferred; say so
   explicitly if it is).
2. Full unit suite green, including a dedicated test class per rule in §9 and §13.
3. `Regression.TestProgramInvocation` passes: two runs over an unchanged tree bump Patch by
   exactly 1.
4. Pointing the tool at a folder with **no** `.sln` works end to end.
5. Pointing it at a folder containing a `.csproj` **and** a SwiftPM package produces a baseline
   containing both units, and a single version stamped into both ecosystems' version locations.
6. Baseline XML is byte-identical across two runs on unchanged source and contains no absolute
   path, timestamp, or toolchain version.
7. With Swift units present and `swift` unavailable, the run exits 1, names the unit and the
   command, and leaves every file on disk unchanged.
8. Adding a hypothetical Java provider would require no edit to any file under
   `Interfaces/` (neutral), `Evaluation/`, or `Persistence/` other than one registration line —
   state in the summary whether this holds.
9. Specs and README updated per §17.

## 20. Open items — raise these, do not silently decide

**O-01 — Build counters.** MVR-06 proposes skipping `CURRENT_PROJECT_VERSION` /
`CFBundleVersion` because they are usually bare integers that VER-01 rejects. The stated
decision was "read all version sources and take the highest," which arguably includes them.
Recommend: skip them for seeding, and optionally mirror `MARKETING_VERSION` into them only when
the existing value already parses as ≥2 segments. **Confirm before implementing.**

**O-02 — Git tags.** Reading the highest semver tag as a seed input is safe and is specified.
*Writing* a tag is an outward-facing, effectively irreversible act. Recommend: `--tag` opt-in,
local tag only, never `push`, and never on by default. **Confirm.**

**O-03 — Xcode build cost.** D-06 puts Xcode symbol graphs in scope, and D-03 makes failure
fatal, so every versioned build of a repo containing an `.xcodeproj` runs `xcodebuild`. If P5
proves this makes the tool impractical in a real build hook, the recommended fallback is a
per-unit opt-out (`.easysemver` config marking a unit version-sync-only) rather than silently
skipping. Report the measured cost from the fixture project when P5 lands.

> **Measured, P5:** against
> [`src/TestFixtures/XcodeProject`](../src/TestFixtures/XcodeProject) — one static-library target,
> one Swift file — a full run takes **4.1 s**: ~1.0 s for `xcodebuild -list -json` and ~3.1 s for
> the symbol-graph build. That is the floor, not the typical case: it is what `xcodebuild` costs
> before compiling anything of consequence, and a real app target's build time is added to it in
> full. On the same machine a one-target SwiftPM package costs ~1.3 s, rising to ~16 s once
> `--build-tests` compiles the test target as well (which UNI-03 requires).
>
> No opt-out was added: nothing measured here proves the tool impractical, and adding a config
> file to skip units is a bigger decision than a timing number justifies. The recommendation
> stands if a real Xcode project proves otherwise.

**O-04 — `--dry-run`.** CLI-07 currently states, by design, that there is no preview mode. With
Xcode builds in the loop and a 60-rule set, a mode that classifies and reports without writing
becomes valuable both for users and for the test suite. Recommend adding it; it does not
conflict with OVR-03 (a dry run is simply not a release). **Confirm.**

**O-05 — MSBuild integration story.** G-02/G-03/G-04 remain unresolved: the README documents a
`UsingTask` hook against a `Task` class that no longer exists, the packaged targets point at a
`tools/` folder that is never packed, and the targets filename does not match the package ID so
NuGet never imports it. A folder-based CLI makes the packaged-targets route the natural answer.
Out of scope for this spec, but flag it in the final summary — the tool is not consumable until
it is fixed.

**O-06 — Non-Swift Apple-adjacent surface.** Objective-C headers in a mixed target are visible
to Swift symbol graphs only through the generated interface. If a discovered Xcode target is
pure Objective-C, it has no Swift symbol graph. Recommend treating it as a version-sync-only
unit and logging that clearly, rather than failing per SWE-05. **Confirm.**
