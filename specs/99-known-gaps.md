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
(**INV-01**, **PKG-01/PKG-02**, **ACT-01…ACT-12**). A tool whose unit of work is *a folder* does not belong bolted
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
ℹ️ Since split out: build-and-test is a reusable workflow still on `macos-latest`, and the release
job that depends on it runs on `ubuntu-latest` (CI-01).
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

**G-20 — ACT-10's release block specified but not in force.** ✅ Resolved by adoption
CLI-10, REP-10, ACT-11 and ACT-12 shipped in one commit; that commit's own run published **v16.0.0**
to nuget.org and cut the GitHub Release with all five PKG-02 archives. The commit after it pinned
ACT-02's `version` default and the README's `uses:` refs to that tag, replaced four hand-written
steps in [`dotnet.yml`](../.github/workflows/dotnet.yml) with ACT-12's two invocations, and added
`ActionRegression.TheDocumentedReleaseStepsAreTheOnesThisRepositoryRuns` — which asserts the README
and the workflow cannot drift, and was confirmed to fail when they do.

The two-commit shape was not avoidable and is not a defect: ACT-02's Action runs a *published*
binary, so nothing here can consume a capability in the same commit that introduces it.
ℹ️ Retiring the `git add EasySemVer.xml src/*/*.csproj` glob was the point. It was silently wrong
for any project it did not match — a green run, a tag, and a commit with no bump in it.
*Satisfies:* ACT-10. *Relates to:* ACT-02, ACT-11, ACT-12, CLI-10, REP-10.

**G-21 — v16.0.0's action manifest fails to load.** ✅ Resolved by **ACT-13**
The `report` input's description carried `${{ steps.version.outputs.report }}` as a worked example.
GitHub evaluates expressions inside input descriptions, where the `steps` context does not exist, so
`action.yml` failed template validation at *Set up job*: `Unrecognized named-value: 'steps'`. Every
consumer of `winterborn-llc/easysemver@v16.0.0` gets that error before any step runs, and this
repository could not cut the fix because its own workflow pinned to that tag.

Fixed by removing the example (it lives in the README), by `uses: ./` so a manifest is loaded by the
run that publishes it, and by `ActionRegression.NoInputDescriptionCarriesAnExpression` — confirmed
to fail when the example is put back.
ℹ️ **v16.0.0 remains broken as an Action and cannot be repaired**: a published release is immutable
and moving the tag would be a history rewrite. It is superseded, not fixed. The tool binary and the
NuGet package at that version are unaffected — only the manifest.
ℹ️ Nothing was published by the failed run. CI-03's ordering held: it died before the version step,
so no commit, no tag, and nothing on nuget.org.
*Relates to:* ACT-02, ACT-09, ACT-10, ACT-13.

**G-22 — The pinned version and the README refs go stale between releases.** ✅ Resolved by **CI-05**
ACT-02's `version` default and the README's `uses:` refs were hand-edited, in a commit separate from
the release that made them true. They were consequently wrong most of the time: `action.yml` pinned
`v16.1.1` while v16.1.2 was the newest release, and every README example told a reader to copy a tag
one release behind the one the pin would download.

Nothing broke, because an exact tag names a manifest and a binary that shipped together whichever
release that was — which is precisely why it went unnoticed. It became load-bearing the moment a
moving `v16` was wanted: a floating tag over a hand-pinned manifest hands out the newest wrapper
around the oldest tool, and widens the gap on every release.

Fixed by stamping both in the release run itself, staged into the release commit so they are inside
the tag, and by three assertions in `ActionRegression` — that exactly one line in `action.yml` looks
like the pin the rewrite targets, that the README's refs are the major tag of that pin, and that a
file staged before the ACT-11 commit step reaches the commit at all, which is the mechanism the
whole thing rides on and is no part of that step's contract.
ℹ️ Releases before v16.1.2 keep whatever pin they were published with. They are not wrong — each
still names a real release — merely older than the tag that contains them.
*Satisfies:* CI-05. *Relates to:* ACT-02, ACT-10, ACT-11.
