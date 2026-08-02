# 05 — Signature Extraction

How each language's current API-surface signature is built. Sources:
[`CsharpUnitBuilder.cs`](../src/EasySemVer/CodeReader/Csharp/CsharpUnitBuilder.cs),
[`SymbolGraphReader.cs`](../src/EasySemVer/CodeReader/Swift/SymbolGraphReader.cs), with the data
models in [`DataObject/Csharp`](../src/EasySemVer/DataObject/Csharp) and
[`DataObject/Swift`](../src/EasySemVer/DataObject/Swift) and contracts in
[`Interfaces/Csharp`](../src/EasySemVer/Interfaces/Csharp) and
[`Interfaces/Swift`](../src/EasySemVer/Interfaces/Swift).

The two topologies are **not** mapped onto each other (ML-01, D-04). A Swift protocol is modelled
as a protocol, not as an interface; a C# record is modelled as a record. The only vocabulary they
share is the packageable unit, `Version` and `VersionType`.

## The C# signature model

```
CsharpProject                  { Name }
├── Classes[] / Interfaces[] / Structs[] / Records[] / Enums[] / Delegates[]
│     CsharpType               { Name (FQN), Kind, DeclaringType, IsStatic, IsAbstract,
│                                IsSealed, BaseType, ImplementedInterfaces[],
│                                GenericParameters[] { Name, Constraints } }
│     ├── Methods              { MethodName, MethodType }
│     │     └── Overrides[]    { ReturnType, IsStatic, IsVirtual, IsAbstract, IsOverride,
│     │                          IsSealed, HasDefaultImplementation, GenericParameters[] }
│     │           └── Parameters[] { ParameterName, ParameterType, IsRequired, RefKind, IsParams }
│     ├── Properties           { Name, Type, IsReadable, IsWritable, IsInitOnly, IsStatic,
│     │                          IsRequired, HasDefaultImplementation }
│     ├── Fields               { Name, Type, IsStatic, IsReadOnly, IsConstant }
│     └── Events               { Name, HandlerType, IsStatic }
├── (records additionally)     PositionalParameters[], IsValueType
├── (enums additionally)       UnderlyingType, Members[] { Name, Value }
└── (delegates additionally)   ReturnType, Parameters[]
```

Named collections ([`CsharpMethodList`](../src/EasySemVer/DataObject/Csharp/CsharpMethodList.cs),
[`CsharpPropertyList`](../src/EasySemVer/DataObject/Csharp/CsharpPropertyList.cs)) SHALL support
lookup by name (`Contains(name)`, indexer, `Keys`). They scan rather than caching a dictionary,
because a cache built in `Add` would go stale when the serializer populates the list through its
base class.

## The Swift signature model

```
SwiftModule                    { Name }
├── Classes[] / Structs[] / Actors[] / Enums[] / Protocols[]
│     SwiftType                { Name, Kind, AccessLevel (public|open), IsFinal, IsFrozen,
│                                Superclass, Conformances[], GenericParameters[],
│                                Availability[], ObjCExposure }
│     ├── Initializers[]       { Name, Parameters[], IsFailable, IsRequired, IsConvenience,
│     │                          IsAsync, Throws }
│     ├── Functions[]          { Name (with argument labels), Parameters[], ReturnType, IsStatic,
│     │                          IsMutating, IsAsync, Throws, IsFinal, GenericParameters[],
│     │                          HasDefaultImplementation, ExtensionConstraints }
│     ├── Properties[]         { Name, Type, IsSettable, IsStatic, IsMutating, IsAsync, Throws }
│     └── Subscripts[]         { Name, Parameters[], ReturnType, IsSettable, IsStatic }
├── (enums additionally)       RawValueType, Cases[] { Name, AssociatedValues[], RawValue }
├── (protocols additionally)   AssociatedTypes[]
├── Extensions[]               { ExtendedType, Constraints, AddedConformances[], members… }
├── GlobalFunctions[], GlobalVariables[], TypeAliases[], Operators[]
└── SwiftParameter             { Label, InternalName, Type, HasDefault, IsInout, IsVariadic,
                                 Ownership }
```

## C# extraction requirements

**SIG-01 — Parse source, compile ad-hoc.** ✅
Every discovered `.cs` file (DSC-06) SHALL be parsed with the Roslyn C# parser, and a per-project
ad-hoc `CSharpCompilation` created over the syntax trees with a minimal metadata-reference set
(the core runtime assemblies for `object`, `Enumerable`, `Console`). The signature is read from
**the source assembly's** symbol tree, not the compilation's merged global namespace — walking
the merged namespace pulled public types out of referenced assemblies into the signature. A
project with no `.cs` files yields an empty project signature (logged, non-fatal).
ℹ️ Consequence of the minimal reference set: types defined in *other projects or NuGet packages*
resolve as error symbols, so their recorded names may be the short written name rather than a
namespace-qualified one. Names are stable run-to-run, so diffs still work, but collisions between
same-named types from different namespaces are theoretically possible. (Gap **G-16**.)

**SIG-02 — Types in scope.** ✅ *(replaced by CSX-01; G-15 resolved)*
The signature SHALL include a type iff it is declared `public`, is a namespace-level **or public
nested** member, and its fully-qualified name does not match an excluded prefix (SIG-03). Every
kind is in scope and modelled as its own concept: class, interface, struct, record, record
struct, enum, delegate. Nested types are recorded flat under their `Outer.Inner` name, carrying
the name of their declaring type.

**SIG-03 — Namespace exclusions.** ✅
Types whose fully-qualified name starts with any of `Newtonsoft.`, `Microsoft.`, `Coverlet.`,
`System.`, `XUnit.` SHALL be excluded.

**SIG-04 — Type identity = fully-qualified name.** ✅
A type is identified by its namespace-qualified name with the `global::` prefix stripped. Moving
a type between namespaces is therefore a remove + add (Major). Pairing is by (name, kind), so a
struct that becomes a class also reads as remove + add.

**SIG-05 — Property capture.** ✅ *(extended by CSX-02/CSX-03)*
For each `public` property the signature SHALL record: name, type (FQN), `IsReadable`,
`IsWritable`, **`IsInitOnly`**, **`IsStatic`**, **`IsRequired`**, and whether an interface member
carries a default implementation.
ℹ️ Accessor *accessibility* is still not inspected: `public string X { private get; set; }`
records as readable. `init` counts as writable **and** sets `IsInitOnly`, which is what makes
`set` → `init` detectable (R42).

**SIG-06 — Method capture.** ✅
For each `public` method symbol, excluding property `get`/`set` accessors, the signature SHALL
record the method. This deliberately includes constructors (`.ctor`), operator overloads (`op_*`),
indexer accessors and event accessors.

**SIG-07 — Overload grouping.** ✅
Methods SHALL be grouped by **name**; each overload contributes an entry holding its ordered
parameter list and its own facets.

**SIG-08 — Parameter requiredness.** ✅
`IsRequired` SHALL be `false` iff the parameter is nullable-annotated (`T?`) **or** declares an
explicit default value; otherwise `true`. Modifiers `ref`/`out`/`in`/`params` are captured
separately (CSX-03) and drive R37.

**SIG-09 — Canonical overload signature string.** ✅
Where an overload must be compared or looked up as a single value
([`GetMethodSignature`](../src/EasySemVer/Extensions/ExtendICsharpMethodOverride.cs)), it SHALL be
rendered as the comma-joined list of `ParameterType ParameterName`, with **required** parameters
wrapped in square brackets, e.g. `[string input], System.Int32 count`.
ℹ️ This string *includes* requiredness while the overload-removal matcher (R02) *ignores* it;
that asymmetry is intentional (CLS-06).

**SIG-10 — Determinism.** ✅
Extraction SHALL be deterministic for identical source, and every collection SHALL be sorted by
identity before persistence (BAS-04).

**SIG-11 — Per-overload return type.** ✅ *(CSX-04; G-14 resolved)*
Return type SHALL be recorded **per overload** as well as per method name. A return-type change
on any overload is detectable; R03's first-overload limitation is gone.

## Swift extraction requirements

**SIG-20 — The symbol graph is the only source.** ✅ *(SWE-01, D-02)*
Swift signatures SHALL be read from the toolchain's symbol-graph JSON, produced by `swift build`
(or `xcodebuild` for Xcode targets) with `-emit-symbol-graph`,
`-emit-symbol-graph-dir`, `-emit-extension-block-symbols` and
`-symbol-graph-minimum-access-level public`. There is no hand-rolled Swift parser.

**SIG-21 — Access-level filter.** ✅ *(SWE-02)*
Only `public` and `open` declarations enter the signature. The filter is applied to the parsed
graph, not trusted from the flag. A declaration dropping to `internal` therefore surfaces as a
removal (Major) via S01/S16, which is correct.

**SIG-22 — Identity is declaration-derived.** ✅ *(SWE-03)*
Identity SHALL be the symbol's `pathComponents` joined with dots and, for functions, the full
Swift name **including argument labels** (`Gadget.move(to:animated:)`). The mangled precise
identifier joins relationships during parsing and is never persisted: mangling schemes change
between toolchain versions and would churn the baseline.

**SIG-23 — Synthesized members are excluded.** ✅
Symbols whose precise identifier carries `::SYNTHESIZED::` SHALL be dropped. Those are members
the compiler derives from a protocol conformance — `Equatable.!=`, `Hashable.hashValue`, actor
plumbing — and keeping them would make a toolchain upgrade look like an API change.

**SIG-24 — Extensions.** ✅ *(SWM-02)*
Members an extension adds to a type declared in the **same** module SHALL be folded into that
type, tagged with the extension's constraints. Extensions on types from **other** modules SHALL
be recorded as their own entities keyed by extended type plus constraints.

**SIG-25 — Determinism over toolchain output.** ✅ *(SWE-04)*
Only modelled fields are persisted; raw JSON is never stored. All collections are sorted by
identity before persistence. Nothing toolchain-version-dependent — mangled names, symbol
ordering, source locations, doc comments — enters the file.

**SIG-26 — Extraction failure is fatal.** ✅ *(SWE-05, D-03)*
If Swift units are discovered and `swift`/`xcodebuild` is missing, exits non-zero, times out, or
produces no graph for a discovered target, the run SHALL fail with exit 1. The message names the
unit, the exact command, and the tool's stderr. No baseline is written, no version is stamped,
the working tree is untouched.
ℹ️ One deliberate exception: a discovered **Xcode** target that produces no Swift graph at all is
treated as version-sync-only and logged, rather than failing the run — a pure Objective-C target
legitimately has no Swift symbol graph (§20 O-06).

**SIG-27 — Temp artifacts.** ✅ *(SWE-06)*
Symbol graphs SHALL be emitted to a temporary directory outside the folder root and cleaned up
afterwards, so extraction never dirties the user's tree or feeds its own output back into
discovery.
