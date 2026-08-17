# 11 — Testing Requirements

Test projects: [`src/Test`](../src/Test) (unit, xUnit) and
[`src/IntegrationTest`](../src/IntegrationTest) (end-to-end, xUnit). Internals are exposed to
both via `InternalsVisibleTo`
([`AssemblySettings.cs`](../src/EasySemVer/Settings/AssemblySettings.cs)).

**TST-01 — Every classification rule is tested.** ✅ 81/81 (41 C#, 38 Swift, 2 neutral)
Each also carries the spec id it implements, asserted by TST-M10.
Each evaluator SHALL have a dedicated test class asserting at minimum:
1. the declared `EvaluationImpact` (locks the rule's severity),
2. a no-difference case returns `false`,
3. a representative difference case returns `true`,

built from hand-constructed signature graphs
([`Build`](../src/Test/Build.cs), [`BuildSwift`](../src/Test/Swift/BuildSwift.cs)) — **never**
from live extraction, so a rule test failing means the rule is wrong and nothing else.

Rules whose correctness is *directional* SHALL additionally assert the non-firing direction.
That covers R32/R33, R34/R35, R39/R40, R20/R21, R42, R17, and on the Swift side S04/S05,
S06/S07, S14/S15, S16/S17, S23/S24, S29/S30, S31/S32, S35/S36, S09/S10, S12/S13, S18, S27/S28.

Locations: [`src/Test/Evaluators/`](../src/Test/Evaluators) for the neutral rules,
[`Evaluators/Csharp/`](../src/Test/Evaluators/Csharp) and
[`Evaluators/Swift/`](../src/Test/Evaluators/Swift) for the language rules.

**TST-02 — Version arithmetic.** ✅
[`TestVersion`](../src/Test/TestVersion.cs) SHALL cover segment extraction, the increment table
including the `int.MaxValue` rollover cases (VER-04/VER-05), short and blank version
normalization (MVR-02), and `TryParse` rejecting unparseable input without throwing.

**TST-03 — Version source extraction.** ✅
[`TestExtractingVersionFromCsProjFile`](../src/Test/TestExtractingVersionFromCsProjFile.cs)
covers the csproj rows of VER-06;
[`TestSwiftVersionSources`](../src/Test/Swift/TestSwiftVersionSources.cs) and
[`TestXcodeVersionSources`](../src/Test/Swift/TestXcodeVersionSources.cs) cover podspec, Swift
version file, git tag, `MARKETING_VERSION` and `CFBundleShortVersionString` — including that a
non-literal value is read-skipped and write-skipped (MVR-04) and that build counters are left
alone (MVR-06).

**TST-04 — Extraction against real source.** ✅
[`TestCsharpExtraction`](../src/Test/TestCsharpExtraction.cs) SHALL run real extraction over a
fixture source file covering the full C# topology: every type kind, positional record parameters,
enum members and underlying type, delegate signatures, fields, events, modifiers, generic
constraints, nested types, interface default implementations, per-overload return types.

[`TestCsharpSignatureIsolation`](../src/Test/TestCsharpSignatureIsolation.cs) SHALL assert the
other half of SIG-03: that a unit's signature contains what the unit's own source declares and
nothing reached through a metadata reference. The compilation's framework references come from
whichever runtime the tool is executing on, so a symbol leaking in from metadata would make the
baseline a property of the machine that wrote it. It asserts the outcome rather than the
mechanism, so it holds however the symbol walk is later narrowed, and covers the partial-type case
where one type has several declaring syntax references.
ℹ️ It replaces the former `TestSelfSignature`, which extracted the test project's own sources: a
smoke test whose expectations changed every time the test project did.

**TST-05 / TST-M8 — Integration regression.** ✅ **Resolved** *(was blocked by G-01)*
[`Regression.TestProgramInvocation`](../src/IntegrationTest/Regression.cs) SHALL invoke
`Program.Main()` twice against the real repository: the first run establishes the baseline; the
second — with no code changes in between — SHALL increment **Patch by exactly 1**. This is the
proof that the baseline can be written at all. The test's own comment notes it legitimately fails
on the first run after a real Major/Minor change.
ℹ️ Side effect to be aware of: this test **mutates the working tree** (bumps csproj versions,
rewrites `EasySemVer.xml`) — by design, but CI and local runs must expect dirty files afterwards.

**TST-M2 — Neutral rules.** ✅
[`TestUnitAdded`](../src/Test/Evaluators/TestUnitAdded.cs),
[`TestUnitRemoved`](../src/Test/Evaluators/TestUnitRemoved.cs) and
[`TestChangeClassifier`](../src/Test/Evaluators/TestChangeClassifier.cs) cover NCL-01/NCL-02
across languages, including the mixed case: a Swift unit removed while a C# unit is added → Major.

**TST-M3 — Discovery.** ✅
[`TestFolderDiscovery`](../src/Test/TestFolderDiscovery.cs) asserts over fixture folder trees
that a `.csproj` under `bin`/`obj`/`.packages`/`.build`/`node_modules`/`Pods`/`Packages`/
`DerivedData` is ignored, that a folder with no `.sln` works, that a folder with no units at all
yields nothing, that discovery is ordered, and that a directory bundle is treated as a leaf.

**TST-M4 — Baseline round-trip.** ✅
[`TestBaselineFile`](../src/Test/Persistence/TestBaselineFile.cs) asserts that a populated unit
array serializes, deserializes and re-serializes to identical XML; that the signature survives
the round trip; that units are sorted by `(Language, UnitId)`; that an unknown or absent
`formatVersion` is rejected; that an unreadable or missing file degrades to an empty baseline;
and that the written file contains no absolute path.

**TST-M5 — Swift extraction.** ✅
[`TestSwiftSourceReader`](../src/Test/Swift/TestSwiftSourceReader.cs) feeds **checked-in Swift
source** ([`Widgets.swift.txt`](../src/Test/Fixtures/Widgets.swift.txt)) through the reader, which
is now the whole of extraction. The fixture covers struct, class, actor, enum with associated
values, enum with a raw value type, protocol with and without default implementations, extension
on an in-module type, extension on an external type, generics with constraints, `async`/`throws`,
`mutating`/`inout`/variadic, `@frozen`, `@available` and `@objc` — and asserts that synthesized
conformance members never reach the model.
ℹ️ The same file used to be the input a real toolchain was run over to produce the checked-in
symbol graphs this test read. It is now the input directly, and the graphs are gone.

**TST-M5a — No toolchain, structurally.** ✅
[`TestSwiftNeedsNoToolchain`](../src/Test/Swift/TestSwiftNeedsNoToolchain.cs) asserts that nothing
under `CodeReader/Swift` except the git-tag version source can reach `IRunProcess` at all, that the
Swift provider is not handed one, and that a whole package is discovered and extracted with an
empty `PATH`. A process runner finding its way back into the Swift reader would leave every other
test in the suite passing and merely make the tool need Xcode again, which is exactly the kind of
regression an assertion about behaviour cannot see.

**TST-M6 — Swift, end to end.** ✅
[`SwiftRegression`](../src/IntegrationTest/SwiftRegression.cs) discovers, extracts and versions the
fixture package [`src/TestFixtures/SwiftPackage/`](../src/TestFixtures/SwiftPackage) through the
real tool. It carries no toolchain trait any more, because there is nothing to skip it for: one of
its tests runs the whole tool with `PATH` emptied and asserts it succeeds.
ℹ️ The manifest is still checked in as `Package.swift.template` and renamed by the fixture when it
copies the tree to a temporary directory — otherwise this repository would be a multi-language
tree and every run over it, including TST-05, would discover targets that are fixtures.

**TST-M6a — Xcode, end to end.** ✅
[`XcodeRegression`](../src/IntegrationTest/XcodeRegression.cs) exercises the Xcode path end to
end against [`src/TestFixtures/XcodeProject/`](../src/TestFixtures/XcodeProject): target discovery
from `project.pbxproj`, source resolution through the group hierarchy, extraction,
`MARKETING_VERSION` read and written back, `CURRENT_PROJECT_VERSION` left untouched, and a
byte-identical baseline on a second run. No trait, and no Xcode.
ℹ️ The project bundle is checked in as `App.xcodeproj.template` and renamed by the fixture, for
the same reason the SwiftPM manifest is.

**TST-M7 — Failure path.** ✅
[`TestSwiftExtractionFailure`](../src/Test/Swift/TestSwiftExtractionFailure.cs) declares a target
in the manifest whose source directory does not exist, and asserts that the run fails, that the
message names the target and says where it looked, and that both the baseline file and every file
on disk are left byte-identical. It also asserts the case that is *not* a failure: a target whose
directory holds no Swift is an empty module, not a broken package (O-06).
`SwiftRegression.MissingToolchainFailsTheRun` asserts the same against a real empty `PATH`.

**TST-M10 — Rule identifiers are pinned to the specs.** ✅
[`TestRuleIds`](../src/Test/TestRuleIds.cs) SHALL read the rule tables at run time rather than
restating them, and assert: every rule carries a well-formed id; no id is claimed by two rules
except where the spec defines one requirement with two directions (R41); every id a rule claims
has a row in its table; and every live row has a rule behind it, with retired rows (R07, R14)
recognised as retired rather than missing.
ℹ️ This was not hypothetical. Six C# rules carried an id in their documentation that disagreed
with specs/07 — `MethodOverrideAdded` claimed R16 where the table says R04, `PropertyAdded`
claimed R17 where it says R16, and four more — and were corrected when the ids became a published
contract.

**TST-M11 — The JSON report's contract.** ✅
[`TestJsonChangeReport`](../src/Test/Reporting/TestJsonChangeReport.cs) SHALL assert the exact
property set and order, that the deliberately omitted fields stay omitted (REP-05), lower-case
enum values, both version objects sharing one shape, a four-segment version keeping its string
intact (VER-07), determinism, and that nothing machine-specific reaches the document (REP-07).
[`JsonReportRegression`](../src/IntegrationTest/JsonReportRegression.cs) SHALL assert the same
through a whole run, including that the version it reports is the one that reached the disk, that
two dry runs produce byte-identical reports, and that a failed run leaves no report behind
(REP-08) — the last of which is only observable end to end.

**TST-M9 — Hygiene.** ✅ **Resolved** *(was G-18)*
`Experimental.cs` and its hard-coded `/Users/andrew/…` path are deleted, as are the
`Test.csproj` content references to the non-existent `SampleCsProj.xml` / `TargetCsProj.xml`.

**TST-06 — Verification snapshot (2026-08-09).** ℹ️
.NET SDK 10.0.100, Swift 6.3.3, macOS 26.5.2:

| Suite | Result |
|-------|--------|
| `dotnet build` | ✅ success — **no warnings** (`NU5129` went with PKG-05) |
| `Test` (unit) | ✅ **575/575 passed** in under a second |
| `IntegrationTest` | ✅ **65/65 passed** in about 30 seconds (6 `Regression`, 9 `JsonReportRegression`, 43 `ActionRegression`, 4 SwiftPM, 3 Xcode) |

ℹ️ `ActionRegression` (ACT-09) needs `bash` and `tar`, which every GitHub-hosted runner has (`jq`
went with the `jq` block CLI-10 removed from `action.yml`); it is traited `Toolchain=Bash` so a
machine without them can exclude it. Filtering it out costs 43 tests. It is also the whole of the
wall clock: it shells out to `bash` and `git` 43 times.

ℹ️ The seven Swift and Xcode tests now run in about **0.3 seconds together**, and are no longer
traited at all. They took the toolchain path's cost with them: a SwiftPM run was ~1.3 seconds after
`--build-tests` was dropped (and ~11-20 before that), and an Xcode run was ~4.1, of which ~1.0 was
`xcodebuild -list` doing nothing but resolving dependencies. They were also sensitive to machine
load — a box already saturated with another Swift build could hit the five-minute manifest timeout
and fail the suite. None of that applies to reading files.
