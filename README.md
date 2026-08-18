# EasySemVer from Winterborn

EasySemVer works out what your next version number is, from what actually changed in your public
API, and writes it into every place a version already lives in your code.

Nobody decides the number and nobody has to remember to bump it. A removed method is Major because
it is Major, on the run that removed it.

- **Major** — something a caller relied on stopped working: a type or member removed, a signature
  or return type changed, a parameter made required, an interface gaining an undefaulted
  requirement, a class sealed, a conformance dropped.
- **Minor** — something was added: a new unit, type, member or overload; a constraint loosened; a
  setter gained.
- **Patch** — everything else, including implementation-only changes.

Roughly eighty rules sit behind those three lines; [what counts as a change](#what-counts-as-a-change)
has the shape of them and the specs have the tables. Every successful run increments by at least a
Patch, because it assumes it is running for a release.

It reads **C#**, **VB.NET** and **Swift** — `.csproj` and `.vbproj` projects, SwiftPM targets, Xcode
targets — and it reads them from source rather than from compiled assemblies, so it does not care
whether your build has run yet.

It also **versions** ten more ecosystems without reading them. See
[Languages](#languages) for exactly which, and for what the difference means.

**It reads the objects you declared, and only those.** Types, members, signatures, conformances,
as they are written down. It does not interpret your logic, so an API you assemble at runtime is
not one it can see: a surface built by reflection, dispatch on a string key, endpoints registered
from configuration, a wire contract that only exists once something is serialized. None of that is
declared, so none of it is compared, and changing it reads as an implementation change — a Patch.
The assumption is that **the objects as represented are your contract**. Where that is not true of
part of your API, treat the version EasySemVer computes as a floor rather than the whole verdict,
and keep deciding that part yourself.

Two ways to use it. The **GitHub Action** is the whole of the setup for most repositories and is
what the next section covers. If your release does not live in GitHub Actions,
[install it and run the command](#running-it-yourself) instead — it is the same tool, and the
Action is a thin wrapper around the binary.

Looking to contribute? The architecture, the settled design rules and what a change is expected to
bring with it are in the [contributors' guide](readme-contributors.md).

---

# The GitHub Action

Add it to a workflow and the repository versions, commits and tags itself. Nothing needs .NET
installed: the Action downloads a self-contained binary for the runner's platform.

**Seed a version first.** EasySemVer only writes into version properties that already exist, so
put a starting value wherever you want the number to land — an `<AssemblyVersion>`, a
`MARKETING_VERSION`, a `.podspec` — and commit it before the first run. With nothing seeded
anywhere the run still succeeds and still tags a release, but no file in your source carries the
version, and your artifacts go out with whatever they had. [Seeding versions](#seeding-versions)
is the full list of places that count.

## The whole workflow

Two steps go into your release workflow, in these two places:

```
checkout → [ EasySemVer: version ] → your build and tests → [ EasySemVer: commit and tag ] → your publish
```

Save this as `.github/workflows/release.yml`, or lift the two marked steps into the workflow you
already have:

```yaml
name: Release

on:
  push:
    branches: [ main ]

# never two releases at once
concurrency:
  group: release
  cancel-in-progress: false

jobs:
  release:
    runs-on: ubuntu-latest

    permissions:
      # to push the updated versions and tags back into the repo
      contents: write

    steps:
    - uses: actions/checkout@v5
      with:
        # tags are one of the places the version is read from
        fetch-depth: 0

    # Increment our new versions before we build, so what the build carries the new version
    - name: Compute and apply the version
      id: version
      uses: winterborn-llc/easysemver@v21

    # Your build, package and tests, unchanged.

    # After your tests, before you publish, commit the version details and apply the tags
    - name: Commit and tag the release
      uses: winterborn-llc/easysemver@v21
      with:
        commit: true
        tag: true
        report: ${{ steps.version.outputs.report }}

    # Your publish, if you have one.
```

The ordering is the part worth getting right:

- **Version before the build**, or your artifacts go out carrying the previous number.
- **Commit and tag after the tests.** Nothing is committed until that second step, so a failure
  anywhere in between ends the run having released nothing at all.
- **Publish after the commit**, because a package cannot be unpublished. If someone pushed to the
  branch mid-run, the commit is rejected and the run dies while there is still nothing outside
  your repository to take back.

`report:` hands the second step the first one's verdict, so the version is computed once no matter
how many times the action appears. Those two steps are lifted from
[`.github/workflows/dotnet.yml`](.github/workflows/dotnet.yml), the workflow EasySemVer releases
*itself* with, and a test fails if the two ever differ.

## Adding them to a workflow you already have

The two steps, on their own — the first before your build, the second after your tests:

```yaml
- name: Compute and apply the version
  id: version
  uses: winterborn-llc/easysemver@v21
```

```yaml
- name: Commit and tag the release
  uses: winterborn-llc/easysemver@v21
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
  uses: winterborn-llc/easysemver@v21
  with:
    commit: true
    tag: true
```

Split it the moment something is built between the two, for the ordering reasons above.

## Configuring it

| Input | Default | |
|-------|---------|-|
| `folder` | `.` | The folder root, relative to the workspace |
| `dry-run` | `false` | `true` classifies and reports without writing anything |
| `commit` | `false` | `true` commits the bump and pushes it |
| `tag` | `false` | `true` also creates and pushes `v<version>`. Needs `commit: true` |
| `max-minor` | — | Cap the minor segment, carrying into major. See [Segment ceilings](#segment-ceilings-and-the-255-limit) |
| `max-patch` | — | Cap the patch segment, carrying into minor. Set both to `255` if you ship a framework or dylib target |
| `do-not-exclude` | — | Directory names discovery should keep, one per line. See [Excluded directories](#excluded-directories) |
| `vnext-token-name` | `vnext` | The token stamped with the new version is `{{`this`}}`. See [Putting the version in your own text](#putting-the-version-in-your-own-text) |
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
- uses: winterborn-llc/easysemver@v21
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
  uses: winterborn-llc/easysemver@v21
  with:
    dry-run: true

- run: echo "Merging this would be a ${{ steps.version.outputs.change-type }} release."
```

### If your repository has Swift in it

Nothing extra. Swift is read from your source files, so the job runs on whatever runner you were
already using and needs neither a Swift toolchain nor Xcode. See
[How Swift is read](#how-swift-is-read) for what that does and does not see.

### Which release you get

`@v18` is a moving tag, kept on the newest release of that major the way `actions/checkout@v5` is:
fixes and features arrive without an edit, a new major never does. Name an exact release tag
instead if you would rather nothing changed until you changed it. Either way you run the binary
published alongside the manifest you pointed at — the release stamps the two together, so a moving
tag never means a new wrapper around an old tool.

## Reading the verdict

Every invocation publishes its verdict as step outputs, whether or not it commits anything:

```yaml
- id: version
  uses: winterborn-llc/easysemver@v21

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

## In a workflow, without the Action

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

The same tool, without the workflow: install it once and call it from whatever runs your release.
The seeding note above applies here too.

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
| `--max-minor <n>` | Carry minor into major once it passes `n`. No ceiling by default |
| `--max-patch <n>` | Carry patch into minor once it passes `n`. No ceiling by default |
| `--do-not-exclude <name>` | Keep a directory discovery would skip. Repeatable; a bare name, not a path |
| `--vnext-token-name <name>` | Stamp `{{`name`}}` with the new version instead of `{{vnext}}`. The name, not the braces |

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

From MSBuild, if that is where your release process lives:

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

Reference for everything the two sections above assume: what a run does, what moves the number,
and where the number is read from and written to.

## What it does, in order

1. Walks the folder once and discovers every packageable unit.
2. Extracts each unit's API signature **in its own language's terms** — a Swift protocol is a
   protocol, not "an interface"; a C# record is a record.
3. Reads the baseline (`EasySemVer.xml` at the folder root) written by the previous run.
4. Runs the classification rules and takes the highest impact anything reported.
5. Seeds from the highest version found in any version location in any unit, increments it, and
   writes that one version into every location that already exists.
6. Replaces every `{{vnext}}` under the folder root with that same version, for the places no
   version location covers.

Every successful run increments by at least a Patch: the tool assumes it runs for builds that are
releases. Gate it accordingly.

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

Every rule reads declarations, never behaviour. A method that keeps its signature and changes what
it does is a Patch, and so is a change to anything your code builds at runtime rather than
declares — the disclaimer at the top of this file is the long version.

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

## Languages

Languages come in two tiers, and the difference is worth understanding before you rely on either.

**Read** languages are the product as described above: EasySemVer finds their packages, reads their
public API, decides Major/Minor/Patch, and stamps the result.

**Versioned** languages are found and stamped, but **never read** — so they never contribute to the
Major/Minor/Patch decision. In a repository that mixes tiers, the change type is decided by the
Read languages and the resulting version is written into everything. In a repository with *only*
Versioned languages, every run is a Patch: EasySemVer becomes a version stamper rather than a
version decider, which is useful, but it is not the same product.

| Language | Tier | Package is | Version lives in |
|---|---|---|---|
| C# | **Read** | a `.csproj` | `AssemblyVersion`, `PackageVersion`, `FileVersion` |
| VB.NET | **Read** | a `.vbproj` | the same MSBuild properties |
| Swift | **Read** | a SwiftPM or Xcode target | six locations, below |
| JavaScript / TypeScript | Versioned | a `package.json` | `"version"` |
| Rust | Versioned | a `Cargo.toml` | `[package] version` |
| Python | Versioned | a `pyproject.toml` | `[project]` or `[tool.poetry] version` |
| Dart / Flutter | Versioned | a `pubspec.yaml` | top-level `version:` |
| Java | Versioned | a `pom.xml` | `/project/version` (Maven only) |
| PHP | Versioned | a `composer.json` | `"version"`, if you declare one |
| Go | Versioned | a `go.mod` | a git tag — see below |
| C / C++ | Versioned | a `CMakeLists.txt` | `project(… VERSION …)` |
| Ruby | Versioned | a `.gemspec` | the gemspec literal, or `VERSION` in `version.rb` |
| Perl | Versioned | a `Makefile.PL`, `Build.PL` or `dist.ini` | `dist.ini`, and `$VERSION` in every `.pm` |
| Java / Kotlin / Groovy (Gradle) | Versioned | a `build.gradle` or `build.gradle.kts` | `version = "…"`, or `gradle.properties` |

Why the split: a hand-rolled reader that quietly misses part of your public API turns a breaking
change into a Patch, on a run nobody is watching. A Versioned language is *visibly* incomplete —
this table says so, and so does the run log, once per package. A bad reader would be *invisibly*
wrong, and this tool's whole job is to be trusted with a number nobody double-checks.

C++, Ruby and Perl are expected to stay Versioned permanently rather than provisionally. A C++
header does not say what it declares until the preprocessor has run; Ruby's `private` is a method
call, not a declaration; and Perl's grammar can be changed by the program being parsed. For those
three there is no honest static answer, so none is invented.

Gradle modules are listed as `gradle` rather than as a language, because a Gradle module does not
say whether it is Java, Kotlin or Groovy and often contains more than one. Guessing from the source
directories would be wrong silently, which is worse than being vague accurately.

**Go is the odd one out.** A `go.mod` has no version field — a Go module's version *is* its git tag,
because `go get` resolves against the tag and nothing else. So Go seeds from your highest existing
tag like every other language, but it has nowhere to write unless you pass `--tag`.

Adding a package for one of these does not require configuration. If the manifest is there and it
carries a literal version, it is found.

## Version locations

The starting version is the **highest** value found across all of these, and the new version is
written back to every one of them that already exists.

| Language | Location | Read | Write |
|----------|----------|:----:|:-----:|
| C# / VB.NET | `.csproj` / `.vbproj` `AssemblyVersion`, `PackageVersion`, `FileVersion` | ✅ | ✅ |
| Swift / Xcode | `MARKETING_VERSION` in `project.pbxproj` | ✅ | ✅ |
| Swift / Xcode | `CURRENT_PROJECT_VERSION` in `project.pbxproj` | ❌ never | ✅ |
| Swift / Xcode | `CFBundleShortVersionString` in `Info.plist` | ✅ | ✅ |
| Swift | `s.version` in a `.podspec` | ✅ | ✅ |
| Swift | a `*Version.swift` constant, e.g. `static let version = "1.2.3"` | ✅ | ✅ |
| the Versioned languages | their own manifest, per the [Languages](#languages) table | ✅ | ✅ |
| any | git tags matching `v?MAJOR.MINOR.PATCH` | ✅ | only with `--tag` |

A manifest is only written where the version is already a **literal**. A `composer.json` with no
`version` key, a `pyproject.toml` using `dynamic = ["version"]`, a Maven module inheriting its
version from a parent — all are left exactly as they are. EasySemVer updates version locations; it
never creates one, in any language.

It also never touches a version that is not yours. A manifest mentions versions it does not own — a
dependency's, a parent POM's, an `engines` constraint — and only the package's own version is
rewritten.

`CURRENT_PROJECT_VERSION` is **written but never read**. Writing it means the build counter moves
on every run without anyone maintaining it by hand, which is what Xcode and the App Store want.
Reading it would be a disaster: the value is routinely a bare integer like `87`, which parses as
`87.0.0`, and since the seed is the highest version found anywhere, one build counter would drag
the whole folder's version up with no way back down. `CFBundleVersion` in `Info.plist` is usually
`$(CURRENT_PROJECT_VERSION)` and follows from it.

Git tags are read as a seed on every run. Writing one needs `--tag`, and even then EasySemVer
creates a **local** tag and never pushes it — a local tag is something you can delete, a pushed one
is not. Publishing it is left to whatever already publishes your releases; the GitHub Action's own
`tag: true` step does that, after your tests rather than before them. If the tag already exists the
run leaves it alone rather than failing, so re-running is safe.

`--tag` is what makes Go versionable at all, and it is worth having for PHP and Python too, since
both commonly version by tag rather than by a literal in the manifest.

### Segment ceilings, and the 255 limit

`--max-minor` and `--max-patch` cap a segment: instead of ticking past the ceiling, it resets to
zero and adds one to the segment above, so `1.0.255` with `--max-patch 255` becomes `1.1.0`. The
result always sorts above what it replaced, so a version never goes backwards.

**There is no default, and that is deliberate.** A carried patch publishes a Minor bump on a run
where nothing was added — the number stops meaning what the rest of this README says it means.
That is worth paying only when the target genuinely cannot hold the number.

The case where it can't is **Mach-O `DYLIB_CURRENT_VERSION`**, which caps the second and third
segments at 255 (and the first at 65535). In a stock Xcode project that setting derives from
`CURRENT_PROJECT_VERSION` — which EasySemVer now writes — so on a **framework or dylib target**,
the first run after patch passes 255 fails to link. Apps don't link that setting and are unaffected.

So set the ceilings if you ship a framework or dylib target from an `.xcodeproj`:

```bash
easysemver /path/to/your/folder --max-minor 255 --max-patch 255
```

To be clear about what this is *not*: Swift Package Manager package versions are git tags and have
no such limit, and neither do `MARKETING_VERSION`, `CFBundleShortVersionString`, or `.podspec`
versions. If you ship only apps, packages, or pods, leave the ceilings off and keep versions that
mean what they say. Other ecosystems have their own ceilings if you ever need them — .NET assembly
versions and Win32 `FILEVERSION` fields cap every segment at 65535.

## Putting the version in your own text

The table above is every place EasySemVer knows the shape of. For everywhere else — a changelog
heading, a Helm chart, a docs page, an installer script — mark the spot with `{{vnext}}` and the
run replaces it with the version it just computed:

```markdown
## {{vnext}} — unreleased

- The thing you did.
```

becomes

```markdown
## 2.4.0 — unreleased

- The thing you did.
```

Every occurrence in every file under the folder root, in files of any kind. The stamped files are
reported like every other write, so `commit: true` stages them and the release commit carries them.

**The token is consumed.** After the run the file says `2.4.0`, not `{{vnext}}` — which is the
point, and the thing to understand before adopting it. A file that wants the version every release
has to be marked again every release; for a changelog, writing the next entry is what does that.

Skipped without asking: excluded directories (the same list [discovery
uses](#excluded-directories)), the `EasySemVer.xml` baseline, binary files, and text that is not
UTF-8. Everything else in a stamped file survives byte for byte, a byte-order mark included.

If `{{vnext}}` legitimately means something else in your repository, rename the one EasySemVer
stamps:

```bash
easysemver . --vnext-token-name release   # now it looks for {{release}}
```

```yaml
- uses: winterborn-llc/easysemver@v21
  with:
    vnext-token-name: release
```

Pass the name, not the braces. Naming a token you never write is also how you turn the replacement
off — which is what this repository does to its own release run, since the file you are reading
documents the default.

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

## Excluded directories

Discovery skips, at any depth: any directory whose name begins with `.` (so `.git`, `.build`,
`.swiftpm`, `.packages`), plus `bin`, `obj`, `build`, `DerivedData`, `Pods`, `Carthage` and
`node_modules`. This is not politeness — an unexcluded dependency checkout would pull other
people's source into your signature and make every dependency update a Major change.

Every run says what it skipped:

```
Skipped 9 directories: bin (4), node_modules (1), obj (4). Pass --do-not-exclude <name> for any that holds code you version.
```

`--do-not-exclude <name>` keeps one of them, and repeats for more than one. It takes a bare
directory name, never a path, because the exclusion matches a single path segment. It overrides
the leading-dot rule too, so a project that genuinely keeps source under a dotted directory can
say so:

```bash
easysemver . --do-not-exclude build --do-not-exclude .generated
```

`Packages` is **not** excluded, deliberately. SwiftPM cloned dependencies into `Packages/` in the
Swift 3 era and has used `.build/checkouts/` since; today it is where a modular Xcode app keeps its
own local packages. Excluding it defended a dead convention while silently dropping first-party
units — and a silent false negative is much worse here than the visible false positive of
discovering a vendored copy, which shows up as new units in your baseline. If you do vendor
dependencies into `Packages/`, that is what the skip log and `--do-not-exclude` are for in
reverse: you will see the extra units on the first run and can move them.

## How Swift is read

**There are no prerequisites.** Targets come from the text of `Package.swift` and
`project.pbxproj`, and signatures come from your `.swift` files. No Swift toolchain, no Xcode, no
network, no build. A folder with Swift in it costs about as much to version as a folder without.

Earlier versions asked the toolchain instead: `swift package dump-package` and `xcodebuild -list`
to find the targets, then a `swift build` or `xcodebuild build` per package to emit symbol graphs.
That was more accurate, because the compiler had resolved the program — and it meant every
versioned run needed Xcode, a network, and credentials for every private package dependency, and
failed outright when any of those was missing.

Reading source instead is an approximation, in the same way the C# reader is one. What it costs:

- **An inferred type is not recorded.** `public let store = Store()` has no written type, so a
  change of type there is invisible. Write the type if it is part of your contract.
- **Macro-generated declarations are not seen**, because they are not in the file.
- **Every branch of an `#if` is read**, because picking one needs the build configuration.
- **A class's first inheritance entry is read as its superclass** unless the module declares it as
  a protocol. `class Foo: Codable` reads `Codable` as a superclass. It is wrong in the same way on
  every run, so it never churns your baseline.
- **A target whose name is computed** rather than written as a literal in `Package.swift` is not
  discovered. It says so in the log.

Every one of those errs towards reporting more surface than you have, never less. What still fails
the run with exit 1 and writes nothing: a manifest or project that declares a target whose source
is nowhere to be found. A target that exists and simply has no Swift in it — an Objective-C or C
target — is versioned like anything else and compared against nothing.

**Upgrading from v18 or earlier:** your existing baseline is kept, but its Swift units are dropped
and re-seeded, because the old signatures described the same API in different words. Expect one
release in which every Swift target reads as new. Nothing else in the baseline is affected, and a
repository with no Swift in it sees no change at all.

