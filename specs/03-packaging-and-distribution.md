# 03 — Packaging and Distribution

Sources: [`EasySemVer.csproj`](../src/EasySemVer/EasySemVer.csproj),
[`.github/workflows/dotnet.yml`](../.github/workflows/dotnet.yml).

EasySemVer is distributed as **a command**, two ways. It is not a library, and nothing imports it
into a consuming build — see [INV-01](02-invocation-and-distribution.md).

## Channels

**PKG-01 — A `dotnet tool` package.** ✅
The project SHALL pack as a .NET tool (`PackAsTool`) with command name **`easysemver`**, package
id `Winterborn.Library.EasySemVer`, multi-targeting `net8.0`/`net9.0`/`net10.0` so it runs on
whatever SDK a consumer already has:

```bash
dotnet tool install -g Winterborn.Library.EasySemVer
easysemver /path/to/folder
```

ℹ️ The package id still reads `…Library…`, which is now a misnomer — it is kept because renaming
it would orphan every existing install rather than upgrade it. Changing it is a one-line decision
whenever the cost of the rename is judged worth paying.

**PKG-02 — Self-contained binaries.** ✅
CI SHALL publish self-contained, framework-independent binaries for `linux-x64`, `linux-arm64`,
`osx-x64`, `osx-arm64` and `win-x64`, attached to the GitHub Release for a `v*` tag. This is the
channel that matters for a repository with no .NET in it at all — a Swift-only one, most
obviously — and for the planned GitHub Action (INV-05).
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
On every push to `main`, CI SHALL restore, build `Release`, run the unit suite, run the
integration suite, and push the tool package to nuget.org with `--skip-duplicate` (idempotent
republish). The build job runs on `macos-latest` because the Swift-traited tests need a Swift
toolchain and Xcode.
ℹ️ The integration step mutates the working copy by design (TST-05).

**CI-02 — Version source of truth for publishing.** ✅ (by design)
CI SHALL NOT compute versions itself; the version baked into the artifacts is whatever the
committed `.csproj` files carry — versioning happens on the developer's release build via the
tool, and CI merely publishes the committed result. `--skip-duplicate` makes a push of an
unchanged version a no-op instead of an error.

**CI-03 — Releases are tag-driven.** ✅
The binary matrix SHALL run only for a `v*` tag, not for every push to `main`, and SHALL attach
its archives to that tag's GitHub Release. Runtime packs are downloadable for every target, so a
single Linux runner cross-publishes all five.
