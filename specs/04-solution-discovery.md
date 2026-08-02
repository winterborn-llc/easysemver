# 04 — Solution Discovery

How the tool locates the solution, its projects, and their source files.
Sources: [`Program.cs`](../src/EasySemVer/Program.cs),
[`CsProjFile.cs`](../src/EasySemVer/CodeReader/CsProjFile.cs),
[`ExtendString.GetSolutionDirectory`](../src/EasySemVer/Extensions/ExtendString.cs),
[`SolutionBuilder.cs`](../src/EasySemVer/CodeReader/SolutionBuilder.cs).

**DSC-01 — Solution root = nearest ancestor with a `.sln`.** ✅
Starting from the starting directory (CLI-03/04), the tool SHALL walk parent-ward and choose
the first directory containing a `*.sln` file at its top level. Failure to find one is fatal
(CLI-05).

**DSC-02 — Consistent solution-file recognition.** ⚠️
All code paths that identify "the solution directory" SHOULD recognize the same set of
solution file types. Currently two implementations disagree: `Program.GetSolutionDirectory`
matches only `.sln`, while the string extension `GetSolutionDirectory` (used by
`CsProjFile` to stamp each project's `SolutionDirectory`, and by the classification fallback)
matches `.sln` **and** `.slnx`. A solution using only the newer `.slnx` format would fail at
startup despite the helper supporting it. (Gap **G-09**.)

**DSC-03 — Project enumeration.** ✅
The project set SHALL be every `*.csproj` found under the solution root, recursively.
ℹ️ There is deliberately no include/exclude configuration: test projects, sample projects —
everything found participates, both as signature sources and as version-sync targets.
ℹ️ There is also no artifact filtering: a `.csproj` sitting under `bin/`, `obj/`, or a
package cache inside the solution root would be picked up. (In practice SDK builds do not put
`.csproj` files there, so this has not caused an observed issue.)

**DSC-04 — Projects must be readable.** ✅
Constructing the model for a project whose file does not exist SHALL throw
`FileNotFoundException`; the file's XML is read fully at construction time, and its version is
extracted immediately (see [VER-06](08-version-model.md)).

**DSC-05 — Project identity.** ✅
In the signature, a project's identity is its **csproj filename without extension**
(e.g. `EasySemVer`). Renaming a `.csproj` file therefore reads as project-removed +
project-added (Major, per [R07/R14](07-change-classification.md)). Directory location and
solution-file membership are irrelevant to identity.

**DSC-06 — Source-file enumeration.** ✅
A project's source set SHALL be every `*.cs` file under the `.csproj`'s directory,
recursively. ℹ️ No exclusions: generated files under `obj/` (e.g. `*.AssemblyInfo.cs`,
`*.GlobalUsings.g.cs`) are parsed too. They contain no public namespace-level classes, so
they do not alter signatures today, but the lack of a filter is noted as gap **G-10**.
ℹ️ Files outside the project directory (linked files) and conditional compilation are not
considered; the source on disk is the truth.

**DSC-07 — Enumerate once.** ⚠️
Discovery SHOULD run once per invocation and be reused. The current implementation enumerates
the full project set **four times** per run (in `Program.Execute`, in the
`SignaturesToCompare` constructor, in `GetSolutionProjectFilePaths`, and again inside
`CompareSignatures`, which rebuilds a `SignaturesToCompare` with an empty path and thereby
falls back to **current-working-directory-based** discovery). Functionally redundant and a
hidden cwd dependency; see gap **G-13**.
