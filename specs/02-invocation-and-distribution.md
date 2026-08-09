# 02 — Invocation and Distribution

Sources: [`Program.cs`](../src/EasySemVer/Program.cs),
[`RunOptions.cs`](../src/EasySemVer/Settings/RunOptions.cs),
[`EasySemVer.csproj`](../src/EasySemVer/EasySemVer.csproj),
[`README.md`](../README.md).

## Command-line contract

**CLI-01 — Deliverable is a console executable.** ✅
The tool SHALL build as a console executable (`OutputType=Exe`) with assembly name
`Winterborn.Tools.EasySemVer`, multi-targeting `net8.0`, `net9.0`, and `net10.0`
(`LangVersion` 14). `public static int Main(params string[] args)` is public so tests can invoke
a run in-process, and returns its exit code rather than calling `Environment.Exit` — see ERR-03.

**CLI-02 — At most one directory argument.** ✅
The tool SHALL accept zero or one directory argument, plus any recognised flags. Two or more
directories SHALL fail the run with the message that a single directory parameter is required.

**CLI-03 — Zero arguments → current directory.** ✅
With no arguments, the starting directory SHALL be the process's current working directory.
This is the primary mode when invoked from an MSBuild target, whose working directory is the
project directory.

**CLI-04 — The directory argument is the folder root.** ✅ *(replaced by FLD-01; G-06 resolved)*
The single directory argument SHALL be the folder root, resolved to a full path. It is used, not
merely validated. Implemented in [`RunOptions.Parse`](../src/EasySemVer/Settings/RunOptions.cs).

**CLI-05 — No solution-root discovery.** ✅ *(retired by FLD-02; G-09 resolved)*
The tool SHALL NOT search ancestors for a `.sln`/`.slnx` and SHALL NOT require one to exist. A
folder containing no solution file is a valid, ordinary input. Both walk-up helpers are deleted.

**CLI-06 — Exit codes.** ✅
The process SHALL exit `0` on success. Any unhandled error SHALL print the full exception to
stdout and exit `1`. (See [10-logging-and-error-handling.md](10-logging-and-error-handling.md)
for why exit 1 is a deliberate build-failure signal.)

**CLI-07 — `--dry-run` previews without writing.** ✅ *(added per §20 O-04)*
`--dry-run` SHALL run discovery, extraction, classification and version resolution, log the
verdict and the version it would produce, and write nothing: no baseline, no version stamps. A
dry run is not a release, so it does not conflict with OVR-03. Without the flag, every successful
invocation increments by at least Patch and rewrites state.

**CLI-08 — a dry run explains its verdict.** ✅
`--dry-run` SHALL list every detected change, grouped by the unit it was found in, one line per
change carrying that change's impact and the symbol it concerns, and end with the change type
(with a per-impact tally) and the version transition. A verdict with no evidence behind it is not
reviewable, which is the whole point of the mode. Findings are ordered by unit and then by symbol
so that identical input produces identical output, for the same reason BAS-04 sorts a baseline.
A run without the flag keeps the quieter one-line-per-firing-rule summary (LOG-03).

**CLI-09 — `--json <path>` requires a path.** ✅
`--json` SHALL consume the following argument as its path and SHALL fail if none follows. The
path is not mistaken for the folder argument whichever order the two are given in, and an
unrecognised `--option` is still rejected by name.

**CLI-10 — The tool publishes its own GitHub Actions surface.** ✅
When running under GitHub Actions, EasySemVer SHALL write the verdict to `$GITHUB_OUTPUT` and a
Markdown rendering of it to `$GITHUB_STEP_SUMMARY` itself. It SHALL do so by default when
`GITHUB_ACTIONS` is `true`, and `--github` / `--no-github` SHALL force it on or off regardless.
The published names are ACT-05's, so a workflow calling the CLI directly and a workflow using the
Action read the same outputs.

The mapping from report to step outputs is identical in every workflow that runs this tool, and
before this requirement **every one of them wrote its own `jq` to do it** — including
[`action.yml`](../action.yml), which carried a second copy. That is a formatting concern, not a
consumer's, and it belongs in the one place that already holds every value. A workflow step is now
the bare command:

```yaml
- id: version
  run: easysemver .
```

ℹ️ Detection rather than a required flag, because a flag you must remember is a flag you will
forget, and the failure is silent: an empty `steps.version.outputs.version` in a later step, not an
error. `--no-github` exists for the case that motivated the escape hatch — a workflow that wants the
JSON only, and does not want a job summary appended on its behalf.
ℹ️ Both destinations are appended to, never truncated: a job may run this tool more than once, and a
summary is shared with every other step in the job.
ℹ️ A missing `$GITHUB_OUTPUT` or `$GITHUB_STEP_SUMMARY` is a skip with a log line, never a failure.
The versioning run has already succeeded by then, and failing a release over a report would be the
wrong trade in a way REP-08 makes clear.

## Machine-readable report

**REP-01 — `--json <path>` writes the run's verdict as a file.** ✅
`easysemver <folder> --json <path>` SHALL write a JSON document describing the run to
`<path>`. It SHALL NOT write JSON to stdout and SHALL NOT move the human log anywhere: LOG-01
keeps stdout, unconditionally. A workflow that wants the verdict reads the file; nothing has to
parse a log, and no existing stream contract changes.

**REP-02 — The document.** ✅
```json
{
  "formatVersion": 1,
  "dryRun": false,
  "changeType": "major",
  "oldVersion": { "version": "2.3.4", "major": 2, "minor": 3, "patch": 4 },
  "newVersion": { "version": "3.0.0", "major": 3, "minor": 0, "patch": 0 },
  "findings": [ … ]
}
```
`findings` is specified by REP-09.
- `changeType` is `"major" | "minor" | "patch"`, lower case. So are all enum-valued fields: this
  is a wire format, not a dump of a C# enum, and lower case removes a class of casing bugs in
  shell and YAML comparisons.
- Both version objects share one shape. `version` is the canonical string and the full truth;
  `major`/`minor`/`patch` are numbers and are the *first three* segments, which matters because a
  version may legitimately carry more (VER-07). The decomposition is there because consumers
  routinely want the parts — a Docker tag set of `3`, `3.0`, `3.0.0` from one run.
- `dryRun` is the only signal that a report describes a preview rather than something that
  happened, so it is always present.

**REP-03 — `formatVersion` has its own lifecycle.** ✅
It starts at `1` and is versioned independently of the baseline's `formatVersion` (currently 3).
The two documents have different audiences and different failure modes — a JSON consumer breaking
is not a versioning run breaking — so sharing a number would couple them for no reason.

**REP-04 — Fields may be added; nothing may be removed or retyped.** ✅
Adding a field is backwards-compatible and SHALL NOT bump `formatVersion`. Removing one, renaming
one, or changing its type SHALL bump it. This is what makes REP-05's minimalism safe rather than
short-sighted: the document can grow into whatever a real consumer turns out to need.

**REP-05 — Only what a consumer demonstrably needs.** ℹ️ *(findings since added by REP-09)*
Discovered units, individual findings, and the list of files written were all specified, weighed
and deliberately **left out**. They belong in the log, which already renders them (CLI-08), and
no consumer for them existed at the time of writing. Per REP-04 any of them can be added later
without breaking a reader, whereas shipping them now would have committed the contract to a
shape nobody had asked for. Do not re-add them by reflex; add them when something needs them.

ℹ️ Findings met that condition on 2026-08-03 and were added as REP-09. The written-file list met it
on 2026-08-09 and was added as REP-10. Both are the process working as intended rather than a
reversal — the requirement asked for a demonstrated need, and in each case one arrived.
**Discovered units remain deliberately absent, on the original grounds.**

**REP-06 — The verdict is stated, never inferred.** ℹ️
`changeType` is not redundant with the two versions, and a consumer SHALL NOT derive it by
comparing them. VER-05's overflow rollover makes that inference wrong: `1.0.2147483647 + Patch`
becomes `1.1.0`, which reads as a Minor bump, and `1.2147483647.123 + Minor` becomes `2.0.0`,
which reads as Major. The field is the only place the classification itself appears.

**REP-07 — Determinism, and no machine-specific content.** ✅
The document SHALL contain no absolute paths, timestamps, durations, machine names or tool
versions. Two runs over unchanged source on two machines SHALL produce byte-identical JSON, for
the same reason a baseline must (BAS-04) — so that diffing two reports shows only real change.
ℹ️ The folder root is deliberately absent: the caller passed it, so restating it would buy
nothing and would be the one field that broke this property.

**REP-09 — `findings`: the evidence behind the verdict.** ✅
The document SHALL carry every finding the run made, in the order `ChangeReport` already sorts
them — unit, then symbol, then rule — so REP-07 holds for the array as it does for the rest:

```json
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
```
- `ruleId` is the rule's identifier from the tables in
  [07](07-change-classification.md) and [12 §13](12-multi-language-swift-and-folder-model.md).
  It is what makes a verdict auditable in a machine's hands rather than only a reader's: a
  consumer can point at the rule that cost it a Major without matching on prose.
- The rule's **class name is not published.** It is an implementation detail, and a consumer keyed
  to it would break on a rename that changed no behavior. `ruleId` is the stable identity, which
  is the reason [`TestRuleIds`](../src/Test/TestRuleIds.cs) pins every id to its spec row.
- `impact` and `language` are lower case, like every other enum-valued field (REP-02).
- `description` completes "*symbol* …" — "was removed". It is prose for a human reading the
  document, and nothing should match on it; `ruleId` is there for that.
- The array SHALL be present and empty rather than absent when a run found nothing. An absent
  array would make "no changes" and "an older writer" indistinguishable.
ℹ️ A run can legitimately report an empty array and a `changeType` above `patch`: CLS-04's
fail-safe raises the floor when there is no comparable baseline, and there is no symbol to name.
This is REP-06 again — the verdict is stated, never inferred, including from this array.
ℹ️ Adding this field does not bump `formatVersion` (REP-04), and it introduces no absolute path,
timestamp or machine name: `unitId` is machine-stable by ML-03, so REP-07 is unaffected.

**REP-08 — A failed run writes no report.** ✅
If the run fails, `<path>` SHALL NOT be written or modified. This matches BAS-06 — a failed run
writes nothing at all — and the exit code (CLI-06) remains the failure channel. A half-truthful
report is worse than no report.

**REP-10 — `writtenFiles`: every file the run changed.** ✅ *(added 2026-08-09; REP-05's condition met)*
The document SHALL carry a `writtenFiles` array holding every file the run modified — the baseline
and every version location stamped — as folder-root-relative paths with forward slashes, sorted
ordinally and deduplicated. It SHALL be present and empty on a dry run.

```json
"writtenFiles": [ "EasySemVer.xml", "src/EasySemVer/EasySemVer.csproj" ]
```

The demonstrated need is staging. A workflow that commits the bump has to name what to stage, and
until this field existed it named the paths by hand:

```yaml
git add EasySemVer.xml src/*/*.csproj      # what this replaces
```

That glob is a latent defect, not merely a nuisance. It misses a project added one level deeper and
does so **silently**: the run stays green, the tag is created, and the commit it points at does not
contain the version it claims. `git add -u` is not the alternative — TST-05's integration step
mutates the working copy on purpose, so a blanket stage would sweep test debris into a release
commit. Only the tool knows the answer, and now it says so.

ℹ️ Each provider reports its own writes ([`ILanguageProvider.WriteVersion`](../src/EasySemVer/Interfaces/ILanguageProvider.cs)
returns them) rather than the caller inferring the list from a unit's version sources. The
inference is right today and would go wrong the first time a provider wrote a file that was not one
of them, which is the kind of coupling ML-02 exists to prevent.
ℹ️ Paths are relative to the **folder root**, not to the repository root — REP-07 forbids the
absolute path that would make them unambiguous. Where the two differ, a workflow stages from the
folder root (`working-directory:`), which is what the copy-paste block in the README does.
ℹ️ Adding this field does not bump `formatVersion` (REP-04), and it introduces no absolute path,
timestamp or machine name, so REP-07 is unaffected.

## Invocation surfaces

**INV-01 — The CLI is the product.** ✅ *(replaces MSB-01/MSB-02)*
EasySemVer SHALL be consumed as a command that takes a folder. It SHALL NOT require, or ship,
any MSBuild integration: no targets file, no `Task` class, no `PackageReference` that a
consuming project imports. That model was inherited from the AutoVersion design, never worked
(G-02/G-03/G-04), and is withdrawn rather than repaired — a tool whose unit of work is *a
folder* does not belong bolted to one arbitrary project inside it.

Consumers wire it in with whatever their build already uses. An MSBuild `Exec` remains perfectly
possible and is the consumer's own business:

```xml
<Target Name="EasySemVer" BeforeTargets="Build" Condition="'$(Configuration)'=='Release'">
    <Exec Command="easysemver &quot;$(MSBuildProjectDirectory)&quot;" />
</Target>
```

**INV-02 — Two distribution channels.** ✅
See [03-packaging-and-distribution.md](03-packaging-and-distribution.md): a `dotnet tool` for
anyone who already has a .NET SDK, and self-contained per-platform binaries for anyone who does
not — a Swift-only repository, most obviously.

**INV-03 — Release-only gating is the consumer's job.** ℹ️ *(was MSB-03)*
Because every run is a release (OVR-03), consumers SHOULD invoke EasySemVer only for builds that
are releases. The tool performs no configuration check of its own and has no opinion about
branches; `--dry-run` (CLI-07) exists for the cases where you want the verdict without the
consequences.

**INV-04 — Timing is unconstrained.** ℹ️ *(was MSB-04)*
Signatures are extracted from **source**, never from compiled assemblies, so it does not matter
whether EasySemVer runs before or after a build. The increment is relative to the persisted
baseline, not to build artifacts.

**INV-05 — GitHub Action.** ✅ *(was planned; specified as ACT-01…ACT-09 below)*
The third surface is a GitHub Action wrapping the CLI, at [`action.yml`](../action.yml). It is
built on REP-01…REP-08: the machine-readable verdict is what it turns into step outputs, so no
workflow ever has to scrape a log.

## The GitHub Action

**ACT-01 — A composite action at the repository root.** ✅
`action.yml` SHALL sit at the repository root, so that `uses: winterborn-llc/easysemver@<ref>`
resolves it with no subdirectory. It SHALL be a **composite** action. The deliverable is already a
self-contained native binary (PKG-02), so a Docker action would add an image pull and a JS action
would add a second language plus a committed `dist/` bundle to keep in sync with source — neither
buys anything here. Composite also runs on every runner OS, which a Docker action does not.

**ACT-02 — It runs the published binary, at a pinned version.** ✅
The Action SHALL obtain the tool by downloading the PKG-02 release archive for the runner's
platform, for the tag named by the `version` input. That input SHALL default to a specific tag and
SHALL NOT track the latest release: a workflow that silently changed behaviour when this repository
cut a release would make every consumer's build non-reproducible.
ℹ️ This is the reason the Action could not exist before a release did — the dependency runs from
the Action to the release, and it cannot be satisfied retroactively.

**ACT-03 — Platform coverage is exactly PKG-02's.** ✅
The Action SHALL resolve `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64` and `win-x64` from the
runner's reported OS and architecture, and SHALL fail by name on any other combination. Guessing a
default would surface as an exec-format error several steps later, in a place that does not name
the cause.

**ACT-04 — Inputs.** ✅
`folder` (default `.`), `dry-run` (default `false`), `commit` (default `false`), `tag` (default
`false`), `version`, and `token`. Every boolean input SHALL accept only `true` or `false` and SHALL
fail on anything else rather than treating it as false: a typo'd `dry-run: yes` that silently
stamped versions and rewrote the baseline would be a bad way to find out about the typo, and a
typo'd `commit: yes` that silently released nothing would be found days later by someone wondering
why a version on a package feed has no commit behind it.

**ACT-05 — Outputs come from the tool, never from the log.** ✅ *(revised: was the Action's own `jq`)*
The Action SHALL publish `version`, `old-version`, `change-type`, `dry-run`, `major`, `minor`,
`patch` and `report`. It SHALL obtain them by invoking the tool with `--github` (CLI-10) and SHALL
NOT parse the report itself: the Action is a caller of the CLI like any other, and a mapping it
re-implemented would be a second thing to keep correct. `change-type` is stated, never derived by
comparing the two versions (REP-06), and `dry-run` describes what happened rather than echoing the
input — both properties now hold by construction, because the values come from the run that
produced them.
ℹ️ `--github` explicitly rather than relying on CLI-10's `GITHUB_ACTIONS` detection. Inside a
composite action the tool is a step in someone else's workflow, and the outputs this Action promises
its callers should not depend on an environment variable being what we expect.

**ACT-06 — The Action acts only when asked.** ✅ *(revised 2026-08-09; was "does not act on it")*
The Action SHALL NOT commit, tag or push **unless a caller has explicitly asked it to**, and SHALL
NOT publish at all. Both `commit` and `tag` SHALL default to `false`.

The principle behind this requirement is that pushing a version bump to someone's repository is not
a decision a version calculator gets to make. That principle is satisfied by the *decision* being
the caller's, which is exactly what an opt-in input is. It was **not** the principle that the caller
must hand-write the mechanics of a decision they have already made — and reading it that way is what
left a dozen lines of `git` plumbing duplicated in every consuming workflow, each copy free to get
the staging, the ordering or the atomicity subtly wrong.

ℹ️ Publishing stays out, and not for symmetry: there is no single way to publish. A package feed, a
container registry and a release page share nothing an Action could encode, so there is nothing here
to get right. Committing and tagging are one mechanism with one correct implementation, which is the
difference.
ℹ️ The default is `false`, so no workflow written against the previous requirement changes behaviour.

**ACT-11 — `commit` and `tag`.** ✅ *(added 2026-08-09)*
With `commit: true` the Action SHALL stage exactly the files the run reported writing (REP-10),
commit them as `EasySemVer: <version>`, and push to the branch that triggered the workflow. With
`tag: true` it SHALL additionally create `v<version>` and push both refs **atomically**.

- `tag: true` without `commit: true` SHALL fail by name. A tag names a commit; on a caller's
  checkout `HEAD` is the commit *before* the bump, so tagging it would be quietly wrong.
- `commit: true` with `dry-run: true` SHALL fail by name. CLI-07 wrote nothing to commit, and
  succeeding silently would leave someone hunting for a release that never happened.
- Both SHALL accept only `true` or `false`, on ACT-04's reasoning.
- The committer identity SHALL default to `github-actions[bot]` and SHALL NOT overwrite one the
  caller has already configured — a workflow using a signing bot keeps it, and needs no input here
  to say so.

Staging comes from `writtenFiles` rather than from a path list or `git add -u`, which is the whole
reason REP-10 exists: a hand-written glob misses a project added one level deeper *silently*, and a
blanket stage sweeps in whatever else the job has touched. TST-05's integration step mutates the
working copy on purpose, so for this repository that second failure is not hypothetical.

`--atomic` is what makes CI-03's ordering guarantee real at the ref level. Without it a rejected
branch push still leaves the tag on the remote, pointing at a commit that never landed — which is
the state the "git before nuget.org" ordering exists to prevent, arrived at by a different route.

ℹ️ The branch is `$GITHUB_REF_NAME`, never a hardcoded `main`, and the push is `HEAD:<branch>`
because `actions/checkout` leaves a detached HEAD. Both are what let one block be copied into a
repository whose default branch is named something else (ACT-10).
ℹ️ A commit message template and a tag prefix were weighed and left out, on REP-05's reasoning: no
consumer for either exists yet, and an input can be added later without breaking one.

**ACT-12 — `report`: versioning and committing can be separated.** ✅ *(added 2026-08-09)*
Given `report: <path>`, the Action SHALL commit and tag the verdict in that report and SHALL NOT
version again: the install and run steps are skipped entirely. The verdict — version and `dryRun` —
SHALL be read from the report rather than from inputs, so the two forms cannot disagree.

This exists because **versioning and committing belong at different points in a pipeline**, and a
single step forces them together. The version must be stamped *before* the build, or the artifacts
carry the old one. The commit must not be pushed *until the tests pass*, or a failing test leaves a
release commit and a tag with no release behind them. Any pipeline that builds something between
the two needs both, and collapsing them would have made this Action unusable for the repository
that ships it — which is the strongest possible signal that the collapse was wrong.

```yaml
- uses: winterborn-llc/easysemver@<tag>            # stamp the version
  id: version

#   ...restore, build, test...

- uses: winterborn-llc/easysemver@<tag>            # release it, now that it is known good
  with:
    commit: true
    tag: true
    report: ${{ steps.version.outputs.report }}
```

ℹ️ Skipping the run is not an optimisation; it is the correctness property. A second invocation that
re-ran the tool would bump twice in one job, and the symptom — a minor number climbing two at a
time — points nowhere near the cause.
ℹ️ A caller who needs no build in between omits `report` and gets ACT-10's single step. Neither form
is the "real" one: they are the same three inputs, and which you want is decided by whether anything
has to happen between stamping a version and releasing it.

**ACT-13 — The manifest is validated by the run that releases it.** ✅ *(added 2026-08-09, after v16.0.0)*
No input description SHALL contain a `${{ }}` expression, and this repository's own workflow SHALL
reference the action as `uses: ./` rather than by tag.

Both halves come from one incident. **v16.0.0 shipped an `action.yml` carrying a worked example —
`report: ${{ steps.version.outputs.report }}` — inside the `report` input's *description*.** GitHub
evaluates expressions in descriptions, against a context where `steps` does not exist, so the
manifest failed to load with `Unrecognized named-value: 'steps'`. It failed at *Set up job*, before
any step ran, which meant:

- the release was unusable by every consumer, not merely wrong at the edges; and
- this repository could not cut the fix, because its own workflow pinned to the broken tag.

Neither is caught by anything below the runner. `ActionRegression` parses `action.yml` with
YamlDotNet, which validates YAML and knows nothing of GitHub expressions — the file is well-formed
by every measure available off a runner. This is ACT-09's boundary doing exactly what ACT-09 says it
does, and the answer is not a better parser:

- **`uses: ./`** means the manifest a run is about to publish is the manifest that run loaded. A
  manifest that cannot load can no longer be released, because the release fails first. It also
  removes the deadlock: a broken manifest is always recoverable in one commit.
- **No expressions in descriptions** is the blunt rule that makes the specific mistake unrepeatable
  without waiting for a runner to say so. Worked examples belong in the README.

ℹ️ The binary is still the published one — `version:` defaults to the pinned release (ACT-02), so
the dogfooding rule is untouched. `uses: ./` changes which *manifest* is loaded, not which *tool*
runs.
ℹ️ ACT-10 is weakened by exactly one line in exchange: the `uses:` line differs between the
documented block and the executed one, and the test normalises it. Everything after it — the inputs,
where the meaning is — still has to match byte for byte. That trade buys a class of unpublishable
bug, and ACT-02's pinned default is still asserted against the README's refs, so the tag a consumer
copies is still checked.
ℹ️ Output values are the opposite case and are asserted to *be* expressions over `steps`: that
context does exist there, and the rule above must not be over-applied to where it belongs.

**ACT-10 — One release block, identical in every repository.** ✅ *(added 2026-08-09; in force from v16.0.0)*
Versioning, committing and tagging a release SHALL be **one step**, documented in
[`README.md`](../README.md) as a block copied verbatim into any repository, and this repository's
own [`dotnet.yml`](../.github/workflows/dotnet.yml) SHALL run that exact text. A regression test
SHALL assert the two are identical, so the documented block cannot drift from the one exercised on
every push here.

```yaml
- name: Version, commit and tag
  id: version
  uses: winterborn-llc/easysemver@<tag>
  with:
    commit: true
    tag: true
```

That is the whole block where nothing has to happen in between. Where something does — this
repository builds and tests between stamping the version and releasing it — the block is ACT-12's
two invocations, and *that* pair is what `dotnet.yml` runs and what the test asserts, because it is
the form this repository can actually demonstrate.

Either way the size is the requirement rather than a happy result. Nothing in either form names a
path, a project, a branch or a language, so there is nothing to adapt on paste: ACT-11 supplies the
git mechanics, REP-10 the paths, `$GITHUB_REF_NAME` the branch. What stays repo-specific — the
build, the tests, the publish — sits outside it.

ℹ️ Anything a caller must still hand-write here is a defect in this requirement, not an exercise for
the caller. Every line of `git` in a consuming workflow is a line that can be subtly wrong in a way
that only shows up on a release: staged too little, tagged the wrong commit, pushed non-atomically.
ℹ️ This makes the tool's own pipeline evidence rather than an example. Documentation that is not
executed rots quietly, and the failure lands in someone else's workflow, on a copy-paste, in a
repository we never see.
ℹ️ The `uses:` ref here, the README's other examples and ACT-02's pinned `version` default SHALL name
one release, asserted by the same test. They move together when a tag is cut.
ℹ️ Adoption took two commits, and had to: ACT-02's Action runs a *published* binary, so a block
depending on CLI-10 and REP-10 could not land in the same commit as CLI-10 and REP-10 — the release
providing them did not exist yet. The first commit shipped the capability and its own run published
v16.0.0; the second pinned to it and switched `dotnet.yml` over. That was G-20, now closed. Expect
the same two-step shape for any future change that this repository's own release pipeline consumes.

**ACT-07 — A failed run fails the step, with nothing published.** ✅
Exit 1 (CLI-06) SHALL fail the step and stop the Action. No outputs are published, and REP-08
guarantees there is no report to read in that case, so no consumer ever sees a half-truth. The
verdict is deliberately **not** encoded in the exit code: a non-zero exit fails a workflow step, so
signalling "this change is Major" that way would be indistinguishable from the tool falling over.

**ACT-08 — Inputs reach the shell through the environment.** ✅
Input values SHALL NOT be interpolated into a `run:` block with `${{ }}`, which substitutes before
bash parses the script and would run a folder named `; rm -rf /` rather than name it. They are
passed as environment variables and quoted at the point of use.

**ACT-09 — What the Action's tests do and do not cover.** ℹ️
A GitHub Action cannot be run locally, so
[`ActionRegression`](../src/IntegrationTest/ActionRegression.cs) tests the parts that are testable:
it parses `action.yml`, asserts the wiring (ACT-01, ACT-05), checks the platform table against the
release job's own matrix so the two cannot drift (ACT-03), checks the documented release block
against the workflow that runs it (ACT-10), and executes the `run:` scripts extracted from the file
itself against a real tool with only the release download stubbed (ACT-04, ACT-07). What that
leaves untested is everything owned by the runner: the real download, `gh`'s authentication,
`$GITHUB_OUTPUT` actually becoming step outputs, and the Windows and cross-architecture paths — the
harness runs one platform, the host's. Those are covered only by running the Action for real.
ℹ️ The harness earns its keep: it caught an empty-bash-array expansion that tripped `set -u` under
bash 3.2, which is what a macOS runner's `/bin/bash` still is. An Ubuntu-only smoke test would
have passed straight over it.
