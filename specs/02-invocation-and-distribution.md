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
The intended third surface is a GitHub Action wrapping the CLI. Nothing of it exists yet, and it
implies at least one capability the tool does not have: a machine-readable verdict, so a workflow
can consume the computed version and change type as step outputs rather than scraping the log.
