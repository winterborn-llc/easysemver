# EasySemVer from Winterborn

EasySemVer computes and applies Semantic Versioning for **a folder** by watching what happens to
the public API surface inside it. Point it at a directory; it finds every packageable unit in the
tree — C# `.csproj` projects, SwiftPM targets, Xcode targets — reads every version already
written anywhere in it, diffs each unit's API against the previous run, and moves one
folder-wide version by Major, Minor or Patch accordingly.

Nobody decides the number. A removed method is Major because it is Major, on the run that removed
it, whether or not anyone remembered.

- **Major** — something a caller relied on stopped working: a type or member removed, a signature
  or return type changed, a parameter made required, an interface gaining an undefaulted
  requirement, a class sealed, a conformance dropped.
- **Minor** — something was added: a new unit, type, member or overload; a constraint loosened; a
  setter gained.
- **Patch** — everything else, including implementation-only changes.

Roughly eighty rules sit behind those three lines; [what counts as a change](#what-counts-as-a-change)
has the shape of them and the specs have the tables. Every successful run increments by at least a
Patch, because it assumes it is running for a release.

Two ways to use it. The **GitHub Action** is the whole of the setup for most repositories and is
what the next section covers. If your release does not live in GitHub Actions,
[install it and run the command](#running-it-yourself) instead — it is the same tool, and the
Action is a thin wrapper around the binary.

---

# The GitHub Action

Add it to a workflow and the repository versions, commits and tags itself. Nothing needs .NET
installed: the Action downloads a self-contained binary for the runner's platform.

## The whole workflow

Save this as `.github/workflows/release.yml`. The steps marked **YOUR …** are the ones you almost
certainly already have in some form — they are here to show *where EasySemVer's two steps sit
relative to them, and why that ordering is the only one that works*.

```yaml
name: Release

on:
  push:
    branches: [ main ]

# One release at a time. Two runs landing together would both seed from the same version, compute
# the same next one, and race to push it. `cancel-in-progress: false` deliberately: a
# half-finished release - package pushed, commit not - is worse than a queued one.
concurrency:
  group: release
  cancel-in-progress: false

jobs:
  release:
    runs-on: ubuntu-latest

    # Naming any permission replaces the defaults wholesale, so every one you need has to be
    # listed. `contents: write` is what lets the second EasySemVer step push the commit and the
    # tag. If you publish over OIDC, `id-token: write` belongs here too.
    permissions:
      contents: write

    steps:
    # YOUR CHECKOUT - with one requirement on it. `fetch-depth: 0` brings the tags down, and git
    # tags are one of the places EasySemVer looks for the version to seed from. A shallow clone
    # hides them, and the run seeds from something older than your last release.
    - uses: actions/checkout@v5
      with:
        fetch-depth: 0

    # YOUR TOOLCHAIN - setup-dotnet, setup-node, setup-swift, whatever your build needs.
    # EasySemVer needs none of it and does not care whether this runs before or after it; the
    # exception is a folder containing Swift, which needs a real toolchain for the extraction
    # itself (see "If your repository has Swift in it" below).
    - uses: actions/setup-dotnet@v5
      with:
        dotnet-version: 10.0.x

    # ---------------------------------------------------------------------------------------
    # EASYSEMVER, 1 of 2. Reads the API surface of every unit in the folder, diffs it against
    # the committed EasySemVer.xml, and writes the new version into every version location that
    # already exists. Publishes the verdict as step outputs.
    #
    # BEFORE your build, so that everything the build produces carries the new version. Nothing
    # is committed here - the bump exists only in the runner's working copy so far.
    # ---------------------------------------------------------------------------------------
    - name: Compute and apply the version
      id: version
      uses: winterborn-llc/easysemver@v18

    # YOUR BUILD, PACKAGE AND TESTS - unchanged, and now carrying the version stamped above.
    # Nothing has been committed or pushed yet, so a failure anywhere in here ends the run having
    # released nothing at all: no commit, no tag, no package, nothing to retract.
    - run: dotnet build --configuration Release
    - run: dotnet test --configuration Release --no-build

    # ---------------------------------------------------------------------------------------
    # EASYSEMVER, 2 of 2. Stages exactly the files the first step wrote, commits them as
    # `EasySemVer: 2.4.0`, tags `v2.4.0`, and pushes the branch and the tag atomically.
    #
    # `report:` hands it the first step's verdict, so the version is computed exactly once
    # however many times this action appears in the job. With it set, the tool is neither
    # downloaded nor run again.
    #
    # AFTER your tests, so a failing test cannot leave a release commit and a tag behind with no
    # release under them. BEFORE you publish anything, because a package cannot be unpublished:
    # if someone pushed to the branch mid-run, this push is rejected and the run dies while there
    # is still nothing outside this repository to take back.
    # ---------------------------------------------------------------------------------------
    - name: Commit and tag the release
      uses: winterborn-llc/easysemver@v18
      with:
        commit: true
        tag: true
        report: ${{ steps.version.outputs.report }}

    # YOUR PUBLISH - last, and now every artifact you push is accounted for by a commit and a tag
    # anyone can find. EasySemVer stops short of this on purpose: there is no one way to publish,
    # so there is nothing here for it to get right on your behalf.
    - run: dotnet nuget push bin/Release/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json
```

Those two steps are not an illustration. They are lifted from
[`.github/workflows/dotnet.yml`](.github/workflows/dotnet.yml), the workflow EasySemVer releases
*itself* with, and a test fails if the two ever differ.

## The two steps, on their own

If you already have a release workflow, this is the whole of what you append to it — the first
before your build, the second after your tests:

```yaml
- name: Compute and apply the version
  id: version
  uses: winterborn-llc/easysemver@v18
```

```yaml
- name: Commit and tag the release
  uses: winterborn-llc/easysemver@v18
  with:
    commit: true
    tag: true
    report: ${{ steps.version.outputs.report }}
```

Plus the three things the job around them needs:

| | Why |
|-|-----|
| `permissions: contents: write` | To push the commit and the tag. Naming any permission replaces the defaults wholesale, so name every one you need. |
| `fetch-depth: 0` on the checkout | EasySemVer seeds from the highest version it can find, and git tags are one of the places it looks. A shallow clone hides them and seeds too low. |
| `concurrency: { group: release, cancel-in-progress: false }` | Two pushes landing together would seed from the same version, compute the same next one, and race. |

**If you build nothing in between, use one step instead.** Versioning, committing and tagging
collapse into a single invocation; it names no path, no project, no branch and no language, so it
goes into any repository unedited:

```yaml
- name: Version, commit and tag
  id: version
  uses: winterborn-llc/easysemver@v18
  with:
    commit: true
    tag: true
```

Split it the moment something is built between the two, for the reasons in the comments above:
the version has to be stamped before the build, and nothing may be pushed until the tests pass.

## Reading the verdict

Every invocation publishes its verdict as step outputs, whether or not it commits anything:

```yaml
- id: version
  uses: winterborn-llc/easysemver@v18

- run: echo "${{ steps.version.outputs.change-type }} → ${{ steps.version.outputs.version }}"
```

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

The run also appends a job summary naming the verdict and the changes behind it, so a release
explains itself in the run page without anyone opening a log.

## Configuring it

| Input | Default | |
|-------|---------|-|
| `folder` | `.` | The folder root, relative to the workspace |
| `dry-run` | `false` | `true` classifies and reports without writing anything |
| `commit` | `false` | `true` commits the bump and pushes it |
| `tag` | `false` | `true` also creates and pushes `v<version>`. Needs `commit: true` |
| `report` | — | Act on an earlier step's `report` output instead of versioning again |
| `version` | the release this manifest shipped with | Which EasySemVer release to run |
| `token` | `${{ github.token }}` | Only needs overriding if this repository is private to you |

`commit` and `tag` both default to `false`. Pushing a version bump to your repository is your
decision and those inputs are where you make it; what the Action will not do either way is
publish.

A run that fails exits 1 and fails the step, and publishes no outputs at all — the verdict is
never encoded in the exit code, because "this change is Major" would then be indistinguishable
from the tool falling over.

### Versioning a subdirectory

```yaml
- uses: winterborn-llc/easysemver@v18
  with:
    folder: src
```

Only the first step needs it. The second takes its verdict from `report` and touches whatever the
first one wrote.

### Checking a pull request without releasing

`dry-run: true` classifies and reports and writes nothing — no version, no baseline. Useful as a
PR check that says what merging would do:

```yaml
- id: version
  uses: winterborn-llc/easysemver@v18
  with:
    dry-run: true

- run: echo "Merging this would be a ${{ steps.version.outputs.change-type }} release."
```

### If your repository has Swift in it

Swift signatures come from the Swift toolchain's own symbol graph, and Xcode targets from
`xcodebuild`, so the job needs `runs-on: macos-latest` and a toolchain it can reach. See
[Swift prerequisites](#swift-prerequisites) — including that a Swift unit whose signature cannot
be extracted fails the run rather than being skipped.

### Which release you get

`@v18` is a moving tag, kept on the newest release of that major the way `actions/checkout@v5` is:
fixes and features arrive without an edit, a new major never does. Name an exact release tag
instead if you would rather nothing changed until you changed it. Either way you run the binary
published alongside the manifest you pointed at — the release stamps the two together, so a moving
tag never means a new wrapper around an old tool.

### Without the Action

The same outputs are available from the CLI, because the tool publishes them itself. Under GitHub
Actions it writes `$GITHUB_OUTPUT` and the job summary without being asked, so the step is the
bare command:

```yaml
- id: version
  run: easysemver .

- run: echo "${{ steps.version.outputs.change-type }} → ${{ steps.version.outputs.version }}"
```

`--github` forces that on where the detection cannot see the environment; `--no-github` forces it
off, for a workflow that wants `--json` only and would rather not have a job summary appended on
its behalf. Either way there is no `jq` to write: the mapping from report to step outputs is the
same in every workflow, so it lives in the tool rather than in each of them.

---

# Running it yourself

## Installing

**As a .NET tool**, if you already have a .NET SDK:

```bash
dotnet tool install -g Winterborn.Tools.EasySemVer
```

**As a standalone binary**, if you don't — a Swift-only repository, for instance. Grab the archive
for your platform from the [releases page](https://github.com/winterborn-llc/easysemver/releases);
it bundles its own runtime and needs nothing else installed. Archives are published for
`linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64` and `win-x64`.

## The command

```bash
easysemver /path/to/your/folder
```

With no argument it uses the current working directory. There is at most one directory argument;
everything else is a flag:

| Flag | |
|------|-|
| `--dry-run` | Classify and report without writing anything — no version, no baseline |
| `--json <path>` | Write the verdict to a file a script can read |
| `--github` / `--no-github` | Force the GitHub Actions outputs and job summary on or off |

`0` on success. `1` on any failure, with the exception printed — deliberate, so that a versioning
failure on a release build is impossible to miss.

## The JSON report

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
is for people. Take the verdict from `changeType` rather than from the array: a run with no
comparable baseline reports no findings and still classifies above Patch.

## Wiring it into a build

EasySemVer is a command; wire it in with whatever your build already uses. It deliberately ships
no MSBuild integration of its own — a tool whose unit of work is *a folder* does not belong
bolted to one arbitrary project inside it.

From a release script or CI step:

```bash
easysemver "$CI_WORKSPACE"
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

---

# How it decides

## What it does, in order

1. Walks the folder once and discovers every packageable unit.
2. Extracts each unit's API signature **in its own language's terms** — a Swift protocol is a
   protocol, not "an interface"; a C# record is a record.
3. Reads the baseline (`EasySemVer.xml` at the folder root) written by the previous run.
4. Runs the classification rules and takes the highest impact anything reported.
5. Seeds from the highest version found in any version location in any unit, increments it, and
   writes that one version into every location that already exists.

Every successful run increments by at least a Patch: the tool assumes it runs for builds that are
releases. Gate it accordingly.

## Seeding versions

EasySemVer only updates version properties that already exist; it never creates one. Seeding a
value is how a team opts a location in:

```xml
<PropertyGroup>
    <AssemblyVersion>1.0.13</AssemblyVersion>
    <PackageVersion>1.0.13</PackageVersion>
    <FileVersion>1.0.13</FileVersion>
</PropertyGroup>
```

Add as many or as few as you want kept in sync. If they disagree, the highest wins and everything
converges on it from the next run onwards.

## Version locations

The starting version is the **highest** value found across all of these, and the new version is
written back to every one of them that already exists.

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

## What counts as a change

Roughly eighty rules, each one a small class with its own test. The full tables are in
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

---

# Contributing

## The specs are the contract

[`specs/`](specs/) is the requirement set, not a description written afterwards. Every requirement
has an id (`CLS-03`, `ML-02`, `ACT-11`) and a status marker, code is traced back to it, and
[99-known-gaps.md](specs/99-known-gaps.md) is the honest list of everything that still diverges.
Start at [specs/README.md](specs/README.md).

A change either conforms to the requirements or changes them. Both are fine; doing neither is not.
If behaviour moves, the spec doc moves in the same commit — including its status marker, and the
gaps list if the change opens or closes one.

## Rules of the design

These are settled and load-bearing. Read them before proposing an alternative shape, because most
of them exist to prevent a specific failure that has already happened once.

**The folder is the unit.** One version per folder root, seeded from the highest value found
anywhere inside it, written back to every location that already exists. Not per-project, not
per-language, not per-target.

**Every run is a release.** There is no "no change" outcome: a successful run always increments by
at least a Patch. Gating is the caller's job.

**Never create a version location.** The tool only updates what a team already seeded. A
`.podspec` with no literal version, a target with no `MARKETING_VERSION`, are read-skipped and
write-skipped.

**The neutral core knows three things** — the packageable unit, the `VersionType` verdict, the
`Version` value. There is no shared abstraction of "type" or "member", and there never will be:
each language is modelled in its own topology, with its own vocabulary
([specs/12 §3](specs/12-multi-language-swift-and-folder-model.md)).

**A language is a plugin.** Adding one means a provider implementing `ILanguageProvider`, its own
`Interfaces/`, `DataObject/`, `CodeReader/` and `Evaluators/` subfolders, and **one registration
line** in [`Providers/LanguageProviders.cs`](src/EasySemVer/Providers/LanguageProviders.cs) — with
no edits to the neutral core. If a change to the core is needed to add a language, the seam is
wrong and that is the bug to fix.

**A classification rule is one small class.** It declares its `Rule` name, its
`EvaluationImpact`, and a `ChangeDescription`, and yields the symbols it found. It is registered
in its language's `CompareSignatures` list. Findings are keyed `(language, rule)`, so a rule name
must be unique within its language but may repeat across languages.

**The baseline is deterministic.** Two runs over unchanged source on two machines produce
byte-identical `EasySemVer.xml`. Anything machine-dependent — absolute paths, timestamps,
toolchain versions, hash iteration order — is a defect, because it makes every checkout report a
change that is not one.

**Failure is fatal and loud.** Exit 1, print the exception, write nothing. No partial baselines,
no skip-and-warn: a baseline missing a unit under-reports the *next* change, silently, on a run
nobody is watching.

**Documentation is executed.** The release steps in this README are asserted against
[`.github/workflows/dotnet.yml`](.github/workflows/dotnet.yml) byte for byte by
`ActionRegression`, and the release rewrites the version pins in `action.yml` and the `uses:` refs
here. Do not hand-edit either — a stale pin used to be a routine bug and is now a test failure.

## Tests

| | |
|-|-|
| [`src/Test`](src/Test) | Unit tests. Host-free, and built from hand-constructed signature graphs rather than live extraction, so a failing rule test means the rule is wrong and nothing else. |
| [`src/IntegrationTest`](src/IntegrationTest) | End to end: the real tool, the real Action scripts pulled out of `action.yml`, real git repositories with real remotes. |

```bash
dotnet test src/Test/Test.csproj
dotnet test src/IntegrationTest/IntegrationTest.csproj
```

Every classification rule has a dedicated test class asserting at minimum its declared impact, a
no-difference case that does not fire, and a representative difference that does; directional
rules assert the non-firing direction too ([specs/11](specs/11-testing.md)). A rule without one is
not finished.

The Swift and Xcode suites shell out to a real toolchain, so the full run wants macOS with Xcode
configured; CI keeps that requirement inside a single job. Be aware that the integration
regression versions this repository on purpose, so it leaves the working copy dirty.

## Pull requests

- **Branch, push, and let CI run.** [`ci.yml`](.github/workflows/ci.yml) builds and tests every
  branch but `main`, using the same reusable job that gates the release — a branch cannot go green
  against a weaker suite than the one guarding `main`. Fork PRs are not built today: `ci.yml` has
  no `pull_request` trigger, and adding one is the change to make when this repository starts
  taking them.
- **Do not touch versions by hand.** Not the `.csproj` properties, not `EasySemVer.xml`, not
  `action.yml`'s `default: v…`, not this README's `uses:` refs. A push to `main` versions, commits,
  tags, publishes and repoints all of them. A hand-edit is either overwritten or a merge conflict.
- **Bring the spec with you.** New behaviour gets a requirement; changed behaviour edits the one
  it breaks; a known limitation goes in the gaps list rather than being left for the next person
  to rediscover.
- **Say why in the commit.** Subjects are imperative and sentence case ("Match an overload on its
  generic arity as well as its parameters"). The body carries the reasoning, and so do the
  comments — this codebase explains *why* at the point the decision lives, and a change that
  removes that reasoning is a change that will be undone by accident later.
