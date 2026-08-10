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
  "formatVersion": 2,
  "dryRun": false,
  "changeType": "major",
  "oldVersion": { "version": "2.3.4", "major": 2, "minor": 3, "patch": 4 },
  "newVersion": { "version": "3.0.0", "major": 3, "minor": 0, "patch": 0 },
  "findings": [
    {
      "rule": "MethodsContinueToExist",
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

Each finding carries the `language` and `rule` of the rule that fired, so a verdict can be audited
by a script and not just read: that pair is the key in the rule tables in
[specs/07](specs/07-change-classification.md) and
[specs/12](specs/12-multi-language-swift-and-folder-model.md), and a rule name is unique within a
language rather than globally. Match on `language` and `rule`, never on `description` — the prose
is for people. Take the verdict from `changeType` rather than from the
array: a run with no comparable baseline reports no findings and still classifies above Patch.

## Installing

**As a .NET tool**, if you already have a .NET SDK:

```bash
dotnet tool install -g Winterborn.Tools.EasySemVer
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
<EasySemVer formatVersion="4">
   <Unit language="csharp" unitId="Widgets" unitKind="csproj" path="src/Widgets/Widgets.csproj">
      <CsharpProject name="Widgets"> … </CsharpProject>
   </Unit>
   <Unit language="swift" unitId="Sources/Gadgets:Gadgets" unitKind="swiftpm-target" path="Sources/Gadgets">
      <SwiftModule name="Gadgets"> … </SwiftModule>
   </Unit>
</EasySemVer>
```

Two runs over unchanged source on two machines produce byte-identical files: there are no
absolute paths, timestamps, machine names or toolchain versions in it.

A **missing** baseline is a first run: it is treated as "no history", every unit reads as added,
and the run classifies Minor. A baseline that is **present but unreadable** — damaged, or written
in an older `formatVersion` — fails the run instead, exiting 1 having written nothing. It is
history the run was supposed to classify against, and releasing a version with nothing behind it
is worse than stopping. Delete the file to start from an empty baseline; that costs one release
classified as if every unit were new, which is the point at which you get to decide that is
acceptable rather than finding out afterwards.

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

Reading the verdict is three lines. If what you want is the whole file to paste into
`.github/workflows/`, skip to [a complete workflow](#a-complete-workflow) below.

```yaml
- uses: actions/checkout@v4

- id: version
  uses: winterborn-llc/easysemver@v18

- run: echo "${{ steps.version.outputs.change-type }} → ${{ steps.version.outputs.version }}"
```

It downloads the standalone binary for the runner's platform, so nothing needs .NET installed.

The `@v<major>` ref above is a moving tag, kept on the newest release of that major the way
`actions/checkout@v5` is — you get fixes and features without editing your workflow, and never a
major version you did not ask for. Name an exact release tag instead if you would rather nothing
changed until you changed it. Either way you run the binary published alongside the manifest you
pointed at: the release stamps the two together, so a moving tag never means a new wrapper around
an old tool.

| Input | Default | |
|-------|---------|-|
| `folder` | `.` | The folder root, relative to the workspace |
| `dry-run` | `false` | `true` classifies and reports without writing anything |
| `commit` | `false` | `true` commits the bump and pushes it |
| `tag` | `false` | `true` also creates and pushes `v<version>`. Needs `commit: true` |
| `report` | — | Act on an earlier step's `report` output instead of versioning again |
| `version` | the release this manifest shipped with | Which EasySemVer release to run |
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

### Without the Action

The same outputs are available from the CLI, because the tool publishes them itself. Under GitHub
Actions it writes `$GITHUB_OUTPUT` and a job summary without being asked, so the step is the bare
command:

```yaml
- id: version
  run: easysemver .

- run: echo "${{ steps.version.outputs.change-type }} → ${{ steps.version.outputs.version }}"
```

`--github` forces that on where the detection cannot see the environment; `--no-github` forces it
off, for a workflow that wants `--json` only and would rather not have a job summary appended on
its behalf. Either way there is no `jq` to write: the mapping from report to step outputs is the
same in every workflow, so it lives in the tool rather than in each of them.

### Releasing

Versioning, committing and tagging is one step. Copy it into any repository unedited — it names no
path, no project, no branch and no language:

```yaml
- name: Version, commit and tag
  id: version
  uses: winterborn-llc/easysemver@v18
  with:
    commit: true
    tag: true
```

That stages exactly the files the run wrote, commits them as `EasySemVer: 2.4.0`, tags `v2.4.0`,
and pushes the branch and the tag atomically.

**If you build anything in between, split it.** The version has to be stamped before the build, or
your artifacts carry the old one; the commit must not be pushed until the tests pass, or a failing
test leaves a release commit and a tag with no release behind them. Hand the second step the first
one's report and it commits that verdict instead of computing a new one:

```yaml
- name: Compute and apply the version
  id: version
  uses: winterborn-llc/easysemver@v18

# ...restore, build, test...

- name: Commit and tag the release
  uses: winterborn-llc/easysemver@v18
  with:
    commit: true
    tag: true
    report: ${{ steps.version.outputs.report }}
```

With `report` set the tool is neither downloaded nor run, so the version is computed exactly once
however many times the action appears in your job. This is the pair EasySemVer releases *itself*
with, not an illustration of one: it is lifted from
[`.github/workflows/dotnet.yml`](.github/workflows/dotnet.yml), and a test fails if the two ever
differ.

Three things either form needs from the job around it:

| | Why |
|-|-----|
| `permissions: contents: write` | To push the commit and the tag. Naming any permission replaces the defaults wholesale, so name every one you need. |
| `fetch-depth: 0` on the checkout | EasySemVer seeds from the highest version it can find, and git tags are one of the places it looks. A shallow clone hides them and seeds too low. |
| `concurrency: { group: release, cancel-in-progress: false }` | Two pushes landing together would seed from the same version, compute the same next one, and race. |

Put it **before** you publish anything. A package cannot be unpublished, so if someone pushed to
your branch mid-run you want the push rejected and the run dead having released nothing — which is
what the ordering and the atomic push buy you.

Both inputs default to `false`. Pushing a version bump to your repository is your decision, and
those inputs are where you make it; what the Action will not do either way is publish, because
there is no single way to publish and so nothing for it to get right.

A run that fails exits 1 and fails the step, and publishes no outputs at all — the verdict is
never encoded in the exit code, because "this change is Major" would then be indistinguishable
from the tool falling over.

### A complete workflow

Everything above, assembled — the three job-level requirements included. Save it as
`.github/workflows/release.yml`, put your own build and tests where the comment is, and the
repository versions, commits and tags itself on every push to `main`. Nothing in it names a
path, a project or a language, so it goes in unedited:

```yaml
name: Release

on:
  push:
    branches: [ main ]

# One release at a time. Two runs landing together would seed from the same version, compute
# the same next one, and race to push it.
concurrency:
  group: release
  cancel-in-progress: false

jobs:
  release:
    runs-on: ubuntu-latest

    # Naming any permission replaces the defaults wholesale, so name every one you need.
    permissions:
      contents: write

    steps:
    # fetch-depth: 0 for the tags. They are one of the places the version is seeded from, and
    # a shallow clone hides them and seeds too low.
    - uses: actions/checkout@v5
      with:
        fetch-depth: 0

    - name: Compute and apply the version
      id: version
      uses: winterborn-llc/easysemver@v18

    # Your build, your tests, your packaging. The version is already stamped on disk, so
    # whatever you produce here carries it — and nothing has been committed yet, so a failure
    # anywhere in between releases nothing.

    - name: Commit and tag the release
      uses: winterborn-llc/easysemver@v18
      with:
        commit: true
        tag: true
        report: ${{ steps.version.outputs.report }}
```

Two things to change for your repository, if they apply. A folder with Swift or an `.xcodeproj`
in it needs `runs-on: macos-latest` and a toolchain the run can reach, per *Swift prerequisites*
above. And if you version a subdirectory rather than the checkout, add `with: { folder: src }`
to the first step — the second one takes its verdict from `report` and needs nothing further.

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
