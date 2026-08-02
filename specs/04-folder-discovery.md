# 04 — Folder Discovery

How the tool locates the folder root, the packageable units inside it, and their source files.
Sources: [`RunOptions.cs`](../src/EasySemVer/Settings/RunOptions.cs),
[`FolderScanner.cs`](../src/EasySemVer/Evaluation/FolderScanner.cs),
[`CsharpLanguageProvider.cs`](../src/EasySemVer/Providers/CsharpLanguageProvider.cs),
[`SwiftLanguageProvider.cs`](../src/EasySemVer/Providers/SwiftLanguageProvider.cs),
[`MagicValues.cs`](../src/EasySemVer/Settings/MagicValues.cs).

> Replaces the former *04 — Solution Discovery*. DSC-01 and DSC-02 are **retired** by FLD-02;
> DSC-03 and DSC-06 are generalized below.

**DSC-01 — Retired.** ~~Solution root = nearest ancestor with a `.sln`.~~ There is no walk-up.
The directory handed to the CLI is the root (FLD-01), full stop. *Retired by FLD-02.*

**DSC-02 — Retired.** ~~Consistent solution-file recognition.~~ No code path looks for a
solution file at all. *Retired by FLD-02; closed gap G-09.*

**DSC-03 — Unit enumeration.** ✅ *(generalized by UNI-01…UNI-03)*
The unit set SHALL be every packageable unit found under the folder root, recursively:
- one per `*.csproj` (UNI-02),
- one per SwiftPM **target**, enumerated by `swift package dump-package` (SWD-01),
- one per Xcode **target**, enumerated by `xcodebuild -list -json` (SWD-02).

ℹ️ There is deliberately no include/exclude configuration: test projects, sample projects and
test targets all participate, both as signature sources and as version-sync targets.

**DSC-04 — Units must be readable.** ✅
Constructing the model for a project whose file does not exist SHALL throw
`FileNotFoundException`. For Swift, a target whose signature cannot be extracted SHALL fail the
run outright (SWE-05) rather than being skipped.

**DSC-05 — Unit identity.** ✅ *(generalized by ML-03)*
A unit is identified by `(Language, UnitId)`, where `UnitId` is stable across machines and
checkouts and contains no absolute path:
- C#: the `.csproj` filename without extension (e.g. `EasySemVer`),
- SwiftPM: `<package-directory-relative-path>:<target-name>`,
- Xcode: `<xcodeproj-relative-path>:<target-name>`.

Renaming a unit therefore reads as remove + add (Major + Minor, so Major). For C#, directory
location remains irrelevant to identity.

**DSC-06 — Source-file enumeration.** ✅ *(G-10 resolved)*
A C# project's source set SHALL be every `*.cs` file under the `.csproj`'s directory,
recursively, **subject to the same exclusions as discovery** (DSC-08). Generated files under
`obj/` are therefore no longer parsed.
ℹ️ Swift source is never enumerated directly: signatures come from the toolchain's symbol graph
(SWE-01), so what is compiled is what is measured.
ℹ️ Files outside the project directory (linked files) and conditional compilation are still not
considered; the source on disk is the truth.

**DSC-07 — Enumerate once.** ✅ *(G-13 resolved)*
Discovery SHALL run once per invocation and its result SHALL be passed to every downstream
stage. Classification receives the two signatures and nothing else — it can no longer
re-discover anything, and the working-directory fallback that made it able to is gone.

**DSC-08 — Excluded directories.** ✅ *(FLD-04; G-10 resolved)*
Discovery and source enumeration SHALL skip, at any depth:
- any directory whose name begins with `.` — covers `.git`, `.build`, `.packages`, `.swiftpm`,
- `bin`, `obj`, `build`, `DerivedData`, `Pods`, `Carthage`, `node_modules`, `Packages`.

The list lives in `MagicValues.ExcludedDirectoryNames`. This is not optional politeness: with a
folder root instead of a solution root, an unexcluded `.packages/` or `.build/checkouts/` would
pull **dependency source** into the signature and make every dependency update a Major change.
Resolved SwiftPM dependencies and system/binary/plugin/macro targets are excluded for the same
reason (SWD-04).

**DSC-09 — Deterministic order.** ✅
Discovery results SHALL be sorted, so that nothing downstream depends on the file system's
enumeration order (supports BAS-04).

**DSC-10 — An empty folder is not an error.** ✅ *(FLD-05)*
A root containing no recognizable units SHALL log that fact, classify as Patch, write an empty
baseline, and exit 0.
