# 99 — Known Gaps and Deviations

Consolidated list of every place the implementation diverges from the requirements in this
folder, ordered by severity. "Verified" means confirmed by executing the code; "code-read" means
established by inspection. IDs are never reused, so references from issues and commits keep
resolving.

## Blockers

*None.* G-01 — the one blocker — is resolved; see below.

## Major

*None.*

## Minor

**G-16 — Minimal compilation references degrade foreign type names.** (Code-read)
Cross-project and NuGet types resolve as error symbols in the ad-hoc compilation, so recorded
names may lack namespaces. Stable run-to-run, so diffs work, but theoretically collidable.
*Relates to:* SIG-01.

**G-20 — ACT-10's release block is specified but not yet in force.** (Verified)
CLI-10 and REP-10 are implemented and tested, but the copy-paste release block they exist to enable
cannot be adopted until a release ships them: the Action runs a *published* binary (ACT-02), so a
workflow using the block would download a binary that rejects `--github` and reports no
`writtenFiles`. Until then [`dotnet.yml`](../.github/workflows/dotnet.yml) keeps the hand-written
version, commit and tag steps — including the `git add EasySemVer.xml src/*/*.csproj` glob REP-10
exists to retire, which stays silently wrong for any project it does not match.

*Closing it* is one commit, taken after the release that carries CLI-10, REP-10 and ACT-11 lands on
nuget.org and as a GitHub Release: bump ACT-02's pinned `version` default and every `uses:` ref in
the README to that tag, replace the four steps in `dotnet.yml` with ACT-12's two invocations, and add the
ACT-10 regression test asserting the workflow and the README agree.
ℹ️ Only the *adoption* is blocked. ACT-11 itself is implemented and covered — `ActionRegression`
exercises the commit step against a real repository with a real remote, including the atomicity
guarantee, which was confirmed by removing `--atomic` and watching the test fail.
*Relates to:* ACT-02, ACT-10, ACT-11, CLI-10, REP-10.

## Informative / hygiene

**G-17 — Stale documentation.** ✅ Resolved by the doc-12 rewrite of
[README.md](../README.md): folder-based usage, the version-source table and Swift prerequisites
replaced the "Auto-Version" content, the stale package version, the post-build rationale and the
`UsingTask` instructions. The warning about the MSBuild route that briefly replaced them is gone
too, now that the route itself is (G-02/G-03/G-04 withdrawn).

## Withdrawn

Requirements that were dropped rather than met, because the shape of the product changed. IDs are
never reused.

**G-02 — Documented MSBuild integration cannot work.** ⊘ Withdrawn
**G-03 — Packaged targets point at an executable that isn't packed.** ⊘ Withdrawn
**G-04 — Targets file not auto-imported (`NU5129`).** ⊘ Withdrawn

All three were defects in one mechanism: consuming EasySemVer as a `PackageReference` that hooks
itself into a consuming build. On 2026-08-02 that mechanism was withdrawn — EasySemVer is a CLI,
distributed as a `dotnet tool`, as self-contained binaries, and as a GitHub Action
(**INV-01**, **PKG-01/PKG-02**, **ACT-01…ACT-09**). A tool whose unit of work is *a folder* does not belong bolted
to one arbitrary project inside it.

Deleting `Winterborn.EasySemVer.targets` also removed the `NU5129` warning, so the build is now
warning-free. **§20 O-05 is closed by withdrawal, not by repair.**

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
