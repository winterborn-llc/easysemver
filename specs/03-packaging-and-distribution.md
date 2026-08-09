# 03 — Packaging and Distribution

Sources: [`EasySemVer.csproj`](../src/EasySemVer/EasySemVer.csproj),
[`.github/workflows/dotnet.yml`](../.github/workflows/dotnet.yml).

EasySemVer is distributed as **a command**, two ways. It is not a library, and nothing imports it
into a consuming build — see [INV-01](02-invocation-and-distribution.md).

## Channels

**PKG-01 — A `dotnet tool` package.** ✅
The project SHALL pack as a .NET tool (`PackAsTool`) with command name **`easysemver`**, package
id `Winterborn.Tools.EasySemVer`, multi-targeting `net8.0`/`net9.0`/`net10.0` so it runs on
whatever SDK a consumer already has:

```bash
dotnet tool install -g Winterborn.Tools.EasySemVer
easysemver /path/to/folder
```

ℹ️ The package id still reads `…Library…`, which is now a misnomer — it is kept because renaming
it would orphan every existing install rather than upgrade it. Changing it is a one-line decision
whenever the cost of the rename is judged worth paying.

**PKG-02 — Self-contained binaries.** ✅
CI SHALL publish self-contained, framework-independent binaries for `linux-x64`, `linux-arm64`,
`osx-x64`, `osx-arm64` and `win-x64`, attached to the GitHub Release for a `v*` tag. This is the
channel that matters for a repository with no .NET in it at all — a Swift-only one, most
obviously — and it is what the GitHub Action installs (ACT-02).
ℹ️ Roslyn plus a bundled runtime makes each archive roughly **36 MB** compressed, ~99 MB expanded.
ℹ️ The binaries are unsigned; macOS Gatekeeper will quarantine a downloaded one until it is
cleared. Signing is not currently done.

**PKG-03 — Self-versioned (dogfooding).** ✅
`EasySemVer.csproj` SHALL carry all three version properties (`PackageVersion`,
`AssemblyVersion`, `FileVersion`) so the tool versions itself using its own mechanism. The `Test`
and `IntegrationTest` projects carry seed values for the same reason.

**PKG-04 — Retired.** ~~Tool executable in `tools/`.~~ *Requirement withdrawn.*
This described the packaged-targets mechanism, which no longer exists (INV-01). `PackAsTool` now
lays the tool out under `tools/<tfm>/any/` as its own convention; nothing resolves a path into it
by hand. *Was gap **G-03**.*

**PKG-05 — Retired.** ~~Targets filename must match the package ID.~~ *Requirement withdrawn.*
There is no targets file to import, so `NU5129` no longer applies and the build is warning-free.
*Was gap **G-04**.*

**PKG-06 — Retired.** ~~Dynamic loading support.~~ *Requirement withdrawn.*
`EnableDynamicLoading` and `CustomTasksAssembly` were vestiges of the `UsingTask` design and are
deleted.

**PKG-07 — Third-party dependencies.** ✅
The tool depends on Roslyn — `Microsoft.CodeAnalysis.Common` and `.CSharp`, v5.0.0 — and nothing
else.
ℹ️ `Microsoft.CodeAnalysis.Workspaces.MSBuild` was dropped once the packaging work made its cost
visible: it had been unused since the `AdhocWorkspace` block was deleted (REN-05), but it was
still dragging a whole `BuildHost-net472` tree — Newtonsoft.Json, System.CommandLine and their
localized resources — into the package. Removing it took the tool package from 30 MB and 382
files to **17 MB and 113**.
ℹ️ Swift and Xcode support add no package references at all: they shell out through
[`IRunProcess`](../src/EasySemVer/Interfaces/IRunProcess.cs) and parse with `System.Text.Json`.

**PKG-08 — Package metadata.** ✅
The package SHALL carry `README.md` at its root, the logo at `Resources/logo.png`, the project and
repository URL `https://github.com/winterborn-llc/easysemver`, and the MIT licence
([LICENSE](../LICENSE)).

## Continuous integration / publishing

**CI-01 — Pipeline.** ✅
On every push to `main`, CI SHALL version the repository, restore, build `Release`, run the unit
suite, run the integration suite, commit and tag the bump, and push the tool package to nuget.org
with `--skip-duplicate` (idempotent republish). The build job runs on `macos-latest` because the
Swift-traited tests need a Swift toolchain and Xcode.
ℹ️ The integration step mutates the working copy by design (TST-05), which is why the commit stages
exactly what the run reported writing (REP-10) and never `git add -u`.
ℹ️ One release at a time (`concurrency: release`, `cancel-in-progress: false`). Two pushes landing
together would otherwise seed from the same version, compute the same next one, and race to push
it; a half-finished release is worse than a queued one.

**CI-02 — CI computes the version, using the previous release.** ✅ *(revised; was "CI SHALL NOT compute versions")*
CI SHALL run EasySemVer itself rather than publishing whatever the committed `.csproj` files
happen to carry, and SHALL do so with the **previously published** tool, not the binary the run
builds. Dogfooding with this run's own build would let a change that breaks versioning
mis-version the very release that introduced it; using the published one means such a change
breaks the *next* release instead, loudly and with the culprit already on `main`.

**CI-03 — Git before nuget.org.** ✅
The version commit and the tag SHALL be pushed **before** the package. A package cannot be
unpublished: if someone pushed to `main` mid-run, the branch push is rejected, the run fails, and
nothing has been published — which is the right way round. The two are pushed atomically, so a tag
can never survive a rejected branch and point at a commit that never landed.
ℹ️ A `GITHUB_TOKEN` push raises no events, so neither the commit nor the tag can re-trigger this
workflow. That is also why the tag must be consumed in the run that creates it, and why the binary
matrix is a downstream job here rather than a separate tag-triggered workflow.

**CI-04 — The release is drafted, filled, then published.** ✅
The GitHub Release SHALL be created as a **draft**, have all five PKG-02 archives attached, and be
published only once they are all on it. Publishing freezes a release — `Cannot upload assets to an
immutable release`, HTTP 422 — so any asset attached afterwards fails. A draft is also invisible to
everyone but the repository's maintainers, which is the correct state for a release that has no
binaries on it yet. Creation is idempotent, because re-running the workflow is exactly what you do
when one of the five publishes fails.
