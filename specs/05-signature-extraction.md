# 05 — Signature Extraction (C#)

How the current API-surface signature is built from source.
Source: [`SolutionBuilder.cs`](../src/EasySemVer/CodeReader/SolutionBuilder.cs), with the data
model in [`src/EasySemVer/DataObject/`](../src/EasySemVer/DataObject) and contracts in
[`src/EasySemVer/Interfaces/`](../src/EasySemVer/Interfaces).

## The signature model

```
Solution                       (list of projects)
└── Project                    { Name }
    └── Class                  { Name = fully-qualified type name }
        ├── Property           { Name, Type (FQN), IsReadable, IsWritable }
        └── Method             { MethodName, MethodType (return type FQN) }
            └── Override       (one per overload: ordered parameter list)
                └── Input      { ParameterType (FQN), ParameterName, IsRequired }
```

Named collections ([`MethodList`](../src/EasySemVer/DataObject/MethodList.cs),
[`PropertyList`](../src/EasySemVer/DataObject/PropertyList.cs)) SHALL support lookup by name
(`Contains(name)`, indexer, `Keys`) and SHALL reject duplicate names on add.

## Extraction requirements

**SIG-01 — Parse source, compile ad-hoc.** ✅
Every discovered `.cs` file (DSC-06) SHALL be parsed with the Roslyn C# parser, and a
per-project ad-hoc `CSharpCompilation` created over the syntax trees with a minimal
metadata-reference set (the core runtime assemblies for `object`, `Enumerable`, `Console`).
The signature is then read from the compilation's symbol tree. A project with no `.cs` files
yields an empty project signature (logged, non-fatal).
ℹ️ Consequence of the minimal reference set: types defined in *other projects or NuGet
packages* resolve as error symbols, so their recorded names may be the short written name
rather than a namespace-qualified one. Names are stable run-to-run, so diffs still work, but
collisions between same-named types from different namespaces are theoretically possible.
(Gap **G-16**.)

**SIG-02 — Types in scope: public, namespace-level classes.** ✅
The signature SHALL include a type iff:
- it is declared `public`,
- it is a **class** (`TypeKind.Class`; records-as-classes qualify),
- it is a direct member of a namespace (nested types are not traversed), and
- its fully-qualified name does not match an excluded prefix (SIG-03).

Interfaces, structs, enums, and delegates are currently **out of scope** (marked `TODO` in
the code), as are events, fields, and nested types. Changes to those constructs are invisible
to versioning — see gap **G-15** for the SemVer consequences.

**SIG-03 — Namespace exclusions.** ✅
Types whose fully-qualified name starts with any of `Newtonsoft.`, `Microsoft.`,
`Coverlet.`, `System.`, `XUnit.` SHALL be excluded. (Guards against dependency/generated
symbols leaking into a project's own signature.)

**SIG-04 — Class identity = fully-qualified name.** ✅
A class is identified by its namespace-qualified name with the `global::` prefix stripped
(e.g. `Test.TestSelfSignature`). Verified by
[`TestSelfSignature`](../src/Test/TestSelfSignature.cs). Moving a class between namespaces is
therefore a remove + add (Major).

**SIG-05 — Property capture.** ✅
For each `public` property of an in-scope class the signature SHALL record: name, property
type (FQN), `IsReadable` (a get accessor exists), `IsWritable` (a set — including `init` —
accessor exists).
ℹ️ Accessor *accessibility* is not inspected: `public string X { private get; set; }`
records as readable. `init` accessors count as writable.

**SIG-06 — Method capture.** ✅
For each `public` method symbol of an in-scope class, excluding property `get`/`set`
accessors, the signature SHALL record the method (SIG-07). This deliberately includes
constructors (recorded under the symbol name `.ctor`) and operator overloads (`op_*`), so
breaking changes to construction and operators are versioned like any method change.
ℹ️ Inferred from Roslyn semantics, not covered by tests: implicit default constructors and
`add_`/`remove_` event accessors also satisfy these filters and enter the signature.

**SIG-07 — Overload grouping.** ⚠️
Methods SHALL be grouped by **name**; each overload contributes an `Override` entry holding
its ordered parameter list. The group's recorded return type (`MethodType`) is taken from the
*first overload encountered*. *Limitation:* overloads of the same name with different return
types cannot be represented distinctly, so a return-type change on a non-first overload can
go undetected. (Gap **G-14**.)

**SIG-08 — Parameter requiredness.** ✅
`IsRequired` SHALL be `false` iff the parameter is nullable-annotated (`T?`) **or** declares
an explicit default value; otherwise `true`.
ℹ️ Modifiers `ref`/`out`/`in`/`params`, and method modifiers
`virtual`/`abstract`/`override`/`sealed`/`static`, are not yet captured (code `TODO`s).

**SIG-09 — Canonical overload signature string.** ✅
Where an overload must be compared or looked up as a single value
([`GetMethodSignature`](../src/EasySemVer/Extensions/ExtendIMethodOverride.cs)), it SHALL be
rendered as the comma-joined list of `ParameterType ParameterName`, with **required**
parameters wrapped in square brackets, e.g. `[string input], System.Int32 count`.
ℹ️ Note this string *includes* requiredness while the overload-removal matcher (R02)
*ignores* it. That asymmetry is intentional: requiredness changes are classified by direction
in R17, not by R02 (CLS-06).

**SIG-10 — Determinism.** ✅ (implicit requirement)
Extraction SHALL be deterministic for identical source: same classes, members, ordering
semantics, and names on every run, since classification depends on exact string equality of
names and types. The current implementation satisfies this via Roslyn's stable symbol
enumeration; nothing may introduce run-dependent naming (timestamps, GUIDs, hash ordering)
into the model.
