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
On every push to `main`, CI SHALL restore, build `Release`, run the unit suite and run the
integration suite; and then, only if all of that passed, version the repository, commit and tag
the bump, and push the tool package to nuget.org with `--skip-duplicate` (idempotent republish).
ℹ️ Build-and-test is a [reusable workflow](../.github/workflows/build-and-test.yml) on
`ubuntu-latest`, as is the release job that consumes it. It ran on `macos-latest` for as long as
the Swift suites needed a Swift toolchain and the Xcode one needed `xcodebuild`; neither does now
(SIG-20), so nothing in this pipeline needs a Mac. The two jobs stay separate anyway: the `needs:`
edge carries the guarantee that step order used to, that nothing is committed, tagged or published
until the suite has passed on that commit.

**CI-01a — Every other branch runs the same check.** ✅
Every push to a branch other than `main` SHALL run the same build-and-test workflow, via
[ci.yml](../.github/workflows/ci.yml), so a branch cannot go green against a weaker suite than the
one guarding `main`. `main` is excluded from that trigger because CI-01 already covers it, and
`pull_request` is not a trigger because for a branch in this repository it would only double the
run. A pull request from a fork is consequently not built.
ℹ️ Branch runs cancel in progress; the release pipeline does not. A superseded branch check
describes a commit you have already replaced, whereas a half-finished release does not.
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

**CI-05 — `v<major>` follows the newest release.** ✅ *(added 2026-08-09)*
Once CI-04 has published a release, CI SHALL force-move the lightweight tag `v<major>` onto the
commit that release was cut from, so that `uses: winterborn-llc/easysemver@v17` resolves to the
current `17.x`. GitHub Actions resolves a `uses:` ref literally and performs no version matching of
its own, so a moving tag is the only mechanism there is; it is the convention `actions/checkout@v5`
and every other well-known action follows.

Two things move with it, stamped by the same run rather than remembered:

- **ACT-02's `version` default**, rewritten to the release being cut, so the manifest inside a tag
  pins the binary released beside it. Without this the moving tag is hollow — it would hand out a
  fresh wrapper around whichever binary was last hand-pinned, and the gap would widen every
  release.
- **The README's `uses:` refs**, rewritten to `v<major>`, so a major bump cannot leave the
  documentation pointing at a major this repository has moved off.

Both are staged before the ACT-11 commit step and ride into the release commit on it, which works
because that step commits the index rather than a pathspec. That is not part of ACT-11's contract,
so `ActionRegression` asserts it directly rather than trusting it.

The move SHALL be the **last** thing the pipeline does. This ref is what `@v<major>` resolves to for
every consumer, so it must never name a draft, or a release still missing four of its five
archives. A run that dies half-built therefore leaves the tag where it was, and consumers on the
last release that completed — the correct outcome, reached by doing nothing.

ℹ️ It is deliberately not folded into ACT-11's `tag: true`. That tag is half of CI-03's atomic
push, where a force-moved ref has no business being, and it is pushed long before any binary exists.
ℹ️ A bare `v17` is invisible to the seeding in
[`GitTagVersionSource`](../src/EasySemVer/CodeReader/Swift/GitTagVersionSource.cs), which matches
`v?MAJOR.MINOR.PATCH` only. Even if it were read it could not seed above the release it names.
