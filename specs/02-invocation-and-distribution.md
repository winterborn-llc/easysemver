# 02 — Invocation and Distribution

Sources: [`Program.cs`](../src/EasySemVer/Program.cs),
[`RunOptions.cs`](../src/EasySemVer/Settings/RunOptions.cs),
[`EasySemVer.csproj`](../src/EasySemVer/EasySemVer.csproj),
[`README.md`](../README.md).

## Command-line contract

**CLI-01 — Deliverable is a console executable.** ✅
The tool SHALL build as a console executable (`OutputType=Exe`) with assembly name
`Winterborn.Library.EasySemVer`, multi-targeting `net8.0`, `net9.0`, and `net10.0`
(`LangVersion` 14). `public static int Main(params string[] args)` is public so tests can invoke
a run in-process, and returns its exit code rather than calling `Environment.Exit` — see ERR-03.

**CLI-02 — At most one directory argument.** ✅
The tool SHALL accept zero or one directory argument, plus any recognised flags. Two or more
directories SHALL fail the run with the message that a single directory parameter is required.

**CLI-03 — Zero arguments → current directory.** ✅
With no arguments, the starting directory SHALL be the process's current working directory.
This is the primary mode when invoked from an MSBuild target, whose working directory is the
project directory.

**CLI-04 — The directory argument is the folder root.** ✅ *(replaced by FLD-01; G-06 resolved)*
The single directory argument SHALL be the folder root, resolved to a full path. It is used, not
merely validated. Implemented in [`RunOptions.Parse`](../src/EasySemVer/Settings/RunOptions.cs).

**CLI-05 — No solution-root discovery.** ✅ *(retired by FLD-02; G-09 resolved)*
The tool SHALL NOT search ancestors for a `.sln`/`.slnx` and SHALL NOT require one to exist. A
folder containing no solution file is a valid, ordinary input. Both walk-up helpers are deleted.

**CLI-06 — Exit codes.** ✅
The process SHALL exit `0` on success. Any unhandled error SHALL print the full exception to
stdout and exit `1`. (See [10-logging-and-error-handling.md](10-logging-and-error-handling.md)
for why exit 1 is a deliberate build-failure signal.)

**CLI-07 — `--dry-run` previews without writing.** ✅ *(added per §20 O-04)*
`--dry-run` SHALL run discovery, extraction, classification and version resolution, log the
verdict and the version it would produce, and write nothing: no baseline, no version stamps. A
dry run is not a release, so it does not conflict with OVR-03. Without the flag, every successful
invocation increments by at least Patch and rewrites state.

**CLI-08 — a dry run explains its verdict.** ✅
`--dry-run` SHALL list every detected change, grouped by the unit it was found in, one line per
change carrying that change's impact and the symbol it concerns, and end with the change type
(with a per-impact tally) and the version transition. A verdict with no evidence behind it is not
reviewable, which is the whole point of the mode. Findings are ordered by unit and then by symbol
so that identical input produces identical output, for the same reason BAS-04 sorts a baseline.
A run without the flag keeps the quieter one-line-per-firing-rule summary (LOG-03).

**CLI-09 — `--json <path>` requires a path.** ✅
`--json` SHALL consume the following argument as its path and SHALL fail if none follows. The
path is not mistaken for the folder argument whichever order the two are given in, and an
unrecognised `--option` is still rejected by name.

## Machine-readable report

**REP-01 — `--json <path>` writes the run's verdict as a file.** ✅
`easysemver <folder> --json <path>` SHALL write a JSON document describing the run to
`<path>`. It SHALL NOT write JSON to stdout and SHALL NOT move the human log anywhere: LOG-01
keeps stdout, unconditionally. A workflow that wants the verdict reads the file; nothing has to
parse a log, and no existing stream contract changes.

**REP-02 — The document.** ✅
```json
{
  "formatVersion": 1,
  "dryRun": false,
  "changeType": "major",
  "oldVersion": { "version": "2.3.4", "major": 2, "minor": 3, "patch": 4 },
  "newVersion": { "version": "3.0.0", "major": 3, "minor": 0, "patch": 0 },
  "findings": [ … ]
}
```
`findings` is specified by REP-09.
- `changeType` is `"major" | "minor" | "patch"`, lower case. So are all enum-valued fields: this
  is a wire format, not a dump of a C# enum, and lower case removes a class of casing bugs in
  shell and YAML comparisons.
- Both version objects share one shape. `version` is the canonical string and the full truth;
  `major`/`minor`/`patch` are numbers and are the *first three* segments, which matters because a
  version may legitimately carry more (VER-07). The decomposition is there because consumers
  routinely want the parts — a Docker tag set of `3`, `3.0`, `3.0.0` from one run.
- `dryRun` is the only signal that a report describes a preview rather than something that
  happened, so it is always present.

**REP-03 — `formatVersion` has its own lifecycle.** ✅
It starts at `1` and is versioned independently of the baseline's `formatVersion` (currently 3).
The two documents have different audiences and different failure modes — a JSON consumer breaking
is not a versioning run breaking — so sharing a number would couple them for no reason.

**REP-04 — Fields may be added; nothing may be removed or retyped.** ✅
Adding a field is backwards-compatible and SHALL NOT bump `formatVersion`. Removing one, renaming
one, or changing its type SHALL bump it. This is what makes REP-05's minimalism safe rather than
short-sighted: the document can grow into whatever a real consumer turns out to need.

**REP-05 — Only what a consumer demonstrably needs.** ℹ️ *(findings since added by REP-09)*
Discovered units, individual findings, and the list of files written were all specified, weighed
and deliberately **left out**. They belong in the log, which already renders them (CLI-08), and
no consumer for them existed at the time of writing. Per REP-04 any of them can be added later
without breaking a reader, whereas shipping them now would have committed the contract to a
shape nobody had asked for. Do not re-add them by reflex; add them when something needs them.

ℹ️ Findings met that condition on 2026-08-03 and were added as REP-09; the reasoning above stands
for the other two, which are still out. This is the process working as intended rather than a
reversal — the requirement asked for a demonstrated need, and one arrived. **Discovered units and
the written-file list remain deliberately absent, on the original grounds.**

**REP-06 — The verdict is stated, never inferred.** ℹ️
`changeType` is not redundant with the two versions, and a consumer SHALL NOT derive it by
comparing them. VER-05's overflow rollover makes that inference wrong: `1.0.2147483647 + Patch`
becomes `1.1.0`, which reads as a Minor bump, and `1.2147483647.123 + Minor` becomes `2.0.0`,
which reads as Major. The field is the only place the classification itself appears.

**REP-07 — Determinism, and no machine-specific content.** ✅
The document SHALL contain no absolute paths, timestamps, durations, machine names or tool
versions. Two runs over unchanged source on two machines SHALL produce byte-identical JSON, for
the same reason a baseline must (BAS-04) — so that diffing two reports shows only real change.
ℹ️ The folder root is deliberately absent: the caller passed it, so restating it would buy
nothing and would be the one field that broke this property.

**REP-09 — `findings`: the evidence behind the verdict.** ✅
The document SHALL carry every finding the run made, in the order `ChangeReport` already sorts
them — unit, then symbol, then rule — so REP-07 holds for the array as it does for the rest:

```json
"findings": [
  {
    "ruleId": "R02",
    "impact": "major",
    "language": "csharp",
    "unitId": "Widgets",
    "symbol": "Widgets.Widget.Spin()",
    "description": "was removed"
  }
]
```
- `ruleId` is the rule's identifier from the tables in
  [07](07-change-classification.md) and [12 §13](12-multi-language-swift-and-folder-model.md).
  It is what makes a verdict auditable in a machine's hands rather than only a reader's: a
  consumer can point at the rule that cost it a Major without matching on prose.
- The rule's **class name is not published.** It is an implementation detail, and a consumer keyed
  to it would break on a rename that changed no behavior. `ruleId` is the stable identity, which
  is the reason [`TestRuleIds`](../src/Test/TestRuleIds.cs) pins every id to its spec row.
- `impact` and `language` are lower case, like every other enum-valued field (REP-02).
- `description` completes "*symbol* …" — "was removed". It is prose for a human reading the
  document, and nothing should match on it; `ruleId` is there for that.
- The array SHALL be present and empty rather than absent when a run found nothing. An absent
  array would make "no changes" and "an older writer" indistinguishable.
ℹ️ A run can legitimately report an empty array and a `changeType` above `patch`: CLS-04's
fail-safe raises the floor when there is no comparable baseline, and there is no symbol to name.
This is REP-06 again — the verdict is stated, never inferred, including from this array.
ℹ️ Adding this field does not bump `formatVersion` (REP-04), and it introduces no absolute path,
timestamp or machine name: `unitId` is machine-stable by ML-03, so REP-07 is unaffected.

**REP-08 — A failed run writes no report.** ✅
If the run fails, `<path>` SHALL NOT be written or modified. This matches BAS-06 — a failed run
writes nothing at all — and the exit code (CLI-06) remains the failure channel. A half-truthful
report is worse than no report.

## Invocation surfaces

**INV-01 — The CLI is the product.** ✅ *(replaces MSB-01/MSB-02)*
EasySemVer SHALL be consumed as a command that takes a folder. It SHALL NOT require, or ship,
any MSBuild integration: no targets file, no `Task` class, no `PackageReference` that a
consuming project imports. That model was inherited from the AutoVersion design, never worked
(G-02/G-03/G-04), and is withdrawn rather than repaired — a tool whose unit of work is *a
folder* does not belong bolted to one arbitrary project inside it.

Consumers wire it in with whatever their build already uses. An MSBuild `Exec` remains perfectly
possible and is the consumer's own business:

```xml
<Target Name="EasySemVer" BeforeTargets="Build" Condition="'$(Configuration)'=='Release'">
    <Exec Command="easysemver &quot;$(MSBuildProjectDirectory)&quot;" />
</Target>
```

**INV-02 — Two distribution channels.** ✅
See [03-packaging-and-distribution.md](03-packaging-and-distribution.md): a `dotnet tool` for
anyone who already has a .NET SDK, and self-contained per-platform binaries for anyone who does
not — a Swift-only repository, most obviously.

**INV-03 — Release-only gating is the consumer's job.** ℹ️ *(was MSB-03)*
Because every run is a release (OVR-03), consumers SHOULD invoke EasySemVer only for builds that
are releases. The tool performs no configuration check of its own and has no opinion about
branches; `--dry-run` (CLI-07) exists for the cases where you want the verdict without the
consequences.

**INV-04 — Timing is unconstrained.** ℹ️ *(was MSB-04)*
Signatures are extracted from **source**, never from compiled assemblies, so it does not matter
whether EasySemVer runs before or after a build. The increment is relative to the persisted
baseline, not to build artifacts.

**INV-05 — GitHub Action.** ✅ *(was planned; specified as ACT-01…ACT-09 below)*
The third surface is a GitHub Action wrapping the CLI, at [`action.yml`](../action.yml). It is
built on REP-01…REP-08: the machine-readable verdict is what it turns into step outputs, so no
workflow ever has to scrape a log.

## The GitHub Action

**ACT-01 — A composite action at the repository root.** ✅
`action.yml` SHALL sit at the repository root, so that `uses: winterborn-llc/easysemver@<ref>`
resolves it with no subdirectory. It SHALL be a **composite** action. The deliverable is already a
self-contained native binary (PKG-02), so a Docker action would add an image pull and a JS action
would add a second language plus a committed `dist/` bundle to keep in sync with source — neither
buys anything here. Composite also runs on every runner OS, which a Docker action does not.

**ACT-02 — It runs the published binary, at a pinned version.** ✅
The Action SHALL obtain the tool by downloading the PKG-02 release archive for the runner's
platform, for the tag named by the `version` input. That input SHALL default to a specific tag and
SHALL NOT track the latest release: a workflow that silently changed behaviour when this repository
cut a release would make every consumer's build non-reproducible.
ℹ️ This is the reason the Action could not exist before a release did — the dependency runs from
the Action to the release, and it cannot be satisfied retroactively.

**ACT-03 — Platform coverage is exactly PKG-02's.** ✅
The Action SHALL resolve `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64` and `win-x64` from the
runner's reported OS and architecture, and SHALL fail by name on any other combination. Guessing a
default would surface as an exec-format error several steps later, in a place that does not name
the cause.

**ACT-04 — Inputs.** ✅
`folder` (default `.`), `dry-run` (default `false`), `version`, and `token`. `dry-run` SHALL accept
only `true` or `false` and SHALL fail on anything else rather than treating it as false: a typo'd
`dry-run: yes` that silently stamped versions and rewrote the baseline would be a bad way to find
out about the typo.

**ACT-05 — Outputs come from the report, never from the log.** ✅
The Action SHALL publish `version`, `old-version`, `change-type` and `dry-run`, read from the
`--json` document (REP-01). It also publishes `major`, `minor` and `patch` — the decomposition
REP-02 exists to serve — and `report`, the path to the document itself, so that a field added later
under REP-04 is readable without the Action having to grow an output for it first. `change-type`
SHALL be read, never derived by comparing the two versions (REP-06). `dry-run` SHALL be read from
the report rather than echoed from the input, so that it describes what happened.

**ACT-06 — The Action reports the verdict and does not act on it.** ✅
It SHALL NOT commit the bumped versions or the baseline, create a tag, or publish anything. Those
are outward-facing acts belonging to the calling workflow, for the same reason the tool reads git
tags but never writes one (VER-*, README). What the Action owes its caller is a trustworthy
verdict; what the caller does with it is a policy the caller owns.

**ACT-07 — A failed run fails the step, with nothing published.** ✅
Exit 1 (CLI-06) SHALL fail the step and stop the Action. No outputs are published, and REP-08
guarantees there is no report to read in that case, so no consumer ever sees a half-truth. The
verdict is deliberately **not** encoded in the exit code: a non-zero exit fails a workflow step, so
signalling "this change is Major" that way would be indistinguishable from the tool falling over.

**ACT-08 — Inputs reach the shell through the environment.** ✅
Input values SHALL NOT be interpolated into a `run:` block with `${{ }}`, which substitutes before
bash parses the script and would run a folder named `; rm -rf /` rather than name it. They are
passed as environment variables and quoted at the point of use.

**ACT-09 — What the Action's tests do and do not cover.** ℹ️
A GitHub Action cannot be run locally, so
[`ActionRegression`](../src/IntegrationTest/ActionRegression.cs) tests the parts that are testable:
it parses `action.yml`, asserts the wiring (ACT-01, ACT-05), checks the platform table against the
release job's own matrix so the two cannot drift (ACT-03), and executes the `run:` scripts
extracted from the file itself against a real tool with only the release download stubbed
(ACT-04, ACT-07). What that leaves untested is everything owned by the runner: the real download,
`gh`'s authentication, `$GITHUB_OUTPUT` actually becoming step outputs, and the Windows and
cross-architecture paths — the harness runs one platform, the host's. Those are covered only by
running the Action for real.
ℹ️ The harness earns its keep: it caught an empty-bash-array expansion that tripped `set -u` under
bash 3.2, which is what a macOS runner's `/bin/bash` still is. An Ubuntu-only smoke test would
have passed straight over it.
