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
  "newVersion": { "version": "3.0.0", "major": 3, "minor": 0, "patch": 0 }
}
```
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

**REP-05 — Only what a consumer demonstrably needs.** ℹ️
Discovered units, individual findings, and the list of files written were all specified, weighed
and deliberately **left out**. They belong in the log, which already renders them (CLI-08), and
no consumer for them existed at the time of writing. Per REP-04 any of them can be added later
without breaking a reader, whereas shipping them now would have committed the contract to a
shape nobody had asked for. Do not re-add them by reflex; add them when something needs them.

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

**INV-05 — GitHub Action.** ⚠️ *planned, not implemented*
The intended third surface is a GitHub Action wrapping the CLI. Nothing of it exists yet, but the
capability it depends on now does: REP-01…REP-08 give it a machine-readable verdict to turn into
step outputs, so no workflow ever has to scrape a log.
