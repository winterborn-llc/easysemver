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
`AssemblyVersion`, `FileVersion`) so the tool versions itself using its own mechanism. The `Test` and `IntegrationTest` projects carry seed values for the same
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

**PKG-07 — Third-party dependencies.** ✅ *(G-19 resolved)*
The tool depends on Roslyn (`Microsoft.CodeAnalysis.Common` / `.CSharp` / `.Workspaces.MSBuild`,
v5.0.0) and nothing else. The `Newtonsoft.Json` attributes that compiled only via a transitive
dependency are gone, along with the type that carried them. Swift and Xcode support add no
package references: they shell out through
[`IRunProcess`](../src/EasySemVer/Interfaces/IRunProcess.cs) and parse with
`System.Text.Json`.

## Continuous integration / publishing

**CI-01 — Pipeline intent.** ✅ *(G-05 resolved)*
On every push to `main`, CI SHALL: restore, build `Release`, run the unit suite, run the
integration suite, and push the produced `.nupkg` to nuget.org with `--skip-duplicate`
(idempotent republish). The [workflow](../.github/workflows/dotnet.yml) runs on `macos-latest`
because the Swift-traited tests need a Swift toolchain and Xcode; everything else runs anywhere.
ℹ️ The integration step mutates the working copy by design (TST-05).

**CI-02 — Version source of truth for publishing.** ✅ (by design)
CI SHALL NOT compute versions itself; the version baked into the `.nupkg` is whatever the
committed `.csproj` files carry — i.e. versioning happens on the developer's release build
via the tool, and CI merely publishes the committed result. `--skip-duplicate` makes pushes
of an unchanged version a no-op instead of an error.
