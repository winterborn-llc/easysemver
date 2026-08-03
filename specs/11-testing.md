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

**TST-M5 — Swift extraction, toolchain-free.** ✅
[`TestSymbolGraphReader`](../src/Test/Swift/TestSymbolGraphReader.cs) feeds **checked-in
symbol-graph JSON** ([`src/Test/Fixtures/`](../src/Test/Fixtures)) through the parser, so the unit
suite runs on any machine. The fixture covers struct, class, actor, enum with associated values,
protocol with and without default implementations, extension on an in-module type, extension on
an external type, generics with constraints, `async`/`throws`, `@available`, `@objc` — and
asserts that synthesized conformance members and mangled names never reach the model.
ℹ️ The fixtures were produced by a real toolchain from
[`Widgets.swift.txt`](../src/Test/Fixtures/Widgets.swift.txt), then stripped of source locations
so they carry nothing machine-specific.

**TST-M6 — Swift extraction, live.** ✅
[`SwiftRegression`](../src/IntegrationTest/SwiftRegression.cs) builds and extracts the fixture
package [`src/TestFixtures/SwiftPackage/`](../src/TestFixtures/SwiftPackage) for real. It is
marked `[Trait("Toolchain", "Swift")]` so it can be skipped where Swift is absent
(`dotnet test --filter Toolchain!=Swift`). The package has **no external dependencies**, so
`swift build` never resolves and the suite needs no network.
ℹ️ The manifest is checked in as `Package.swift.template` and renamed by the fixture when it
copies the tree to a temporary directory. A real `Package.swift` in this repository would make
the repository itself a multi-language tree, so every run — including TST-05 — would require a
Swift toolchain and an `swift build`.

**TST-M6a — Xcode extraction, live.** ✅
[`XcodeRegression`](../src/IntegrationTest/XcodeRegression.cs) exercises the Xcode path end to
end against [`src/TestFixtures/XcodeProject/`](../src/TestFixtures/XcodeProject): target
discovery via `xcodebuild -list -json`, symbol-graph extraction, `MARKETING_VERSION` read and
written back, `CURRENT_PROJECT_VERSION` left untouched, and a byte-identical baseline on a second
run. Same `Toolchain=Swift` trait.
ℹ️ The project bundle is checked in as `App.xcodeproj.template` and renamed by the fixture, for
the same reason the SwiftPM manifest is.

**TST-M7 — Failure path.** ✅
[`TestSwiftExtractionFailure`](../src/Test/Swift/TestSwiftExtractionFailure.cs) stubs the process
runner with "command not found", "non-zero exit" and "timed out", and asserts that the run fails,
that the message names the unit, the exact command and the tool's stderr, and that both the
baseline file and every file on disk are left byte-identical.
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

**TST-06 — Verification snapshot (2026-08-02).** ℹ️
.NET SDK 10.0.100, Swift 6.3.3, macOS:

| Suite | Result |
|-------|--------|
| `dotnet build` | ✅ success — one warning, `NU5129` (PKG-05, gap G-04) |
| `Test` (unit) | ✅ **502/502 passed** |
| `IntegrationTest` | ✅ **18/18 passed** (3 C#, 5 JSON report, 4 SwiftPM, 3 Xcode, 3 others) |

ℹ️ The Swift-traited tests shell out to `swift` and `xcodebuild` and are therefore sensitive to
machine load: on a box already saturated with another Swift build they will hit the five-minute
manifest timeout and fail. On an idle machine the SwiftPM suite takes about 90 seconds (most of
it `--build-tests`) and the Xcode suite about 10.
