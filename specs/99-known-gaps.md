# 99 — Known Gaps and Deviations

Consolidated list of every place the implementation diverges from the requirements in this
folder, ordered by severity. "Verified" means confirmed by executing the code; "code-read" means
established by inspection. IDs are never reused, so references from issues and commits keep
resolving.

## Blockers

*None.* G-01 — the one blocker — is resolved; see below.

## Major

**G-02 — Documented MSBuild integration cannot work.** (Code-read) — *partially addressed*
The README no longer documents the `UsingTask` + `<EasySemVer />` route, because the assembly
contains no MSBuild `Task` class (it was deleted with the old `AutoVersion` project). The README
now documents direct invocation instead and states plainly that the packaged integration is not
consumable. A task wrapper is still not present. *Violates:* MSB-02. *See:* 12 §20 **O-05**.

**G-03 — Packaged targets point at an executable that isn't packed.** (Code-read)
`Winterborn.EasySemVer.targets` resolves `..\tools\EasySemVer.exe`, but the `.csproj` packs no
`tools/` folder. *Violates:* PKG-04. *See:* 12 §20 **O-05**.

**G-04 — Targets file not auto-imported (`NU5129`).** (Verified at build)
The buildTransitive targets file must be named `Winterborn.Library.EasySemVer.targets` (matching
the package ID) for NuGet to import it into consumers. This is the one remaining build warning.
*Violates:* PKG-05. *See:* 12 §20 **O-05**.

## Minor

**G-16 — Minimal compilation references degrade foreign type names.** (Code-read)
Cross-project and NuGet types resolve as error symbols in the ad-hoc compilation, so recorded
names may lack namespaces. Stable run-to-run, so diffs work, but theoretically collidable.
*Relates to:* SIG-01.

## Informative / hygiene

**G-17 — Stale documentation.** ✅ Resolved by the doc-12 rewrite of
[README.md](../README.md): folder-based usage, the version-source table, Swift prerequisites, and
an explicit warning about the MSBuild route replaced the "Auto-Version" content, the stale
package version, the post-build rationale and the `UsingTask` instructions.

## Resolved

Retained so the IDs are never reused and so references from issues and commits still resolve.

**G-01 — Baseline serialization throws; no run can complete.** ✅ Resolved by **BAS-02**
The persisted graph is now concrete, public, parameterless-constructible types with
concrete-typed members; the interface view the rules consume comes from *explicit* interface
implementation, which `XmlSerializer` never sees. No interface-typed member remains anywhere on
the persisted graph, and no parallel DTO tree exists to drift.
*Satisfies:* PER-05, SYN-01, TST-05 — `Regression.TestProgramInvocation` passes.

**G-05 — CI workflow is stale and cannot go green.** ✅ Resolved by the §17 workflow rewrite:
`env.project` is `EasySemVer`, the SDK is `10.0.x`, the runner is `macos-latest` so the Swift
suite can run, and the unit and integration suites are separate steps.
*Satisfies:* CI-01.

**G-06 — CLI directory argument is ignored.** ✅ Resolved by **FLD-01**
[`RunOptions.Parse`](../src/EasySemVer/Settings/RunOptions.cs) uses the argument as the folder
root and validates *that* path. *Satisfies:* CLI-04.

**G-07 — Adding a new method or property classified as Patch.** ✅ Resolved
Fixed by rules R15 (`MethodAdded`) and R16 (`PropertyAdded`), both Minor, each with a test class.
*Satisfies:* CLS-05.

**G-08 — Optional→required parameter classified as Minor.** ✅ Resolved
Fixed by rule R17 (`MethodInputParameterMadeRequired`, Major), which reuses R02's matcher but
fires only in the breaking direction, so required→optional stays Minor via R04.
*Satisfies:* CLS-06.

**G-09 — `.slnx` recognized inconsistently.** ✅ Resolved by **FLD-02**
No code path looks for a solution file at all; both walk-up helpers are deleted.
*Satisfies:* DSC-01/DSC-02, now retired.

**G-10 — Source discovery includes `obj/` generated files.** ✅ Resolved by **FLD-04**
Source enumeration goes through the same `FolderScanner` exclusion list as discovery, so build
output and package caches are skipped at any depth. *Satisfies:* DSC-06/DSC-08.
ℹ️ Fixed alongside a related leak found while testing: extraction walked the compilation's
*merged* global namespace, so public types from referenced assemblies (`Internal.Console` out of
`System.Console.dll`) were entering real baselines. It now walks the source assembly's namespace.

**G-11 — Version edge cases crash.** ✅ Resolved by **MVR-02**
Fewer than three segments normalize to three on parse; a blank version behaves as `0.0.0` for
`ToString` and comparison. `Version.TryParse` was added so a version source with an unparseable
value is skipped with a warning rather than failing the run. *Satisfies:* VER-02/VER-07.

**G-12 — Logging half-implemented.** ✅ Resolved by **ERR-M2**
`Indent`/`Outdent` semantics corrected, the indent-aligned message is the one written,
continuation lines align past the timestamp column, and every path routes through `Log` — there
are no bare `Console.WriteLine` calls left in the tool. *Satisfies:* LOG-01/LOG-02/LOG-03.

**G-13 — Redundant discovery + hidden cwd dependency in classification.** ✅ Resolved by
**FLD-03** and **REN-04**
Discovery runs once per invocation and its result feeds every downstream stage.
`CsharpSignaturesToCompare` carries only the two signatures and the paired-type history; it has
no path, no `Save`, and no way to re-discover anything. *Satisfies:* DSC-07, CLS-07.

**G-14 — Return type stored per method name, not per overload.** ✅ Resolved by **CSX-04**
Return type is recorded per overload; R03 compares both the per-name value and each matched
overload's own. *Satisfies:* SIG-11, R03.

**G-15 — Interfaces, structs, enums, delegates, events, fields are invisible.** ✅ Resolved by
**CSX-01…CSX-05**
Every public type kind is modelled as its own concept, nested types included; fields, events,
enum members, positional record parameters, and the full modifier set are captured; rules
R18–R42 classify changes to all of it. The `TODO`s in the builder are gone.
*Satisfies:* SIG-02, and closes the last two ⚠️ rows of the CLS scenario matrix.

**G-18 — Test hygiene.** ✅ Resolved by **TST-M9**
`Experimental.cs` and its hard-coded absolute path are deleted, as are the `Test.csproj`
references to non-existent content files. *Satisfies:* TST-07.

**G-19 — Dead/vestigial code and an implicit dependency.** ✅ Resolved by **REN-05**
Deleted: `CsProjSignature`, `ExtendIList`, `ExtendDirectoryInfo`, `ExtendFileInfo`,
`ExtendString.GetXmlNodeValue`, the commented-out `AdhocWorkspace` block, and the
`Newtonsoft.Json` attributes on `Solution` (along with `Solution` itself, replaced by the unit
array). *Satisfies:* PKG-07.
