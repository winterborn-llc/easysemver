# 03 — Packaging and Distribution

Sources: [`EasySemVer.csproj`](../src/EasySemVer/EasySemVer.csproj),
[`.github/workflows/dotnet.yml`](../.github/workflows/dotnet.yml).

## NuGet package

**PKG-01 — Package identity.** ✅
The package SHALL be `Winterborn.Library.EasySemVer`, produced automatically on every build
(`GeneratePackageOnBuild=true`), with project/repository URL
`https://github.com/winterborn-llc/easysemver`, MIT-licensed
([LICENSE](../LICENSE)), title "EasySemVer by Winterborn".

**PKG-02 — Package contents.** ✅
The package SHALL include:
- `README.md` at the package root (`PackageReadmeFile`),
- the logo at `Resources/logo.png` (`PackageIcon`, also embedded in the assembly),
- the MSBuild targets file under `buildTransitive/`.

**PKG-03 — Self-versioned (dogfooding).** ✅
`EasySemVer.csproj` SHALL carry all three version properties (`PackageVersion`,
`AssemblyVersion`, `FileVersion` — currently `12.0.15`) so the tool versions itself using its
own mechanism. The `Test` and `IntegrationTest` projects carry seed values for the same
reason (and are deliberately included in the solution-wide sync).

**PKG-04 — Tool executable in `tools/`.** ❌
The package SHALL contain the runnable tool at `tools/EasySemVer.exe`, because the shipped
targets file resolves it there (MSB-01). Nothing in the `.csproj` currently packs a `tools/`
folder, so the packaged integration cannot execute. (Gap **G-03**.)

**PKG-05 — Targets filename must match package ID.** ❌
For NuGet to auto-import a `buildTransitive` targets file, it must be named
`<PackageId>.targets`, i.e. `Winterborn.Library.EasySemVer.targets`. The file is currently
named `Winterborn.EasySemVer.targets`, producing warning `NU5129` at pack time and **no
auto-import** in consuming projects. (Gap **G-04**.)

**PKG-06 — Dynamic loading support.** ℹ️
`EnableDynamicLoading=true` and `CustomTasksAssembly` remain from the UsingTask-based design
(MSB-02). They are harmless but only meaningful if an MSBuild task class returns.

**PKG-07 — Third-party dependencies.** ⚠️
The tool depends on Roslyn (`Microsoft.CodeAnalysis.Common` / `.CSharp` /
`.Workspaces.MSBuild`, v5.0.0). *Deviation:* [`Solution.cs`](../src/EasySemVer/DataObject/Solution.cs)
uses `Newtonsoft.Json` attributes with **no direct package reference** — it compiles only via
a transitive dependency of `Workspaces.MSBuild`. Either the attributes should be removed
(XML, not JSON, is the persistence format — see PER-05) or the dependency made explicit.
(Gap **G-19**.)

## Continuous integration / publishing

**CI-01 — Pipeline intent.** ⚠️
On every push to `main`, CI SHALL: restore, build `Release`, run all tests, and push the
produced `.nupkg` to nuget.org with `--skip-duplicate` (idempotent republish).
*Deviations in the current [workflow](../.github/workflows/dotnet.yml)* (gap **G-05**):
- `env.project` is still `AutoVersion`; the push step globs
  `./src/AutoVersion/bin/Release/*.nupkg`, a path that no longer exists after the rename.
- The workflow installs only .NET SDK `9.0.x`, but the projects target `net10.0`
  (`LangVersion` 14), so the build step cannot succeed as written.
- The test step runs the whole solution; the integration test currently aborts (**G-01**),
  which would fail CI even after the SDK/path fixes.

**CI-02 — Version source of truth for publishing.** ✅ (by design)
CI SHALL NOT compute versions itself; the version baked into the `.nupkg` is whatever the
committed `.csproj` files carry — i.e. versioning happens on the developer's release build
via the tool, and CI merely publishes the committed result. `--skip-duplicate` makes pushes
of an unchanged version a no-op instead of an error.
