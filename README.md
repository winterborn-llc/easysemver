# EasySemVer from Winterborn

EasySemVer computes and applies Semantic Versioning for **a folder** by watching what happens to
the public API surface inside it. Point it at a directory; it finds every packageable unit in the
tree — C# `.csproj` projects, SwiftPM targets, Xcode targets — reads every version already
written anywhere in it, diffs each unit's API against the previous run, and moves one
folder-wide version by Major, Minor or Patch accordingly.

```bash
easysemver /path/to/your/folder
```

With no argument it uses the current working directory. Add `--dry-run` to classify and report
without writing anything, and `--json <path>` to drop the verdict somewhere a script can read it:

```json
{
  "formatVersion": 1,
  "dryRun": false,
  "changeType": "major",
  "oldVersion": { "version": "2.3.4", "major": 2, "minor": 3, "patch": 4 },
  "newVersion": { "version": "3.0.0", "major": 3, "minor": 0, "patch": 0 },
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
}
```

The report is always a file — never stdout — so the log stream stays exactly where it was. A run
that fails leaves no report behind.

Each finding carries the `ruleId` of the rule that fired, so a verdict can be audited by a script
and not just read: that is the id from the rule tables in [specs/07](specs/07-change-classification.md)
and [specs/12](specs/12-multi-language-swift-and-folder-model.md). Match on `ruleId`, never on
`description` — the prose is for people. Take the verdict from `changeType` rather than from the
array: a run with no comparable baseline reports no findings and still classifies above Patch.

## Installing

**As a .NET tool**, if you already have a .NET SDK:

```bash
dotnet tool install -g Winterborn.Library.EasySemVer
```

**As a standalone binary**, if you don't — a Swift-only repository, for instance. Grab the archive
for your platform from the [releases page](https://github.com/winterborn-llc/easysemver/releases);
it bundles its own runtime and needs nothing else installed. Archives are published for
`linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64` and `win-x64`.

## What it does, in order

1. Walks the folder once and discovers every packageable unit.
2. Extracts each unit's API signature **in its own language's terms** — a Swift protocol is a
   protocol, not "an interface"; a C# record is a record.
3. Reads the baseline (`EasySemVer.xml` at the folder root) written by the previous run.
4. Runs the classification rules and takes the highest impact anything reported.
5. Seeds from the highest version found in any version location in any unit, increments it, and
   writes that one version into every location that already exists.

Every successful run increments by at least a Patch: the tool assumes it runs for builds that are
releases. Gate it accordingly — see *Wiring it into a build* below.

## The baseline file

`EasySemVer.xml` lives at the folder root and **should be committed**. It is what makes the diff
meaningful across machines and across time, and it is why runs should be gated on release builds
rather than every developer build.

It is a flat array of packageable units, each carrying its own language's signature:

```xml
<EasySemVer formatVersion="3">
   <Unit language="Csharp" unitId="Widgets" unitKind="csproj" path="src/Widgets/Widgets.csproj">
      <CsharpProject name="Widgets"> … </CsharpProject>
   </Unit>
   <Unit language="Swift" unitId="Sources/Gadgets:Gadgets" unitKind="swiftpm-target" path="Sources/Gadgets">
      <SwiftModule name="Gadgets"> … </SwiftModule>
   </Unit>
</EasySemVer>
```

Two runs over unchanged source on two machines produce byte-identical files: there are no
absolute paths, timestamps, machine names or toolchain versions in it. A missing or unreadable
baseline is never fatal — it is treated as "no history", and the next successful run heals it.

## Version locations

The starting version is the **highest** value found across all of these. The new version is
written back to every one of them that already exists. EasySemVer never creates a version
property; seeding one is how a team opts in.

| Language | Location | Read | Write |
|----------|----------|:----:|:-----:|
| C# | `.csproj` `AssemblyVersion`, `PackageVersion`, `FileVersion` | ✅ | ✅ |
| Swift / Xcode | `MARKETING_VERSION` in `project.pbxproj` | ✅ | ✅ |
| Swift / Xcode | `CFBundleShortVersionString` in `Info.plist` | ✅ | ✅ |
| Swift | `s.version` in a `.podspec` | ✅ | ✅ |
| Swift | a `*Version.swift` constant, e.g. `static let version = "1.2.3"` | ✅ | ✅ |
| any | git tags matching `v?MAJOR.MINOR.PATCH` | ✅ | ❌ never |

`CURRENT_PROJECT_VERSION` and `CFBundleVersion` are build counters, not versions, and are left
alone entirely. Git tags are read as a seed but never written: creating a tag is an
outward-facing act EasySemVer will not take on your behalf.

## What counts as a change

Roughly seventy rules, each one a small class with its own test. The full tables are in
[specs/07](specs/07-change-classification.md) for C# and
[specs/12 §13](specs/12-multi-language-swift-and-folder-model.md) for Swift, but the shape is:

- **Major** — anything a caller could be relying on that no longer works: a type or member
  removed, a signature or return type changed, a parameter made required, a property setter
  withdrawn, an interface gaining an undefaulted requirement, an enum member renamed or
  revalued, a class sealed, a conformance dropped.
- **Minor** — anything additive: a new unit, type, member or overload; a constraint loosened; a
  setter gained; an interface requirement that ships with a default implementation.
- **Patch** — everything else, including implementation-only changes and a declaration merely
  being marked deprecated.

One rule differs deliberately between the two languages: **adding a case to a public Swift enum
is Major**, because a client switching exhaustively over it stops compiling. Adding a member to a
C# enum is Minor, because C# has no such requirement.

## Excluded directories

Discovery skips, at any depth: any directory whose name begins with `.` (so `.git`, `.build`,
`.swiftpm`, `.packages`), plus `bin`, `obj`, `build`, `DerivedData`, `Pods`, `Carthage`,
`node_modules` and `Packages`. This is not politeness — an unexcluded dependency checkout would
pull other people's source into your signature and make every dependency update a Major change.

## Swift prerequisites

Swift signatures come from the Swift toolchain's own symbol graph. There is no hand-rolled Swift
parser in this tool.

- A folder containing a `Package.swift` needs a **Swift toolchain** on the path.
- A folder containing an `.xcodeproj` needs **Xcode** configured, and pays a full `xcodebuild` on
  every versioned run.
- If Swift units are present and their signatures cannot be extracted — no toolchain, a failed
  build, a timeout — **the run fails with exit 1 and writes nothing**. There is no skip-and-warn:
  a partial baseline would silently under-report the next change.

A folder with no Swift in it needs none of this.

## Wiring it into a build

EasySemVer is a command; wire it in with whatever your build already uses. It deliberately ships
no MSBuild integration of its own — a tool whose unit of work is *a folder* does not belong
bolted to one arbitrary project inside it.

From a release script or CI step:

```bash
easysemver "$GITHUB_WORKSPACE"
```

Or from MSBuild, if that is where your release process lives:

```xml
<Target Name="EasySemVer" BeforeTargets="Build" Condition="'$(Configuration)' == 'Release'">
    <Exec Command="easysemver &quot;$(MSBuildProjectDirectory)&quot;" />
</Target>
```

Conditioning on `Release` keeps day-to-day builds from bumping versions and causing merge
conflicts in the baseline. Timing does not otherwise matter: signatures are read from **source**,
not from compiled assemblies, so the run works equally well before or after the build.

## As a GitHub Action

```yaml
- uses: actions/checkout@v4

- id: version
  uses: winterborn-llc/easysemver@v15.3.3

- run: echo "${{ steps.version.outputs.change-type }} → ${{ steps.version.outputs.version }}"
```

It downloads the standalone binary for the runner's platform, so nothing needs .NET installed.

| Input | Default | |
|-------|---------|-|
| `folder` | `.` | The folder root, relative to the workspace |
| `dry-run` | `false` | `true` classifies and reports without writing anything |
| `version` | pinned | Which EasySemVer release to run |
| `token` | `${{ github.token }}` | Only needs overriding if this repository is private to you |

| Output | Example |
|--------|---------|
| `version` | `2.4.0` |
| `old-version` | `2.3.4` |
| `change-type` | `major`, `minor` or `patch` |
| `dry-run` | `true` if the run wrote nothing |
| `major` / `minor` / `patch` | `2` / `4` / `0`, for a `2` / `2.4` / `2.4.0` tag set |
| `report` | Path to the raw JSON, for anything the outputs above don't cover |

Read `change-type` rather than comparing the two versions: across an overflow rollover a Patch
bump can look like a Minor one, so the comparison is wrong in a way that is very hard to spot.

The Action **does not commit, tag, or publish**. It hands you the verdict and stops — pushing a
version bump to someone's repository on their behalf is not a decision a version calculator gets
to make. If you want the bump committed, the calling workflow does it:

```yaml
- id: version
  uses: winterborn-llc/easysemver@v15.3.3

- run: |
    git config user.name  "github-actions[bot]"
    git config user.email "41898282+github-actions[bot]@users.noreply.github.com"
    git commit -am "Version ${{ steps.version.outputs.version }}"
    git push
```

A run that fails exits 1 and fails the step, and publishes no outputs at all — the verdict is
never encoded in the exit code, because "this change is Major" would then be indistinguishable
from the tool falling over.

## Seeding versions

EasySemVer only updates version properties that already exist. Add seed values to whichever ones
you want kept in sync; you do not need all of them.

```xml
<PropertyGroup>
    <AssemblyVersion>1.0.13</AssemblyVersion>
    <PackageVersion>1.0.13</PackageVersion>
    <FileVersion>1.0.13</FileVersion>
</PropertyGroup>
```

If they disagree, the highest wins and everything converges on it from the next run onwards.

## Exit codes

`0` on success. `1` on any failure, with the exception printed — deliberate, so that a versioning
failure on a release build is impossible to miss.

## Specifications

[`specs/`](specs/) holds the full requirement set: invocation, discovery, extraction,
persistence, classification, the version model, synchronization, logging and testing, plus a
[known-gaps list](specs/99-known-gaps.md).
