# Conversion Strategies — Technical Reference

> **📖 This is the exhaustive technical reference.** For a shorter, example-driven overview of how
> each Go construct maps to C#, start with **[`ConversionStrategies.md`](ConversionStrategies.md)** —
> every section there links back here for the full detail. Read this document when you need the
> *why*: the exact emitted form, the edge cases, the Phase-3 fixes, the behavioral-test guards, and
> the C#-vs-Go semantic reasoning behind a decision. It is the authoritative record; the summary is
> the front door.

> **Updated 2026-06-27 for the "go2cs2" generation of the converter.** This is a living
> document; as more use cases are converted these strategies are refined. The current converter
> is written in **Go** (using the official `go/ast` + `go/types` toolchain, under `src/go2cs/`)
> and emits C# that leans on two things the visible code does not show in full: a hand-written
> runtime library, **`golib`** (`src/core/golib/`), and a set of **[Roslyn](Glossary.md#roslyn) source generators**
> (`src/gen/go2cs-gen/`) that synthesize the Go semantics which cannot be written directly in C#.
> Notes that previously referenced the retired ANTLR4/C# converter or the old `gocore` library
> have been updated to reflect this. See also: [`Architecture.md`](Architecture.md),
> [`Glossary.md`](Glossary.md), [`Roadmap.md`](Roadmap.md), and [`CLAUDE.md`](../CLAUDE.md).

The guiding goal: the generated C# should be both *behaviorally* and *visually* similar to the
original Go, so that a Go developer can read the output and follow it. The runtime library and
the generators exist to keep the visible converted code close to the Go original.

> **How this reference is organized.** Each `##` topic opens with the high-level rule (the same
> ground the summary covers) and is then followed by `###` subsections documenting specific
> conversion decisions, edge cases, and fixes — most keyed to the [behavioral test](Glossary.md#guard) that guards them.
> When updating the converter, add the deep detail here and the reader-facing example to the
> summary (see [`../CLAUDE.md`](../CLAUDE.md), "Record the conversion decision").

## Topics

* [Package Conversion](#package-conversion)
* [Package-Level Variable Initialization Order](#package-level-variable-initialization-order)
* [Compiled Library versus Source Code](#compiled-library-versus-source-code)
* [Constant Values](#constant-values)
* [Handling "int" and "uint" Types](#handling-int-and-uint-types)
* [Untyped Constants and Named Numeric Types](#untyped-constants-and-named-numeric-types)
* [The "nil" Value](#the-nil-value)
* [Empty Interface](#empty-interface)
* [Inline Assignment Order of Operations](#inline-assignment-order-of-operations)
* [Short Variable Redeclaration (Shadowing)](#short-variable-redeclaration-shadowing)
* [Return Tuples](#return-tuples)
* [Slices and Arrays](#slices-and-arrays)
* [Maps and Channels](#maps-and-channels)
* [Type Aliasing](#type-aliasing)
* [Delegates to Value Receiver Instances](#delegates-to-value-receiver-instances)
* [Defer / Panic / Recover](#defer--panic--recover)
* [Expression Switch Statements](#expression-switch-statements)
* [Type Switch Statements](#type-switch-statements)
* [Struct Types](#struct-types)
* [Struct Type Embedding](#struct-type-embedding)
* [Interfaces](#interfaces)
* [Pointers](#pointers)
* [Implicit Pointer Dereferencing](#implicit-pointer-dereferencing)
* [Break / Continue Labels](#break--continue-labels)
* [Source Generators](#source-generators)
* [Manually-Converted Declarations (managed-referent pointers)](#manually-converted-declarations-managed-referent-pointers)
* [Deterministic Output](#deterministic-output)
* [Examples](#examples)

## Package Conversion
Although a Go package more traditionally parallels a C# namespace, Go includes referenceable functions directly from within a package root, for example, the `Println` function in the `fmt` package is called like: `fmt.Println("Hello, world")`. For C#, only type declarations, e.g., `class`, `struct`, `enum`, etc., are allowed in a namespace; functions exist as part of a `class` or `struct`. Described from a C# perspective, all Go functions are [`static`](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members), i.e., the functions exist separately from an instance of a type. Go supports the notion of a receiver function which allows a function to be targeted to an instance of a type (paralleling the operation of a C# extension function), but this is still a static function.

As such, the conversion strategy for a Go package is to convert it into a static C# partial class, e.g.: `public static partial class fmt_package`. Using a partial class allows all functions within separate files to be available with a single import, e.g.: `using fmt = go.fmt_package;`. The receiver functions are emitted as extension methods on that partial class (decorated with `[GoRecv]`, see [Source Generators](#source-generators)).

So that Go packages are more readily usable in C# applications, all converted code is in a root `go` namespace. Package paths are simply converted to namespaces, so a Go import like `import "unicode/utf8"` becomes a C# using like `using utf8 = go.unicode.utf8_package;`. Each package also emits a `package_info.cs` carrying a `[GoPackage]` assembly attribute plus the package-wide global `using` aliases (Go's built-in types, exported type aliases, etc.).

A consequence of converting a Go method to a C# **extension method** is that C# only discovers an extension method when its containing static class's *namespace* is in scope (via a `using <namespace>;` directive or the enclosing namespace) — a class **alias** such as `using atomic = go.@internal.runtime.atomic_package;` resolves the *type* (`atomic.Uint32`) but does **not** bring the class's extension methods into scope. This matters when a file calls a method on a value whose type comes from a multi-segment-path package (one that lands in a sub-namespace, e.g. `internal/runtime/atomic` → `go.@internal.runtime`): Go never requires importing a value's package merely to call a method on it, so such a file may emit no import — and hence no `using @internal.runtime;` — leaving the extension method invisible and the call mis-binding to a wrong (e.g. embedding-promoted) overload (CS1929). The converter therefore registers the namespace of **every cross-package method's defining package** as a file-local `using` at the call site, independent of the file's explicit imports. (Packages in the root `go` namespace — most top-level stdlib packages — need nothing extra, since same-namespace extension methods are always visible. This is a stdlib-structural concern that only surfaces under multi-segment package paths, so it is guarded by the Phase-3 `runtime` build rather than a single-package behavioral test.)

Go projects that contain a `main` function are converted into a standard C# executable project, i.e., `<OutputType>Exe</OutputType>`. The conversion process can reference and convert needed external projects as library projects, i.e., `<OutputType>Library</OutputType>`, per any encountered `import` statements. In this manner an executable with packages compiled as project-referenced assemblies can be created. To create a single executable, like the original Go counterpart, a [self-contained executable](https://docs.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained) can be produced.

### Cross-package imports (importing another package / assembly)

When a package imports another and uses its exported surface, the converter must agree, on both the **producer** side (converting the imported package) and the **consumer** side (resolving the `import`), on the imported package's C# `(namespace, class)` and emit a `ProjectReference` to its generated `.csproj`. The package class is `<packageName>_package` and the namespace is the root `go` plus the import path's leading segments, so the two sides line up when the Go package name equals the import path's last segment (the usual layout: `import "x/y/barlib"` → package `barlib` → `go.x.y.barlib_package`). The consumer emits `using barlib = …barlib_package;` and references members as `barlib.Thing`.

Resolving *where* an imported package lives is **module-aware**: the standard library is found under `GOROOT` and mapped to `$(go2csPath)core\<pkg>`, but a **local/user module** (reached via a `go.mod` `replace`, or simply co-located in the same module tree) is invisible to the legacy `go/build` GOPATH resolver. The converter therefore falls back to the dependency directory captured from the module-aware `go/packages` load, treats that package's converted output as **in-place** (co-located with its Go source), and emits a `ProjectReference` **relative to the referencing project** (e.g. `..\barlib\barlib.csproj`) so the generated `.csproj` is portable. This is what makes "import a sibling package and use it" compile as separate assemblies.

**Exported type aliases cross packages.** A package-level Go type alias that is exported — `type Temperature = Celsius` — is recorded in that package's `package_info.cs` as an assembly attribute in its `<ExportedTypeAliases>` block:

```csharp
// <ExportedTypeAliases>
[assembly: GoTypeAlias("Temperature", "go.CrossPkgLib_package.Celsius")]
// </ExportedTypeAliases>
```

A consumer that imports the package and names `CrossPkgLib.Temperature` cannot use a C# member-access for it (C# has no namespace-level type alias). Instead, the converter parses the imported package's `package_info.cs`, reads its `[GoTypeAlias]` attributes, and emits a corresponding `global using` into the consumer's own `<ImportedTypeAliases>` block — keyed by a package-qualified name whose `.` separator is the extended-Unicode dot `ꓸ` (`ꓸ`, a valid C# identifier character), since `CrossPkgLib.Temperature` is not a legal C# identifier:

```csharp
// <ImportedTypeAliases>
global using CrossPkgLibꓸTemperature = go.CrossPkgLib_package.Celsius;
// </ImportedTypeAliases>
```

The consumer's converted code then refers to the alias as `CrossPkgLibꓸTemperature`. (This round-trip depends on the module-aware resolution above to locate the imported package's `package_info.cs`; a stdlib dependency is found under the `core` output tree, a local module via its `go/packages` directory.) Guarded by the `CrossPkgLib`/`CrossPkgUser` cross-package behavioral test pair.

**A collision-renamed alias chain resolves to its concrete target.** When an exported type's name *collides* with a method name it is `Δ`-renamed (see [Type-vs-Method Name Collisions](#type-vs-method-name-collisions)); and when that type is *also* an empty interface — `type Token any` colliding with a `Token()` method, encoding/json's shape — the producer's `package_info.cs` carries a **two-hop chain**. The collision analysis records `Token → ΔToken`, and `visitTypeSpec` (which renders an empty-interface target as `object`) records the renamed declaration `ΔToken → object`:

```csharp
[assembly: GoTypeAlias("Token", "ΔToken")]
[assembly: GoTypeAlias("ΔToken", "object")]
```

A consumer that resolves only the FIRST hop and then qualifies the intermediate `Δ`-name as a package member emits `global using jsonꓸToken = go.encoding.json_package.ΔToken;` — but `ΔToken` is an assembly-scoped `global using`, **not** a namespace member of `json_package`, so it is CS0426 (encoding/json's `Token` consumed by html/template, internal/coverage/cfile, expvar, log/slog, internal/fuzz, …). The imported-alias loader (`loadImportedTypeAliases`) therefore follows the chain within the producer's OWN exported aliases to its **concrete** target, emitting `global using jsonꓸToken = object;`. A chain whose final target is a real `Δ`-renamed member (a delegate/struct such as `ΔFilter`, which is *not* itself an exported alias) stops there and stays package-qualified, unchanged. Guarded by the `CrossPkgLib`/`CrossPkgUser` pair — an empty-interface `Token` colliding with a `Sensor.Token()` method, named as a `var` type in the consumer and its boxed value read back, output-compared vs Go.

**A same-named cross-package alias target is fully qualified.** Two *different* packages can share a Go package name — html/template and text/template are both `package template`. When such a package aliases the other's type — html/template's `type FuncMap = template.FuncMap`, whose target lives in text/template — the alias RHS must name the target's OWN `(namespace, class)`: `go.text.template_package.FuncMap`. `getFullTypeName` had gated its cross-package branch on the package *name* (`pkg.Name() != packageName`), so a same-named foreign type read as *same-package* and fell through to the `t.String()` path, whose cross-package slash-strip drops BOTH the `text` path segment AND the `_package` class — emitting `global using FuncMap = go.template.FuncMap;` (CS0234; `template` is not a namespace of `go`). The check now compares package **identity** (`pkg != v.pkg`), matching `getTypeName` and `collectCrossPackagePaths`, so the branch fires and the target fully qualifies. (A code-*body* reference already rendered correctly — `getTypeName` keyed on identity — so only the `global using` alias RHS was wrong.) Guarded by the `CrossPkgSameNameAlias` behavioral test: a `package atomic` that aliases the same-named `sync/atomic`'s `Int32` (`type Int32 = atomic.Int32`), whose `global using` RHS must render `go.sync.atomic_package.Int32`, not the dropped-segment `go.atomic.Int32`.

**Exported structs and interfaces cross packages.** An exported struct's fields and methods are reachable on the consumer side exactly as the producer emits them — `CrossPkgLib.Sensor{Name: …, Temp: …}` lowers to a C# constructor call and `s.Name` / `s.Hot()` to field/method access on the imported struct — because the struct and its `[GoRecv]` extension methods live in the (referenced) library assembly.

A cross-package **interface satisfaction** is subtler. Go is structurally typed, so a consumer may assign any value with the right method set to an interface; C# requires the *nominal* `partial struct T : I` implementation glue, which the [`ImplementGenerator`](#source-generators) can only add to `T` **in T's own assembly** (`isLocalImplType`). The converter records a `[assembly: GoImplement<T, I>]` for each concrete→interface conversion it *witnesses while converting T's package* — so for a consumer to use `Sensor` as `Labeled` across the assembly boundary, the satisfaction must be witnessed in the **library** that declares `Sensor`. The idiomatic Go interface-satisfaction assertion does exactly this:

```go
var _ Labeled = Sensor{}   // in CrossPkgLib — records GoImplement<Sensor, Labeled> in this assembly
```

With that, the library emits `[assembly: GoImplement<Sensor, Labeled>]`, `Sensor : Labeled` is realized in the library assembly, and a consumer's `var l CrossPkgLib.Labeled = s` / `CrossPkgLib.Describe(s)` compile as ordinary upcasts. (A library that returns the interface from a constructor — `func New(...) Labeled { return Sensor{…} }` — witnesses it the same way.) A type that satisfies an interface but is *never* used as it within its own package is not yet auto-realized cross-package; proactively recording every local concrete→local interface structural match is a possible future enhancement, weighed against the extra generated glue it would add to every package. Also guarded by the `CrossPkgLib`/`CrossPkgUser` pair (Phase 3: struct field access + interface satisfaction).

#### A sub-package import whose leading segment is a package alias root-qualifies
When a package imports both a parent package and its sub-package — testing/fstest importing `io` **and** `io/fs` — the converter emits `using io = io_package;` (a **type** alias for the io package class) and, for io/fs, a **relative** namespace target `io.fs_package`. In C# the leading `io` segment of `io.fs_package` binds to that type alias, so `io.fs_package[.FS]` resolves to the nonexistent nested type `io_package.fs_package[.FS]` — CS0426 (the `using fs = …` alias line, the embedded `fs.FS` getter, and the generated `TypeGenerator` copies). The converter records every direct-import using-alias identifier bound in the package and prefixes `go.` onto any multi-segment relative namespace/type whose leading segment is one of them, so the segment resolves as the child **namespace** it names:

```go
import (
    "io"
    "io/fs"
)
type fsOnly struct{ fs.FS }
```
```csharp
using fs = go.io.fs_package;
public go.io.fs_package.FS FS;
```

The unqualified `io.fs_package.FS` is retained as the `promotedInterfaceImplementations` map **key** (which feeds alias-less generator files where the relative form resolves). This complements the existing alias-vs-child-namespace Δ-rename, which only catches `<currentNS>.<alias>` collisions. (Recurs for any parent+sub-package import pair; a behavioral guard is owed — the io/fs embedded-interface pattern needs a parent+sub-package pair absent from the core baseline stdlib.)

The same collision detection must see GOROOT-VENDORED namespaces. `visitImportSpec` resolves a GOROOT package's `golang.org/x/…` import to its on-disk `vendor/…` path (and namespace) when the importing file lives under GOROOT, but `computeImportAliasRenames` built `packageChildNamespaces` from the raw `imp.Path()` — so a vendored sub-namespace like `go.vendor.golang.org.x.text.unicode` was absent from the map. `rootQualifyIfAmbiguous` then could not see that a stdlib alias's leading segment collides with it: bidirule (at `vendor/golang.org/x/text/secure/bidirule`, importing both `unicode/utf8` and the vendored `golang.org/x/text/unicode/bidi`) emitted `using utf8 = unicode.utf8_package;`, whose `unicode` bound to the in-scope vendored `unicode` namespace rather than stdlib `go.unicode` (CS0234). `computeImportAliasRenames` now applies `resolveGorootVendoredPath` to each closure path when the package lives under GOROOT — matching the emission — so the vendored namespaces populate the map and the alias root-qualifies to `go.unicode.utf8_package`. Gated on GOROOT so a user module's own `golang.org/x` dependency is untouched; **guard owed** (the fix fires only for a GOROOT-vendored package, which the behavioral harness — never under GOROOT — cannot express; validated by the bidirule reconvert [A/B](Glossary.md#ab)).

**A DOTTED build tag (`goexperiment.X`, `amd64.vN`) is matched against the host toolchain's tool tags.** The converter re-checks each file's `//go:build` constraint after `go/packages` has already loaded it (to drop files for the wrong GOOS/GOARCH when converting cross-platform). Its evaluator only handled bare identifiers (`linux`, `amd64`), so a *dotted* tag parsed as a selector and fell through to `false`. That is wrong for an experiment enabled BY DEFAULT: `coverageredesign`, `regabiwrappers`, and `regabiargs` are in the host's `go/build` `ToolTags`, so `go/packages` loaded their `//go:build goexperiment.X` `_on.go` files — but the re-check then re-EXCLUDED them (the selector → `false`), dropping the package-level consts (`testing`'s `goexperiment.CoverageRedesign`, CS0117 ×4). The evaluator now resolves a `*ast.SelectorExpr` tag by membership in `build.Default.ToolTags` — so an enabled experiment's `_on.go` survives and a disabled one's `!goexperiment.X` `_off.go` survives, exactly the one `go/packages` chose. Blast radius is only `internal/goexperiment` (the sole stdlib package whose file selection flipped). **Guard owed** — the fix depends on the host toolchain's active tool tags, which the `go2cs/*` behavioral harness cannot express portably; validated by the reconvert A/B (only `internal/goexperiment` changes) and the [census](Glossary.md#census) (the consts appear, `testing`'s CS0117 clear).

### Standard-library solution file (`.slnx`)

A whole-standard-library run (`go2cs -stdlib`) also emits a Visual Studio solution — **`go-src-converted.slnx`** — at the output root (`-go2cspath`), so the freshly converted stdlib is openable / buildable as **one unit** immediately after a run, rather than depending on a hand-maintained solution that drifts. It is the auto-generated counterpart of the committed `src/go-src-converted.slnx`, and its XML mirrors the format of `src/go2cs.slnx` (a `<Configurations>` block plus `<Folder>`/`<Project>` entries, CRLF line endings, no BOM). It references:

* every converted stdlib project it emitted (`core/<pkg>/<projectName>.csproj`, grouped under a `/core/` folder),
* any per-package **test** projects (`*_test.csproj`, grouped under a `/tests/` folder — inert until Phase 4 emits them, and the folder is omitted entirely when there are none),
* the shared **`golib`** runtime (`core/golib/golib.csproj`), and
* the **`go2cs-gen`** source-generator/analyzer project (`gen/go2cs-gen/go2cs-gen.csproj`, under a `/generators/` folder).

The stdlib project list is gathered by walking the emitted `core/` output tree (so it also picks up future test projects with no code change), and every project is emitted in stable **alphabetical** order for deterministic output. All paths are **solution-relative** (forward slashes) so the generated solution is portable — no absolute, machine-specific paths. The `golib` and `go2cs-gen` references use the same `core\golib` / `gen\go2cs-gen` layout the converted `.csproj` files already assume via `$(go2csPath)` (which resolves to `$(SolutionDir)`), so the solution locates them wherever those csproj references already resolve. The file is only rewritten when its content changes, so repeated runs are a no-op.

## Package-Level Variable Initialization Order

Go initializes package-level variables in **dependency order** (spec: "within a package, package-level variable initialization proceeds stepwise, each step selecting the earliest variable … that has no dependencies on uninitialized variables"), where dependencies are resolved **through function calls and function literals** referenced by the initializer. The default conversion emits a package var as a C# static field with an initializer — but C# executes static field initializers in textual order **within** one file of the partial package class and in an **undefined** order **across** files. Three dependency shapes therefore break at runtime while compiling cleanly (the Phase-3 → Phase-4 distinction in miniature):

1. **Cross-file:** syscall's `var procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx")` (syscall_windows.go) reads `modkernel32` declared in zsyscall_windows.go — with the wrong file order the receiver box is nil (NullReference in the type initializer, the first Phase-4 crash of any program importing `os`).
2. **Cross-file through a function:** syscall's `var Stdin = getStdHandle(STD_INPUT_HANDLE)` — the initializer calls a package *function* whose body reads zsyscall_windows.go's `procGetStdHandle`. Same failure, invisible to a direct-reference scan; dependencies must be resolved transitively through same-package function bodies (and func-literal bodies — an IIFE initializer executes at init time), exactly as Go's own analysis does.
3. **Same-file forward reference:** `var first = base + 1` declared above `var base = 41` — C# reads `base`'s zero value (silently wrong value, no crash).

**The conversion:** `collectMovedInitVars` (converter: `initOrderOperations.go`) walks `types.Info.InitOrder` — Go's authoritative dependency-sorted initializer list — resolving each initializer's same-package var dependencies transitively through package function/method bodies. An initializer moves when a dependency is cross-file, a same-file forward reference, or **itself moved** (a moved var is only assigned in the ctor, so its dependents can no longer stay field initializers regardless of layout). A moved var is emitted as a **bare field** plus a tiny **init method beside it in its home file** (so the rendered expression keeps that file's using aliases), and a generated `package_init.cs` supplies the package class's **static constructor**, calling the methods in InitOrder:

```go
// syscall_windows.go
var procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx")
```

```csharp
// syscall_windows.cs
internal static ж<LazyProc> procSetFilePointerEx;
internal static void initᴛprocSetFilePointerEx() { procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx"u8); }

// package_init.cs (generated)
partial class syscall_package {
    static syscall_package() {
        initᴛprocSetFilePointerEx();
        // … every relocated initializer, in types.Info.InitOrder …
    }
}
```

This is correct by C#'s own initialization guarantees: **all** static field initializers (every partial-class file) run **before** the static-constructor body, so every non-relocated dependency is already initialized when the ctor runs; the ctor then applies the relocated initializers in Go's order. Vars with no order hazard (the overwhelming majority — only ~11 stdlib packages relocate anything) keep their readable inline form. Cross-**package** order needs no handling: accessing another package's static field triggers that type's initialization first (.NET guarantees), matching Go's imported-packages-first rule. Adding an explicit static ctor also removes `beforefieldinit` from the package class, giving it *precise* initialization semantics.

Notes: a **blank** (`_`) initializer never relocates (its value is unreadable, so its order is immaterial — it still runs as a field initializer for its side effect); the `initᴛ` method name composes the TempVarMarker so it cannot collide with any Go identifier; an **addressed** global relocates as a default-valued heap box whose ctor assignment writes through the ref property into the same box; the rare multi-value forms (tuple-deconstructing package vars, hoisted multi-value initializers) are not yet relocatable and warn loudly if flagged (no stdlib occurrence). Guarded by the `PackageVarInitOrder` behavioral test (all three hazard shapes plus IIFE and moved-dependency closure, output-compared vs Go).

## Compiled Library versus Source Code

One big difference between Go and many other languages is the notion of _source availability_. Traditionally programming languages have depended on using a pre-compiled library — both to avoid recompiling the library and to protect source as intellectual property. Go was born in an era of faster computing and prolific open source; it relies on having access to all source at compile time, including library code. Go takes advantage of this to make interesting optimizations, especially around when a structure escapes the stack to the heap. Keeping structures off the heap means they do not need to be tracked for garbage collection, and the Go compiler manages this automatically. The interesting consequence is that, for a given use of a library as source, an application structure may or may not escape to the heap depending on how it flows through the code — an optimization only possible when all source is compiled together.

Because this is a complex optimization, the converter currently assumes structures can escape to the heap except in the simplest-to-detect cases (see [Pointers](#pointers)). A future option could distinguish optimizations targeted at a compiled library (very safe escape analysis) versus a standalone application (more aggressive). A longer-term plan is to allow already-converted packages to be referenced as compiled libraries (e.g., from NuGet), which fits the consumption model most C# developers are accustomed to; this requires a mapping from original Go package to published package reference plus an embedded manifest of each package's exported type aliases.

## Constant Values
Go constants hold arbitrary-precision literals with expression support, and assignment of a constant to a variable happens at compile time. The converter preserves the constant value (and, in a comment, the original expression). A *typed* Go constant is emitted with its concrete C# type, e.g.:

```csharp
public const nint MaxRetries = 3;
```

An *untyped* Go constant is emitted using a golib "untyped" wrapper type — `UntypedInt`, `UntypedFloat`, or `UntypedComplex` — declared `static readonly` so it can hold a value that does not fit a single primitive and can implicitly adapt to whatever numeric type its use site requires (mirroring how an untyped Go constant takes its type from context):

```csharp
internal static readonly UntypedInt win = 100;
public static readonly UntypedInt N = /* 11 + 1 */ 12;
```

A Go untyped *float* constant defaults to `float64`, so its C# literal carries the double suffix `D` — not `F` — regardless of whether the value happens to fit in `float32`. (Emitting `F` whenever the value fit would make `z := 1.0` a `float`, breaking later `float64` arithmetic with CS0266.) A literal in an explicit `float32` context keeps `F`:

```go
z := 1.0           // untyped float -> float64
var f float32 = 2.5 // float32 context
```
```csharp
var z = 1.0D;
float32 f = 2.5F;
```

A native-sized integer constant (`nint`/`nuint`, including the `uintptr` alias) whose value does not fit a C# constant of that type — e.g. `const MaxUintptr = ^uintptr(0)` (= `0xFFFFFFFFFFFFFFFF`), a `ulong` literal that needs a *non-constant* `nuint` conversion — cannot be a C# `const` (CS0133/CS0266). It is emitted as `static readonly` with an `unchecked` cast instead (small native-int consts like `const nint iota = 0` stay `const`):

```csharp
public static readonly uintptr MaxUintptr = /* ^uintptr(0) */ unchecked((uintptr)18446744073709551615);
```

The same `unchecked` cast is emitted for a **named** constant declared over a *wide unsigned* underlying whose folded value overflows int32 — `const unknownClass = ^Class(0)` (x/text/unicode/bidi, `type Class uint`) and `const _m = ^Word(0)` (go/constant via math/big, `type Word uintptr`) both fold to the all-ones literal `18446744073709551615` (a C# `ulong`), which has no implicit conversion to the `[GoType]` wrapper struct (CS0266). The native-int-const detection, which previously fired only for a `uintptr` underlying, now also fires for `uint`/`uint64` underlyings, so the const emits `unchecked((Class)18446744073709551615)`. A **small** named const stays uncast (`const c = Class(5)` → `Class(5)`, an ordinary in-range constant conversion) — the cast is added only when the value is out of int32 range, so no other named-const emission churns. (Guarded by the `NamedNumericConstCast` behavioral test — a beyond-int32 `^Named(0)` over `uint` and over `uint64` plus a small in-range control, values verified vs Go; shared root, cleared go/constant and bidi one error each.)

**`uintptr` is a DISTINCT golib struct** (`golib/uintptr.cs`), not an alias of `System.UIntPtr`: Go's `uint` and `uintptr` are distinct types (both may appear in one type switch; `%T` reports them differently; conversion between them is explicit), and the historical alias erased that identity — type switches collided (CS8120), `%T` lied, and overloads could not distinguish them. The struct holds a single public mutable `nuint Value` field (PascalCase — it is public so `Interlocked`/`Volatile` seams can target the inner storage; the intrinsics cannot take a ref to a user struct) and carries the full operator surface so `uintptr`-typed expressions KEEP the type. The conversion matrix is empirically tuned to C#'s user-defined-conversion candidate rules (encompassing counts only STANDARD conversions, so nothing ever chains two user-defined operators; a PARTIAL outbound operator set is unstable — undeclared targets see multiple viable std-hop candidates, CS0457): implicit both ways with `nuint` plus implicit from smaller unsigned/`char`/`UntypedInt`; explicit inbound from signed types and `uint64`; the FULL exact outbound matrix (all integer widths + `float32`/`float64` + unsafe `void*`). Knock-ons handled with it: `const uintptr` is illegal C# (user struct) so every uintptr const emits `static readonly`; a uintptr-typed switch tag/label can never be a constant/relational pattern (CS9135) so those switches use the if-else `==` form; wrappers over uintptr (`[GoType("num:uintptr")]`) gain generated `nuint`/`UntypedInt` bridges; generic-math-constrained golib helpers (`unsafe.Add/Slice/String`) gain non-generic `uintptr` overloads; and the manual managed-referent types declare direct `uintptr` bridges (token out, panic-on-nonzero in).

**Numeric literal formatting is preserved** wherever Go and C# syntax overlap: hex (`0x4000`), binary (`0b1011`), and decimal literals — including `_` digit separators — emit with their original source text (`0x4000` never flattens to `16384`), keeping bit masks and addresses recognizable; required `U`/`UL`/`L` suffixes and casts compose with the preserved text (`0xFFFFFFFFU`). Go-only forms re-render as decimal: `0o…` octal has no C# syntax, and a legacy leading-zero octal (`0755`) would silently re-bind as decimal 755 in C#.

See [Untyped Constants and Named Numeric Types](#untyped-constants-and-named-numeric-types) for how these interact with native-int and named numeric types. See also [example](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/basics/numeric-constants).

## Handling "int" and "uint" Types

In Go the `int` and `uint` types are sized according to the platform build target, i.e., 32-bit or 64-bit. C#'s `int`/`uint` are always 32-bit and `long`/`ulong` are always 64-bit. As of C# 9.0, native-sized integer types exist that behave exactly like their Go counterparts: [`nint` and `nuint`](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9#performance-and-interop). The converter maps Go `int` → `nint` and Go `uint` → `nuint`; `uintptr` also maps to `nuint`. The fixed-width Go types (`int8/16/32/64`, `uint8/16/32/64`, `byte`, `rune`) are kept as readable C# aliases of the same name (e.g. `global using uint16 = System.UInt16;`).

**Narrow-integer arithmetic.** A subtle semantic gap: Go evaluates arithmetic on a sub-`int`-width integer (`int8`/`uint8`/`int16`/`uint16`) at that operand's own width, with overflow **wrapping** — `var a, b uint8 = 200, 100; a + b` is `44` (300 mod 256). C#, however, **promotes** arithmetic on `byte`/`sbyte`/`short`/`ushort` to `int`, so `a + b` is `300` and is *not* implicitly assignable back to the narrow type. Where a narrow-arithmetic result is used in a context that requires the narrow type — e.g. passed to a narrow-typed parameter — the converter emits an explicit cast back to that type, which both compiles (the implicit `int`→narrow conversion is rejected, CS1503) and restores Go's wrapping:

```csharp
takeU8((uint8)(a + b));   // Go take(a + b), a/b uint8 → 44 (wraps), not 300
takeU8((uint8)(~a));      // Go take(^a) → 55
```

The same cast applies in the **assignment** context — a narrow-arithmetic value assigned to a narrow variable, array/slice element, or struct field (`y := a + b; y = y + 1; arr[0] = a + b; bx.b = a + b`) — and in the **declaration** context — a typed-var initializer (`var z uint8 = a + b`). All emit `(uint8)(a + b)` for the same two reasons. (A double cast is avoided when another path already narrowed the RHS, e.g. a bitwise op with an untyped constant emits its own `(byte)(b | 128)`.)

The cast is applied only when the value's Go type already matches the target (parameter / LHS / declared type), so Go accepts it without a conversion, and only for an arithmetic (binary/unary) expression — a bare identifier is already the narrow type. (Guarded by the `NarrowArithmeticArg` behavioral test, which verifies the wrapped values match Go across all four contexts. Wider integer types — `int32`/`uint32` and up — are not promoted by C# and need no cast.)

A redundant-cast guard on this decision — skip the cast when the converted RHS is *already* a full narrowing (`(byte)(b | 128)`) — must distinguish a WHOLE-expression cast from one that only converts the FIRST operand. `buf[i] = byte(e/100) + '0'` (runtime `print.go`) emits the RHS `(byte)(e / 100) + (rune)'0'`, which *starts* with `(byte)(` but only casts `e/100`; the binary result is still `int` (the `(rune)'0'` promotes it), so the narrowing cast is still required (CS0266). The guard therefore checks that the cast-paren's matching close is at the very end of the RHS (a parenthesis-balance walk that skips `(`/`)` inside char/string literals), not merely that the RHS begins with `(byte)(`. (Guarded by the `NarrowByteArithFirstOperandCast` behavioral test — including a wrapping case; cleared 3 runtime CS0266 in `print.go`'s exponent formatting.)

The same narrowing applies to a **`return` of narrow-integer arithmetic**. `func lowerASCII(c byte) byte { return c + ('a'-'A') }` (runtime `env_posix`) returns `byte + int` (the untyped char constant promotes to `int`) → CS0266 against the `byte` result type. The cast was applied on the assignment and value-spec paths but not the return path; it is now applied in `visitReturnStmt` when the function's result type at that position is narrow and the returned expression is binary/unary arithmetic (reusing the same gate — a bare identifier, a call, or an already-whole-expr-narrowed return is untouched, and a non-narrow result type is unaffected). (Guarded by the `NarrowByteArithReturn` behavioral test — a per-branch return plus a wrapping case; cleared the `env_posix.lowerASCII` CS0266.)

A related **wide** case: a computed *constant* arithmetic expression assigned to a **native-width integer** (`uintptr`/`uint`/`int` → C# `nuint`/`nint`) whose folded value overflows int32. `pattern = 1<<maxBits - 1` (runtime `mbitmap`, `maxBits` = 57) is a `uintptr` constant, but the converter folds the untyped sub-shift `1<<maxBits` to a **signed** C# `long` literal (`144115188075855872L`, since it exceeds int32 and the untyped operand is treated as signed), so the whole RHS is `long` — which has no implicit conversion to the native target (CS0266). A `UL`/`(nuint)` suffix would not help (`ulong`→`nuint` is also an explicit conversion). The converter wraps the whole RHS in the native target's cast: `pattern = (uintptr)(144115188075855872L - 1)`. This fires **only** when the constant fits int64 but is out of int32 range — exactly the signed-`long` fold range. A value that overflows *int64* (a large unsigned `uintptr` like `1<<63 + 1<<62`) is deliberately left alone: its sub-shift already mis-emits (a `1<<63` int-shift), so casting it would convert a visible compile error into a silent wrong value — that is a separate defect to fix on its own, not to mask. (Guarded by the `NativeIntWideConstAssign` behavioral test — `uintptr`/`uint`/`int` targets with int64-range constants, values verified vs Go; cleared the `mbitmap` CS0266, the last one in `runtime`.)

**The signed integer minima sign-fold at the unary level.** Go folds `-literal` into one constant, but the emitter classifies the POSITIVE operand literal alone, and both signed minima's magnitudes overflow their own type: `[]int32{-2147483648}` (internal/fuzz mutator's `interesting32`) saw 2147483648 > MaxInt32 and emitted `-(nint)2147483648L`, which has no implicit conversion back to an int32 slot (CS0266); the int64 minimum's operand 9223372036854775808 does not even parse as int64, routing through the unsigned branch to `-(nuint)9223372036854775808UL` — and C# defines no unary minus on `nuint` at all (CS0023). `convUnaryExpr`'s `token.SUB` handling now mirrors its FLOAT arm: for an INT literal operand it classifies the range on the **unary expression's resolved (sign-folded) constant**. The exact int32 minimum in an int32-typed context emits the plain negated literal `-2147483648` — C# special-cases the negated decimal int-min as an `int` constant, **by value**, so `_` digit separators survive (`-2_147_483_648` compiles, proven by the guard) — and the exact int64 minimum emits `-9223372036854775808L` (the matching `long` special case), wrapped as `((nint)(-9223372036854775808L))` in a Go-`int` context where `long` has no implicit conversion. Decimal source formatting is preserved per the literal-formatting rule; hex/binary re-render as decimal (C# has no signed special case for those forms — `-0x80000000` binds as a `long`-typed expression). Everything else keeps the default path: in-int32 operands never had a problem, and a folded int32-min in a WIDER context (`var x int64 = -2147483648`, or boxed to `any` where Go-`int` must stay `nint`) keeps the implicitly-convertible `-(nint)…L` form — the full-stdlib A/B footprint was exactly the one mutator.cs line. (Guarded by the `IntMinLiterals` behavioral test — int32-min plain and underscored in `[]int32`, int64-min in `[]int64`, the nint-min `:=` form, between-minima and non-minimal negative controls, and min-value comparisons, values vs Go; the pre-fix converter fails it CS0266 ×2 + CS0023 ×2.)

**Constant-literal return inside a lambda with an unsigned result (delegate-type inference, CS8917).** A Go closure assigned to a local — `casePC := func(casi int) uintptr { if pcs == nil { return 0 }; return pcs[casi] }` (runtime `select.go`) — is emitted as `var casePC = (nint casi) => { … };`, whose delegate type C# must **infer from the return-expression types**. The literal `return 0` is typed `int`; `return pcs[casi]` is typed `nuint` (`uintptr`). C#'s best-common-type algorithm uses the expression types (not constant convertibility), and `int` has no common type with `nuint`/`uint`/`ulong` (there is no implicit `int`→unsigned conversion for a non-constant), so the `var` assignment fails with CS8917 ("no best type found for the lambda"). The converter casts the literal to the result type so both returns share it: `return (uintptr)(0)`. Gated tightly to avoid churn and new errors: only **inside a lambda body** (`conversionInLambda` — a *named* func's `return 0` to a `nuint` result compiles as an ordinary constant conversion and needs no cast), only for a bare **integer literal** (the sole shape that trips the `int`-vs-unsigned inference gap — `byte`/`uint16` widen to `int`, and the signed/`nint`/`long` kinds share a common type with `int`, so those never hit CS8917), and only when the result is a **basic** `uint`/`uint32`/`uint64`/`uintptr` (a *named* type over an unsigned kind is left alone — `(gclinkptr)(0)` would only compile if that type defined an int conversion, so casting it could introduce a new error). Runs after the narrow-arithmetic return cast, with which it is disjoint (that handles binary/unary arithmetic on sub-`int` types; this handles a bare literal to a wide unsigned type). (Guarded by the `ClosureMixedReturnUnsigned` behavioral test — `uintptr`/`uint64`/`uint32`/`uint` mixed-return closures plus a signed control that stays uncast, values verified vs Go; cleared the `select.go` `casePC` CS8917.)

> One sticking point: not all C# indexing constructs accept a `nint`. Explicit indexers support `nint`, but [implicit index support](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-index-support) (the `Index`/`Range` syntax) currently only works with `int`, so range-operation indices are cast to `int` where needed. (The earlier strategy of compiling to `long`/`ulong`, or of custom `@int`/`@uint` structs selected by a `TARGET32BIT` directive, has been superseded by `nint`/`nuint`.)

## Untyped Constants and Named Numeric Types

This area is where Go's flexible numeric model meets C#'s stricter one, and it has a few moving parts worth calling out.

**Untyped constants.** As noted under [Constant Values](#constant-values), an untyped Go constant becomes a golib `UntypedInt`/`UntypedFloat`/`UntypedComplex`. These wrappers define implicit conversions to **and from** every numeric type so the value can slot into whatever context uses it, just like an untyped Go constant. The trade-off is that mixing an `UntypedInt` directly into heavily-typed arithmetic (e.g. `someUint64 * untypedConst`) can become ambiguous to C#'s overload resolution, since the wrapper is convertible in either direction. Context-typing of untyped *local* constants (emitting them with the concrete type their use demands instead of the wrapper) is an area of ongoing refinement.

One resolved instance: an argument to the **`min`/`max` builtins** that is a named untyped constant renders as its `UntypedInt` static, which golib's `min<T>(T, params ReadOnlySpan<T>)` overloads reject (CS1503 — params-span element binding does not apply the user-defined implicit conversion): runtime `min(n, maxObletBytes)` (`mgcmark.go`, `uintptr` sibling) and `min(debug.profstackdepth, maxProfStackDepth)` (`runtime1.go`, `int32`). The converter casts such an argument to the call's Go-resolved result type — `min(n, (uintptr)(maxObletBytes))` — and, once one argument is cast, every constant-valued sibling too (`min(big, limit, 500)` → `…, (uintptr)(limit), (uintptr)(500))` — a bare literal is a C# `int` and would break `T` inference against the cast type). Typed arguments and literal-only calls are unchanged. (Guarded by the `MinMaxBuiltin` extension — untyped consts typed by `uintptr`/`int32` siblings plus the mixed literal case, values vs Go.)

**Named numeric types.** A Go type definition over a numeric base — `type Celsius float64`, `type level int`, `type Flags uint` — is emitted as a partial struct carrying a `num:` `[GoType]` attribute, and the `TypeGenerator` source generator fills in the body:

```csharp
[GoType("num:nint")]  partial struct level;   // type level int
[GoType("num:nuint")] partial struct Flags;   // type Flags uint
[GoType("num:float64")] partial struct Celsius; // type Celsius float64
```

The generated struct wraps the underlying value and implements the comparison and arithmetic operators plus implicit conversions to/from the underlying type, so the named type is a distinct C# type that still behaves like its base.

**Increment / decrement.** Go allows `c++` / `c--` on a named integer (e.g. a `for c := chunkIdx(0); …; c++` loop counter). The generator therefore emits `operator ++`/`operator --` returning the **named type** — `operator ++(T value) => (T)(value.m_value + (U)1)` (U the underlying). Without a dedicated operator, C# `c++` falls back to the implicit conversion to the underlying and re-assigns the (promoted) result, which for a **native-int**-backed named type (`num:nuint`/`num:nint`) promotes to `ulong`/`long` and then cannot implicitly convert back to the named type (CS0266). The dedicated operators keep the result in the named type. (Guarded by the `NamedNumericIncDec` behavioral test — `++`/`--` on `uint`- and `int`-backed named types in loop counters; runtime uses this for `chunkIdx`/`arenaIdx`/`statDep` loop counters, ~7 CS0266.)

**Unsigned underlying types and unary minus.** Go permits unary minus on an unsigned value (it wraps: `-x == 0 - x`). C# does **not** allow the unary `-` operator on unsigned operands. So the generator's `IsUnsignedType` check *omits* the unary negation operator for unsigned underlying types (`uint8/16/32/64`, `byte`, `uintptr`, and the native `nuint`/`uint`). Go's unary minus on such a value is instead lowered by the converter to the equivalent subtraction-from-zero form:

```go
var b Flags = 2
_ = -b            // Go: unsigned unary minus (wraps)
```
```csharp
Flags b = 2;
_ = ((Flags)0 - b);   // C#: lowered to (T)0 - x
```

This keeps the generated numeric struct compilable (a `(T)(-value.m_value)` body over `nuint` is a CS0023 error) while preserving Go's wrap-around semantics. The same `(T)0 - x` lowering is used for unsigned unary minus on built-in unsigned values.

**Converting *to* a named numeric type.** The generated struct's implicit conversions are only between the named type and its *exact* underlying basic (`traceArg ↔ uint64`, `arenaIdx ↔ nuint`). So a Go conversion `traceArg(procs)` where `procs` is `int32`, or `arenaIdx(1 << b)` where the shift is `int`, has no matching operator — a plain `(traceArg)procs` is CS0030. The converter coerces the argument through the underlying type first, which is exactly Go's numeric-conversion semantics:

```go
var procs int32 = 5
a := traceArg(procs)   // type traceArg uint64
b := arenaIdx(1 << 4)  // type arenaIdx uint
```
```csharp
int32 procs = 5;
var a = ((traceArg)(uint64)procs);   // through the underlying uint64
var b = ((arenaIdx)(nuint)(1 << 4)); // through the underlying nuint
```

When the argument is *already* the underlying basic (`traceArg(u)` with `u uint64`), the existing single cast already binds, so no extra cast is inserted (no churn). (Guarded by the `NamedNumericConversion` behavioral test; runtime exercises this pervasively for `traceArg`, `arenaIdx`, `traceTime`, the `abi` offset types, etc.)

**Converting *from* a named numeric type.** The mirror direction has the same root: because the wrapper only converts between the named type and its *exact* underlying, a Go conversion *from* a named numeric *to a different basic numeric* — `uint64(nameOff)` where `type NameOff int32`, or `int(idx)` where `type idx uint` — has no matching operator (`(ulong)NameOff` / `(nint)idx` is CS0030). The converter routes it through the named type's underlying basic first — the named→underlying `[GoType]` operator followed by an ordinary numeric C# cast:

```go
var s NameOff = 7  // type NameOff int32
e := uint64(s)     // NameOff -> uint64
var i idx = 9      // type idx uint
f := int(i)        // idx -> int
```
```csharp
NameOff s = 7;
var e = ((uint64)(int32)s);  // through the underlying int32
idx i = 9;
nint f = ((nint)(nuint)i);   // through the underlying nuint
```

When the target basic *is* the named type's exact underlying (`int32(s)` for `NameOff`), the single operator already binds, so no extra cast is inserted (no churn). (Same `NamedNumericConversion` behavioral test; runtime hits this on the `abi` offset types `NameOff`/`TypeOff`/`TextOff` → `uint64`/`uintptr`, `taggedPointer`/`traceTime` → `int64`, etc.)

**Cast parenthesization (visual fidelity).** A Go conversion `T(x)` reads as a function call; the C# cast `(T)x` is the closest equivalent, so the converter keeps it minimally parenthesized to stay close to the source:

* **Basic-target conversions omit the outer wrap.** A conversion whose target is a *basic* C# type — `uint64(a)`, `int(k)`, `float64(b)`, and even `unsafe.Pointer(p)` (go/types models it as a `*types.Basic`) — emits `(uint64)a`, not `((uint64)a)`. The result of a basic-typed cast can never be the receiver of a postfix `.`/`[]`/invocation (Go basic types expose no callable members and the converter emits none on them), and the C# cast operator outranks every binary operator, so the bare form binds correctly in any surrounding context: `f((uint64)a)`, `return (uint64)a;`, `(uint64)a << n`, `(nint)(uint8)k < len(s)`, and `(nint)x.Load() + 5` (which parses as `((nint)(x.Load())) + 5` — postfix `.` binds before the cast). A **named**-type target keeps the defensive outer parens `((Named)x)` — its result *can* be member-accessed (`Named(x).Method()`), which is parent-context-dependent and not decidable at the conversion site. **`string` is the exception among basic types:** its C# representation is the member-accessible golib `@string` struct, so a `string(x)` conversion *is* a valid postfix receiver — the variadic-string spread `string(r)...` → `((@string)(rune)r).ꓸꓸꓸ`, an index `string(b)[i]` → `((@string)b)[i]`, or `len(string(b))`. Dropping the wrap there reparses the postfix against the cast's inner operand (`(@string)(rune)r.ꓸꓸꓸ` binds `.ꓸꓸꓸ` to `r`, CS1061), so a `string` target retains the outer parens like a named type. (`unsafe.Pointer` stays in the no-wrap set: although its C# `@unsafe.Pointer` is a struct, Go exposes no members on `unsafe.Pointer` and the converter never emits a postfix on such a conversion result.)
* **Identity conversions are not double-cast — EXCEPT a plain constant argument.** `arenaIdx(x)` where `x` is already `arenaIdx` is a Go no-op. This arises for an untyped-constant shift that adopts the target type from context (`arenaIdx(1 << bits)`, whose operand go/types already types as `arenaIdx`, so the inner conversion has already emitted `(arenaIdx)((nuint)1 << bits)`), and for a plain `arenaIdx(yArenaIdx)`. Wrapping the already-typed expression in a second `(arenaIdx)` cast just doubles it, so the converted argument is returned as-is. The exception: a plain **constant** argument (`Word(1)`) — go/types types the constant AS the target (identity), but the render is the bare literal, which under a binary operator resolves as `int` and degrades the whole expression (math/big's `mask := Word(1)<<s - 1`, CS0029). The named cast is re-imposed at the conversion site — `((Word)1) << (int)(s)) - 1` — which also made the older `:=`-declaration-only patch in visitAssignStmt redundant (it now sees the cast already present).

**Named-numeric wrappers carry the full INTEGER operator surface.** The generated `[GoType("num:…")]` wrapper defines `+ - * / % ++ --` returning the wrapper; integer underlyings additionally define `~`, the shifts `<< >>` (int count), and the binary bitwise `& | ^` — all returning the WRAPPER type, exactly Go's typing (`Word >> ŝ` IS a `Word`). Without them C# resolved compound expressions through the implicit-to-underlying conversion and the whole expression degraded to the raw numeric (math/big's Word arithmetic, CS0266 ×45). Floats/complex omit the integer-only operators.

**Named SLICE types keep the named type when sliced.** The generated slice wrapper's Range indexer and `Slice()` overloads return the wrapper (`nat[a:b]` IS a `nat` — a fresh wrapper sharing the same backing window), so a method call directly on a slice expression binds the named type's extensions (`u[s:].norm()` bound the raw `slice<Word>` instead, math/big CS1929 ×21). The explicit `ISlice<T>` implementations keep the raw slice type.

A conversion **between two named slice types** sharing an identical underlying (tar's `sparseElem(s[i*24:])`, both `[]byte`) hops through the shared underlying slice — `((sparseElem)(slice<byte>)(…))` — since the wrapper-returning slicing makes the argument the NAMED wrapper and a direct cast would chain two user-defined operators (CS0030). (Guarded by `SortArrayType`'s `Roster(byAge[0:2])`.)

(Guarded by `NamedNumericConversion`, `NamedNumericShiftConv`, `NamedTypeBitwiseConst`, `IotaEnum`, `FuncTypeParam`, and `CrossPkgUser`; the `string`-target exception is guarded by `StringConvPostfix` and `UnsafeOperations`; verified by the full behavioral suite — output comparisons confirm the precedence is unchanged.)

**Generated conversion operators between named numerics of *different* assemblies.** The two paragraphs above are the *converter's inline* casts. Separately, when the converter sees a conversion *between two named numeric types* it records a `[assembly: GoImplicitConv<…>]` and the `ImplicitConvGenerator` emits a user-defined `implicit operator` for it. The emitted body constructs one named type from the other's underlying value: `new Target((ValueType)src.Value)`. When both named types live in the **same** assembly this is fine (e.g. runtime's `muintptr ↔ Δhex`), but when the operator must **construct a *foreign* named numeric** — one declared in another C# assembly — two problems appear that only manifest cross-assembly:

* A direct cast to the foreign named type has no route. `(NameOff)src.Value` where `src.Value` is `ulong` and `NameOff` (`internal/abi`) is a *different assembly* is **CS0030** — C# does not select the foreign type's `int32`-based user conversion for a `ulong` source across the assembly boundary (the same cast to a *local* named type compiles). It must go **through the foreign type's underlying basic**: `new …NameOff((int)src.Value)`.
* The default host can be a phantom. The operator is hosted in `partial struct {sourceType}`; if that source is the *foreign* type (reached here via a local alias, e.g. runtime's `global using nameOff = abi.NameOff`, so the cross-package dot is hidden and the conversion records as `Inverted`), the `partial struct NameOff` declares a new *empty local* type rather than extending the foreign one — **CS1729** (no constructor). The operator is relocated into the **local** type instead.

So for a foreign *constructed* type the generator emits, fully-qualified and hosted in the local type:

```csharp
// runtime, dur↔hex style: foreign abi.NameOff constructed from local Δhex
partial struct Δhex {
    public static implicit operator global::go.@internal.abi_package.NameOff(global::go.runtime_package.Δhex src)
        => new global::go.@internal.abi_package.NameOff((int)src.Value); // through the underlying int32
}
```

The override fires **only** when the `new`-constructed side (the LH type: the *source* when the conversion is `Inverted`, else the *target*) is foreign; same-assembly operators are emitted byte-identically as before (no churn). Because the trigger is inherently cross-assembly, the behavioral-test harness (single-assembly, and unable to import a foreign named numeric — `internal/*` types are un-importable from a test module and the baseline stubs expose none) cannot host it; the guard is the **`go-src-converted/runtime` build**, where `NameOff`/`TypeOff`/`TextOff` ↔ `Δhex` naturally occur (this fix cleared 3×CS0030 + 3×CS1729 there).

A **same-assembly** pair also needs the through-underlying routing when the two named numerics have **incompatible underlyings** — internal/trace's public `type Time int64` ↔ unexported `type timestamp uint64`, converted both ways (`Time(ev.Ts)` / `timestamp(ts)`). The default `new Time((ΔTime)src.Value)` casts `src.Value` (a `ulong`, since `timestamp` is `uint64`-backed) straight to the wrapper, which routes through the wrapper's `long`-based user conversion — but `ulong`→`long` is not an implicit C# conversion, so the cast is **CS0030**. (This is the *mixed-accessibility* case: `Time` is exported and `timestamp` is not, so the operator is already relocated into the less-accessible `timestamp` struct — orthogonal to the underlying.) The generator now, for a **local** numeric pair, casts through the constructed type's underlying C# keyword when the source underlying does **not** implicitly convert to it: `new Time((long)src.Value)`, `new timestamp((ulong)src.Value)`. The source/constructed underlyings are read from each side's `[GoType("num:X")]` tag (a sibling generator cannot see the generated `Value` property), and the implicit-convertibility test is the fixed C# numeric-conversion table over the fixed-width integer/float basics. Crucially this fires **only** on pairs the default cast could not compile (the default `(Wrapper)src.Value` succeeds *iff* that same source→underlying conversion is implicit), so every already-compiling conversion stays byte-identical — the full behavioral suite's [goldens](Glossary.md#golden) are unchanged. `uintptr`-backed pairs keep the existing `nuint`-hop override; `int`/`uint` native-width wrappers are deliberately left to the default (their classification is version-sensitive and the failing corpus cases are fixed-width). (Guarded by the `NamedIntSignednessConv` behavioral test — a public `int64` ↔ unexported `uint64` named pair converted both ways, including a `^uint64(0)`→`int64` case whose `-1` result verifies the cast preserves the bit pattern exactly, output-compared vs Go; internal/trace's `timestamp`→`Time` inverse operator relies on it.)

The recorded `GoImplicitConv` must also be able to **name the foreign type**. The recorded type name carries the foreign package's import qualifier — the DOT form `driver.IsolationLevel` for an unrenamed type, or a `ꓸ` global-using alias (`CrossPkgLibꓸGrade`) for a `Δ`-renamed one — but the attribute sits in `package_info.cs` at file scope and the generated operator lands in a `.g.cs`, neither of which carries the body files' import `using`s. A `Δ`-renamed foreign numeric resolves through its own `ꓸ` global using, but the **dot form needs a resolving `using driver = go.database.sql.driver_package;`** in `package_info.cs`'s `ImportedTypeAliases` block. The STRUCT-conversion branch of `checkForImplicitConversion` already drives that using by calling `recordConversionPackageUsing(argType)`/`(funcType)`, but the **aliased-NUMERIC branch omitted it** — so a cross-package named-numeric conversion (database/sql's `driver.IsolationLevel(opts.Isolation)`, where `sql.IsolationLevel` and `driver.IsolationLevel` are distinct named ints) left `driver` unresolved in both the attribute and the generated operator (CS0246). The numeric branch now records the same package usings. (Guarded by an extension to the `CrossPkgUser` cross-assembly test — a local `float64`-based named numeric converted to the *unrenamed* `CrossPkgLib.Celsius`, which renders in dot form and so needs the registered using; a `Δ`-renamed target like `CrossPkgLib.Grade` would have resolved via its alias and would not have caught the gap.)

The same underlying routing applies when an untyped-constant **shift** is re-typed to a named numeric. An untyped shift `1 << k` is re-typed to the type it assumes from context (so it can combine with typed operands); when that resolved type is a *named* numeric, the re-type must go through the underlying — `(arenaIdx)((nuint)1 << k)`, not a bare `(arenaIdx)(1 << k)` (CS0030). The shift's *width* is likewise decided by the underlying (a `nuint`/`uint64`-backed named type shifts the left operand in that width to avoid the `int`-overflow seen for `1 << 63`). Non-named shifts are unchanged. (Guarded by the `NamedNumericShiftConv` behavioral test — wide `uint`/`uint64`-backed and narrow `uint8`-backed named types; runtime hits this on `arenaIdx(1 << arenaBits)`.)

The unsigned named-numeric path above gets a width-cast operand, but a **signed** constant operator expression whose target is a plain builtin `int64` has no such cast, so C# would compute it in `int32` and overflow at compile time in checked mode (CS0220): `int64(1<<63 - 1)`, `var d int64 = 1<<40 + 7`, or `12345 * 1000000000 + 54321` passed to an `int64` parameter. Go evaluates each as a constant in its `int64` type. For a signed constant binary/shift expression whose folded value is **outside the C# `int32` range**, the converter emits the **folded 64-bit literal** (`9223372036854775807L`, `1099511627783L`, `12345000054321L`) instead of the operator form — correct, and self-contained. In-range constants are unchanged (they keep the readable `1 << k` form). (Guarded by the `UntypedConstArithmetic` behavioral test; runtime hits this in `mgcmark`/`netpoll`/`runtime1`.)

**UNSIGNED** constant expressions fold under a much narrower trigger (2026-07-03): every other unsigned shape already has a working mechanism — a *typed* unsigned shift gets the width-cast operand (`(uint64)1 << 40`), an int64-range untyped subtree is folded by the signed arm when recursion reaches it (`(281474976710655L) + arenaBaseOffset` in runtime `mranges`), and a named-const reference renders via its `Untyped*` wrapper (`(uintptr)m5 ^ 4` in runtime `hash64`). The one unfixable shape is an untyped constant **operator** subtree (a BinaryExpr) whose value exceeds **int64 entirely**: `1<<63` nested inside `(1 << 63) - 1` — go/types lands the uint64 conversion on the outermost constant node, so the inner shift stays untyped, no width cast reaches it, and C# computes it in int32. `int64((1 << 63) - 1 - (1<<63)%uint64(n))` (math/rand `Int63n`, CS0220) emits as `(int64)(9223372036854775807UL - (((uint64)1 << (int)(63))) % (uint64)n)`: the constant subtree folds to `UL`, the standalone *typed* shift keeps its readable width-cast form. Gated to plain-`uint64` underlying targets (`constExprHasBeyondInt64UntypedOperatorSubexpr`) — a native-width `uintptr` target would need a further cast the fold cannot safely synthesize, so that pre-existing caveat keeps its visible error. A first broader cut (any untyped subtree beyond int32, any unsigned target) regressed runtime's `hash64`/`mranges` by stealing exactly those already-working shapes — the narrow trigger is load-bearing. (Guarded by the `UntypedConstArithmetic` extension — the Int63n shape, value-compared vs Go.)

The same coercion is needed where the converter itself inserts a C# `(int)` cast on a named-numeric value — a **slice bound** (`summary[sc+1:ec]` with `sc`/`ec` of type `chunkIdx`), a **shift count** (`1 << (d % 64)` with `d` of type `statDep`), or the **length of an `unsafe.Pointer`-to-array slice** (`(*[N]T)(ptr)[:n]` → `new slice<T>(new ReadOnlySpan<T>(ptr, (int)n))`, since the `ReadOnlySpan<T>` constructor takes a C# `int` — see *Slicing a pointer-to-array*). A bare `(int)(sc + 1)` is CS0030 for the same reason, so the converter emits `(int)(nuint)(sc + 1)` / `(int)(nint)(d % 64)` — through the named type's underlying basic; a plain `nint`/`nuint` length is narrowed `(int)(n)`. Plain basic operands keep the bare `(int)(x)` form. (Guarded by the `NamedNumericIntCast` behavioral test; the Span length by `StdLibInternalAbi`.)

**Defined types over a struct — forwarded fields.** A Go type definition over a *struct* — `type winlibcall libcall` — makes the underlying struct's fields accessible on the named type (`w.fn`), without promoting its methods. The named type is emitted as `[GoType("libcall")] partial struct winlibcall;` and the `TypeGenerator` wraps the underlying value (`private libcall m_value;`). For the underlying's fields to be reachable, the generator **forwards each as a get/set property** over `m_value`:
```csharp
private libcall m_value;                 // NOT readonly — see below
public nuint fn { get => m_value.fn; set => m_value.fn = value; }
public nuint n  { get => m_value.n;  set => m_value.n  = value; }
// … args, r1, r2, err
```
The underlying struct is resolved with `GetStructDeclaration` (same package, or a *source*-referenced package — a metadata-only struct is not resolved, so its fields are not forwarded), and its members come from `GetStructMembers`. Crucially `m_value` is **mutable** (not the wrapper's usual `readonly`), so a write through a pointer — `c.Value.fn = fn`, where `c` is a `ж<winlibcall>` and `c.Value` is `ref winlibcall` — invokes the setter on the real storage and persists. (The `readonly`→mutable choice is decoupled from the nullable-`m_value` form that only the lazily-allocated `array` backing needs.) Forwarding is skipped for a non-struct underlying (a named type over an interface or another named type) and for an underlying that contributes no fields, so those wrappers are unchanged. *Composite-literal construction* of such a type (`winlibcall{fn: x}`) is a separate, not-yet-handled case (the runtime accesses these only by field). (Guarded by the `NamedTypeOverStruct` behavioral test — write-through and read-back of forwarded fields through a pointer; runtime hits this on `winlibcall` over `libcall`, `syscall_windows.go`.)

**Defined types over an array-backed defined type — the IArray view.** A second-level definition — `type pallocBits pageBits`, where `type pageBits [8]uint64` is itself an array-backed `[GoType]` wrapper — is `len()`'d and indexed directly in Go (runtime `mpallocbits.go`), which requires `IArray` on the **outer** wrapper (golib `len(IArray)`; CS1503 otherwise, and the named-over-array *indexing* sites in `mgcscavenge`/`proc`/`traceback` fail the same way). The generator detects this in the bare-name branch — the resolved underlying struct contributes no declared members but its own `[GoType]` definition is an array form (`[N]elem`) — and implements `IArray<elem>` on the wrapper as a **view** (`IArrayViewTypeTemplate`). Every member delegates through a private `view` accessor that first touches `m_value.Value` **on the mutable field** — materializing the underlying's *lazily-allocated* backing in the wrapper's own storage — and then returns a value copy sharing that heap `T[]`, so element refs land in the real storage. (Going through the plain copying `Value` property instead silently dropped writes on a zero-valued wrapper — the backing allocated on the copy — which is the historical `pallocBits` lost-writes trap, reproduced and pinned before the fix. A struct member cannot ref-return its own field — CS8170 — so the ensure-then-share-copy shape is the correct one; the `(pageBits)(b)` reinterpret conversions keep compiling and, once the backing exists, write through shared storage.) (Guarded by the `NamedArrayWrapper` behavioral test — `len`, index read/write, and a write via the `(*pageBits)(b)` reinterpret observed through the original, values vs Go; cleared runtime's 5 `pallocBits → IArray` CS1503 **plus a −3 CS0021 cascade** of named-over-array indexing, 86 → 74 with the `copy` overload below.)

**`copy` from a defined slice type.** `copy(dst, src)` where `src` is a *named* slice type — `type pMask []uint32`, runtime `proc.go`'s `copy(nidlepMask, idlepMask)` — cannot bind the generic `copy<T1,T2>(in slice<T1>, in slice<T2>)`: the wrapper implements `ISlice<uint32>` but *is not* a `slice<T2>`, and generic inference does not see user-defined conversions, so resolution fell onto `copy(slice<byte>, @string)` (CS1503 ×2 per call). golib adds `copy<T1, T2>(in slice<T1> dst, ISlice<T2> src)` — `T2` infers from the implemented interface — copying element-wise through the interface indexer with the same min-length/convert semantics; a genuine `slice<T>` source still binds the more-specific slice/slice overload, so existing calls are unchanged. (Guarded by the same `NamedArrayWrapper` test — `copy` count/values plus post-copy independence of source and destination, vs Go.)

The wrapper also forwards the underlying's **field-box accessors**. Taking the address of a wrapper's field — `&p.x` on a `*pinnerBits`, where `type pinnerBits gcBits` (runtime `pinner.go`) — emits the box-accessor form `Δp.of(pinnerBits.Ꮡx)`, whose owning type is the **wrapper**; without a forwarded accessor the static exists only on `gcBits` (CS0117). For every forwarded *field* (properties cannot be `ref`'d and get none, matching the plain-struct template) the generator emits the accessor as a **true ref through `m_value`** into the underlying struct's field: `public static ref uint8 Ꮡx(ref pinnerBits instance) => ref instance.m_value.x;` — a genuine ref chain into the wrapper's own storage, so a write through the resulting box persists (a copy here would silently drop writes — the trap that sank an earlier `pallocBits` forwarding attempt). Emitted only when members are forwarded, which is exactly when `m_value` is mutable. (Guarded by the `NamedTypeOverStruct` extension — `bump(&c.a)` writes through the wrapper's field address and the original observes it; cleared runtime `pinner.go`'s 3 CS0117, 89 → 86.)

**Untyped constants in a typed-element context (`append`).** Because an untyped constant renders as a bare C# `int`/`double` literal or an `Untyped*` wrapper, passing one as an `append` element to a typed slice trips C#'s overload resolution: `append<T>(ISlice, params T[])` infers `T` from the element while the `slice<T>` overloads infer `T` from the slice, so `append([]uint16, replacementChar)` (or `append(buf, 7, 8)`) would pick `slice<int>` and fail (CS0121 / CS0029). The converter therefore casts an untyped *numeric*-constant `append` element to the slice's element type, matching Go's implicit conversion:

```go
var a []uint16
a = append(a, replacementChar)   // replacementChar is an untyped const
a = append(a, 7, 8)
```
```csharp
slice<uint16> a = default!;
a = append(a, (uint16)(replacementChar));
a = append(a, (uint16)(7), (uint16)(8));
```

The same cast reaches an untyped numeric constant referenced through a **cross-package SELECTOR**.
`isUntypedNumericConstArg` had matched only a bare `*ast.Ident`, so `append([]byte, tabwriter.Escape)`
(go/printer's block builder — `tabwriter.Escape` is `const Escape = '\xff'`, rendered as a golib
`UntypedInt`) kept the ambiguity (CS0121 ×6). The gate now also inspects an `*ast.SelectorExpr`'s `Sel`
constant object, casting the element to the slice's element type: `append(block, (byte)(tabwriter.Escape))`.
That same selector gate also feeds the deferred method-value arg cast — `deferǃ(syscall.Seek, …,
(nint)(io.SeekStart), …)` (internal/poll) now casts the const to the parameter type rather than the
default-type wrap — and the `regexp/syntax` `unicode.MaxRune` append; both are equal-or-better and compile.
A same-package untyped const (a bare ident) is unchanged. (Guarded by the `CrossPkgUser` extension —
`append([]byte, CrossPkgLib.Sep)` (rune `':'`) and `append([]rune, CrossPkgLib.Precision)` (int `2`), both
cross-package untyped consts reached through a selector, output-compared vs Go; without the fix the appends
are CS0121.)

**A string-literal spread** — `append(b, "runtime error: "...)` (runtime `error.go`'s message builder) — renders the literal as a `"…"u8` `ReadOnlySpan<byte>`, which has no spread property (`.ꓸꓸꓸ` → CS1061). The spread emission wraps a direct string-literal source in the member-accessible `@string` — `append(b, ((@string)"runtime error: "u8).ꓸꓸꓸ)` — whose `ꓸꓸꓸ` returns the `Span<byte>` the `append<T>(slice<T>, params Span<T>)` overload binds; this is the same wrap the `string(r)...` conversion spread uses (above). A non-literal spread source (a slice, a `@string` variable) is unchanged. (Guarded by the `StringConvPostfix` extension — two literal spreads appended and value-compared vs Go.)

**A string-literal CONCAT as an object/interface vararg argument** — runtime `stack.go`'s newline+tab join in `print`'s diagnostics — needs the same u8 suppression the direct literal argument already gets, propagated INTO the `BinaryExpr`'s operands: both halves otherwise render as `"…"u8` spans, and a `ReadOnlySpan<byte>` cannot box to `object` (CS1503) nor be `+`-concatenated. The binary-expression conversion now honors an incoming `BasicLitContext.u8StringOK=false`, so the operands render as plain C# strings whose `+` and boxing are fine; the default context leaves every other path unchanged. (Guarded by the `StringConvPostfix` extension — a concat with an escape into an `fmt.Println` vararg plus a nested three-way concat, values vs Go.)

Typed arguments and already-explicitly-converted elements (`uint16(r)`) are left as-is.

Relatedly, when the shifted (left) operand of a shift is an untyped constant — `1 << k` — Go gives the whole shift the type it assumes from context (e.g. `uintptr` when compared with a `uintptr`), but the bare C# literal makes the result `int`, which then cannot compare or combine with the typed operand (CS0034). The shift result is cast to its resolved type:

```go
var u uintptr = 7
_ = u < 1<<8   // 1<<8 takes type uintptr
```
```csharp
uintptr u = 7;
_ = u < (uintptr)(1 << (int)(8));
```

A C# **compound shift-assignment** (`<<=`/`>>=`) requires the shift count to be `int`; the count's own (possibly unsigned/native-width) type is rejected — `s.allocCache >>= (nuint)x` is CS0019. So the count is cast to `int` (`s.allocCache >>= (int)x`). This applies whether the assignment target is a simple variable or a **selector/pointer-field** LHS (`s.allocCache`, a field reached through a `*mspan`) — both paths emit the same `(int)` count cast. (Guarded by the `ShiftPrecedenceUnsigned` behavioral test — simple-variable and struct-field shift-assigns with an unsigned count; runtime hits the field form in `malloc`/`mbitmap`'s `allocCache` bit walks.)

A related case is a **computed constant mask under a native-int bitwise operator**. `i & ((1 << shift) - 1)` or `i &^ (blockSize - 1)`, where `i` is a `uintptr`/`uint` (C# `nuint`/`uintptr`) and `shift`/`blockSize` are native-int constants: the mask is a Go compile-time constant, but because the native const is emitted as a `static readonly` (not a C# `const`) the *expression* is not a C# constant, so it renders as a bare `int` — and `nuint & int` is CS0019 (no common type, and no implicit constant conversion since the operand is non-constant). The converter casts such a computed-constant operand to the native result type — `(uintptr)i & (uintptr)((1 << (int)shift) - 1)`. A *small* bare literal (`x & 7`) is left alone (C#'s constant conversion fits it), but a **large** literal whose value exceeds the C# `int32` range (`uintptrMask & 0x00ffffffffff`) is emitted by `convBasicLit` with its own `(nint)`/unsigned cast — so it is no longer a bare `int` and `nuint & nint` is CS0019 too; such a literal operand is cast to the native result type the same way (`& (uintptr)(nint)1099511627775L`). A named untyped-const reference is handled by the wrapper cast below. There is also a `&^` (AND-NOT) twist: it is rendered `& ~y`, and `~` promotes its operand to `int`, so even a *small* constant operand (`p &^ 15` → `nuint & ~15` = `nuint & (int)-16`) is CS0019 — a negative `int` cannot convert to an unsigned native type, even as a constant. So a constant right operand of `&^` with a native-int result is also cast to the native type, `& ~(uintptr)15`, performing the complement in that width (a non-constant native operand, `p &^ mask`, already complements correctly and is left alone). (All guarded by the `NativeIntConstMask` behavioral test — computed mask, large-literal mask, and small-literal `&^`; runtime exercises this in arena/page mask arithmetic such as `arenaIndex`/`alignDown`, `mallocinit`'s `uintptrMask &`, and `os_windows`'s `ptr &^ 15` 16-byte align.)

Similarly, when a *named* untyped numeric constant (emitted as the `UntypedInt`/`UntypedFloat` wrapper) is an operand of arithmetic with a concrete numeric type, the wrapper's bidirectional implicit conversions can make the result resolve to the wrong type (`a * two32`, `uint64 * UntypedInt`, yields `int` — CS0029). The named-const operand is cast to the concrete operand's type (comparisons resolve through the implicit conversion, so only arithmetic is cast):

```go
const two32 = 1 << 32
var a uint64 = 100
_ = a*two32 + 3
```
```csharp
internal static readonly UntypedInt two32 = 4294967296;
uint64 a = 100;
_ = a * (uint64)two32 + 3;
```

A constant too large for `int64`/`uint64` (or `float64`) is emitted as `GoUntyped` (= `System.Numerics.BigInteger`), which has no implicit operator with the built-in numeric types — so it is cast in *comparisons* too, not just arithmetic (`x > Two129` where `Two129 = 1<<129`):

```csharp
public static readonly GoUntyped Two129 = /* 1 << 129 */ ...;
_ = x > (float64)Two129;
```

### The `&^=` (bit-clear) compound assignment on a narrow type
C# has no `&^` (AND-NOT) operator, so Go's `a &^= b` expands to `a &= ~b`. The `~` complement always promotes its operand to `int`, and `int` is not implicitly convertible to a narrower or unsigned LHS type (`byte`/`ushort`/`uint`/`ulong`/`uintptr`/`nuint`) — so `flags &= ~b` is CS0266. The complemented value is therefore cast back to the LHS type, inside `unchecked` because for a *constant* operand `~b` folds to a negative `int` constant whose checked narrowing would overflow (CS0221):

```go
h.flags &^= hashWriting   // h.flags is uint8
```
```csharp
h.Value.flags &= unchecked((uint8)~hashWriting);
```

An LHS type that `int` widens to implicitly (`int`/`int32`/`int64`) needs no cast and stays `a &= ~b`. (Guarded by the `AndNotAssignNarrow` behavioral test, which exercises both an ident LHS and a struct-field LHS — they route through different assignment-emission paths.)

### Logical operators on a named boolean type cast through `bool`
A Go defined type whose underlying type is `bool` (`type boolVal bool`) is modeled as a `[GoType("bool")]` struct with an implicit `bool` conversion but no logical operators. Go's `!`, `&&`, and `||` on such a value yield that **same named type**, so `return !y` / `return x && y` in a function returning an interface the type implements (go/constant's `UnaryOp`/`BinaryOp`, returning the `Value` interface) still satisfies the interface. A bare `!y` / `x && y` in C# collapses to a plain `bool` — which cannot implicitly convert to the interface (CS0029), and `!` has no operator on the struct (CS0023). The converter casts each operand through `bool`, applies the operator, then casts the result back to the named type so it keeps satisfying the interface:

```go
case boolVal:
    return !y          // y is boolVal, result must be the Value interface
```
```csharp
case boolVal y: {
    return ((boolVal)(!(bool)y));
}
```

Binary `&&`/`||` take the parallel form `((boolVal)((bool)x && (bool)y))`. A predeclared-`bool` operand keeps the bare `!x` / `x && y` form (no golden churn). (Guarded by the `NamedBooleanLogic` behavioral test.)

### Casting a negative value to a non-keyword type parenthesizes the operand
C# parses `(T)-value` as a cast only when `T` is a keyword primitive (`int`, `long`, `nint`, `byte`, …). For a using-**alias** (`int64`=`long`, `uint64`=`ulong`, `rune`=`int`, …) or a `[GoType]` **named** type (`level`), `(int64)-1` / `(level)-1` is instead parsed as `type MINUS value` — CS0075 ("to cast a negative value, you must enclose the value in parentheses") and CS0119 ("'long' is a type, not valid in the given context"). So a cast whose operand leads with a unary `+`/`-` and whose target is not a C# keyword type parenthesizes the operand:

```go
lvl := level(-1)              // named conversion
mask := -1 << uint(bits)      // int64-typed wide shift
```
```csharp
var lvl = ((level)(-1));
var mask = ((int64)(-1) << (int)((nuint)bits));
```

Two emission sites carry it: the type-conversion cast (convCallExpr, `castOperandNeedsParens`) covers `level(-1)`/`int64(-1)`, and the wide-shift left-operand cast (convBinaryExpr) covers `-1 << bits` (a wide shift type does not promote to `int`, so its left operand is cast to that type). A keyword target (`(int)-1`, `(nint)-1`) and a non-negative operand keep the bare form (no golden churn). (Guarded by the `CastNegativeNamedType` and `ShiftNegativeWideConst` behavioral tests.)

## The "nil" Value
In Go, `nil` is the equivalent of C# `null`. Where possible, converted code uses the golib [`NilType`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/NilType.cs) with a default instance called `nil` (defined in [`go.builtin`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/builtin.cs)). `NilType` provides comparison operators so `x == nil` / `x != nil` work across the runtime types (slices, maps, channels, pointers, interfaces), each of which defines what "nil" means for it (e.g. a `map<K,V>` whose backing dictionary is null is the nil map: reads return the zero value, `len` is 0, ranging yields nothing, and a write panics — matching Go).

The same null-safe-zero-value principle applies to value types whose backing store is a reference. A zero-value `string` converts to `@string s = default!`, which runs no constructor, so the backing `byte[]` is null. Rather than [NRE](Glossary.md#nre) on the first read, `@string` treats a null backing as Go's empty string `""` for every read — length 0, no bytes to index/range, `== ""` is true, prints empty, and concatenation yields the other operand (`var s string; s += "x"` → `"x"`). Constructors still allocate, so only the `default(@string)` zero value relies on this. (Guarded by the `StringZeroValueConcat` behavioral test.)

### Pointer-to-interface assignment through selector fields
A selector assignment whose LHS field is an interface (`h.d = s`) uses the type of the **whole selector expression**, not just the selected identifier name, when deciding whether to wrap the RHS in an interface adapter. If the RHS is a pointer-typed identifier, the adapter receives the pointer box so a dereferenced value alias is not copied into a pointer-only implementation. The generated form matches other pointer-to-interface conversion sites:

```go
func assignDescriber(h *holder, s *Setting) {
    h.d = s
}
```
```csharp
internal static void assignDescriber(ж<holder> Ꮡh, ж<Setting> Ꮡs) {
    ref var h = ref Ꮡh.Value;
    ref var s = ref Ꮡs.Value;

    h.d = new SettingжDescriber(Ꮡs);
}
```

This is intentionally keyed on selector/index expression type instead of the root identifier, so struct fields such as `go/types`' `operand.expr ast.Expr` and ordinary behavioral fields both take the same path. Guarded by `PointerInterfaceStructField`, including the assignment case after the struct-literal cases.

## Empty Interface
In Go, every type satisfies the method-less interface `interface{}`, now spelled `any`. This operates fundamentally like .NET's `System.Object`, so the converter maps the Go empty interface to `any` (a global alias for `object`). For example, a Go `func(i interface{})` becomes `void f(any i)`, and a `map[any]string` becomes `map<any, @string>`.

### A string literal returned as `any` boxes through `@string`
A Go string literal normally emits as a `"…"u8` `ReadOnlySpan<byte>` (which converts implicitly to `@string`). But a `ReadOnlySpan<byte>` has **no conversion to `object`**, so a string literal RETURNED (or returned as a tuple element) where the result type is the empty interface fails with CS0029 — testing's `func (f *chattyFlag) Get() any { return "test2json" }`. Such a result must box a golib `@string` (preserving Go string identity for a later `x.(string)` assertion), so `visitReturnStmt` renders the literal as `(@string)"…"` for an empty-interface result element:

```csharp
[GoRecv] internal static any Get(this ref chattyFlag f) {
    if (f.json) {
        return (@string)"test2json";   // NOT "test2json"u8 (CS0029)
    }
    return f.on;
}
```

`resultParamIsInterface` excludes the empty interface (`andNotEmptyInterface`), so the interface-conversion arm never fires for `any`; the per-element context sets `u8StringOK` off and `castToGoString` on instead. Only string basic-literals consult those flags, so a non-string `any` result is unaffected. Also corrects a latent semantic bug in the multi-result form (`return "<no value>", true` from a `(any, bool)` result rendered a raw C# string, which would fail a Go `x.(string)` assertion). Guarded by `InterfaceCasting`.

The same boxing applies to an **assignment** whose target's static type is the empty interface — a plain
local (`arg = "<nil>"`, go/types format.go's sprintf over an `any` range variable, CS0029), a
selector/index target (`h.value = "field"`), and a mixed-statement reassignment all render the literal
`(@string)"…"`. `visitAssignStmt` threads the same `u8StringOK`-off / `castToGoString`-on literal context
into each RHS conversion site when `lhsIsEmptyInterface` reports the target is `any` (the NON-empty
interface wrap stays with `convertExprToInterfaceType`, which the empty interface deliberately bypasses).
(Guarded by the `AnyStringLitAssign` behavioral test — an `any` local, an `any`-typed range variable, and
an `any` struct field each assigned a string literal, then type-switched on `string`, output-compared vs Go.)
The same boxing applies to every **composite-literal** position whose declared slot type is the empty
interface — the interface-wrap machinery deliberately bypasses `any` there too, so a string-literal
element otherwise renders either as the u8 span (no conversion to the generated `object` slot —
CS1503/CS0029) or as a bare C# string (compiles, but boxes a `System.String`, so a later Go
`x.(string)` assertion or `case string:` fails at runtime):

```go
type pair struct { label string; value any }
p  := pair{"tag", "val"}          // positional field
n  := &node{inner: "hi"}          // keyed field (typed, elided, and pointer-elided forms alike)
m  := map[string]any{"k": "mv"}   // map value
mk := map[any]int{"ky": 7}        // map key
s  := []any{"a", "b"}             // slice/array element
sp := [3]any{1: "sp"}             // sparse-array element
```

```csharp
var p  = new pair("tag", (@string)"val");            // NOT bare "val" (wrong boxed identity)
var n  = Ꮡ(new node(inner: (@string)"hi"));          // NOT "hi"u8 (CS1503)
var m  = new map<@string, any>{["k"u8] = (@string)"mv"};
var mk = new map<any, nint>{[(@string)"ky"] = 7};
var s  = new any[]{(@string)"a", (@string)"b"}.slice();
var sp = new array<any>(3){[1] = (@string)"sp"};
```

Keyed elements resolve their target slot in `convKeyValueExpr` (struct field via `info.Uses`; map/sparse
element and map key via the threaded composite type) and take the same `u8StringOK`-off /
`castToGoString`-on literal context; positional struct fields and slice/array elements flip the
per-element flags (`u8StringArgOK` off, `useGoStringArg` on) that `convExprList` feeds each element's
literal context. A TYPE-PARAMETER slot is excluded even though its underlying constraint is an
interface (`isEmptyInterfaceTarget`) — a `~string`-constrained field takes the literal directly. Only
string basic-literals are affected; every non-`any` slot keeps its exact prior form. (Guarded by the
`AnyStringLitComposite` behavioral test — all the shapes above, each read back through a `string`
type-switch to prove runtime identity, output-compared vs Go.)

The same boxing applies to a **channel send** whose element type is the empty interface — both the
statement form and the select-case registration form. The send value previously converted with no
target-type context at all, so the literal's default `"…"u8` span failed against the channel's
`in object` send parameter (CS1503):

```go
ch := make(chan any, 1)
ch <- "text"                    // statement send
select { case ch <- "sel": … }  // select-case send (registration form)
```

```csharp
ch.ᐸꟷ((@string)"text");                              // NOT "text"u8 (CS1503)
switch (select(ch.ᐸꟷ((@string)"sel", ꓸꓸꓸ))) { … }    // registration form takes the same box
```

Both send positions route through a shared `convSendValueExpr` (`visitSendStmt.go`), which resolves
the channel's ELEMENT type and applies the same `isEmptyInterfaceTarget`/`isStringBasicLit` gate as
the assignment and composite-literal positions (a type-parameter element is excluded; only string
basic-literals are affected). The same helper also activates the NON-empty interface element wrap —
see [Maps and Channels](#maps-and-channels). (Guarded by the `AnyStringLitChanSend` behavioral test —
statement and select-case sends read back through a `string` type-switch and an `x.(string)`
assertion to prove runtime identity, output-compared vs Go.)

## Inline Assignment Order of Operations
All right-hand operands in assignment expressions in Go are evaluated before assignment to the left-hand operands. C# can operate equivalently using tuple deconstruction (_thanks to Eugene Bekker for the [suggestion](https://github.com/GridProtectionAlliance/go2cs/issues/6)_). For the following Go code:

```go
x, y = y, x+y
```
the equivalent C# code operates as follows:
```csharp
(x, y) = (y, x + y);
```

Go's **partial redeclaration** — `a, b := f()` where `a` already exists in the same scope — reuses `a` (assigns it) and declares only the new names. A blanket `var (a, b)` would re-declare the reused variable, so the converter emits `var` per *newly-declared* element only:

```go
frac, e := normalize(frac)   // frac is the existing parameter; e is new
```
```csharp
(frac, var e) = normalize(frac);
```

The same per-element mechanism handles a destructured element whose **address is taken** (`list, delta := netpoll(0); injectglist(&list)`). Such a local must be heap-boxed so its `Ꮡlist` companion exists, but the combined `var (list, delta) = …` deconstruction cannot declare it as a `ref var … = ref heap(…)`. The converter emits the escaping element's heap declaration first, then a mixed deconstruction-assignment in which the escaping element is the pre-declared box ref-local and the rest declare with `var`:

```go
list, delta := netpoll(0)
injectglist(&list)
```
```csharp
ref var list = ref heap<gList>(out var Ꮡlist);
(list, var delta) = netpoll(0);     // list is the box ref-local; delta is newly declared
injectglist(Ꮡlist);                 // Ꮡlist now exists
```

Without this, `&list` emits `Ꮡlist` with no box (CS0103), and the `Ꮡ(value)` copy fallback would silently lose writes made through the pointer. (Guarded by the `TupleDestructureEscapingLocal` behavioral test — a mutate-through-pointer proves the real local is updated; runtime exercises it in the `netpoll` poll loops.)

A subtler case: a newly-declared tuple element can be flagged *escaping* by analysis yet need **no** heap box — typically an already-pointer local that is merely returned (`pp, now := pidleget(now)`, where `pp` is a `*p` that the function returns). The heap-decl path above only owns elements that produce an actual `ref var … = ref heap(…)`; an escaping element with no such declaration must still be counted as newly-declared so it receives its `var`, or the deconstruction emits `(pp, now) = …` with `pp` declared nowhere (CS0103). Both the mixed (`(var pp, now) = …`, reusing the value parameter `now`) and the all-shadowing (`var (ppΔ1, gpΔ1) = …`) forms are handled. (Guarded by the `TupleMixedDeclareReassign` behavioral test; runtime hits it in `pidlegetSpinning` and `findRunnable`.)

**A tuple deconstruction into INTERFACE variables hoists the call when a component needs converting.** Reassigning a multi-value call into pre-declared interface locals (`c, err = sd.dialTCP(…)` with `var c Conn`) can require a per-component interface conversion C#'s tuple assignment cannot perform implicitly — a `ж<TCPConn>` component satisfies `Conn` only through its generated pointer adapter, an *explicit* conversion (CS0266 ×11 in net's dial.go). Mirroring the return-statement tuple arm, the call is hoisted into temp markers and each component converts in a tuple literal:

```csharp
var (ᴛ1, ᴛ2) = Ꮡsd.dialTCP(ctx, laΔ1, raΔ1);
(c, err) = (new TCPConnжConn(ᴛ1), ᴛ2);
```

The arm fires only for a statement-position deconstruction (one call RHS, several LHS) where some non-empty-interface target's tuple component is a non-identical, non-interface type; all other deconstructions keep the direct form. (Guarded by the `InterfaceCasting` extension `makeCounter` — a `(*Counter, error)` call deconstructed into an `Incrementer` — runtime-verified against Go.)

### An address-taken reference-typed local heap-boxes too — `Ꮡ(value)` copies are only for reads
An INHERENTLY heap-allocated local (interface/pointer/slice/map/chan/func) is already a
reference, so escape analysis blanket-marks it and the box machinery historically skipped it —
`&local` fell back to the `Ꮡ(value)` **copy** constructor. That is only sound when nothing
writes through the pointer: dwarf's `zeroArray(&typ)` (with `typ Type`, an interface local)
writes `*t = &tt` in the callee, and the copy-box silently dropped the write (C# printed the
un-replaced value — a behavioral divergence, not a compile error). The box predicate
(`identHasHeapBox`) now boxes such a local when its address is **genuinely taken** — by a
capturing closure (the pre-existing box-ref-var case) or anywhere in the current function
(memoized `&ident` scan) — so `&swapped` references a real aliasing box:

```go
var swapped Animal = Dog{}
replaceAnimal(&swapped)      // callee: *a = &Cat{}
```
```csharp
ref var swapped = ref heap<Animal>(out var Ꮡswapped);
swapped = new Dog(nil);
replaceAnimal(Ꮡswapped);     // callee writes through the SAME box — "Meow!"
```

Details: the box declaration always uses the parameterless `heap<T>(out …)` form for these
(`new Animal()` on an interface is CS0144, and the reference-like zero value is exactly what
the box provides); a `[]T` slice local routes to `heap<slice<T>>` (the array-branch prefix
test mistook `[]` for an array and emitted a mismatching `heap<array<T>>`); and the
pointer-form ident render in convIdent deliberately keeps the PLAIN value render for these
locals (`new Middle(Inner: inner)` wants the held pointer; only an explicit `&inner` wants
the `ж<ж<T>>` box, via convUnaryExpr). Non-escaping and never-addressed reference locals are
unchanged (no churn). (Guarded by `InterfaceCasting`'s `replaceAnimal` — the swap is visible
through the original variable; the churned goldens `PointerToPointer`,
`UnsafePointerReinterpret`, `DerefPointerToField`, `PointerCastSliceRange`,
`EscapedLoopVarSiblingIndex` all re-verified against Go.)

### A field-addressed value local heap-boxes — `Ꮡ(x).of(…)` copy-boxes orphan writes
Escape analysis's address-of walk marked `&x` (direct) and `&x[k]` (element) but had **no
selector arm**, so a value-struct local whose FIELD address was taken in plain assignment
(or composite-literal / return) position stayed unboxed, and convUnaryExpr fell back to the
`Ꮡ(x).of(T.Ꮡval)` **copy**-box — writes through the pointer landed in the copy and were
silently lost (Go reads the write back through `x`; C# printed the original value — a
behavioral divergence, not a compile error). The walk now peels a value-field selector
chain (`x.f1.…fn`, every hop a direct `FieldVal` selection with no pointer indirection) to
its root ident and marks the root escaping, so the emission routes through the identity box:

```go
x := Thing{val: 7}
p := &x.val
*p = 99
return x.val                       // Go: 99
```
```csharp
ref var x = ref heap<Thing>(out var Ꮡx);
x = new Thing(val: 7);
var p = Ꮡx.of(Thing.Ꮡval);
p.Value = 99;
return x.val;                      // 99 — the pointer aliases x's box
```

Multi-hop chains chain the accessors (`&w.inner.val` → `Ꮡw.of(Wrap.Ꮡinner).of(Thing.Ꮡval)`),
and a field promoted through a VALUE embed roots at the local too (`&o.ev` →
`Ꮡo.of(Outer.Ꮡev)`). A hop that crosses a POINTER — an explicit `w.ptr.val` deref or a field
promoted through an embedded pointer (both are `Selection.Indirect()`) — aliases the
POINTEE's storage instead, so the root deliberately stays unboxed: `w.ptr.of(Thing.Ꮡval)`
already writes through the held box. (Guarded by `LocalStructFieldAddr` — plain, nested,
method-body, value-embed-promoted, composite-literal, and return positions plus the
pointer-hop negative control, all output-compared vs Go; the one churned golden
`UnsafePointerParamPin` — `&h.v` under `unsafe.Pointer` — re-verified.)

A **blank-identifier element** in a split multi-assign is a C# discard, never a declaration. Go's `_, _, _, _ = a, b, c, d` (a common "mark these used" idiom) is emitted as one bare discard per element with **no** `var` — the per-element discard test keys off each LHS ident, not just the single-LHS case, so every blank stays a discard:

```go
_, _, _, _ = fi, fn, gi, gn
```
```csharp
_ = fi;
_ = fn;
_ = gi;
_ = gn;
```
A blanket `var _` on each would declare `_` once and then collide on every later element (CS0128 "a local named `_` is already defined"). (Guarded by the `BlankIdentifierCollision` behavioral test; runtime hits it in `softfloat64`'s `fdiv64`.)

## Short Variable Redeclaration (Shadowing)

When using Go's short variable declaration syntax, e.g., `x := 2`, a variable can be redeclared in a lesser (nested) scope. The inner declaration "shadows" the outer one: the inner instance is manipulated while the outer value is preserved, and once the inner scope ends the outer variable still holds its original value.

C# forbids a local (or a lambda parameter) from shadowing an enclosing local of the same name (CS0136). So rather than the older save/restore approach, the converter **renames** the shadowing inner variable with a `Δ` disambiguation suffix (`x` → `xΔ1`, `xΔ2`, …) and rewrites all references within that scope to the renamed identifier. The outer variable is untouched, so its value is naturally preserved. For example:

```go
func sumWithLenLocal(buf []int) int {
    total := 0
    len := len(buf)       // a local named like the built-in, shadowing it
    for i := 0; i < len; i++ {
        total += i
    }
    return total + len
}
```

converts to:

```csharp
internal static nint sumWithLenLocal(slice<nint> buf) {
    nint total = 0;
    nint lenΔ1 = len(buf);          // renamed; the built-in call stays `len(...)`
    for (nint i = 0; i < lenΔ1; i++) {
        total += i;
    }
    return total + lenΔ1;
}
```

The same `Δ` mechanism handles a local shadowing a called built-in (as above), a nested-block variable shadowing a function-level one, an IIFE/closure parameter colliding with an outer local, and a type-switch guard (`switch x := x.(type)`) whose variable shadows an enclosing one — the guard is renamed within the switch (`case T xΔ1:`) while references after the switch still resolve to the enclosing variable, matching Go's scoping.

The nested-block detection holds across a **closed sibling block**: a declaration that *follows* a nested block inside the same enclosing block (runtime `procresize`'s second `trace := traceAcquire()` after an inner `if {…}` that declared its own `trace`) is still checked against enclosing scopes. The shadow tracker's processing flag is shared across the nesting levels of a block tracker, so an inner block's cleanup must *restore* it for the still-open enclosing block rather than clear it — clearing it made the follow-on declaration skip the check and collide with the function-level local (both emitted `Δtrace` — the LAST runtime compile error, CS0136). Composition with the collision rename is suffix-based: the function-level local keeps the base name (`Δtrace` after its collision prefix), the shadows number independently of the prefix (`traceΔ1`, `traceΔ2` — a shadow name no longer collides, so it takes no `Δ` prefix). (Guarded by the `GlobalShadowedByLocal` extension `nestedBlockShadow` — the three bindings verified by value vs Go.)

A local shadowing a **same-named package function it calls in its own initializer** — `signame := signame(gp.sig)` (runtime `panic.go`) — renames the same way. Go starts the shadow *after* the initializer, so the call resolves to the function; C# scopes the local over its own initializer, so an unrenamed call would bind the (non-invocable) string local (CS0149). Detection is object-accurate: any identifier `go/types` resolves to the *function* while a same-named local exists means Go bound it where the local was not yet in scope — the old position guard ("call before the declaration") excluded exactly the own-initializer case. (Guarded by the `BuiltinShadowLocal` extension — a package `signame` shadowed in its own initializer, values vs Go.)

A **block-scoped `const`** that shadows an enclosing parameter or variable is renamed the same way. `func f(ns int64) { …; const ns = 10e6; use(ns) }` (runtime `notetsleep_internal`) is legal in Go but the inner `const ns` and the param `ns` both emit as `ns` in C# (CS0136). A const is tracked separately from variables — its `go/types` object is a `*types.Const`, not the `*types.Var` the scope stack records — so the shadow-rename pass had ignored it; it now records a shadowing const (detected by the same by-name enclosing-scope check) and rewrites its declaration and every use to `nsΔ1`, leaving the enclosing `ns` untouched. Only a *shadowing* const is renamed (a plain block const keeps its name, no churn). (Guarded by the `ConstShadowsParam` behavioral test — the inner uses bind the const value, the outer uses bind the param.)

Renaming depends on correctly identifying which declarations are *function-level* — the set a nested variable of the same name must avoid (C# forbids the nested one even when the function-level one is declared *later*). A `for init; …` loop's `:=` variable, and a range `:=` key/value, are scoped to their own statement, **not** the function body, so they are deliberately excluded from that set. Recording a for-loop variable as function-level (it is encountered first, in source order) would mask the real function-level variable of the same name declared afterward — `for b := …{} for b := …{} … b := newBucket(…)` — leaving *all three* emitted as `b` and colliding (CS0136). With the for-loop variables correctly treated as inner scopes, they are renamed `bΔ1`/`bΔ2` while the function-level `b` keeps its name. (Guarded by the `ForVarMasksFuncLevel` behavioral test; runtime hit this in `stkbucket`.)

The same forward-collision rule applies at **every block level, not just the function body**. C# CS0136 fires whenever a name is declared in two scopes where one encloses the other, *regardless of declaration order* — so a nested variable must be renamed if the same name is declared anywhere in an enclosing block, whether that declaration appears before or after it in source. The scope-stack walk only records declarations already seen (backward), and the function-level forward set covers only the function body; a variable declared **later in an intermediate enclosing block** would otherwise be missed. To close that gap, each block scope (function body, `if`/`for`/`range`/`switch`/`select` bodies, bare blocks, and `case`/comm-clause bodies) is pre-scanned for its directly-declared names (`:=` and `var`, excluding a control statement's own init `:=`, which is scoped to that statement) when the scope is pushed, so forward declarations are visible to the shadow check. For example, the runtime's `runGCProg` has two `for off := …` loops followed by `off := n - nbits` *in the same enclosing `for {}` body* — the block-level `off` encloses both loops, so the loop variables are renamed `offΔ1`/`offΔ2` while the block-level `off` keeps its name. (Guarded by the `ForVarMasksBlockLevel` behavioral test — distinct from `ForVarMasksFuncLevel`, where the later same-named variable is function-level; this cleared 5 runtime CS0136 in `runGCProg`/`mprof`/`runtime1`/`time`.)

The mirror image — a local shadowing a package-level **GLOBAL** — is resolved the other way: the *global reference* is qualified rather than the local renamed. C# locals are function-scoped, so a local `trace := traceAcquire()` shadows a same-named global `var trace` throughout the function, and an *earlier* read of the global binds to the not-yet-declared local (CS0841; the wrong variable regardless). Renaming the local is the fragile, entangled path (it interacts with collision renames and the shadow-rename counter); instead a use whose ident resolves to a package-level var **of this package** — while a same-named function-level local is declared — is emitted qualified with the package static class: `runtime_package.Δtrace.minPageHeapAddr`, which a local can never shadow. This is the same package-class qualifier the box-field accessor uses for a shadowed owning type (below). Runtime's `traceallocfree.traceSnapshotMemory` reads the global `trace.minPageHeapAddr` before its local `trace := traceAcquire()` (both collision-renamed `Δtrace`); the qualifier is gated so an ordinary global (no shadowing local) and the local's own uses (which resolve to the local, not the package scope) keep their bare, Go-like form — no churn. (Guarded by the `GlobalShadowedByLocal` behavioral test — a collision-renamed global and a plain global each read before a same-named local; cleared runtime's last CS0841.)

Two subtleties complete this for loops whose variable's box hoists before the loop. **A hoisted loop-variable box is block-scoped in C#, one per name per container.** A loop variable that escapes to the heap *and whose box is emitted before the loop* — today that is a string/int/chan/func **range** variable, or the legacy fallback of a `for i := …` clause variable referenced by a *clause* func literal; every other case boxes per-iteration inside the body (slice/array/map ranges via the deferred range-var box, and `for` clause variables via the per-iteration carrier rewrite — Go 1.22 semantics, see [Break / Continue Labels](#break--continue-labels)) — is emitted as a `ref var i = ref heap<…>(out var Ꮡi)` declaration hoisted into the *enclosing container* (function body, block, or switch/select clause) — see [Pointers](#pointers) — so other loops in that container that reuse `i` genuinely collide with it, unlike the ordinary all-loop-scoped case above. Loop variables are therefore grouped **per container and name**: the first whose box *actually claims a container-level name* is the keeper and keeps its name; every other direct-child loop variable with that name in the same container is force-shadow-renamed. The claim test mirrors the emission exactly — the var escapes AND is not inherently heap-allocated (a pointer/slice/map/chan/interface/func var is already a reference and gets no box) AND the box actually hoists (per the split above). A group with no claiming var is untouched, so ordinary same-named sibling loops keep their Go names — a claiming sibling would otherwise emit a *duplicate hoisted box* in the same scope (CS0128), and a non-claiming sibling's loop-scoped variable (or deferred in-body box) nests inside the block that owns the box name (CS0136). (The historical motivating cases — runtime `typesEqual`'s `for i := 0` pair inside one switch case and `runqputslow`'s three `for i := …` loops — now box per-iteration inside their bodies and no longer claim container names at all; `EscapedLoopVarSiblingIndex` keeps guarding the sibling grouping and was re-baselined to the per-iteration shape.) A function-body-level keeper is additionally recorded as function-level (so non-loop uses elsewhere shadow-rename as before), but never masks a real function-level declaration — preserving the `ForVarMasks…` invariant above. A name group with no escaped variable is untouched (loop-scoped in C# too — no churn).

**Escape analysis marks only the arg's storage ROOT, not every identifier in a pointer argument.** Passing an expression to a pointer parameter escapes the storage the pointer refers to — the *peeled root* of a literal `&expr` (through parens, field selectors, index expressions, and derefs), or the bare identifier itself. An identifier appearing merely in a *subexpression* of the argument contributes a value, not its own address: in `xs[i].link(&xs[i+1])` or `typesEqual(tin[i], vin[i], seen)` the container (`xs`/`tin`'s elements) escapes but the index `i` does not. The old contains-anywhere check heap-boxed every such loop index — a spurious allocation on a hot path (Go keeps these in registers), gratuitous `Ꮡi` machinery in the emitted code, and the very duplicate-hoist collisions the grouping above then had to resolve (`typesEqual`'s pair now emits two plain `for (nint i = 0; …)` loops, no boxes, no renames). A direct `&i` anywhere — including nested inside a larger argument — is still caught independently by the address-of analysis. **And a renamed variable used as an LHS index/map key is rewritten there too.** An assignment `a[i] = …` / `m[ns] = …` / `p.f[k] = …` reassigns the *root* (`a`/`m`/`p`); the index/key expression is a separate value, so a shadow-renamed variable used there (`a[iΔ1]`, `m[nsΔ1]`) must be rewritten by descending the target's index/selector/deref chain and renaming each index. Missing this is a *silent* bug — the LHS key kept the enclosing variable's name, so `m[ns] = nsΔ1*100` wrote to the wrong key with no compile error — as well as a CS0136/CS0165 once the loop variable itself is renamed. (Both guarded by the `EscapedLoopVarSiblingIndex` behavioral test — the array case would not compile and the map case would silently return the wrong value without the pair, its `boxedSiblings` extension covers two genuinely-escaping siblings in one switch case (both take `&i`; first keeps the name, second renames), and its `caseSiblings` extension proves the index-only pair stays UNBOXED; cleared the 2 `runqputslow` CS0136, a CS0841, and the 2 `typesEqual` CS0128.) The target-chain descent also visits a **method-call receiver** in the chain — `x.ptr().Value.next = …` (runtime `stackpoolalloc`, where the loop `x` is renamed `xΔ1` because a func-body `x` is declared after the loop). The `x` is buried inside the `x.ptr()` call, past the selector/index steps, so without visiting the call the use kept the raw `x` — read before its (later) declaration → CS0841, or a silent wrong bind. Visiting the whole call renames its receiver and argument identifiers (the call's result is the navigated base, so the descent stops there). (Guarded by the `ShadowedVarMethodCallLHS` behavioral test — write-through through the method verified vs Go; cleared the `stack.cs` CS0841.)

The reverse collision — a package **method named like a built-in** — needs the opposite treatment. In Go a method `func (b *pageBits) clear()` and the universe `clear` built-in coexist: the method is only ever reached as `b.clear()`, while a free `clear(s)` is always the built-in. But the method is emitted as a `clear(this ref pageBits)` extension on the package's static class, and C# member lookup binds that same-class member for an *unqualified* free `clear(s)` call — shadowing the using-static `go.builtin.clear` and failing (`CS1620`/`CS1503`). So a built-in call whose name the package also declares as a method/function is emitted **qualified** — `builtin.clear(s)` — which resolves to the golib built-in regardless of the same-class shadow; the method call stays `b.clear()`. (This also required golib to gain the Go 1.21 `clear` built-in itself, in slice/span/map forms — plus an `IMap<TKey, TValue>` overload for a **named map type's value**: the generated wrapper implements `IMap<K,V>` and forwards to the shared underlying map, so `clear(h)` on an `http.Header`-style named map empties the caller's storage, and a nil named map stays a no-op (net/http/httputil's `clear(h)`, CS0411 without it). Guarded by the `ClearBuiltinShadow` behavioral test — including a named-map value cleared through an alias and a nil named map; runtime hit the original shadowing on `pageBits.clear`/`sweepClass.clear`, ~11 errors.)

For a **function-literal parameter** that shadows an enclosing local, the rename must reach the parameter *declaration* itself, not just the body: `run(func(n int){ … n … })` where an outer `n` is in scope emits `run((nint nΔ1) => { … nΔ1 … })`. The body's uses already resolve to `nΔ1`; if the signature still declared the bare `n` (the raw name), the body's `nΔ1` would be undeclared (CS0103). The parameter name in the emitted lambda signature therefore comes from the same shadow-aware identifier mapping as the body (the raw name when nothing is shadowed, so plain function types and non-shadowing parameters are unchanged). (Guarded by the `ClosureParamShadow` behavioral test; the runtime hit this pervasively on `mcall`/`systemstack(func(gp *g){…})` where the closure's `gp` shadows an outer `gp`, ~40 CS0103.)

Conversely, a **local that shadows a *pointer parameter*** must not inherit the parameter's special emission. A deref-aliased pointer parameter is `ж<T> Ꮡp` with `ref var p = ref Ꮡp.Value`, so passing it whole to a `*T`-expecting function emits its box `Ꮡp`. But a *local* `t` shadowing a `t *T` parameter (`func mapKeyError2(t *_type, …){ … var t *_type; … }`) is a plain pointer local — passing it should stay `use(tΔ2)`, not `use(ᏑtΔ2)` (the spurious `&` references an undefined `ᏑtΔ2` box → CS0103). The bug was that the "is this a parameter?" check matched by *name*, so the shadowing local was misclassified; it now verifies the resolved object is genuinely one of the function's parameter objects, not just a name match. (Guarded by the `ShadowedPointerParam` behavioral test; runtime hit this on `mapKeyError2`/`interhash`'s inner `var t *_type`, ~11 CS0103.)

### Type-vs-Method Name Collisions

Go keeps types and methods in separate namespaces, so a package may legally declare both a type `foo` and a method `foo` on some receiver. In C# both land in the same package class — the nested type and the `[GoRecv]` extension method — where a type and a method cannot share a name (CS0102). The converter resolves this by `Δ`-prefixing the **type** (`Δfoo`) while the method keeps its core-sanitized name (`foo`), so they no longer collide.

This needs an extra step when the colliding name is also a **golib reserved word** (`slice`, `array`, `channel`, `map`, …). Such a name is `Δ`-prefixed *anyway* — to avoid the golib runtime type (`slice<T>` etc.) — so the method too becomes `Δslice`, and the plain `Δ` no longer separates type from method. In that case the converter appends the type marker `ᴛ` to the **type** only, giving it a name distinct from the method:

```csharp
[GoType] partial struct Δsliceᴛ { … }                          // Go `type slice struct{…}`
[GoRecv] internal static Δsliceᴛ Δslice(this ref builder b, …) // Go `func (*builder) slice(…)`
```

Only the type side is renamed; the method (and every call site and go2cs-gen-generated pointer-receiver overload) stays `Δslice`. This is deliberate: the go2cs-gen generators compute method names independently, so renaming the *method* would desync them — renaming the *type* keeps the converter and generators in agreement (the generators read the type name from the emitted C# syntax/attributes). This mirrors the Go runtime's `type slice struct{…}` (the GC slice header) versus `func (*userArena) slice(…)`.

A **struct field** named like a colliding package-level identifier is *not* renamed: a field is struct-scoped (`g.trace` does not collide with a package type/method `trace` in C#), so the field declaration keeps its core-sanitized name (`trace`). The box-field accessor static the `TypeGenerator` emits for it is therefore `g.Ꮡtrace` (the `Ꮡ`-prefixed declared member name). The converter's `&g.field` address form (`Ꮡg.of(g.Ꮡtrace)`) must use that **declared** field name — it derives the accessor member from `getCoreSanitizedIdentifier` plus the type-colliding rename, *not* from the general identifier path that applies the package-level collision `Δ`-rename. Using the latter would emit `g.ᏑΔtrace`, which has no matching generated static (CS0117). Reserved-word fields keep their `Δ` (the field really is declared `Δarray` for a field named `array`), so the accessor is `Ꮡ`+the declared name in every case. (Guarded by the `CollisionFieldBoxAccessor` behavioral test; runtime hit this on `g`/`m`/`p`'s `trace`/`stack`/`p` fields, ~20 CS0117.) The generated accessor's **accessibility** matches the *field's* (its exportedness), not the field type's name — an exported field `Fun [1]uintptr` (C# `array<nuint> Fun`) yields a **public** `ᏑFun`, so another package's `other.of(ITab.ᏑFun)` can reach it; deriving the scope from the type's simple name (`array` → lowercase → `internal`) would make the cross-package accessor unreachable (CS0117 in runtime's `iface.go` walking `abi.ITab.Fun`).

One case *does* rename the field: when its name equals its **enclosing type's** name *and* that type is itself `Δ`-renamed for a type-vs-method collision. internal/trace's `type Label struct{ Label string }` sits alongside `func (e Event) Label() Label`, so the type becomes `ΔLabel`; the field, whose name equals the type, is renamed to differ (CS0542 — a member cannot share its type's name). The existing rename prefixed a single `Δ`, but that yields `ΔLabel` — *equal* to the renamed type, so the collision persisted. `typeCollidingFieldName` now **doubles** the marker (`ΔΔLabel`) when the name is a package-level collision, exactly as it already did for the keyword-family case (a reserved-word type is `Δ`-renamed too). Deterministic from the name, so the field declaration, the keyed composite-literal key, and every access site all agree:

```csharp
[GoType] partial struct ΔLabel {                 // Go `type Label struct{ Label string }`
    public @string ΔΔLabel;                      // field name == type name, doubled to differ
}
… new ΔLabel(ΔΔLabel: e.label, …)                // composite key
… l.ΔΔLabel                                      // access
```
(Guarded by `FieldNameTypeMethodCollision` — a `Label` field in a `Label` struct with a colliding `Label()` method, read/written through a value, the method result, and a composite literal.)

The double must also apply **across packages**. `typeCollidingFieldName` keys the double on the current package's `nameCollisions` map, which is populated only for the package being converted — so a **cross-package** access of such a field (internal/trace/testtrace reading a `Label`'s field) emitted the SINGLE-marker `l.ΔLabel` against the declaration's double `ΔΔLabel` — CS1061. The access site now consults the FIELD'S OWN package: `fieldTypeIsRenamed` derives the enclosing named type from the selector and asks `packageHasMethodNamed(type.pkg, type.name)` (a cached per-package scan of every func/method name — a type-vs-method collision Δ-renames the type), threading the result through a new `fieldTypeIsRenamed` ident context so `convIdent` upgrades the single marker to the double for the foreign case (the in-package case already doubled via `nameCollisions`, and its result is left untouched). [CNR](Glossary.md#cnr) byte-identical (the pattern is absent from the single-package corpus except the guard). This is the FIELD-access counterpart to the cross-package renamed **type-reference** substitution (getCSTypeName / getDisplayTypeName, further below) — that one covers naming the renamed *type*, this covers accessing its *field*; internal/trace/testtrace needs both. (The type-reference half — `trace.Time`/`Event`/`Stack` in a func signature, or `*time.Location` as a box element — is **also resolved**: a fresh full reconvert renders `traceꓸTime`/`ж<timeꓸLocation>` correctly through the `convertToCSFullTypeName`→`getAliasedTypeName` path described in *Foreign renamed types reference the recorded imported-type alias* below. It was mis-diagnosed as a still-open root off a stale [overlay](Glossary.md#overlay) whose `importedTypeAliases` were not populated.) (Guarded by the `CrossPkgUser`/`CrossPkgLib` extension — `CrossPkgLib.Marker`, a `Marker` field in a `Marker` struct alongside a `Sensor.Marker()` method, its field read across the assembly boundary through an inferred-type value; vs Go.)

The same struct-scoped rule applies to a **keyed composite-literal field name**. `Frame{funcInfo: f}`, where the field `funcInfo` is named like a colliding package type/method (declared unrenamed as `funcInfo`), must emit the C# initializer key `funcInfo:` — the package-level `Δ`-rename that `convExpr` would apply yields `ΔfuncInfo:`, which is not a parameter name of the generated constructor (CS1739). `convKeyValueExpr` therefore emits a struct-field key whose name collides at package level via `getCoreSanitizedIdentifier` (the declared name), not the general identifier path. (Same `CollisionFieldBoxAccessor` test; runtime hit this on `Frame{funcInfo: …}` in `symtab`.)

The *type* half of the same accessor (`receiver.of(Type.Ꮡfield)`) needs care too. Go code routinely names a local after its own type — `m := getg().m`, where `m` is a `*m` — so taking the address of one of its fields (`&m.park`) emits `m.of(m.Ꮡpark)`, in which the bare type reference `m` binds to the **variable** `m` (a `ж<m>`, which has no `Ꮡpark`) instead of the type (CS1061). Because a converted struct is nested in its package's static class, the converter qualifies the type with that class — `m.of(runtime_package.m.Ꮡpark)` — which a same-named local cannot shadow. A bare `m` (binds the variable) and a `go.m` (the struct is not a direct member of the `go` namespace) both fail; the package-class qualifier is the correct form. This is applied **only on a collision** (the `.of()` receiver variable's name equals the type's simple name), so every other box accessor keeps its un-namespaced, Go-like form — no golden churn. (Guarded by the `VarNamedAsType` behavioral test; runtime hit this on `m`/`Δp` locals taking field addresses, ~9 CS1061.)

The same collision fires when the receiver is that variable's **lambda capture**. Inside a closure the captured variable renames to its capture copy (`mʗ1`), so the receiver-equality check alone misses it — but the *enclosing* local `m` is still visible to the C# lambda, so the accessor's bare owning-type reference binds to it all the same: runtime `rwmutex.lockSlow`'s `systemstack(func() { …; notesleep(&m.park) })` emitted `mʗ1.of(m.Ꮡpark)` → CS1061. `boxAccessorType` therefore also qualifies when the receiver is the type name plus the capture marker (`typeName + ʗ…`), yielding `mʗ1.of(runtime_package.m.Ꮡpark)`. (Guarded by a further extension to `CollisionFieldBoxAccessor` — `capturedLocalNamedAfterType`, a type-named local field-addressed inside a capturing closure, write-through verified vs Go; cleared runtime rwmutex's 2 CS1061, 91 → 89.)

The type half also needs the **type-vs-method collision rename** (above). When the accessor's owning type is itself a colliding name — `type funcInfo` versus a method `func (f *Func) funcInfo()`, so the type is declared `ΔfuncInfo` — taking the address of one of its fields must use the renamed type (`Ꮡ(f).of(ΔfuncInfo.Ꮡnfuncdata)`); a bare `funcInfo.Ꮡnfuncdata` binds to the package's static `funcInfo` method group (CS0119). The `boxAccessorType` helper applies the `Δ`-rename to a bare same-package collision name before its receiver-shadow check (the renamed name no longer matches a raw-named local, so the two disambiguations compose). (Guarded by an extension to `CollisionFieldBoxAccessor` — a global whose type is the collision type; runtime hit this in `symtab`'s `pcdatastart`/`funcdata`.)

A **collision-renamed owning type is qualified unconditionally**, not just when it equals the `.of()` receiver — because a Go local named after its type is renamed to the *same* `Δ`-name, so such a local **anywhere in the function** shadows a bare `Δp.Ꮡfield` (C# locals are function-scoped). Runtime's malloc `persistentalloc1` does `persistent = &mp.p.ptr().palloc` and then declares a local `p` further down (renamed `Δp`); the accessor `(~mp).p.ptr().of(Δp.Ꮡpalloc)` bound its bare `Δp` to that later local — CS0841 (use-before-declaration), and CS1061 regardless (the local's type has no `Ꮡpalloc`). The receiver (`(~mp).p.ptr()`) is not the colliding local, so the receiver-name check missed it. `boxAccessorType` now qualifies whenever the type name is `Δ`-prefixed (a type is never shadow-renamed — types are package-level — so a `Δ`-prefixed accessor type is always a collision rename), emitting `(~mp).p.ptr().of(runtime_package.Δp.Ꮡpalloc)`. Qualifying is value-identical to the bare form when nothing shadows, so it is safe to apply to every collision-type accessor. (Guarded by a further extension to `CollisionFieldBoxAccessor` — `localShadowsCollisionType`, a local named after the collision type declared after the accessor; cleared runtime malloc's CS0841 plus two mheap `Δp.Ꮡgcw` CS1061 of the same shape, 148 → 145.)

A related case is the **box name of a shadow-renamed receiver/parameter**. A deref-aliased pointer (a receiver or a `*T` parameter) is emitted as `ref var <name> = ref Ꮡ<raw>.Value` — the `Ꮡ` companion always keeps the **raw** Go name, even when the value alias is shadow-renamed for a collision (`func (p *cpuProfile) add()` where `p` collides with the type `p` → `ref var Δp = ref Ꮡp.Value`). When a pointer-receiver (capture-mode) method is then called on that receiver/parameter, the call routes through the box, and that box reference must use the raw name `Ꮡp` — the value alias `Δp` would yield `ᏑΔp`, which is not in scope (CS0103). The converter builds the box from the raw identifier name (not the shadow-renamed value form), but only when they differ — so non-renamed receivers are unaffected (no churn).

The same raw-box-name rule applies when such a shadow-renamed pointer is **captured by a closure** (where the value alias is referenced through its box, since the `ref`-local can't be captured — see the *box-ref* section below). A value use inside the closure becomes `Ꮡp.Value.n` and a field-address use `Ꮡp.of(T.Ꮡn)` — both rooted at the raw box name `Ꮡp`, never the renamed `ᏑΔp`. The field-address form (`&p.field`) routes through the box-ref address path rather than the generic pointer-variable path: that generic path would prepend `Ꮡ` onto the closure's box-deref read (`Ꮡp.Value`), yielding a double-boxed `ᏑᏑp.Value` (CS0103). Because the captured pointer's box `Ꮡp` *is* the `ж<T>`, the field address is simply `Ꮡp.of(T.Ꮡfield)` — the same form as a captured value struct. (Guarded by the `RenamedReceiverBox` behavioral test, which exercises a shadow-renamed receiver calling a capture-mode method, plus a shadow-renamed pointer parameter both read through and field-addressed inside a closure; runtime hit this on `p`/`Δp` receivers calling methods like `p.addExtra()` and on closures capturing such pointers, ~12+ CS0103.)

A closure that captures an outer variable is emitted with a snapshot copy declared before the lambda — `var sʗ1 = s;` — and uses of the captured variable inside the lambda are rewritten to that capture name `sʗ1`. The capture-name mapping is keyed by **name**, which breaks on a **self-shadowing initializer inside the closure**: runtime `mgcsweep`'s `systemstack(func() { s := spanOf(uintptr(unsafe.Pointer(s.largeType))); … })` declares an inner `s` whose initializer reads the *outer* captured `s`. Both the captured use (the RHS `s.largeType`) and the distinct inner binding were mapped to the same `sʗ3`, so the inner declaration emitted `var sʗ3 = …(~sʗ3)…` — its RHS binding to the not-yet-initialized inner variable (CS0841). The fix records the captured **object** alongside the name, and applies the capture name only when an ident resolves to that exact outer object; the inner binding falls through to its own (shadow-renamed) name. The emission is `var sΔ1 = spanOf(…(~sʗ3)…)` — the inner `s` shadow-renamed to `sΔ1` (distinct from the capture `sʗ3`), its RHS correctly reading the captured `sʗ3`, and later uses of the inner `s` using `sΔ1`. Because the object check passes for every non-shadowing capture (the ident *is* the captured variable), it changes nothing outside this self-shadow case (zero golden churn). (Guarded by the `ClosureSelfShadowCapture` behavioral test — a captured pointer with an inner `s := f(s)` in a `systemstack`-shaped call-argument closure, output verified vs Go; cleared runtime `mgcsweep`'s CS0841.)

The same rule applies to an **escaping local** whose address is taken — `var p _panic; … preprintpanics(&p)` in runtime's `gopanic`, where `p` collides with the type `p`. The heap allocation is `ref var Δp = ref heap(new _panic(), out var Ꮡp)`, so the box is `Ꮡp` (raw) and `&p` must emit `Ꮡp`, not `ᏑΔp`. **Crucially, the box-name rule is keyed to the rename *kind*, because the two kinds name their boxes differently:** a type-**collision** rename prepends the marker (`p` → `Δp`) but keeps the raw box (`Ꮡp`), whereas a nested-scope **shadow** rename appends the marker plus a counter (`i` → `iΔ1`, `iΔ2`) and keeps the *shadow* box (`ref var iΔ1 = ref heap<nint>(out var ᏑiΔ1)`, so `&i` correctly emits `ᏑiΔ1`). The converter therefore rewrites to the raw name *only* when the alias is exactly `Δ`+rawname (the collision form); a shadow-renamed or non-renamed var keeps its existing box name. (Guarded by the `CollisionRenamedLocalBox` behavioral test, with `ForVariants`/`NestedVarShadow` covering the shadow-rename form left unchanged.)

#### A nested closure must not clobber the enclosing closure's capture state
The per-lambda conversion state — `conversionInLambda` (are we inside a closure body?) plus the capture-name maps (`currentLambdaVars`/`currentLambdaVarObjs`) — is what makes closure-body emission rewrite captured references to their box/copy forms: a captured local `s` reads as `sʗ1`, and the current method's **direct-ж receiver** (`func (s *Stmt) …` emitted `this ж<Stmt> Ꮡs`, whose body alias `ref var s = ref Ꮡs.Value` is a `ref`-local that **cannot** be captured by a C# closure) reads through its box as `Ꮡs.Value`. That state was *set* on entering a closure but **reset to `false`/`nil` on exit**, not restored — so a closure that contains an **inner** closure had its state wiped the moment the inner one finished, and every reference in the *outer* closure body **after** the inner one fell back to the bare, un-rewritten name. For a receiver field-read that is a bare ref-local capture — `database/sql (*Stmt).QueryContext`'s `s.db.retry(func(){ …; rows.releaseConn = func(err){…}; if s.cg != nil { … } })`, where `s.cg` sits after the inner `releaseConn` closure — the emission was `s.cg` (CS8175, "cannot use ref local `s` inside an anonymous method/lambda"); the equivalent captured-local case silently split a variable between its bare form and its `ʗ1` copy within one closure. The fix makes `enterLambdaConversion`/`exitLambdaConversion` a proper **LIFO save/restore stack** (`conversionStack`): entering pushes the current state and installs fresh state; exiting **restores the enclosing closure's** state instead of resetting. A closure at top level still restores to `false`/empty (unchanged), so the change is inert except where a closure body continues after a nested closure — there the receiver box-read (`Ꮡs.Value.cg`, `Ꮡs.Value.cg.txCtx()`) and the captured-local copy name are now applied consistently across the whole body. (Guarded by the `NestedLambdaReceiverField` behavioral test — a direct-ж receiver method whose closure holds a nested closure followed by a non-call receiver field read, a field-method call, and another field read, all verified to render `Ꮡs.Value.<field>` and output-compared vs Go; cleared `database/sql`'s 2×CS8175 and re-baselined `DeferValueFieldPtrReceiver` whose defer-then-body sequence exercises the same restore.)

### A parameter that shadows an imported package is renamed at its declaration too
A function parameter whose name equals an imported package the function references — crypto/rsa's `func emsaPSSEncode(…, hash hash.Hash)`, where `hash` shadows the `hash` package named in the signature type `hash.Hash` — is shadow-renamed by the variable analysis (`hash` → `hashΔ1`) so it does not bind the `using hash = hash_package;` alias. Every **usage** already rendered the renamed name (convIdent reads `v.varNames`), but the parameter **declaration** was emitted from the raw `param.Name()`, so the signature kept `hash.Hash hash` while its uses were `hashΔ1` — CS0103 at every use (40 sites in crypto/rsa, 27 in testing/quick's `rand`). The declaration now resolves through the same `v.varNames` map, so it matches the usages:

```go
func emsaPSSEncode(mHash []byte, emBits int, salt []byte, hash hash.Hash) { … hash.Size() … }
```
```csharp
internal static (…) emsaPSSEncode(slice<byte> mHash, nint emBits, slice<byte> salt, hash.Hash hashΔ1) { … hashΔ1.Size() … }
```

A non-shadowed parameter maps to its own raw name (no churn). (Guarded by the `PackageShadowParam` behavioral test.)

A shadow-renamed **pointer** parameter completes the same rule on two more paths. A `*T` parameter is deref-aliased as `ref var <value> = ref Ꮡ<raw>.Value`, so its box companion `Ꮡ<raw>` always keeps the **raw** Go name even when the value alias is shadow-renamed — `func decrypt(rand io.Reader, …)` where `rand` shadows the `math/rand`-style alias becomes `ref var randΔ1 = ref Ꮡrand.Value`. **(A)** An address-of or by-pointer pass of that parameter must therefore use the raw box name `Ꮡrand`, not `Ꮡ`+value-alias `ᏑrandΔ1` (which is not in scope, CS0103) — `boxBaseName` returns the raw name for a pointer *parameter* specifically (unlike an escaping shadow-renamed *local*, whose box *is* the shadow form `ᏑiΔ1`). **(B)** When a function has **both** a pointer parameter and a shadow-renamed value parameter, its signature is rebuilt through a separate `updatedSignature` path (not the `generateParametersSignature` path fixed above), which had kept emitting the value param's raw name — so `EncryptOAEP(hash.Hash hash, …)` diverged from its `hashΔ1` uses again. That path now resolves value-param names through `v.varNames` too, matching the primary fix. Together these cleared 50 errors (crypto/rsa 23 + testing/quick 27). (Guarded by the `PackageShadowPointerParam` behavioral test.)

## Return Tuples
Many Go functions return either a single value or a "value, ok"/"value, error" tuple, where only the declared return arity selects the behavior. You cannot differentiate C# overloads by return type alone, so the runtime types expose a second overload distinguished by an extra discard argument. For map access, the "comma-ok" read routes through a two-value indexer using the discard sentinel `ꟷ`:

```csharp
var v1 = m["Answer"];            // single value: zero value if the key is absent
var (v2, ok) = m["Answer", ꟷ];   // comma-ok: (value, present?)
```

These two forms can behave differently — case in point, [type assertions](https://golang.org/ref/spec#Type_assertions): the single-value form panics on failure, while the comma-ok form returns safely with a boolean success result. Type assertions convert similarly, through a generated `_<T>()` accessor:

```csharp
var t = i._<MyType>();              // panics on failure
var (t, ok) = i._<MyType>(ᐧ);       // comma-ok, safe
```

The asserted type is a **type position**, so an assertion to a **pointer** type renders the pointer *type* `ж<T>`, not a value dereference: `i.(*box)` → `i._<ж<box>>()`. (The starred-operand-is-a-type case previously emitted the type form only inside a `(*T)(p)` cast; a non-parenthesized `*type` fell through to the value-deref path and emitted `box.Value` — CS0426, since `Value` is not a member type of `box`.) (Guarded by the `TypeAssert` behavioral test's `*box` assertion; runtime hit this in `netpoll.go`'s `arg.(*pollDesc)`.)

The types that support these tuple-returns are defined in the [`golib`](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/core/golib) library; ordinary user-code tuple returns convert as normal C# tuples without special handling.

**A package-level `var a, b = f()` reads ValueTuple components.** C# static field initializers cannot deconstruct a tuple, so the per-name field emission assigned the WHOLE result tuple to the first field (CS0029 — edwards25519's `var identity, _ = new(Point).SetBytes(…)`). With exactly one non-blank name the component read is appended to the inline call (`internal static ж<Point> identity = …SetBytes(…).Item1;` — blank names keep their uninitialized `_ᴛNʗ` fields, and the call still runs once). With two or more non-blank names the call is evaluated ONCE into a hidden tuple field and each name reads its component (`internal static (nint, @string) tupleᴛ1ʗ = pair(); internal static nint n = tupleᴛ1ʗ.Item1;` — C# static initializers run in textual order, so the reads follow the temp). Gated to package scope, no explicit type, one call initializer typed as a tuple; in-function `var x, y = f()` keeps the existing path. (Guarded by the `GlobalTupleVarDecl` behavioral test — both shapes plus a call-count probe proving single evaluation, output-compared vs Go.)

**Every trailing argument of a variadic pointer parameter gets the box treatment.** The per-parameter argument loop visits declared parameters only, so `checkInitialized(p, q)` binding two deref-aliased pointer parameters to `...*Point` boxed only the first (`checkInitialized(Ꮡp, q)` — CS1503). The pointer-argument box treatment now fans out from the variadic parameter's index to every trailing argument, mirroring the type-parameter `@string` fan-out; the spread form (`f(s…)`) is excluded as before, and non-variadic calls are byte-identical. (Guarded by the `VariadicPointerParam` extension `pairTotal` — three deref-aliased pointer params forwarded to the variadic, value vs Go.)

**A call-result delegate of a NAMED func type must resolve its signature through `Underlying()`.** All the per-argument treatments above (pointer boxing, interface conversion, `u8` suppression) are driven by `getFunctionSignature`, which for a callee that is itself a call — `valueEncoder(v)(e, v, opts)`, encoding/json — read `info.TypeOf(fun).(*types.Signature)`. When the inner function returns a **named** methodless func type (`valueEncoder` returns `encoderFunc`), `info.TypeOf` is a `*types.Named`, so that assertion failed and the signature came back nil — the per-argument loop never ran, and the pointer receiver `e` (a deref'd `ref var e = ref Ꮡe.Value`) passed its value alias where the `ж<encodeState>` slot wanted the box `Ꮡe` (CS1503). The `*ast.CallExpr` arm now asserts on `Underlying()`, looking through the named func type to its signature (a no-op when the result is already an unnamed signature). Byte-identical across the behavioral corpus and across an A/B of encoding/json+gob+text/template+net/http+reflect — a single line moves (json's `valueEncoder(v)(e,…)` → `(Ꮡe,…)`). (Guarded by the `NamedFuncResultPointerArg` behavioral test — `adder()` returning a named `addFunc` called immediately with a `*State` receiver that must box, mutation through the box observed vs Go.)

**A variadic *closure* rebinds its `params` array to a slice at the top of its body.** A variadic parameter `a ...T` arrives in C# as a `params ꓸꓸꓸT` array named `<name>ʗp` (a distinct name, so it doesn't collide with the slice the body expects); the body then references the bare `<name>` as a `slice<T>`. A top-level function emits a `var <name> = <name>ʗp.slice();` prologue as its first block statement, but a **function literal** emitted no such prologue, so any closure that referenced its variadic parameter used an undefined bare name (CS0103 — internal/dag's `errorf := func(format string, a ...any) { … fmt.Sprintf(format, a...) }` spread `a…` against a name that was never declared). A function literal now emits the same rebinding prologue. Because the prologue is prepended *before* the single-return→expression-body collapse, a variadic closure whose body is a lone `return f(a...)` keeps its block form (the rebound name is a statement-scoped local) rather than collapsing to an undefined expression. An **IIFE** literal is excluded — it emits parameter *names* only (the raw `a`, with the delegate cast supplying the `params` type), so there is no `<name>ʗp` array to `.slice()`. (Guarded by the `VariadicClosureSpread` behavioral test — a single-return closure spreading `a...` into `fmt.Sprintf`, a closure ranging its variadic slice, and a single-return closure forwarding `a...` to another variadic; output-compared vs Go.)

## Slices and Arrays
Go slices and arrays are converted to the golib [`slice<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/slice.cs) and [`array<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/array.cs) structures. A `make`-style allocation uses a constructor; a composite literal builds a C# array and projects it with the `.slice()` / `.array()` extension:

```go
package main

import "fmt"

func main() {
    primes := [6]int{2, 3, 5, 7, 11, 13}   // array literal
    nums := []int{10, 20, 30}              // slice literal
    buf := make([]byte, 4)                 // make
    fmt.Println(primes[0], nums[2], len(buf))
}
```

converts to:

```csharp
internal static void Main() {
    var primes = new nint[]{2, 3, 5, 7, 11, 13}.array();
    var nums = new nint[]{10, 20, 30}.slice();
    var buf = new slice<byte>(4);
    fmt.Println(primes[0], nums[2], len(buf));
}
```

A **named** slice/array type (`type d [3]rune`, `type s []int`) lowers to a struct wrapping `array<T>`/`slice<T>`; its composite literal cannot use C# collection-initializer braces (the lowered struct has no `Add`), so it is constructed through the underlying-collection constructor: `d{0, 32, 0}` → `new d(new rune[]{0, 32, 0}.array())`.

The **empty** composite of such a type is its **zero value**, not a one-element literal: the generic named-composite `nil` filler (which gives a named *struct* composite its `new T(nil)` zero-value ctor argument) previously landed *inside* the element literal — `tmpBuf{}` (`type tmpBuf [32]byte`, runtime `string.go`'s `*buf = tmpBuf{}`) emitted `new tmpBuf(new byte[]{nil}.array())`, a `NilType` element in a `byte[]` (CS0029). An empty **array** composite now emits a zeroed *fixed-length* backing — `new tmpBuf(new byte[32].array())` — because Go's `[N]T{}` is a full-length zero array (an empty `{}` literal would produce a length-0 backing); an empty named-**slice** composite emits an empty non-nil backing — `pm{}` → `new pm(new uint32[]{}.slice())`. (Guarded by the `NamedArrayWrapper` extension — empty array composite read/written at full length, zeroing an existing wrapper through a pointer via `*buf = tb{}`, and an empty slice composite appended to; values vs Go. The nil-vs-empty *distinction* — `pm{} == nil` is `false` in Go — is a separate pre-existing golib model latent: the slice nil-compare conflates nil with empty-but-allocated.)

**Reslicing SHARES the backing array — golib `slice<T>` stores capacity.** A Go reslice is a view adjustment, never a copy: writes through `s[a:b]` are visible through `s` (and vice versa), `append` within capacity writes the shared backing in place, and only `append` beyond capacity reallocates and detaches. The emitted forms are the C# range indexer for 2-index expressions (`s[a:b]` → `s[a..b]`, `s[a:]` → `s[a..]`, `s[:b]` → `s[..b]`, `s[:]` → `s[..]`) and the golib `.slice(low, high, max)` extension for 3-index expressions (`s[a:b:c]` → `s.slice(a, b, c)`; a missing low is the `-1` sentinel). Both take bounds **relative to the view** (the source slice may itself sit at a non-zero offset in its backing array), default a missing high to `len(s)` — the range indexer resolves a from-end `Index` against the slice *length*, so `s[1..]` is Go's `s[1:]` even when `len < cap` — allow high up to `cap(s)`, and panic Go-style (`RuntimeErrorPanic.SliceBoundsOutOfRange`) when out of range. To represent a capacity-restricted 3-index view (`s[a:b:c]`, `c` below the backing array's end) without copying, `slice<T>` stores `m_capacity` as its own field rather than deriving it from the backing array's length; the `slice<T>(T[] array, nint low, nint high, nint max)` constructor builds such views. (Historically golib *copied* on any reslice that didn't span the whole backing array — a `base[2:5]` sub-slice was a detached array with lost aliasing — derived capacity from the backing end, and mis-measured `Available`/`Append` for non-zero-offset views. Guarded by the `SliceAliasing` behavioral test — `copy` into and element writes through a `low>0` reslice, reslice-of-reslice offset compounding, slice-of-array reslices, restricted-capacity 3-index writes, in-place vs reallocating `append`, all read back through the base and value-compared vs Go.)

**A `make` length/capacity/size-hint of a non-`int` integer type is cast to `nint`.** The golib allocating constructors all take `nint` — `slice<T>(nint length, nint capacity)`, `map<K,V>(nint capacity)`, `channel<T>(nint capacity)` — and C# does **not** implicitly convert a `uintptr`/`uint`/`uint32`/`uint64`/`int64` (C# `nuint`/`uint`/`ulong`/`long`) to `nint`. So `make([]byte, n/goarch.PtrSize)` with a `uintptr` length would leave `new slice<byte>(n / …)` with no applicable constructor, and overload resolution falls onto `slice<T>(T[])` — reported as `CS1503` ("cannot convert `nuint` to `byte[]`"); a map/chan with a `uintptr` hint is a direct `nuint`→`nint` CS1503. The converter casts each such length/capacity/hint argument to `nint`: `new slice<byte>((nint)(n / goarch.PtrSize))` (both args of `make([]byte, l, c)`), `new map<K,V>((nint)(hint))`, `new channel<T>((nint)(size))`. A plain `int` (`nint`) and an untyped-constant argument (`make([]byte, 4)` → a bare `4`) bind directly and are **left uncast** (no golden churn) — as are the widening `int8`/`int16`/`uint8`/`uint16` kinds. (Guarded by the `MakeSliceUintptrLen` behavioral test — `uintptr`/`uint`/`uint32`/`uint64` slice lengths, a `uintptr` len+cap, a `uintptr` map hint and chan size, and int/untyped controls, all `len`/`cap`/element values verified vs Go; runtime hits this in `mbitmap`'s `make([]byte, n/goarch.PtrSize)`.)

**Slicing a pointer-to-array.** Go lets a `*[N]T` be sliced directly — `p[lo:hi:max]`, `p[:]` — auto-dereferencing the array. The C# box `ж<array<T>>` has no slice/range members (its underlying `array<T>` does), so the converter dereferences first: `p[1:3:4]` → `(~p).slice(1, 3, 4)`, `p[:]` → `(~p)[..]`, `p[2:]` → `(~p)[2..]`. Without the deref the call binds to the box and fails (CS1929). The resulting slice shares the array's backing storage, matching Go. (The `(*[N]T)(ptr)[:n]` pointer-*cast* form is different — see *Pointer-cast slice* below.) A deref-aliased pointer **parameter** or **receiver** is the exception: it is emitted as the pointed-to *value*, not a box, so a `~` on it would deref a non-pointer (CS0023). When that value is a **named** array type — `b *pageBits` emitted `ref pageBits b`, where `pageBits` is `[N]uint64` — the wrapper has no slice/range members, so its underlying `array<T>` is reached via `.Value`: `b[:2]` → `b.Value[..2]`. When it is an **anonymous** array (`p *[N]T` → `ref array<T> p`) the value already *is* the `array<T>` and is sliced directly (`p[:]` → `p[..]`). Only a pointer-to-array **box** (a local, a field, a call result) gets the `~` deref. (Guarded by the `PointerArraySlice` behavioral test — local box, named-array receiver, and named-array parameter; runtime hits this in `select.go`'s `cas1[:ncases:ncases]` / `mprof.go`'s `stk[:n:n]` (locals) and `mpallocbits.go`'s `pageBits` receiver methods (`clear(b[:])`).)

**Named-slice pointer reinterpret (`(*[][]byte)(buf)` with `buf *Buffers`).** Go converts a pointer-to-named-slice to a pointer to its underlying slice type freely — net's `fd.pfd.Writev((*[][]byte)(buf))`, where poll's `Writev` *reslices the header through the pointer* (`consume` advances `*v`), and the caller must observe it. The C# boxes `ж<Buffers>` and `ж<slice<slice<byte>>>` are unrelated generic instantiations (CS0030), so the conversion emits a **field view over the wrapper's own backing slice**:

```csharp
consume(Ꮡbuf.of(Buffers.Ꮡm_value));
```

Two generator pieces make the view real aliasing: a named-slice wrapper's `m_value` is **mutable** (`ReadOnlyValue = false` — a readonly field would force a defensive copy and lose header writes), and `ISliceTypeTemplate` emits the field-ref accessor `internal static ref slice<T> Ꮡm_value(ref Buffers instance)` that `ж<T>.of()` projects through. Claimed narrowly: pointer→pointer, source pointee a NAMED type whose underlying is a slice identical to the (unnamed) target pointee. (Guarded by the `SortArrayType` extension `consumeOne` — a `(*[]Person)(&crew)` reinterpret whose reslice through the view shrinks the original `Roster`, runtime-verified against Go.)

The **reverse** direction — an underlying-slice pointer to a NAMED-slice pointer, `(*Buffer)(&b)` with `type Buffer []byte` (log/slog/internal/buffer's `sync.Pool.New`) — is **asymmetric**, because the projection above cannot run backwards: a named-wrapper box *contains* the underlying slice (project it out), but a bare-slice box does **not** contain a wrapper to project. So the reverse must **construct** a wrapper box over the addressed slice — `Ꮡ(new Buffer(b))`. The source slice comes two ways: an **address-of** arg (`&b`) reuses its addressed value `b`; an **existing pointer** arg (cryptobyte's `(*String)(out)` with `out *[]byte`) renders its POINTEE through the deref — `Ꮡ(new String(@out))`, where `@out` is `out`'s deref-aliased slice. (The prior code added `.Value` to that pointer expression, but a deref-aliased pointer *parameter* is already the `slice<byte>` value `@out`, which has no `.Value` — CS1061; rendering `*arg` via `convStarExpr` yields the pointee correctly for a parameter, a box, and inside a lambda.) A bare `(ж<Buffer>)(Ꮡ(b))` cast is CS0030 (unrelated instantiations). Unlike the forward view, aliasing with the original is **not** preserved — `Ꮡ(value)` already boxes a copy — but the reinterpret is used through the returned pointer (the pattern), so writes/reads flow through that box. Claimed narrowly by the mirror gate: pointer→pointer, target pointee a NAMED type whose underlying is a slice identical to the (unnamed) source pointee. (Guarded by the `NamedSlicePointerReinterpret` behavioral test — a direct `(*Buf)(&b)`, the closure-returned `func() *Buf { … return (*Buf)(&s) }` shape, and a pointer-**parameter** `fillVia(out *[]byte)` doing `(*Buf)(out)`, appended and read through the same pointer, output-compared vs Go.)

**Pointer-cast slice (`(*[N]T)(ptr)[:n]`).** A Go conversion that casts an `unsafe.Pointer` to a pointer-to-array and slices it produces a `[]T` over the pointed-to memory. It is emitted as the golib **`slice<T>`** — the C# representation of *every* `[]T` — built from a `ReadOnlySpan<T>` over the raw pointer: `new slice<T>(new ReadOnlySpan<T>((T*)ptr, (int)n))`. (Earlier it was a bare `Span<T>`, but a `Span<T>` does **not** range as `(index, element)` tuples — `for i := range s` → CS8130 — and has no `Ꮡ(s, i)` element-address — CS0411; `slice<T>` supports both, since it is `IArray<T>`.) The `ReadOnlySpan<T>` constructor takes a C# `int`, so a Go `int`/`uint` length (`nint`/`nuint`) is narrowed via `getRangeIndexer` (through the underlying for a named numeric); an int literal is left as-is. The slice **copies** the pointed-to memory (`ReadOnlySpan.ToArray()`), which is self-consistent for code that only uses the resulting slice (e.g. runtime's `printDebugLog` ranges `state` and writes `&state[i]`, never re-reading the raw buffer; `os_windows` ranges an unsafe `[]byte` read-only). Since this is always the `(*[N]T)(ptr)` unsafe-cast form, it is memory-layout-dependent code whose raw values flow through the `unsafe.Pointer`=`nuint` round-trip (a transient `fixed` address → not GC-stable), so the runtime values are not the contract — only compilable, rangeable, element-addressable C#. (Guarded by the `PointerCastSliceRange` behavioral Compile + target test — index range, value range, and `&s[i]` element-address over a pointer-cast slice; runtime greened `debuglog`'s `printDebugLog` and `os_windows`, ~25 errors via the cascade. The length narrowing is covered by `StdLibInternalAbi`.)

An **untyped (type-inferred) composite literal** — the inner `{…}` of a `[][]rank{ key: {…} }`, which has no explicit type node — is emitted as a target-typed `new(…)` when its inferred type is a struct (the struct constructor takes the field values). When the inferred type is a **slice or array**, that form is wrong (`slice<rank>`/`array<rank>` have no element-list constructor → CS1729); the converter emits the element-array projection instead — `{rA, rB}` (inferred `[]rank`) → `new rank[]{rA, rB}.slice()`, and an inferred `[2]int` → `new nint[]{…}.array()`. When the inferred type is a **pointer-to-struct** — the `[]*T{ {…} }` shorthand for `&T{…}` — it is emitted as the boxed struct constructor `Ꮡ(new T(field: val, …))` (a bare `new(…)` would target the box `ж<T>`, whose constructor lacks the struct's fields → CS1739). When such an untyped slice/array literal is **keyed** (`{joiningL: stateBefore, …}` — the inner `{…}` of x/net/idna's `joinStates = [][numJoinTypes]joinState{stateStart: {…}, …}`), the element-array projection above cannot take Go's `key: value` syntax — `new joinState[]{ joiningL: stateBefore }` is a C# array initializer, which has no keyed element form (CS1003 ×62). The keyed case is routed to a golib `golib.SparseArray<T>` collection initializer instead — `new golib.SparseArray<joinState>{ [joiningL] = stateBefore, … }.array()` (`.slice()` for a slice element) — the same form the *typed* keyed slice/array path emits (see below); the `.array()`/`.slice()` `IEnumerable<T>` extension materializes the dense backing, and a defined-integer key takes the `[(int)key]` cast exactly as in the typed path. (Guarded by `UntypedNestedSliceComposite`; runtime/lockrank.go's `lockPartialOrder` is a `[][]lockRank` and runtime1.go's `dbgvars` is a `[]*dbgVar` of the positional forms, and x/net/idna's `joinStates` is the keyed form.)

An **indexed (keyed) slice/array literal** — `[]string{lockRankSysmon: "sysmon", …}` — is emitted as a golib `golib.SparseArray<T>` collection initializer (`[index] = value`). Its indexer takes a Go `int`. When an index key's Go type is a **defined integer type** whose underlying type does not implicitly widen to C# `int` (i.e. `int`/`int64`/`uint`/`uint32`/`uint64`/`uintptr`, as opposed to `int8`/`uint8`/`int16`/`uint16`/`int32`), the key is cast to `int` so it satisfies the indexer (CS1503 otherwise): `[lockRankSysmon]` (a `type lockRank int`) → `[(int)lockRankSysmon]`. A key that already widens (e.g. a `uint8`-backed `Kind`) is left uncast.

A keyed slice/array literal whose element type is a **non-empty INTERFACE** routes its elements
through the interface-cast element loop (each value wraps via `convertToInterfaceType`) instead of
`convExprList` — and that loop rendered a `KeyValueExpr` as a FLAT `key, value` pair, feeding the
SparseArray collection initializer one item at a time (`Add(key)` then `Add(value)`:
CS1950 + CS1503 ×21 pairs on go/internal/gccgoimporter's `lookupBuiltinType`,
`[...]types.Type{gccgoBuiltinINT8: types.Typ[types.Int8], …}` — untyped named-const keys in a
function-body literal, immediately indexed). The loop now emits the same `[key] = wrappedValue`
indexer form the non-interface path produces, with the key routed through `sparseArrayKey` (so a
defined-integer-type key keeps its `[(int)key]` cast alongside the interface-wrapped value); a
keyed MAP literal with an interface element type reaching the same loop takes the identical
indexer form. (Guarded by the `SparseArrayIfaceElem` behavioral test — a function-body sparse
literal with untyped named-const keys immediately indexed, the package-level form, and a
named-`uint`-keyed form, elements read back and output-compared vs Go.)

A keyed (sparse, constant-index) literal of a **named array-wrapper** type — internal/trace/oldtrace's `timedEventArgs{1: uint64(ev.StkID)}` where `type timedEventArgs [4]uint64` — backs onto the golib `array<T>(length)` (which has an indexer setter), not a raw C# array. The wrapper's constructor takes an `array<T>` (the positional path already produces one via `.array()`), and the keyed elements render as the `[i] = v` indexed initializer — valid on `new array<uint64>(4){[1] = v}` but *not* on `new uint64[]{[1] = v}` (CS0131, an array-initializer takes no indexed members). A **positional** literal of the same wrapper keeps the `new uint64[]{…}.array()` form (unchanged — no churn). (Guarded by `NamedArrayKeyedLiteral` — a `type args [4]uint64` with multi-keyed, single-keyed, and positional literals, element reads output-compared vs Go.)

A **generic** named array type carries its type parameters (and their constraints) onto the forward declaration, and its element type is emitted fully qualified in the `[GoType]` attribute so the generated array-backed partial — which lives in a file without this file's package-relative `using` aliases — can resolve it:

```go
type table[T any] [3]atomic.Pointer[T]
```
```csharp
[GoType("[3]sync.atomic_package.Pointer<T>")] partial struct table<T>
    where T : new();
```

An **anonymous array/slice field whose element type lives in a multi-segment-path package** — `cpuLogWrite [2]atomic.Pointer[profBuf]`, `children [4]atomic.UnsafePointer` (atomic = `internal/runtime/atomic`) — keeps its `array<…>` wrapper. The field's type name is built structurally from the `[N]`/`[]` marker plus the recursively resolved element, *not* from the type's package-qualified string: that string (`[2]internal/runtime/atomic.Pointer[…]`) goes through a cross-package last-segment strip that would also remove the leading `[2]`, collapsing the field to the bare element type (`atomic.Pointer<…> = new(2)`) whose array `new(2)` initializer then mis-binds the element constructor (CS1503). With the structural rendering the field stays `array<atomic.Pointer<profBuf>> = new(2)`. An array of a current-package or basic-typed element was unaffected (its string has no foreign path to strip). (Guarded by the `ArrayOfCrossPackageType` behavioral test — `[3]atomic.Int32` / `[2]atomic.Uint64` fields; runtime's `trace`/`traceMap` structs hold these.)

#### A struct's array fields get their fixed length from a generated parameterless constructor

A Go `[N]T` array FIELD has a zero value of N zero elements — never nil. The converter emits the field
with a length initializer — `internal array<atomic.Int32> c = new(3);` — but a C# struct field
initializer only runs when an **explicitly declared** parameterless constructor is invoked; the implicit
struct constructor that `new counters()` would otherwise use zeroes every field and SKIPS initializers,
leaving the array's backing `T[]` null (an NPE on the first index or `len`). The `TypeGenerator`
therefore emits an explicit parameterless constructor for every struct, so `new S()` runs the field
initializers and each array field gets its `new(N)` backing. (C# 11 auto-defaults any field without an
initializer; a slice/map/chan field — which has no `new(N)` initializer — stays its nil zero value,
matching Go.) The **NilType constructor preserves the initializers too**: it used to re-assign
`this.field = default!` to every plain member — running *after* the field initializers, which nulled an
array field's fresh `new(N)` backing, so `S{}` (emitted `new S(nil)`) NREd on the first index. The
NilType and parameterless constructor bodies (`AppendZeroValueInitializers`) now assign only what C#'s
implicit zeroing would leave *broken*: the promoted-embed boxes (see
[Struct Type Embedding](#struct-type-embedding)), and — see next paragraph — any plain struct-typed field
whose own type needs construction; everything else is left to the field initializers plus C# 11
auto-default. This is generator-only and produces no golden churn (the `.g.cs` output is not a
golden). It is what lets `ArrayOfCrossPackageType` run as an **output-compared** test (`len(x.c)` /
`len(x.d)` print `3 2`); before the fix, indexing `&x.c[i]` threw a `NullReferenceException`, so the test
was compile+target-only.

A plain (non-embed) struct-typed FIELD whose type *itself* needs construction is the recursive case:
`default(T)` gives such a field a `default(FieldType)`, whose nested promoted-embed box or fixed-array
backing is null — so the first touch NREs even though `T`'s own boxes were constructed. This is fmt's
`pp{ … fmt fmt … }` where `fmt` embeds `fmtFlags` (a ctor-allocated box): `newPrinter`'s `@new<pp>()`
ran `pp()`'s ctor, which left `fmt` as `default(fmt)` with a null box, and `p.fmt.init(&p.buf)` →
`clearflags` NREd — the first crash of any converted `fmt.Println`. `AppendZeroValueInitializers`
therefore also emits `this.f = new FieldType(nil);` for each such field, and because that runs
`FieldType`'s own NilType constructor (which recursively constructs *its* needy fields), a single level of
construction fixes every depth. "Needs construction" (`StructTypeNeedsConstruction`) is: has a promoted
embed, a fixed-array field, or a nested struct field that needs construction — resolved only for
converter-generated structs (`GetStructDeclaration`), so a `true` result guarantees the public
`FieldType(NilType)` constructor exists; a reference field (pointer/interface/delegate) keeps its correct
nil zero value, and a cross-package/golib field type is left `default` (conservative — its own package
constructs it, and its nil zero value is correct anyway). Only the NilType and parameterless constructors
are touched, so this covers `new(T)`/`@new<T>()`/`&T{}`/`T{}`; a `var x T` **local** still emits
`default!` (a distinct converter-level gap — a local of a needy struct is not yet constructed). Guarded by
the `NestedPromotedEmbedInit` output-compared test (a `printer` holding a `formatter` field that embeds
`flags` and holds a `[3]byte`, reached via both `new(printer)` and `&printer{}`, its promoted fields and
array written and printed against Go); before the fix the promoted-field write NRE'd.

#### Prefer the file-local package alias over the fully-qualified `_package` name

A cross-package named type has two C# spellings: the **fully-qualified** form `sync.atomic_package.Int32` (the namespace-rooted class, from `getFullTypeName`) and the **file-local alias** form `atomic.Int32` (the `using atomic = sync.atomic_package;` shorthand, from `getTypeName`). For *visual* fidelity — the converted C# should read like the Go original, which writes `atomic.Int32` — body emission prefers the alias. But the alias is only resolvable where the `using` is in scope, so the choice is made per emission site by `getDisplayTypeName`, which returns the alias form **only when every cross-package type referenced by the type is imported in the current file** (checked against the per-file `importQueue`), and otherwise falls back to the fully-qualified form.

The fallback matters: a Go file may *index* an atomic-typed array field of a struct — `&x.c[i]` → `…at<E>(i)` — without ever naming the element type `E`, so it carries no `import "sync/atomic"` and no `using atomic`. There the element type must stay fully-qualified (it resolves inside `namespace go;` with no alias) or the file fails CS0246. When the package *is* imported (the common case, and every behavioral test of this), the prettier alias is used.

`getDisplayTypeName` is applied at the body-emission sites that land in the current source file:

* **named struct-field declarations** — `internal atomic.Int64 total;` (was `sync.atomic_package.Int64`);
* **heap-box allocations** — `ref var n = ref heap(new atomic.Int32(), out var Ꮡn);`;
* **element-address `at<T>`** — `…at<atomic.Int32>(0)`.

It is **not** used for forms consumed by the source generators in alias-less generated files, which must stay fully-qualified: the `[GoType("…")]` attribute string (e.g. `[GoType("sync.atomic_package.Uint32")]`, `[GoType("[3]sync.atomic_package.Pointer<T>")]`), the `global using` type-alias declarations, and the promoted-interface/embedded-field registration keys. (Embedded fields keep the full form for their promoted accessors; only the named-field branch uses the display name. Struct-embedding promotion across packages re-derives member types from the Roslyn semantic model, not from the field's emitted text, so aliasing the field declaration is safe.) Guarded by `ArrayOfCrossPackageType`, `AtomicValues`, `FuncTypeParam`, `GenericAtomicPointerField`, `GlobalAtomicDefer`, `GlobalAtomicFieldMethod`, and `StructPromotionWithInterface`/`StructPointerPromotionWithInterface`.

#### Combined field-element address `base.at(field, i)`

The address of an element of an array/slice FIELD of a boxed value — `&x.c[i]` where `c` is an
array field, or the implicit address taken to call a pointer-receiver method `x.c[i].inc()` — was
rendered as a two-step chain `Ꮡx.of(counters.Ꮡc).at<atomic.Int32>(i)`: `of(field)` takes the field's
address (a `ж<array<E>>`), then `at<E>(i)` takes the element's. The explicit `<E>` is needed because
golib's standalone `at<TElem>(nint)` is generic in an element type unrelated to the pointer's `T`, so
it cannot be inferred. golib adds combined overloads `ж<T>.at<TElem>(FieldRefFunc<…array<TElem>…>, nint
index)` (one per field-accessor shape and array/slice kind, each forwarding to `of(field).at<TElem>(i)`)
whose `TElem` IS inferred from the field accessor's return type. The converter then collapses the chain
to `Ꮡx.at(counters.Ꮡc, i)` — dropping both the `.of(` step and the `<E>` type argument. It rewrites the
recursively-built field address `base.of(Type.Ꮡfield)` by retargeting its trailing `.of(field)` to
`.at(field, i)`, only when the field segment is parenthesis-free (a plain `Type.Ꮡfield` accessor, so the
final `)` provably matches the last `.of(`); any other shape falls back to the explicit chained form.
The combined overload is behaviorally identical to the chain (it literally forwards to it). (Guarded by
`ArrayOfCrossPackageType`, `IndexedElementDirectBoxMethod` and `PointerFieldArrayElementAddress` — all
output-compared; the `.inc()`/`bump()` element writes verify runtime equivalence.)

The routing gate sees through **nested value fields to the chain root**. `&pp.wbBuf.buf[0]` (runtime
`mwbbuf.go`) roots at the pointer `pp` through the *value* field `wbBuf`; the original gate checked
pointer-ness only one level up (`pp.wbBuf`, a struct), fell to a naive `Ꮡ` prefix (`Ꮡpp.wbBuf…` — CS1061
on the box), and the same failure hit the closure-captured variant (`&mp.trace.buf[gen%2]`, `trace.go`).
The gate now walks intermediate selectors to the root, so any pointer-rooted (or heap-boxed) chain routes
through the recursive `&field` machinery — `pp.of(pstate.ᏑwbBuf).at(wbBuf.Ꮡbuf, 0)` — which already
rendered multi-hop of-chains. A **nested-index** base — `&cache.entries[ck][i]` (2-D array via a pointer,
`symtab.go`) — is an `IndexExpr`, not a selector, so it gets its own arm: recursively take the inner
element's address (`cache.at(pcvalueCache.Ꮡentries, ck)`) and chain the outer `.at<T>(i)` onto it — the
gate also accepts a HEAP-BOXED value root (`&grid.cells[1][2]` on an address-escaping local), fixing that
shape too. An unboxed value-rooted chain keeps the prior naive form (corpus byte-identical). *Known
remaining gap (pre-existing): an intermediate `IndexExpr` inside the selector chain —
`&ptr.items[i].buf[j]`, an array-of-structs hop — defeats the root walk (both arms only step through
selectors) and keeps the CS1061 naive form; the recursive machinery likely has the pieces when a runtime
site demands it.* (Guarded by
the `NestedFieldElementAddr` behavioral test — all three runtime shapes with write-through vs Go; note a
ZERO-VALUED struct's array-field backing is null in the C# emulation — a separate pre-existing latent —
so the test initializes its arrays.)

**Element address of a by-value ARRAY PARAMETER.** Array parameters are cloned by value in the
function preamble (`value = value.Clone();`, Go's array-copy semantics) but are never
escape-analyzed, so they have no heap box — the naive element-address form would name a box that
does not exist (`Ꮡvalue.at<byte>(0)`, CS0103 — syscall `SetsockoptInet4Addr`, `&value[0]` on
`value [4]byte`). The converter boxes a **copy of the wrapper struct** instead:
`Ꮡ(value).at<byte>(0)`. `array<T>` wraps a `T[]` reference, so the copied wrapper SHARES element
storage with the cloned parameter — element reads and writes through the pointer stay behaviorally
correct. (One accepted edge, no stdlib hit: reassigning the *whole* array param after taking an
element address leaves the pointer on the older backing array.) (Guarded by `DeferTypelessReturns`'
`first` — element address of a `[4]byte` parameter, value vs Go.)

A **built-in used as a generic type argument** is rendered in its golib form, the same as anywhere else — in particular Go `string` becomes golib `@string`, never C# `string` (`System.String`). This matters because the converter adds a `new()` constraint to every generic type parameter: `@string` is a struct with a public parameterless constructor and satisfies it, whereas `System.String` would violate it (CS0310), and assigning a string literal — emitted as a `u8` `ReadOnlySpan<byte>` — into such a field would fail (CS0029). So:

```go
type Pair[A any, B any] struct { a A; b B }
var p Pair[int, string]
p.b = "hi"
```
```csharp
Pair<nint, @string> p = default!;
p.b = "hi"u8;
```

This applies uniformly to every type-argument position — first, second-or-later, and nested (`Pair[int, Box[string]]` → `Pair<nint, Box<@string>>`). (The behavioral test `GenericStringTypeArg` guards these cases; `NestedGenericTypes` covers the nesting depth without string args.)

### Converting a string to `[]byte` / `[]rune`
A Go `[]byte(s)` / `[]rune(s)` element-decoding conversion is emitted as the golib element-slice form `slice<byte>(…)` / `slice<rune>(…)`, which relies on the `@string`→`slice<byte>`/`slice<rune>` conversion. When the source is a string **variable** it is already golib `@string`, so the conversion applies directly. When the source is a bare string **literal**, that literal would otherwise render as a `System.String` (no such conversion exists — CS1503/CS1929), so the converter casts it to `@string` first:

```go
bs := []byte("hello")
rs := []rune("héllo")
```
```csharp
var bs = slice<byte>((@string)"hello");
var rs = slice<rune>((@string)"héllo");
```

The `@string` cast fires only on a string-literal argument; a string-variable conversion (`[]byte(s)`) needs no cast. (Guarded by the behavioral test `StringLiteralSliceConversion`.)

### Converting a string literal to a named string type
A Go conversion of a string **literal** to a named type whose underlying type is `string` — `errorString("…")` where `type errorString string` — needs the same `@string` intermediate. The literal renders as a `u8` `ReadOnlySpan<byte>`, which has no conversion to the named type, so a bare `(errorString)"…"u8` is CS0030. The converter routes it through `@string` (which converts implicitly from the `u8` span and to which the named type converts):

```go
return errorString("kaboom")
```
```csharp
return ((errorString)(@string)"kaboom"u8);
```

This is the form the runtime uses for every `panic(errorString("…"))` / `plainError("…")`. (Guarded by the behavioral test `NamedStringConversion`.)

### Converting a string literal to a named `[]byte` / `[]rune` type
The byte/rune-slice sibling of the named-string rule above: a string **literal** converting to a named type whose underlying is `[]byte` or `[]rune` — `htmlSig("<!DOCTYPE HTML")` where `type htmlSig []byte` (net/http `sniff.go`'s signature table) — cannot cast directly either. The `u8` span converts to neither the `[GoType]` wrapper (whose implicit operator takes exactly its underlying `slice<byte>`/`slice<rune>`) nor through `@string` in one hop (C# chains at most one user-defined conversion — CS0030). The converter materializes the underlying slice exactly the way the plain `[]byte("…")` conversion does (the `slice<T>(T[])` builtin over the literal's `@string`), and the wrapper's own operator then applies:

```go
type htmlSig []byte
sig := htmlSig("<!DOCTYPE HTML")
```
```csharp
var sig = ((htmlSig)slice<byte>((@string)"<!DOCTYPE HTML"u8));
```

The rune form decodes code points — `runeSig("héllo")` yields a rune-counted `slice<rune>` — matching Go's conversion semantics. String **variables** are unaffected (no instance in the corpus; a named-slice wrapper conversion from a `@string` variable would surface as a loud CS0030, not silent misbehavior). (Guarded by the behavioral test `NamedByteSliceFromStringLit` — direct, composite-element, and argument positions, byte/rune element reads, all output-compared vs Go.)

### A string literal with high/greedy `\x` escapes emits a byte-array `@string`
Go's `\x` escape is **exactly two** hex digits denoting one raw byte; C#'s `\x` escape is a **greedy** 1-to-4-hex-digit code-*unit* escape, and a C# `"…"u8` literal UTF-8-re-encodes its content. So re-emitting a Go token verbatim as a C# string literal both (a) mis-parses `\xdb` followed by ASCII `"5""0"` (the token `\xdb50`) as the single code unit U+DB50 — a lone high surrogate that cannot UTF-8-encode into a golib `@string` (CS9026, time/tzdata's embedded zip blob) — and (b) silently widens every byte ≥ 0x80 to two UTF-8 bytes, so `@string` byte indexing / `len` would not match Go. Such literals are emitted as the exact bytes in a **parenthesized** byte-array-backed `@string`:

```go
const zipdata = "\x50\x4b\x03\x04\xdb50\xff\x92\x00LMT"   // raw bytes
```
```csharp
internal static readonly @string zipdata = ((@string)(new byte[]{0x50, 0x4b, 0x03, 0x04, 0xdb, 0x35, 0x30, 0xff, 0x92, 0x00, 0x4c, 0x4d, 0x54}));
```

The outer parentheses are load-bearing: an inline-indexed literal (`"…"[i]`) would otherwise bind `[i]` to the inner `byte[]`. Only a `\xHH` **escape** with a byte value ≥ 0x80 or a trailing hex digit trips it — a literal written with actual UTF-8 characters (`"Michał"`, `"白鵬翔"`) round-trips through `"…"u8` and keeps the readable string form, as does an all-ASCII escape run with no greedy extension (image/jpeg's `"\x00\x10\x01\x11"u8[i]`) — so no behavioral-golden churn. (Guarded by the `HexByteStringLiteral` behavioral test.)

### Composite types render structurally (`[]*T` keeps the pointer)
A slice/array type is rendered structurally in every type-name path: the `[N]`/`[]` marker plus the recursively resolved element, never from the `go/types` string form. The string form is path-qualified (`[]*internal/abi.Type`), and the cross-package last-segment strip would eat everything before the slash *including the pointer marker*, silently dropping the `ж<>` (reflect's `[]*abi.Type` fields compiled against the WRONG element type). The recursion also resolves lifted anonymous elements and cross-package generic elements:
```go
ptrs := vals.([]*atomic.Int32)
```
```csharp
var ptrs = vals._<slice<ж<atomic.Int32>>>();
```
Guarded by `ArrayOfCrossPackageType` (the type assert and a `var` declaration).

A **SAME-PACKAGE instantiated generic** is rendered structurally for the same reason — the name plus each type argument recursively resolved, never from the `go/types` string. A *cross-package* generic already took the structural path (`getTypeName`/`getFullTypeName` both special-case `pkg != v.pkg`), but a generic whose OWN type is local while a type ARGUMENT is cross-package fell through to the `t.String()` form: `curve[*repro/sub.Item]`, whose slash-strip then ate everything before the `/` — **including the `curve[` header** — collapsing the wrapper. crypto/elliptic's `var p224 = &nistCurve[*nistec.P224Point]{…}` and its `p256Curve struct { nistCurve[*nistec.P256Point] }` embed emitted `ж<nistec.P224Point>>` / `ref go.nistec.P256Point> …` (a CS1519/CS1526 cascade, ~137 errors across elliptic/ecdh/mlkem768). Both `getTypeName` (the var-type path) and `getFullTypeName` (the struct-embed field path) now render a same-package generic as `Name[args…]` with each arg via the same function, so the arguments carry their short, slash-free package-qualified names and the header survives → `ж<nistCurve<ж<nistec.P224Point>>>`. Byte-identical across the behavioral corpus; an A/B of crypto/elliptic+ecdh shows only wrapper-restorations at every site (var types, adapter ctors, `GoImplement` attributes, the embed accessor, the unmarshaler array). (Guarded by the `CrossPkgUser` extension — a same-package `Holder[*CrossPkgLib.Sensor]` as a var type AND a struct embed, field read/write vs Go.)

### A pointer-element composite literal takes the box for a deref-aliased ident

A bare identifier element of a pointer-element composite literal (`[]*CommentGroup{c}`) renders
the pointer VALUE — the box `Ꮡc` — not the deref'd receiver ref-local `c`. Every named pointer
parameter is deref-aliased in C# (`ref var c = ref Ꮡc.Value`), and the bare name is the value
alias; the array element type is `ж<CommentGroup>`, so the alias form was CS0029 (go/ast's
`CommentMap.addComment` — the sibling `append(list, Ꮡc)` already took the box through the
call-argument pointer arm). The routing mirrors the struct-field pointer arm: the element index
is marked `argTypeIsPtr`, which convExprList turns into the pointer ident context:
```csharp
list = new ж<CommentGroup>[]{Ꮡc}.slice();
```
Gated to bare idents of pointer type — keyed elements (maps) and address-of/composite elements
manage their own pointer rendering. Guarded by the `PointerParamWalk` extension `collect` (the
literal arm and the append arm, aliasing proven by a post-collect write through the original).

### Appending to an interface-typed slice casts the element
A value appended to a `[]Iface` slice whose type is not already the interface -- a pointer rendering as the `*T`-to-interface adapter ctor, or a raw struct value -- leaves both golib `append` overloads applicable (`append<T>(ISlice, params T[])` infers the concrete/adapter type; `append<T>(slice<T>, params Span<T>)` infers the interface -- CS0121). The converter casts such elements to the element interface type:
```csharp
pack = append(pack, (Animal)(new CatжAnimal(Ꮡ(new Cat(nil)))));
pack = append(pack, (Animal)(new Dog(nil)));
```
An already-interface-typed element stays bare. The **empty interface** (`any`) element type is affected identically and takes the same cast — `append(args[:len(args):len(args)], c.output)` with `args []any` and `c.output []byte` infers `T=[]byte` on the `ISlice` overload but `T=any` on the `slice<T>` overload (testing's `flushToParent`, CS0121), and appending a scalar (`append(anys, 5)`) is the same shape; the differing element is cast to `any` so both overloads agree:
```csharp
args = append(args.slice(-1, len(args), len(args)), (any)(c.output));
```
Guarded by `InterfaceCasting` (non-empty interface) and `AppendUntypedConst` (the empty-interface `[]byte`-into-`[]any` and scalar-into-`[]any` cases).

### A struct-literal interface field takes a pointer element's adapter
A composite struct literal whose field is an INTERFACE type, initialized with a POINTER element whose pointer-receiver method set satisfies that interface, must record and route the same `*T`→interface adapter a call argument does — `&handlerWriter{l.Handler(), &logLoggerLevel, capturePC}` (log/slog SetDefault), where field `level` is `Leveler` and `*LevelVar` implements Leveler via a pointer-receiver `Level()`. The struct-field interface routing (`checkStructFields`) recorded/routed a NAMED VALUE element that satisfies the field (`DecodingError{InvalidIndexError(idx)}`) but matched only a `*types.Named` element, so a POINTER element fell through: no `GoImplement<LevelVar, Leveler>(Pointer = true)` was recorded, and the box `ᏑlogLoggerLevel` was passed bare to the interface-typed constructor parameter (CS1503). The detection now takes the concrete satisfying type from the element OR the pointee of a POINTER element (`types.Implements` tested on the element's own pointer method set, the non-interface guard tested on the pointee), so a pointer element records and routes exactly like the value case:
```csharp
new handlerWriter(l.Handler(), new LevelVarжLeveler(ᏑlogLoggerLevel), capturePC)
// [assembly: GoImplement<LevelVar, Leveler>(Pointer = true)]  -- in package_info.cs
```
The record flows through the existing pointer-target arm of `convertToInterfaceType` (the `ж<T>`-wrapped name unwraps to `GoImplement<T, Iface>(Pointer = true)`, and the render wraps the box in the generated `TжIface` adapter), so a same-package local (`streamWriter`→`io.Closer` in net/http/fcgi) and a foreign pointee (`*ast.SelectorExpr`→`ast.Expr`, `*Basic`→`Type`, `*Func`→`Object` in go/types) route through their local or foreign adapters uniformly. Positional and keyed literals both resolve their field (a keyed element renders `d: new SettingжDescriber(Ꮡs)`); an already-interface element and a value element are unchanged. (Guarded by the `PointerInterfaceStructField` behavioral test — a pointer-receiver-only implementer placed in an interface-typed struct field, positional via an addressed global and keyed via an addressed local, output-compared vs Go.)

### Named-string wrapper surface (indexing, sub-slicing, span bridge)
A named type over `string` is indexed and sub-sliced in Go (`tag[i]`, `tag[i:j]` -- reflect `StructTag.Get`), but C# indexing never applies user-defined conversions. The `InheritedType` template therefore forwards the `@string` surface on every named-string wrapper: `byte this[int]` / `byte this[nint]` indexers, a `Range` indexer returning the WRAPPER (a Go sub-slice of a named string keeps the named type), `nint Length` for `len()`, and an implicit `ReadOnlySpan<byte>` operator so `u8`-literal comparisons and assignments bind. Guarded by `NamedStringConversion`.

### A `:=`-declared string local keeps its named type and its heap box
A string-underlying local declared with `:=` takes its EXPLICIT declared type through the same general
declaration path every other type uses (never `var` — a `u8` literal would infer `ReadOnlySpan<byte>`).
The old dedicated string branch hardcoded `@string` as the declared type, which (a) DISCARDED a named
string type — go/types check.go's `fileVersion := asGoVersion(…)` declared its `goVersion` locals as
`@string`, so the `goVersion` extension methods `isValid()`/`cmp()` no longer bound (CS1929 ×4) — and
(b) BYPASSED the escape-analysis heap-box check, so `cause := ""` followed by `&cause` emitted an
unboxed local while the call site referenced the nonexistent box `Ꮡcause` (CS0103):
```csharp
goVersion fileVersion = asGoVersion((~@file).GoVersion);   // named type preserved
ref var cause = ref heap<@string>(out var Ꮡcause);         // escaping local heap-boxes
cause = ""u8;
```
A plain, non-escaping string local emits exactly as before (`@string s = "…"u8;` — the general path's
explicit-type arm resolves to `@string`). The same explicit-type routing applies in the for-init
tuple-declaration form. (Guarded by the `NamedStringDefine` behavioral test — a named-string `:=` with
methods called on the local, an escaping `cause := ""` written through its pointer, and a plain string
local, output-compared vs Go.)

### A typed const of a named string type keeps the named type

The CONST-DECL arm of the same materialization family: `visitValueSpec`'s string-constant emission
hardcoded `@string`, so net/http pattern.go's `const equivalent relationship = "equivalent"` (with
`type relationship string`) emitted `internal static readonly @string equivalent = …` — and every
comparison `rel == equivalent` was then ambiguous, because the `[GoType("@string")]` wrapper and
`@string` convert implicitly BOTH ways (CS0034 ×20 across pattern.cs). A typed string const now keeps
its named type, initializing through the wrapper's `ReadOnlySpan<byte>` implicit operator (the
`StringSurfaceMembers` u8 bridge):

```csharp
internal static readonly relationship equivalent = "equivalent"u8;
```

Function-body typed string consts take the same form (`relationship localRel = "moreSpecific"u8;`);
an UNTYPED string const keeps `@string` (its type is not a `*types.Named`). Full-stdlib footprint:
net/http pattern.cs, traceviewer's `ViewType` consts, and regexp/syntax parse.cs. (Guarded by
`NamedStringConsts` — package-level and local typed consts compared against values and each other, a
method called on a const, and an untyped const staying plain, output-compared vs Go.)

### A grouped var spec with one multi-result call deconstructs
A grouped `var (name, offset, abs = t.locabs() ...)` spec is not a `:=`, so the assignment tuple machinery never saw it -- the per-name path assigned the WHOLE result tuple to the first name and silently DEFAULTED the rest (time appendFormat read a zero abs; a silent-wrongness class beyond the CS0029 that exposed it). Function-local specs now emit the C# tuple deconstruction, matching the `:=` form; package-level specs use the once-evaluated hidden-field component reads:
```csharp
var (ln, ls) = pair();
```
Guarded by `GlobalTupleVarDecl` (both levels, with a call-count check proving single evaluation).

### `string()` of an untyped constant reference hops through the default type
`string(utf8.RuneError)` renders the argument as its cross-package `static readonly` Untyped* wrapper, from which `@string` has no conversion (CS0030). The conversion hops through the constant's DEFAULT Go type first -- exactly Go's conversion semantics; a plain literal is already a C# constant and keeps its direct form:
```csharp
fmt.Println("a" + ((@string)(rune)CrossPkgLib.Sep) + "b");
```
Guarded by `CrossPkgUser` (`string(CrossPkgLib.Sep)`).

### A `:=` from a named untyped constant materializes the default type
`codepoint := unicode.ReplacementChar` must not declare with `var`: the constant renders as its
`static readonly` Untyped* wrapper (`UntypedInt`/`UntypedFloat`/`UntypedComplex`), so `var` binds the
LOCAL to the wrapper type instead of Go's inferred default type, and a later Go conversion like
`string(codepoint)` fails (CS0030 — no `UntypedInt`→`@string` form; go/types conversions.go). The
declaration materializes the Go-inferred default type instead — exactly Go's `:=` typing:
```csharp
rune codepoint = replacementChar;    // NOT `var codepoint = …` (binds UntypedInt)
float64 factor = scale;
```
The gate is an Ident/Selector RHS resolving to a `*types.Const` of untyped NUMERIC kind (int is already
routed to the explicit `nint` form, and string consts to the explicit string path); literals and computed
constant expressions render as plain C# literals and keep `var`. Applies in both the single-declaration
and the mixed-statement paths. (Guarded by the `UntypedConstDefine` behavioral test — untyped rune and
float package constants `:=`-bound then converted/multiplied, output-compared vs Go.)

## Maps and Channels
Go maps and channels convert to the golib [`map<K,V>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/map.cs) and [`channel<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/channel.cs) structures. `make` becomes a constructor; channel send/receive use the runtime operators:

```go
m := make(map[string]int)
c := make(chan int, 3)
```
```csharp
var m = new map<@string, nint>();
var c = new channel<nint>(3);
```

Map reads honor Go's nil-map and comma-ok semantics (see [The "nil" Value](#the-nil-value) and [Return Tuples](#return-tuples)).

### Named map types and constrained map access

A defined map type — `type Grades map[string]int` — emits the `[GoType("map[K, V]")] partial struct` forward declaration (completing the long-standing `visitMapType` stub), implemented by go2cs-gen's Map template: full forwarding of `IMap<K, V>` (including the two-value comma-ok indexer), `IDictionary<K, V>`, enumeration, and the `ISupportMake` factory through the wrapped `map<K, V>`. Its composite literal wraps the concrete map literal in the named constructor — `new Grades(new map<@string, nint>{["a"u8] = 1})` — mirroring named arrays/slices (a direct indexer-initializer would target a default wrapper with no backing dictionary; the old emission produced Go-style `key: value` inside C# braces — CS1513). Comma-ok indexing works through a **constrained map type parameter** too: `v, ok := m[k]` where `M ~map[K]V` detects the map CORE of the constraint (both at the assignment's tuple gate and in the index emission) and routes the same `m[k, ꟷ]` two-value indexer, which lives on `IMap<K, V>` itself. The **nil comparison** `m == nil` — Go's only legal map comparison, maps.Clone's nil-preserve guard — emits the `IMap.IsNil` property (`if (m.IsNil)`; backing-store null, distinct from an allocated empty map — no operator exists on a type parameter, CS8761), and `delete(m, k)` on a constrained map binds a golib `delete(IMap<K, V>, K)` overload (key/value types infer from the interface conversion). (Guarded by the `GenericTypeInference` extension `EqualMaps` — a maps.Equal clone over a named map type through the constraint, comma-ok + comparable-erased equality, values vs Go.)
For source-generated named-map wrappers, the generator parses the `[GoType("map[K, V]")]` payload at the top-level comma, not every comma in the string. This matters for function-valued maps: `type opTable map[CrossPkgLib.Ticks]func(int, int) int` emits `map<global::go.CrossPkgLib_package.Ticks, Func<nint, nint, nint>>`, preserving the full delegate as the value type. Any source-file alias used inside the `[GoType]` payload is resolved through Roslyn and rewritten to its fully-qualified target before the template emits `IMap<K, V>`, `IDictionary<K, V>`, and `ICollection<KeyValuePair<K, V>>`; generated files therefore do not depend on file-local package aliases such as `using token = ...`. (Guarded by `NamedMapCrossPkgKey`.)

Two `[GoType]` payload conventions coexist, and the generator's alias substitution must tell them
apart. The map/channel emitters write dotted types in **source-alias form** (`CrossPkgLib.Ticks`,
via `getTypeName`), which the substitution above resolves; the slice/array element and
defined-over-selector emitters write the **namespace-qualified form** (`io.fs_package.FileInfo`,
via `getFullTypeName`), which roots through the `go` namespace and must pass through untouched.
The telltale is the segment after the leading identifier: a real alias maps to a package *class*,
so its next segment is a type name — a `_package`-suffixed next segment means the leading
identifier is a namespace segment that merely *collides* with a file alias. net/http's fs.go
aliases `io` while declaring `type fileInfoDirs []fs.FileInfo` → `[]io.fs_package.FileInfo`;
substituting the `io.` produced the nonexistent `go.io_package.fs_package.FileInfo` (CS0426 ×48).
The substitution skips exactly those occurrences (a negative lookahead on `_package.`). On the
converter side, the namespace-qualified form must lead with the **canonical** qualifier, never a
file-local Δ collision-rename: a consumer whose own namespace has a same-named child imports under
`using ΔIoLike = IoLike_package;`, but `[]ΔIoLike.FsLike_package.Info` resolves nowhere in the
alias-free `.g.cs` — `canonicalizeQualifierRename` reverts a leading import-rename segment
(mirroring the visitTypeSpec global-using-target rule). (Guarded by `NamedSliceChildPkg` — a
nested-namespace consumer package importing both `IoLike` and `IoLike/FsLike`, with a named slice
of the subpackage's type used across the assembly boundary.)

A map indexed by a **non-empty interface key** converts a concrete key expression through the same interface-adapter path used by assignments and call arguments. For example, `seen[item] = "kept"` where `seen` is `map[Node]string` and `item` is `*Item` emits `seen[new ItemжNode(item)] = "kept"u8`; the comma-ok read emits the same adapter for the key, `seen[new ItemжNode(item), ꟷ]`. This records the pointer implementation (`GoImplement<Item, Node>(Pointer = true)`) and keeps dictionary lookup semantics aligned with Go's interface key identity. Empty-interface map keys keep their existing literal handling (`map[any]...` turns string literals into Go strings rather than UTF-8 spans), and pointer-typed map keys keep the direct pointer-box path. (Guarded by `InterfaceMapKeyPointer`.)

A **pointer-keyed** map indexed by the method's **receiver** supplies the receiver's box as the key, exactly like the deref-aliased pointer-parameter case: `t.m[c]` inside `func (c *conn) …` emits `t.m[Ꮡc]` (plain read, write, and the comma-ok `t.m[Ꮡc, ꟷ]` alike) — net/http transport.go's idle-connection bookkeeping (`t.idleLRU.m[pc]`) passed the deref-aliased VALUE where `ж<persistConn>` was expected (CS1503 plus the `(v, ok)` deconstruction cascade). The box exists only on a **direct-ж** method, so the receiver-as-map-key body shape itself now *promotes* the method to direct-ж (`bodyUsesReceiverAsPointerValue` gained an `IndexExpr` case, gated on a pointer-KEYED map operand) — a method whose only pointer-use of its receiver is the map key still gets `this ж<conn> Ꮡc`. A pointer LOCAL is unchanged (it *is* the key — no `Ꮡ`), and `delete(t.m, c)` boxes through the ordinary pointer-argument rule once the method is direct-ж. (Guarded by `PtrKeyMapReceiverLookup` — pure-shape promotion, plain read/write, comma-ok, and delete through two distinct receiver identities, values vs Go.)

A value sent into a channel of **non-empty interface element type** converts through the same
interface-adapter path used by assignments and call arguments — the send emission previously tested
the CHANNEL type itself for interface-ness (never true, its underlying is `*types.Chan`), so no
conversion ever fired. A value implementation sends bare while recording the implement pair for the
generator (`vs.ᐸꟷ(new dog(name: "rex"u8))` with `[assembly: GoImplement<dog, speaker>]`); a pointer
implementation wraps the box in its generated pointer adapter:

```go
ps := make(chan speaker, 1)
c := &cat{name: "tom"}
ps <- c
```

```csharp
var ps = new channel<speaker>(1);
var c = Ꮡ(new cat(name: "tom"u8));
ps.ᐸꟷ(new catжspeaker(c));   // records GoImplement<cat, speaker>(Pointer = true)
```

A pointer-typed send value renders as its box (parity with the argument-position rule in
`convExprList`), and a type-parameter element (`chan T` in generic code) keeps the bare emission.
The string-literal empty-interface arm of the same helper is described under
[Empty Interface](#empty-interface). (Guarded by `AnyStringLitChanSend` — a value impl and a pointer
impl sent through a `chan speaker`, method-dispatched on receive, output-compared vs Go.)

### Named channel types

A defined channel type — `type closeWaiter chan struct{}` (net/http's h2 bundle) — emits the
`[GoType("chan T")] partial struct` forward declaration (completing the long-standing
`visitChanType` stub; the whole corpus previously had NO `GoType("chan …")` — CS0246 at every use),
implemented by go2cs-gen's Channel template: the wrapper holds a `channel<T>` and forwards its full
surface — the Go-visual send/receive members (`ᐸꟷ`, `ꟷᐳ`, including the select-registration
`ᐸꟷ(v, ꓸꓸꓸ)`/`Sending`/`Receiving` forms), the comma-ok `Receive(ꟷ)`/`Received` pair, `IChannel`'s
object-typed members, enumeration for `range`, the `ISupportMake` factory, and a `(nint size)`
constructor so `make(closeWaiter)` emits `new closeWaiter(1)` (the make path resolves the chan
through `Underlying()`, giving named channels the same unbuffered default as plain `chan T`):

```go
type closeWaiter chan struct{}
func (cw *closeWaiter) Init() { *cw = make(closeWaiter) }
func (cw closeWaiter) Close() { close(cw) }
func (cw closeWaiter) Wait()  { <-cw }
```

```csharp
[GoType("chan EmptyStruct")] partial struct closeWaiter;

[GoRecv] internal static void Init(this ref closeWaiter cw) {
    cw = new closeWaiter(1);
}

internal static void Close(this closeWaiter cw) {
    close<EmptyStruct>(cw);
}

internal static void Wait(this closeWaiter cw) {
    ᐸꟷ<EmptyStruct>(cw);
}
```

Two deliberate wrinkles. **Free-function channel ops name the element type explicitly** —
`ᐸꟷ<EmptyStruct>(cw)`, `close<EmptyStruct>(cw)`: golib's `ᐸꟷ<T>(channel<T>)`/`close<T>(in
channel<T>)` reach the wrapper only through its user-defined conversion to `channel<T>`, which C#
generic inference never considers (CS0411); the explicit type argument lets the conversion apply at
the argument instead (`namedChanElemTypeArg`, applied at the unary-receive, select-registration and
`close` emission sites — a plain `chan T` operand is byte-identical; a package that ALSO
declares a `close` method keeps the `builtin.` shadow qualification of the general builtin path,
so net/http emits `builtin.close<EmptyStruct>(cw)`). **The wrapper's `Close` is an
explicit `IChannel` implementation only**: Go code commonly defines its OWN `Close()` method on a
named channel type (the closeWaiter shape above), and a public instance `Close` would shadow that
method's extension form at every call site; `close(ch)` routes through the golib free function, so
no public surface is lost. (Guarded by `NamedChannelType` — the closeWaiter trio plus a buffered
`type intQueue chan int` exercising make/send/len/cap/receive/comma-ok/close/range/select, output
vs Go.)

### Select statement lowering (terminating and empty clauses)

A `select` lowers to a C# `switch`: with a `default:` clause present, the non-blocking form `switch (ᐧ)` whose case guards are try-operations (`case ᐧ when ch.ꟷᐳ(out v):`); without one, the blocking form `switch (select(ᐸꟷ(a, ꓸꓸꓸ), …))` dispatching on the ready index. Two structural completions (io pipe.go's `read`):

* **An EMPTY clause body still needs its jump.** C# requires every switch section to end in a jump statement (CS8070 on a final `default:`, CS0163 otherwise); the emitted `break;` was suppressed when the *previous* clause ended in a terminal `return` (the was-return flag is reset per statement, and an empty body has none). The flag resets per *clause* now — a bare Go `default:` emits `default: { break; }`.
* **A terminating blocking select gets an unreachable trailing `return default!;`.** Go's spec makes a select with no `default:` whose every comm-clause body ends in a terminating statement itself terminating, so a value-returning function may end with it. The lowered form's guarded `case N when <recv>:` labels cannot prove exhaustiveness to C# (CS0161). Mirroring the switch guarded-terminal-default rule, the emission appends `return default!;` after the closing brace — gated on: no default, every clause terminating (`isTerminatingStmtList`, conservative), no select-targeting `break`, a value-returning signature, and not named-return-defer mode (void wrapper).

The golib non-blocking receive underpinning the default-form guards distinguishes the two "no value" cases per Go semantics: a **closed** empty channel is receive-ready with the zero value; an **open** empty channel reports not-ready, so the `default:` is taken. (Guarded by the `SelectStatement` extensions `firstMsg` — terminal blocking select in a value-returning func — and `poll` — empty `default:` after a returning case, polled both before and after `close`.)

### An escaping comm-clause binding receives into a temp and heap-boxes at clause entry

A `case result := <-ch:` whose bound variable's address is taken in the clause body — internal/fuzz
`coordinatorLoop`'s `c.crashMinimizing = &result` and `writeToCorpus(&result.entry, …)` — escapes to
the heap, so the body's address-of emission references the `Ꮡresult` box companion (an escaping `:=`
local's form). The comm-clause label emitted only a plain `out var result`, never a box, leaving
`Ꮡresult` undeclared (CS0103 ×2, the last own-errors keeping internal.fuzz red after its CS0234s
cleared). The `when` guard's `out var` slot cannot declare a ref local, so `selectCommBinding`
(`visitSelectStmt.go`) receives into a uniquely-numbered temp and opens the clause body with the
entry-time box pattern proven by the escaping-parameter preamble:

```csharp
case 2 when (~c).resultC.ꟷᐳ(out var resultᴛ1): {
    ref var result = ref heap(resultᴛ1, out var Ꮡresult);
```

The gate is `identHasHeapBox` — the exact predicate the body's `&name` emission uses — so the box is
declared iff it is referenced; alias/box names mirror `convertToHeapTypeDecl` (sanitized analyzed name
for the value alias, raw analyzed name behind `Ꮡ` for the box, matching `boxBaseName`). Both bindings
of the `(val, ok)` form are checked. A non-escaping binding keeps the direct `out var <name>` form
(preserving the shadow-rename render, e.g. `out var errΔ5`), and an ASSIGN-mode rebind of an existing
boxed local already writes through its ref alias — the full-stdlib A/B footprint was exactly
internal/fuzz/fuzz.cs. (Guarded by `SelectEscapeBinding` — escaping binding written through both
directions, escaping `(val, ok)` binding with a field address through the box, and a mixed
escaping/plain select, output-compared vs Go; the pre-fix converter fails it with exactly the
CS0103 `Ꮡres` class. A clause taking ONLY a field address (`&res.value`, no whole-var `&res`) still
copy-boxes — the known assignment-position escape-analysis gap, out of scope here.)

## Generic Constraints
A Go generic constraint becomes a C# `where` clause. Most type-set constraints lift to the matching golib/.NET interface — a `[]T` element constraint to `ISlice<T>`, `[N]E` array-core to `IArray<E>`, `map[K]V` to `IMap<K,V>`, `chan T` to `IChannel<T>` — plus, for operator-bearing type sets, the `System.Numerics` operator interfaces (`IAdditionOperators`, `IComparisonOperators`, …) so the body's `+`/`<`/`==` on the type parameter compile. The Go built-in `comparable` maps to golib's [CRTP](Glossary.md#crtp) `comparable<T>`.

### An array-core constraint `~[N]E` lifts to `IArray<E>`

A type-set constraint whose core is an ARRAY — `func polyAdd[T ~[256]fieldElement](a, b T) T` (ML-KEM's
`ringElement`/`nttElement` share the core `[256]fieldElement`) — must map to `where T : IArray<E>`, NOT
to the operator interfaces the general type-set path would produce. An array is a *comparable* type in
Go, so the operator-set resolver put `Array` in the comparable set and lifted `IEqualityOperators<T, T,
bool>`; the named-array `[GoType]` wrapper (which the converter emits for `ringElement` etc.) does not
implement that interface, so every instantiation failed CS0315, and the interface exposes no array
surface, so the body's `t[i]` (CS0021), `for i := range t` (CS8130 on the index deconstruction), and
`for _, x := range t` (CS1579/CS8183) had nothing to bind against. The fix (`getArrayConstraintElem` in
`constraintOperations.go`, a new branch in `getGenericDefinition`) detects a single-array-core type set,
extracts the element type, and emits `where T : /* ~[N]E */ IArray<E>, new()`. The array wrapper already
declares `IArray<E>, ISupportMake<wrapper>` (the go2cs-gen `Array` inherited-type template), whose
`ref E this[nint]` indexer and `IEnumerable<(nint, E)>` enumeration supply exactly the indexing/ranging
surface the body needs, and the `new()` (appended by the same path) covers `var f T`/`T{}` construction.
Greened `crypto/internal/mlkem768` (census 254 → 255); the reconvert A/B changed only `mlkem768.cs`'s
four constraint lines. (Guarded by `GenericArrayConstraint` — two array-wrapper types over a shared
`~[4]fieldElement` core through a generic function that indexes, index-ranges, value-ranges, and
constructs the type parameter, values vs Go.)

### A single-term pointer constraint `[P *T]` erases the parameter to `ж<T>`

A type parameter constrained to a single, non-tilde pointer term — go/types' flat-copy helper
`func clone[P *T, T any](p P) P { c := *p; return &c }` (predicates.go) — cannot be modeled as a C#
type parameter: no C# constraint fixes a parameter to a *specific constructed type*, and `ж<T>`
implements no interface through which `*p`/`&c` could be expressed generically. The operator-lift
fallback emitted `where P : /* *T */ IEqualityOperators<P, P, bool>, new()` with the deref dropped
(`c = p` — CS0029 P→T) and the box mismatched (`return Ꮡc` — CS0029 ж<T>→P), and the call site's
synthesized `clone<ж<ΔSignature>, ΔSignature>(asig)` failed CS0311 (ж<> implements no
IEqualityOperators).

The Go spec makes the faithful lowering an *identity*, not an approximation: a non-tilde term's type
set is a **singleton**, so `P`'s only permissible type argument is `*T` itself. The converter therefore
**erases** such a parameter (`pointerCoreConstraint` in `constraintOperations.go`): it leaves the
emitted `<…>` list and `where` clauses (a breadcrumb comment preserves the Go constraint), renders as
`ж<T>` everywhere it appears (a `getTypeName` arm beside the `*types.Pointer` arm), and the parameter
classification treats a `p P` exactly like a `p *T` (`paramPointerType` — deref alias, `Ꮡ` box naming),
so the entire existing pointer machinery applies unchanged:

```csharp
internal static ж<T> clone<T>(ж<T> Ꮡp)
    /* where P : *T (erased: P renders as ж<T>) */
{
    ref var p = ref Ꮡp.Value;

    ref var c = ref heap<T>(out var Ꮡc);
    c = p;
    return Ꮡc;
}
```

Call sites drop the erased position from any synthesized explicit type-argument list
(`renderedTypeArgs`, applied at convCallExpr's two synthesis blocks and convSelectorExpr's
method-group form): `clone(asig)` emits `clone<ΔSignature>(asig)`, and a callee whose remaining
parameters make C# inference sufficient stays bare (`setThrough(Ꮡn, 55)`). An EXPLICITLY written Go
instantiation equally drops erased positions — full (`setThrough[*int, int](…)` →
`setThrough<nint>(…)`), partial (`clone[*thing](…)` → bare `clone(…)`, the rest inferring), and the
function-VALUE form (`fv := clone[*thing, thing]` → `var fv = clone<thing>;`) — via
`explicitTypeArgsAfterErasure` in convIndexExpr/convIndexListExpr. A C# consumer calls the emitted
method naturally — `T` sits in a real parameter position, so inference works without spelling the
phantom `P`.

The pointer classification flips at every use shape, not just the deref/address pair: returning the
parameter WHOLE yields its box (`return a` → `return Ꮡa;`), passing it onward to another erased
callee — including self-recursion — supplies the box (`cloneChain<T>(clone<T>(Ꮡp), …)`; the
interface-shaped argument arm carves out erased params exactly like instantiated pointers), copying
it into a local is a Go pointer copy (`q := p` → `var q = Ꮡp;`, writes through `q` land in the
caller's referent), and a nil comparison takes the box form with the nil-safe entry alias
(`if p == nil` → `ref var p = ref Ꮡp.DerefOrNil(); … if (Ꮡp == nil)` — a nil argument reaches the
guard instead of throwing at entry, e.g. `orZero[*int, int](nil)`). The NAMED constraint-interface
spellings — `[P PtrOf[T]]` and the embedded `[P interface{ PtrOf[T] }]`, where
`type PtrOf[T any] interface{ *T }` — resolve to the identical singleton type set and erase
identically. The constraint interface's own DECLARATION follows the existing constraint-interface
convention (`[GoType] partial interface PtrOf<T> { /* Type constraints: *T */ }`): a pointer term is
a type-set term, not an embeddable interface (previously it emitted an interface inheriting the
struct `ж<T>` — CS0527), and a GENERIC constraint interface carries its own `<T>` list, so the
arity-0 `<ΔT>` marker list and its generated operator machinery are both suppressed for it.

Erasure is deliberately gated to the identity case: **function** type parameters whose constraint
type-set is a single non-tilde pointer term. Declined shapes warn instead of silently mis-emitting —
an approximate `~*T` admits *named* pointer types, which emit as `[GoType("ж<E>")]` wrapper classes
(not identity with `ж<E>`); pointer unions have no single identity; and erasing a generic *named
type's* parameter would change its emitted arity at every use. None occur anywhere in the converted
stdlib (exhaustive GOROOT census: go/types' `clone` is the only compiled occurrence of the pattern;
see `DESIGN-pointer-core-typeparam.md` on the fix branch for the full study). (Guarded by
`PointerCoreConstraints` — clone/read/write/round-trip through `[P *T]` and the swapped-order
`[T any, P *T]`, flat-copy independence verified, values vs Go.)

### An integer named-numeric wrapper implements the integer operator interfaces

A `[GoType num:]` wrapper (`type stringID uint64`) already declared the *common* numeric operator
interfaces so it could serve a `cmp.Ordered`-shaped constraint (`IAddition`/`ISubtraction`/
`IMultiply`/`IDivision`/`IEquality`/`IComparison`/`IIncrement`/`IDecrementOperators`), but the
*integer-only* three — `IModulusOperators`, `IBitwiseOperators`, `IShiftOperators<T, int, T>` —
were deliberately left off because their operators (`%`, `&|^~`, `<<`, `>>`) are kind-gated. That
left a named integer type unable to satisfy a converter-emitted `~integer` operator constraint:
internal/trace's `type dataTable[EI ~uint64, E any]` instantiated with `type stringID uint64` was
CS0315 ×48 on exactly those three interfaces. The `NumericTypeTemplate` operators already exist
(same kind-gate), so `InheritedTypeTemplate` now also *declares* the three integer interfaces for an
integer underlying (float/complex keep only the common set). `IShiftOperators` additionally requires
`operator >>>` (unsigned right shift) — added to the integer operator block; Go emits no `>>>`, but
the member is needed to satisfy the interface. Cleared internal/trace's 48 CS0315 (49→1, the residual
being the unrelated ΔLabel CS0542). Guarded by `NamedNumericOperatorConstraint` (a generic
`mix[K ~uint64 | ~int32]` applying modulus/bitwise/both-shifts on the type parameter, instantiated
with a named `uint64` and a named `int32`, values vs Go). Corpus-verified against math/big (Word),
archive/tar, and time (Duration).

### Lifted shift constraint uses the BCL shape `IShiftOperators<T, int, T>`

The lifted Integer operator set constrains shifts as `IShiftOperators<T, int, T>` — the shift **count** is `int`, not the type parameter. Every [BCL](Glossary.md#bcl) binary integer implements exactly that shape (`IShiftOperators<TSelf, int, TSelf>`); only C# `int` itself happens to also satisfy the self-typed form, so the self-typed constraint made every non-`int` instantiation fail (CS0315 — strconv's `bsearch[S ~[]E, E ~uint16 | ~uint32]` on `ushort`/`uint`). The shape is also exactly what emitted bodies need: the converter coerces every shift count to `int` (`x << (int)(k)`), so a generic body can only ever perform `T << int`. The generated named-constraint interface template (`Integer` in go2cs-gen) and its dynamic-conversion placeholder shift operators use the same `int`-count shape, keeping the two emitters consistent. (Guarded by the `GenericTypeInference` extensions `bsearchLike`/`halve` — `~uint16 | ~uint32` instantiations with a shift on the type parameter, values vs Go.)

### Builtins over constrained slice type parameters

golib's builtins carry **interface-typed overloads** so a value held as a constrained type parameter (`S ~[]E`, boxed to its `ISlice<E>` constraint) binds directly: `copy(ISlice<T1> dst, ISlice<T2> src)` (plus an `ISlice<byte>`/`@string` form), `clear(ISlice<T> s)`, and two-argument `min`/`max` constrained on `IComparisonOperators` (Go's `cmp.Ordered` lifts to operator interfaces; a constrained `E` has no `IComparable<E>` conversion). The box wraps the same backing array, so interface writes land in the caller's storage — `copy`/`clear` into an `S` are true write-throughs (span windows, memmove semantics for overlap). Overload resolution keeps concrete calls on the exact `slice<T>` overloads (an exact parameter beats a boxing conversion), so nothing outside generic bodies changes. Cleared ~37 of the slices package's constraint seams. (Guarded by the `GenericTypeInference` extension `CopyClearMinMax` — copy into and clear through constrained values, write-through verified by value vs Go.)

**S-preserving sub-slice and append.** Go's sub-slice of a named slice type yields the *same named type sharing the same backing* — pdqsort's recursion depends on it (`pdqsort(s[:mid])` with `s S`). The `ISliceWrap<TSelf, T>` static-abstract factory (`TSelf Wrap(in slice<T> source)`) supplies the non-copying reconstruction: `slice<T>` implements it as identity, every generated named-slice wrapper wraps the window in its own type, and the `~[]E` where-clause carries it (`ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>`). A sub-slice of a constrained type parameter emits golib's `subslice<S, E>(s, lo, hi)` (type arguments explicit — `E` is constraint-only) which routes `S.Wrap(new slice<E>(s.Slice(…)))`; the new `slice<T>(ISlice<T> view)` constructor SHARES storage (unboxes a `slice<T>`, reconstructs any other implementer from its source array and window). `append` on a constrained value binds golib's `append<S, T>(S, params ReadOnlySpan<T>)` (S from the first argument, T from the span — fully inferrable) and wraps the result back to S; its body routes to the core `slice<T>.Append` directly, since a recursive `append(…)` call would resolve back to itself (`slice<T>` satisfies the constraints). The same change fixed the named-slice WRAPPER template's sub-slice members, which routed through `ToSpan()` — *detached copies*, a silent write-through divergence for named slice types generally; they now route through the wrapped `m_value` (sharing). (Guarded by the `GenericTypeInference` extensions `SumHalves` — recursion over sub-slices of S with a write through the deepest view, verified against the caller's array — and `AppendKeep`.)
Every generated named-slice wrapper also implements the non-generic `IArray` surface explicitly. The public typed `Source` remains `T[]` for the concrete wrapper, but the interface member is emitted as `Array IArray.Source => ((IArray)m_value).Source!;`, matching golib's `IArray.Source` contract and keeping `len(IArray)`, element-address helpers, and interface-typed builtins bound to the wrapper. Pointer elements use the same form, e.g. `type queue []*item` emits `ISlice<ж<item>>` plus the explicit `Array IArray.Source` member. (Guarded by `NamedSlicePointerElements`.)

**S where `[]E` is expected.** Go assignability lets a named-slice-typed value pass where the unnamed `[]E` is expected (`rotateRight(s[m:i], …)`, `pdqsortOrdered(x, …)`); the converter materializes such an argument through the SHARING `slice<T>(ISlice<T>)` constructor — `pdqsortOrdered(new slice<E>(x), …)` — a cast cannot apply (interface-constrained source; C# forbids user conversions from interfaces). The constructor unboxes a boxed `slice<T>` directly and otherwise takes the implementer's full-window interface sub-slice, which every golib implementer returns as a boxed shared `slice<T>` — NOT `Source`, which materializes a detached copy (caught by the write-through gate: the helper's write must land in the caller's array). The 3-index form on a constrained value emits `subslice3<S, E>`, and a constrained spread (`append(s, v.ꓸꓸꓸ)` — a `Span<E>`) binds an exact `params Span<T>` twin of the constrained append (betterness otherwise picked the legacy `params T[]` candidate with `T = Span<E>`, a ref struct as type argument — CS9244). (Guarded by the `GenericTypeInference` extension `PassSlice` — S passed to a concrete `[]E` helper, write-through verified by value vs Go.)

**Range-over-func on named/generic Seq types.** Go 1.23's `for v := range seq` (and the two-value `for k, v := range seq2`) on an `iter.Seq[E]`-shaped value emits through golib's yield-adapting `range()` overloads. Three pieces make the named/generic form work: detection unwraps the type's `Underlying()` (a defined or instantiated func type is a `Named`, not a bare `Signature`); a NAMED func type renders as a C# *delegate*, which has no conversion to the overloads' `Action<Func<…>>` parameter — its method GROUP does, so the emission appends `.Invoke`; and because C# cannot infer a type parameter from a method group's parameters, the element types are spelled out from the yield signature: `foreach (var v in range<nint>(countdown(5).Invoke))`. `break` inside the body ends the foreach, which cancels the adapter's producer — the yield function receives `false`, matching Go's semantics; a two-value `range<K, V>` overload adapts pair-yields onto the tuple machinery. One adjacent gate was refined en route: a call's result being a generic instantiation adds explicit type arguments only for conversions and GENERIC callees (`NewOption<nint>(42)` — an untyped-const arg would infer C# `int` where Go infers `nint`), never for a plain function returning a generic named type (`countdown<nint>(5)` was CS0308). (Guarded by the `GenericTypeInference` extensions — a generic `Seq[V]` ranged with `break` and a two-value `KVSeq[K, V]`, values vs Go.)

**An EXPLICITLY-instantiated generic function through a package selector renders its type arguments once.** Go's `pkg.Func[T](…)` is an `IndexExpr` (or `IndexListExpr` for `pkg.Func[K, V]`) whose base `X` is the selector `pkg.Func`. `convIndexExpr`/`convIndexListExpr` renders the `[T]` as `<T>` itself. But the base is *also* a generic-function value, so `convSelectorExpr` — which spells a generic function's inferred type arguments when it appears as a method-group **value** (the `slices.SortFunc(all, slices.Compare)` path, needed because C# can't infer a method group's type parameters) — appended `<T>` a second time, producing `pkg.Func<T><T>()`. Depending on context this surfaced as CS1525 (`reflect.TypeFor[X]()` → invalid expression term), CS0119 (a plain-return generic like `saferio.SliceCap[T]`), or CS8124 (`<T>()` parsed as a one-element tuple) — ~67 errors across encoding/gob, xml, asn1, json, text/template, database/sql/driver, debug/macho·pe·elf, and unique. The index expression now converts its base with a `suppressGenericTypeArgs` context flag, so `convSelectorExpr` skips the value-path append when it is the base of an explicit instantiation (the standalone method-group-value case is unchanged — no flag, still appends). A *local* generic function (`Func[T]()`, base is an `Ident` not a selector) never hit this, since only `convSelectorExpr` appends. (Guarded by the `CrossPkgUser` extension — `CrossPkgLib.Wrap[int](5)` (IndexExpr) and `CrossPkgLib.Pair[string, int](…)` (IndexListExpr), both rendering single type-argument lists, output vs Go.)

### Integer type-parameter conversions route through golib (the `E(100)` family)

C# has no cast to or from a type parameter, so the Go conversions in `rand.N[Int intType]` — `Int(x)`, `uint64(n)` — and an untyped constant compared against the parameter (`n <= 0`, which Go types AS `Int` but C# leaves as `int`, unacceptable to the lifted `IComparisonOperators<Int, Int, bool>`) all failed (CS0030/CS0019). Three coordinated pieces, gated on a constraint whose every type-set term has an **integer underlying** (`typeParamIsInteger`): a conversion **to** the parameter emits golib's runtime-typed `ConvertToType<Int>(…)` (typeof-dispatch that JIT-folds to a single branch per instantiation; signed kinds sign-extend, unsigned zero-extend — Go's exact conversion semantics; a `[GoType("num:*")]` wrapper instantiation falls back to a reflection-cached bridge over its `Value` property/ctor); a conversion **from** the parameter to a basic integer emits `ConvertToUInt64<Int>(n)` (plus a plain numeric cast when the target is not `uint64`); and a **constant operand** of a binary op against the parameter materializes via `ConvertToType<Int>(0)` — except a SHIFT count, which Go types independently and the emission already coerces to `int`. Result: `if (n <= ConvertToType<Int>(0)) … return ConvertToType<Int>(ConvertToUInt64<Int>(n) / 2);`. (Guarded by the `GenericTypeInference` extension `halveN` — `~int32 | ~int64` with the compare, both conversions, and a negative value proving sign-extension, values vs Go; clears math/rand/v2's `N`.)

### A named-wide-integer or type-parameter slice index casts to `nint`

Go permits any integer type as a slice/array index, converting it to `int` for the access. The C#
`slice<E>`/`array<E>` indexer takes `nint`, and the existing wide-*basic* index cast already routed
`uint`/`uint32`/`uint64`/`uintptr`/`int64` through `(nint)`. Two more index kinds need it, both from
internal/trace's `dataTable`:
- a **named type over signed `int64`** — `type ProcID int64` indexing `spans[procID]` (CS1503).
  There is no `this[long]` overload, `int64→nint` does not narrow implicitly, and `int64→ulong`
  (which would bind `this[ulong]`) is a signed→unsigned conversion — so it has no bare path.
  `(nint)(procID)` composes as one *user* conversion (named→long) plus one *built-in* (long→nint).
  Every other kind is deliberately **excluded** — no churn, and casting some would even break: an
  *unsigned* named type (`type kindT uint`/uint32/uint64) binds the golib `this[ulong]` overload
  bare, and a nuint-backed wrapper (`uint`/`uintptr`) is *CS0030* under a `(nint)` cast; an
  `int`/`int32`/`nint` underlying narrows implicitly (`type rank int` stays bare). So only signed
  `int64` both *needs* and *accepts* the cast;
- a numeric **type parameter** — `dataTable[EI ~uint64]` doing `d.dense[id]`. A constrained type
  parameter has no C# cast at all, so it routes through golib's `ConvertToUInt64<K>` bridge (the
  `E(100)` integer-type-param family above) and then narrows: `d.dense[(nint)(ConvertToUInt64<EI>(id))]`
  (an arithmetic index `id/8` is still `EI`, so it wraps the same way).

Cleared ~11 of internal/trace's index CS1503/CS0030 (17→6). The companion **shift-*count*** case —
`1 << (id % 8)` where the count is a numeric type parameter, coerced to `int` by `intCastOperand`
(the same coercion the shift-width machinery uses) — routes through the same bridge:
`(uint8)1 << (int)(ConvertToUInt64<EI>(id % 8))` (a bare `(int)(EI)` is CS0030). Cleared internal/trace's
last two shift-count CS0030 (6→4). Guarded by `NamedNumericSliceIndex` (a generic `lookup[K ~uint64]`
indexing by the type parameter and an arithmetic result, a `pick` indexing by a named `int64`, a
`num:nint` `rank` index that must stay bare, and a `bitset[K ~uint64]` shifting by the type parameter,
values vs Go).

### Method-set interface constraints bind the interface directly; pointer instantiations project through the adapter

A type parameter constrained by a **regular method-set interface** (a pure method set, no type-term
unions — go/ast's `walkList[N Node](v Visitor, list []N)`) emits `where N : Node` against the
arity-0 emitted interface. Only union+method **constraint interfaces** take the generic CRTP form
(`ConstraintTest1<ΔT>`); the method-set arm previously emitted the phantom `Node<N>, new()`, which
was doubly wrong: `Node` is emitted arity-0 (CS0308), and the instantiation may itself be an
*interface* (walkList takes `N=Stmt/Expr/Spec/Decl`), which can never satisfy `new()`. The
interface-typed and interface-inheriting instantiations then satisfy the constraint natively
(`Stmt : Node` is emitted inheritance).

A **pointer** instantiation (`walkList(v, n.Names)` with `N=*Ident`) cannot: the `ж<Ident>` box
does not implement the interface — its generated pointer adapter does. The call site projects the
slice element-wise through the adapter, instantiating `N` as the interface itself:

```csharp
internal static void walkList<N>(Visitor v, slice<N> list)
    where N : Node
{
    foreach (var (_, node) in list) {
        Walk(v, node);
    }
}
// call site, N=*Ident:
walkList(v, widen<ж<Ident>, Node>((~n).Names, elemᴛ1 => new IdentжNode(elemᴛ1)));
```

golib's `widen<T, TWide>(slice<T>, Func<T, TWide>)` copies the slice HEADER only — elements alias
the original objects through the shared boxes, so method calls through the projected slice mutate
the real objects. A callee that reassigned `list[i]` itself would not write back; the projection
targets the read/widen shape (Go itself performs the same per-element interface widening inside
the loop). `convertToInterfaceType` supplies the adapter reference and the `GoImplement` recording,
exactly as at scalar `*T→iface` call sites. (Guarded by `GenericInterfaceConstraint` — pointer,
interface, and embedded-interface instantiations of a method-set-constrained generic, calling a
constraint method on the parameter and widening walkList-style, values vs Go; clears go/ast's
CS0308, the go/* toolchain gate.)

### A SELF-REFERENTIAL generic method-set constraint uses a box-wrapping proxy as the type argument

The widen-to-the-interface escape above works only when the constraint interface is **non-generic**
(`N=Node`, so `N` can *be* `Node`). crypto/elliptic's `nistCurve[Point nistPoint[Point]]` — where
`nistPoint[T]` is a **generic, self-referential** method-set interface (`Add(T,T) T`,
`SetBytes([]byte) (T,error)`, …) — cannot: you can't substitute `Point = nistPoint<Point>` (infinite
regress), and the golib box `ж<P224Point>` cannot *nominally* implement `nistPoint<ж<P224Point>>`
(it is a sealed golib type in another assembly, and Go's structural satisfaction has no C# analog).
Four coordinated pieces make it convert **and dispatch**:

1. **The constraint interface is emitted GENERIC.** A method-set interface whose own Go type
   parameter is used in its member signatures carries its `<T>` (and constraints) in C#, exactly like
   a generic struct — `[GoType] partial interface nistPoint<T> { … T Add(T, T); (T, error) SetBytes(slice<byte> _); }`.
   Without it the declaration is arity-0 yet the constraint that references it spells the arity-1
   `where Point : nistPoint<Point>` (CS0308) and every bare `T` is undefined (CS0246). (Go's
   operator-only constraint interfaces are arity-0 *in Go*, so this is disjoint from the `<ΔT>`
   operator machinery.)

2. **One GENERIC adapter class** implements the outer interface: `nistCurveжCurve<Point> : Curve,
   IжAdapter where Point : nistPoint<Point>` wrapping `ж<nistCurve<Point>>` — NOT a class per
   instantiation. The converter's `GoImplement` records are per-instantiation but all resolve to the
   open form here, so `ImplementGenerator` de-dups on the open `(struct, interface)` pair and forwards
   its type parameters and the struct's own constraint (`GetGenericConstraintClause`). The converter
   composes the reference name+args separately (`nistCurveжCurve<…>`, base+`ж`+iface, then the closed
   args) so the type arguments do not bake into the identifier (the old `nistCurve<…>жCurve` was CS1526).

3. **A self-referential PROXY stands in for the type argument.** For each concrete pointer type used
   to instantiate the generic (`nistCurve[*P224Point]`), the converter renders the type argument as a
   generated proxy `P224PointжnistPoint` (element-simple+`ж`+iface-simple) instead of the box `ж<P224Point>`,
   and records `[assembly: GoImplement<P224Point, nistPoint<P224Point>>(ConstraintProxy = true)]` (the
   interface's own argument is a placeholder). `ImplementGenerator.EmitConstraintProxy` emits:

   ```csharp
   internal sealed class P224PointжnistPoint : nistPoint<P224PointжnistPoint>, IжAdapter {
       private readonly ж<P224Point> m_box;
       public P224PointжnistPoint(ж<P224Point> box) => m_box = box;
       public static implicit operator P224PointжnistPoint(ж<P224Point> box) => new(box);
       public static implicit operator ж<P224Point>(P224PointжnistPoint proxy) => proxy.m_box;
       // T rewritten to the proxy itself; the implicit conversions marshal every T-boundary:
       P224PointжnistPoint nistPoint<P224PointжnistPoint>.Add(P224PointжnistPoint a, P224PointжnistPoint b) => m_box.Add(a, b);
       (P224PointжnistPoint, error) nistPoint<P224PointжnistPoint>.SetBytes(slice<byte> b) => m_box.SetBytes(b);
       // …
   }
   ```

   The proxy implements the interface **over itself**, so `Point = P224PointжnistPoint` satisfies
   `where Point : nistPoint<Point>` (CS0311 otherwise) *and* resolves every `p.Add(…)`/`newPoint().SetBytes(…)`
   call inside `nistCurve`'s body. The implicit `ж<P224Point>`↔proxy conversions do all the T-boundary
   marshalling automatically: each forwarder is a bare `m_box.M(args)` (arguments unwrap to the box on
   the way in, results rewrap to the proxy on the way out — including element-wise inside a
   `(T, error)` tuple), and a value flowing into a `Point`-typed position (`base: Ꮡ(new P224Point(…))`)
   converts implicitly at the site. The proxy forwards to the element's **exported** `ж`-extensions even
   cross-assembly (`m_box.SetBytes` binds nistec's extension from crypto/elliptic).

4. **A `func()`-typed field's method-group initializer is re-wrapped as a lambda.** `nistCurve`'s
   `newPoint func() Point` becomes `Func<P224PointжnistPoint>`, but a method group (`newPoint: nistec.NewP224Point`,
   returning `ж<P224Point>`) cannot convert to it — a C# method-group conversion does not apply the
   user-defined implicit operator (CS0407). Inside a constraint-proxy composite the converter re-wraps
   such an initializer as a lambda, `newPoint: () => nistec.NewP224Point()`, whose *return* position
   does apply the conversion.

(Guarded by `GenericPointerInterfaceImpl` — a self-referential `curve[Point point[Point]]` implementing
`Curve` via pointer receiver, instantiated two ways, with a `newPoint func() Point` field and a
`(T, error)`-returning constraint method, values vs Go. Embedding the constrained generic and greening
the whole crypto-curve family is the next subsection.)

### A struct embedding the constrained generic promotes its members — three residual crypto-curve fixes

crypto/elliptic's `p256Curve struct { nistCurve[*nistec.P256Point] }` — a **non-generic** struct
embedding a **concrete instantiation** of the self-referential-constrained generic above — must PROMOTE
`nistCurve`'s internal fields (`newPoint`, `params`) and methods (`Add`/`Double`/`Params`/`ScalarMult`/…)
onto `p256Curve`, exactly as an embed of a plain struct does, so `p256.params = …` binds and the generated
`p256Curve→Curve` interface adapter can forward `curve.Add(…)` to the promoted shim. Because the type
argument is the box-wrapping proxy of the previous subsection, its rendered name **embeds the marker glyph
`ж`** (`nistCurve<P256PointжnistPoint>`) — the thread that runs through all three fixes that made the whole
crypto-curve family (elliptic, ecdh, nistec) COMPILE (+3 packages):

1. **The proxy marker glyph `ж` is not a pointer prefix.** The generator's simple-name / underlying-name
   helpers (`GetSimpleName`, `GetUnderlyingTypeName`) detected a pointer type `ж<T>` by scanning for a
   *bare* `ж` and slicing from it. The proxy's own name embeds that glyph mid-identifier
   (`P256Point`**`ж`**`nistPoint`), so an embed typed `nistCurve<P256PointжnistPoint>` was mis-sliced into
   garbage (its simple name became `oint.Value`, its underlying name an unresolvable string) and the embed
   promoted **nothing** (CS1061 on `params`, CS1929/CS1501 on every forwarded method). Both helpers now
   match the pointer prefix as the two-character `ж<`, so a marker embedded in an identifier is left intact.

2. **A generic-instantiation embed resolves to its declaration and substitutes its type arguments.** An
   embed of a generic INSTANTIATION (`nistCurve<P256PointжnistPoint>`) resolves to the generic DECLARATION
   (`nistCurve<Point>`) by base-name + arity (`FindStructDeclaration` — an instantiation can never
   string-match a declaration that carries its type PARAMETERS), and a generic struct's extension methods
   now match on the type-parameter-bearing receiver (`nistCurve<Point>`, not the bare `nistCurve`). The
   promoted field and method signatures are harvested from the declaration, so they carry its type
   PARAMETER (`Func<Point>`, `pointFromAffine` returning `(Point, error)`); the template rewrites each to
   the instantiation's type ARGUMENT before emission —

   ```csharp
   internal ref global::System.Func<P256PointжnistPoint> newPoint => ref nistCurve.newPoint;
   internal static (P256PointжnistPoint p, error err) pointFromAffine(this ref p256Curve target, ж<bigꓸInt> Ꮡx, ж<bigꓸInt> Ꮡy)
       => target.nistCurve.pointFromAffine(Ꮡx, Ꮡy);
   ```

   — so no promoted member references the out-of-scope `Point`. (The member ACCESS hop keeps the bare
   property name `nistCurve`; only the emitted TYPE is substituted.) When the ENCLOSING struct is itself
   GENERIC — `wrapped<T>` embedding `tag<T>` (the `GenericStructFields` guard) — the promoted method is
   a GENERIC extension method carrying the struct's own type parameters (`static T show<T>(this wrapped<T>
   target) => target.tag.show();`, the substitution then an identity `T`→`T`), else the `T` in the
   receiver and return is an undefined type name (CS0246).

3. **The constraint proxy imports its element's package namespace.** The proxy forwards each interface
   method to the boxed element's box extension methods (`m_box.Bytes()`), which live in the element type's
   PACKAGE class (`nistec_package`, namespace `go.crypto.@internal`). The `[assembly: GoImplement<…>(ConstraintProxy = true)]`
   attribute driving the proxy sits in `package_info.cs`, whose usings never cover a FOREIGN element, so the
   forwarders bound nothing (`ж<P224Point>` "has no `Bytes`", CS1929/CS1501). `EmitConstraintProxy` now emits
   `using <element-namespace>;` for the box element's namespace.

4. **An open-generic interface cast is CONVERTED but not RECORDED.** Inside a generic method the receiver
   itself is cast to the interface — crypto/ecdh's `return newBoringPrivateKey(c, …)` with `c *nistCurve[Point]`.
   The converter must still WRAP it in the generic adapter (`new nistCurveжΔCurve<Point>(Ꮡc)` — the adapter
   the CLOSED per-instantiation records already generate), but must NOT RECORD it as an implementation: a
   record emits `[assembly: GoImplement<nistCurve<Point>, ΔCurve>]`, whose type-PARAMETER argument `Point`
   is out of scope in an assembly attribute (CS0246). `convertToInterfaceType` now skips the record for an
   open-generic target while still firing the adapter-wrapping conversion.

(Fix 2 is guarded by the `GenericEmbedPromotion` behavioral test — a non-generic struct embedding a concrete
`curve[*p224]` over a self-referential proxy: reading a promoted internal field, calling a promoted method
whose parameter is the type argument (passed the promoted proxy-typed field), and reaching the promoted
methods through a non-generic interface adapter, values vs Go. Fixes 3 and 4 need a cross-package element /
a generic-method interface cast the single-package baseline cannot express; they are validated by the census
— elliptic, ecdh, and nistec now emit their DLLs, 254 → 257 packages.)

### Constraint-only type parameters need explicit type arguments

Go infers a type parameter that appears only in *constraints* through core types — `func Twice[S ~[]E, E Integer](s S)` infers `E` from `S`'s underlying element; the `slices` package's whole `Sort[S ~[]E, E cmp.Ordered] → pdqsortOrdered` chain relies on this. C# never infers a type parameter that does not appear in the parameter list (CS0411 — at *every* call site, concrete instantiations included). When the callee declares such a constraint-only type parameter, the converter renders the call's type arguments explicitly from the instantiation `go/types` already resolved (`info.Instances`): `Twice<Point, int32>(p, 2)` at a concrete site, `Scale<S, E>(s, c)` inside a generic body. Calls to generics whose every type parameter is argument-visible keep their bare Go-shaped form — C# infers them as Go does, no churn. (Guarded by the `GenericTypeInference` extension — a constrained `S`/`E` pass-through chain plus a concrete call to a constraint-only-param generic, values vs Go; clears the 14 CS0411s in the slices/maps wave.)

The same explicit-type-argument rule applies to a generic function referenced as a **method-group value**, not just a call. `slices.SortFunc(all, slices.Compare)` (runtime/pprof) passes `slices.Compare[S ~[]E, E cmp.Ordered]` as `SortFunc`'s comparison delegate; C# cannot infer a generic method group's constraint-only `E` when converting it to `Func<…>` (CS0411). `convSelectorExpr` now spells the arguments on the selector — `slices.Compare<slice<uintptr>, uintptr>` — when the selector is NOT the callee of a call (`!context.isCallExpr`, so convCallExpr's own type-arg site still owns the call form) and `info.Uses[Sel]` is a generic function with an `info.Instances` instantiation. Byte-identical across the behavioral corpus and across an A/B of pprof+slices+sort+maps+cmp+net+go/types (a single line moves — the `slices.Compare` argument; every `Compare(...)` **call** stays bare). GUARD OWED — the shape needs a cross-package generic function with a constraint-only type parameter passed as a method-group value, which the single-package baseline cannot express; the bare-IDENT variant (a same-package generic func passed as a method group) is a parallel latent case left unfixed because `convIdent`, unlike `convSelectorExpr`, carries no call-vs-value flag to gate against double-emitting a direct call's arguments.

### The `comparable` constraint

Go's built-in `comparable` admits every `==`-able Go type — numerics, strings, pointers, channels, and comparable structs/arrays/interfaces. No C# constraint can express that set: golib's old `comparable<T>` CRTP interface was implemented by *nothing* (every real instantiation failed — `maps.Keys[M ~map[K]V, K comparable]` could not be used at all), and lifting `IEqualityOperators` would reject structs, which Go admits. A `comparable` type parameter therefore emits **no C# constraint** beyond the standard `new()` — `where K : /* comparable */ new()` — relying on the two facts that make it sound: Go's checker already validated every instantiation, and emitted equality on type parameters routes through `AreEqual`, never operator `==`.

`AreEqual` itself is not a performance tax on that path: a generic overload `AreEqual<T>(T, T)` — automatically preferred by overload resolution exactly where both operands share the type parameter — takes `EqualityComparer<T>.Default.Equals` for value-type arguments, which the JIT specializes per type and devirtualizes to the type's own `IEquatable<T>` (operator-comparable speed, no reflection or boxing; golib wrappers emit `operator ==` and `Equals` as consistent pairs, so semantics match). Reference/interface type arguments delegate to the reflective `AreEqual(object, object)` overload, preserving its typed-null and runtime-type semantics. (A constraint-differentiated overload pair is not expressible — C# treats `where` clauses as outside the signature, CS0111 — and a source-generated `==` twin is unnecessary given the `EqualityComparer<T>.Default` JIT intrinsic.) (The behavioral `GenericVariadicFunc` golden captures the erased form with unchanged output.)

### The `string | []byte` union
C# generic constraints are conjunctive ("and"), so they cannot express Go's `string | []byte` union directly. The two members share no operators (the union is neither comparable nor additive), so a conforming body may only use the read operations common to both — indexing, `len`, and sub-slicing. These are captured by the golib read-only byte-sequence interface [`IByteSeq<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/IByteSeq.cs), which both `@string` (as `IByteSeq<byte>`) and `slice<T>` implement; the converter emits it for the union and suppresses the (spurious) lifted operator constraints:
```go
func HashStr[T string | []byte](sep T) uint32 { /* uses sep[i], len(sep) */ }
```
```csharp
public static uint32 HashStr<T>(T sep)
    where T : /* string | []byte */ IByteSeq<byte>, new()
{ /* … */ }
```
A sub-slice of a constrained value is itself an `IByteSeq<byte>`, and Go's `string(s[lo:hi])` becomes `new @string(s[lo..hi])` via an `@string(IByteSeq<byte>)` constructor. `len` resolves through a generic `len<T>(IByteSeq<T>)` overload that is dispreferred for concrete `slice<byte>`/`@string` arguments, so existing call sites keep their specific overloads (no ambiguity). The behavioral test `StringByteUnionConstraint` exercises both the `string` and `[]byte` instantiations.

### Explicit type arguments come from the callee's instantiation
A generic function's explicit type arguments are read from the CALLEE's resolved instantiation (`info.Instances`), not from the RESULT type's arguments -- the two lists differ whenever the callee has more type parameters than the result names. reflect's `rangeNum[T, N](num N) iter.Seq[T]` called `rangeNum[int8](v)`: the result `Seq[T]` carries ONE argument where the method needs TWO (CS0305). The result's own arguments still gate *whether* to emit, so a generic callee returning a plain named type keeps C# inference:
```csharp
return rangeNum<int8, int64>(v.Int());
```
Guarded by `GenericTypeInference` (`seqOf[T ~int64, N ~int32 | ~int64](n N) Seq[T]`).

### Increment/decrement on a type parameter
`i++` / `i--` on a constrained type parameter binds `IIncrementOperators<T>` / `IDecrementOperators<T>`, which the lifted **Arithmetic** operator set now includes (reflect `rangeNum`'s loop, CS0023). They live in the numeric-only Arithmetic set -- never the string-including Sum set, since `@string` implements neither. The list is emitted in two places that must stay in sync: the converter's `getLiftedConstraints` (`constraintOperations.go`) and the go2cs-gen `InterfaceTypeTemplate`.

### `uintptr` as a generic numeric type argument
The golib `uintptr` struct declares the full generic-math interface set the lifted numeric constraints demand (`IAdditionOperators` through `IComparisonOperators`, `IShiftOperators<uintptr, int, uintptr>` with a `>>>` operator, `IIncrementOperators`/`IDecrementOperators`) -- matching operators alone never satisfy a C# where-clause (CS0315 at reflect's `rangeNum<uintptr, uint64>`). At runtime, `ConvertToType`/`ConvertToUInt64` have `uintptr` fast paths, and the reflection-cached `TypeParamCaster` probes a public `Value` FIELD as well as the generated wrappers' `Value` property (hand-written wrappers keep a field for `Interlocked`/`Volatile` `ref x.Value` seams). Guarded by `GenericTypeInference` (`growShrink[U ~uint32 | ~uintptr]`).

> **Latent gap ([banked](Glossary.md#banked)):** generated `[GoType("num:*")]` wrapper structs do NOT yet declare the generic-math interfaces -- a NAMED numeric wrapper used as a union-generic type argument would CS0315. No corpus site hits this yet.

### Union-constrained sub-slices cast back to the type parameter
A sub-slice of a `string | []byte` union-constrained value goes through the `IByteSeq<byte>` Range indexer, which returns the INTERFACE -- but Go types the result as the type parameter again, so it assigns back to, passes as, and returns as the parameter (time format_rfc3339, CS0266/CS0310/CS0029). The emission wraps the range forms in the explicit interface-to-type-parameter conversion -- a runtime-checked unbox that shares backing for the `[]byte` instantiation:
```csharp
return parse(((T)(s[0..2]))) + parse(((T)(s[3..5])));
```
Func-literal parameters typed as the union type parameter render as the parameter itself (the enclosing method's type parameter is in scope inside a lambda), matching the Go:
```csharp
var parse = (T part) => {
```
Guarded by `StringByteUnionConstraint` (`trimHead`/`headSum`; `digitSum`).

### Spreading a union-constrained value
A union-constrained value may also be **spread** into a variadic — encoding/json's `appendString[Bytes []byte | string]` does `append(dst, src[lo:hi]...)` (and the open-ended `append(dst, src[lo:]...)`). The sub-slice is typed as the type parameter again, so the cast-back above wraps it, and the spread renders as `((Bytes)(src[lo..hi])).ꓸꓸꓸ`. A bare type-parameter value has no members of its own, so the spread `ꓸꓸꓸ` (which yields the `Span<byte>` the `append<T>(slice<T>, params Span<T>)` overload binds) must be declared on the **constraint interface** — a member access on a constrained type-parameter value resolves through its constraint. `IByteSeq<T>` therefore exposes `Span<T> ꓸꓸꓸ { get; }`; both implementers already satisfy it (`slice<T>` as `Span<T>`, `@string` as `Span<byte>`), so the interface member is implicit and adds no cast (CS1061 otherwise — the type parameter `Bytes` had no `ꓸꓸꓸ`). (Guarded by the `StringByteUnionConstraint` extension `appendRun` — a bounded and an open-ended sub-slice of the union value spread into `append`, both instantiations value-compared vs Go.)

## Type Aliasing
Go supports two kinds of [type aliasing](https://go101.org/article/type-system-overview.html#type-definition): a "type definition" and a "type alias declaration".

### Type Definitions
For a Go "type definition" the new type is a distinct type that shares an [underlying type](https://go101.org/article/type-system-overview.html#underlying-type) with its base. Because converted types are structs (no inheritance), the converter relies on the source generators (see [Source Generators](#source-generators)) to emit the bridging needed for these to be used interchangeably while remaining distinct: implicit conversion operators down to the underlying type (via `ImplicitConvGenerator` / `TypeGenerator`), and, where the base is a built-in like `slice`, the relevant interface (`ISlice<T>`, etc.) implementation. A named type also supports the extension methods (receiver functions) of its underlying types, which the generators surface as proxy/overload methods.

When a pointer conversion `(*Target)(srcPtr)` bridges two structurally-identical structs, the converter records an **indirect** (boxing) implicit conversion `Source → ж<Target>` and `ImplicitConvGenerator` emits `implicit operator ж<Target>(Source src) => Ꮡ(new Target(<members>))`. For a **self-boxing** conversion — `Source` and `Target` are the *same* struct (`mspan → ж<mspan>`), which arises from a self-referential struct's recursive sub-struct conversions — that member-by-member reconstruction is both unnecessary and wrong: a pointer field whose target ctor parameter is itself a `ж<…>` was deref'd (`src.f?.Value ?? default!`), and a value cannot bind a pointer parameter (CS1503). The generator detects self-boxing (the boxed element type equals the source) and emits `Ꮡ(src)` instead — boxing a copy of the whole struct directly, identical in effect for a pointer-free struct and correct for one with pointer fields. (Validated by the green baseline build, which regenerates every `.g.cs`, plus the `TypeConversion` behavioral test for the non-self-boxing form; runtime exercised self-boxing for `mspan`, `g`, `stackScanState`, `hmap`, etc.)

### Type Alias Declarations
For a Go "type alias declaration" the alias matches C# aliasing implemented with the `using` keyword. Since the alias may be exported and referenced across files, the converter emits a **global** `using` (C# 10's [Global Using Directive](https://github.com/dotnet/csharplang/blob/main/proposals/csharp-10.0/GlobalUsingDirective.md)) into the package's generated aliases. For example:

```go
type P = *bool
type M = map[int]int
type table = map[string]int
```
```csharp
global using P = go.ж<bool>;
global using M = go.map<nint, nint>;
global using table = go.map<@string, nint>;
```

**A `global using` RHS renders csproj-alias numerics as C# keywords.** C# resolves a using directive's target *without reference to other using directives* — aliases are invisible to one another — so the golib csproj-level aliases (`uint64`, `float64`, `any`, …) that resolve everywhere else in the compilation are CS0246 inside `global using X = …;`. The alias-declaration emission (only) rewrites those names to their using-safe keyword/BCL equivalents: fiat's `type p224UntypedFieldElement = [4]uint64` emits `global using p224UntypedFieldElement = go.array<ulong>;`. Body code keeps the Go-visual alias names; already-safe names (`byte`, `bool`, `nint`, `go.@string`) are untouched, and the rewrite deliberately skips dot-qualified names so a package type sharing a builtin name is left alone. (Guarded by the `AliasStructComposite` extension `words` — an alias to `[4]uint64` used as a parameter type, output-compared.)

**An alias to an unnamed array/slice resolves through `types.Unalias` at type-switched decision points.** An alias is a `*types.Alias` (Go 1.22+) — neither the AST's `ast.ArrayType` nor the resolved `*types.Array` — so an emission that type-switches on the syntax node or the unresolved type misses it. Three sites resolve through `types.Unalias`: (1) the **`range` operand dispatch** — `for _, e := range w` on an alias-typed array previously matched no arm and emitted the *whole loop* as a C# comment, a silent behavioral hole; it now emits the normal `foreach (var (_, e) in w)`. (2) the **composite-literal dispatch** — `words{10, 20, 30, 40}` emits the same element-array projection the unnamed literal uses, `new uint64[]{10, 20, 30, 40}.array()`; the alias name renders as an Ident (not an `ast.ArrayType`), so it cannot take the composite-initializer bracket rewrite, and keeping it produced a C# collection initializer on the alias (`new words{…}` — CS1061, `array<T>` has no `Add`). (3) **`var` declarations** — a local or package-level `var w words` allocates the fixed-size backing (`words z = new(4);` / `internal static words gw = new(4);`) instead of `default!`/uninitialized, whose null backing array throws NRE on the first element write. An alias to a *named* type is unaffected: unaliasing lands on `*types.Named` and the existing wrapper-struct arms apply, with the alias name preserved. (Guarded by the `AliasStructComposite` extensions — alias-typed range loop, composite literal, and local + global `var` with element writes, values vs Go.)

**A same-package alias TARGET carries the package's FULL namespace, not just its class.** An exported alias whose target is *lifted* into the package class — `type CorpusEntry = struct{…}` lifts its anonymous struct to a nested `CorpusEntryᴛ1` (and the same for an alias to a same-package named type) — must qualify that target with the package's whole namespace. For a package in a **nested** namespace (`internal/fuzz` → namespace `go.@internal`, class `fuzz_package`; `net/http` → `go.net` / `http_package`) the lifted type lives at `go.@internal.fuzz_package.CorpusEntryᴛ1`. Building the qualifier from the bare `<pkg>_package` class alone dropped the intervening namespace segment, so the emitted `global using CorpusEntry = go.fuzz_package.CorpusEntryᴛ1;` — and the matching `[assembly: GoTypeAlias("CorpusEntry", "go.fuzz_package.CorpusEntryᴛ1")]` that every consumer replays verbatim through its `<ImportedTypeAliases>` block — named a namespace that does not exist → CS0234 at the using-alias line and at every use (internal/fuzz's `CorpusEntry`, ×60). The qualifier is now taken from the same `packageNamespace` that emits the `namespace …;` declaration (minus the root, plus the class), so the target and the declaration always agree: `go.@internal.fuzz_package.CorpusEntryᴛ1`. A **top-level** package's namespace is exactly the root (`go`), leaving no intervening segment, so its target stays `go.<pkg>_package.…` — the emission is byte-for-byte unchanged there. (Guarded by the `NestedAliasUser` behavioral test — a top-level `package main` that imports its own nested `inner` subpackage, whose C# namespace is `go.NestedAliasUser`; `inner` exports an anon-struct alias `Entry`, and both `inner`'s own `global using` and the consumer's imported `global using innerꓸEntry` resolve to `go.NestedAliasUser.inner_package.Entryᴛ1`, values vs Go.)

## Delegates to Value Receiver Instances

**A Go METHOD EXPRESSION** — `(*timers).run`, the *unbound* method as a func value whose first parameter is the receiver (runtime `time.go`'s `abi.FuncPCABIInternal((*timers).run)`) — selects a method off a **type**. Emitting the selector naively renders the type in value position (`(ж<timers>).run` — CS0119 + CS1503). Go types the expression as the func signature with the receiver prepended, so the converter renders that signature as the concrete delegate type and casts the method's static form to it: `(Func<ж<timers>, int64, int64>)(run)`. For a `[GoRecv]` method the `RecvGenerator`'s ж-overload matches the delegate exactly; a value-receiver method expression (`counter.get`) casts to its value-typed delegate (`(Func<counter, nint>)(get)`); a direct-ж method's primary form matches directly. (Guarded by the `MethodExpression` behavioral test — pointer- and value-receiver method expressions assigned, passed inline, and *invoked*, with mutations accumulating through the receiver box, values vs Go.)

A method expression on a **FOREIGN type** — `(*http.Request).Write` (net/http/httputil persist.go) — must additionally **qualify** the method's static form: the `[GoRecv]`/extension static (and its `RecvGenerator` ж-overload) lives in the *defining* package's class, so the bare name is CS0103 (or mis-binds a same-named local method group, CS0123). The Go operand necessarily spells the type through its package qualifier, so the converter peels `(*http.Request)` to the `http` package ident and renders it exactly as any package-qualified reference does (sanitization and collision renames included): `(Func<ж<http.Request>, io.Writer, error>)(http.Write)`. A same-package method expression keeps the bare name — the static is in scope — so existing emissions are unchanged. (Guarded by the `CrossPkgUser` extension — pointer-receiver `(*CrossPkgLib.Sensor).Calibrate` (write observed through the original receiver) plus value-receiver `Sensor.Hot` / `Celsius.Add` foreign method expressions, each invoked through its func value, output-compared vs Go.)

The **bound method value** — `d.compute = metricReader(read).compute` (runtime `metrics.go`), `types.MethodVal` used as a *value* — forwards through a lambda that captures the receiver expression and carries the **method's own parameters**, explicitly typed: `(ж<statAggregate> p1, ж<metricValue> p2) => ((metricReader)read).compute(p1, p2)`. The previous emission hardcoded arity zero (`() => x.m()`), mismatching any non-nullary target delegate (CS1593). One documented divergence: the receiver expression is evaluated *inside* the lambda (per call), where Go binds it once at method-value creation — acceptable for the compile milestone and the simple receivers observed. (Guarded by the `MethodExpression` extension — a bound `c.add` invoked repeatedly, mutations accumulating through the bound receiver, values vs Go.)

An **INTERFACE-receiver** method value in assignment context is exempt from that lambda: an interface method is a genuine C# instance method, so a plain method **group** over the evaluated receiver expression both compiles and matches Go's bind-once semantics exactly — `f = conf.Sizes.Alignof` (go/types sizes.go, `conf` the `ref` receiver) emits `f = conf.Sizes.Alignof;`, evaluating `conf.Sizes` once at delegate creation. The synthesized lambda there was doubly wrong: it re-evaluated the receiver per call *and* captured `conf` — capturing a `ref` receiver is CS1628. This mirrors the value-context rule below, which already leaves interface receivers on the plain emission; whole-stdlib footprint of the change: sizes.cs ×6, database/sql convert.cs, debug/buildinfo, net/http h2_bundle — every hunk a lambda collapsing to its method group. (Guarded by the `IfaceFieldMethodValueBind` behavioral test — a method value on an interface field of a pointer receiver with the field REBOUND after the value is taken, proving bind-once, output-compared vs Go.)

A **POINTER-receiver method value in a value context** — passed as a call argument rather than assigned: `s.nonDefaultOnce.Do(s.register)`, `registerMetric(…, s.nonDefault.Load)` (internal/godebug) — cannot use the bare selector: the `[GoRecv]` emission is an extension method whose first parameter is a **value type**, and C# cannot create a delegate from that (CS1113/CS1061). Go binds the receiver **address** once at method-value creation (`s.register` ≡ `(&s).register`), so the converter emits exactly that binding as a **box-bound method group** over the `RecvGenerator`'s ж-overload (class-typed, delegate-legal): `Ꮡs.register` for the receiver itself, `Ꮡs.of(Setting.ᏑnonDefault).Load` for a receiver value-field chain (the `&x.field` machinery renders the real field box). Unlike the assignment-context lambda above, this form matches Go's bind-once semantics exactly. A method whose body contains such a method value on its own receiver (or a value-field chain of it) is promoted to **direct-ж** by the capture-mode pre-pass (`bodyHasPointerMethodValueOnReceiver`) so the receiver box `Ꮡrecv` exists in scope. (Guarded by the `ReceiverFieldMethodCall` extension — method values on the receiver, on a receiver value field, and on a boxed local's field, passed as func values and invoked with mutations landing on the real storage, values vs Go.)

The **VALUE-receiver** analog captures rather than binds. When a value-receiver method value roots at the enclosing method's receiver — `kdf.hash.New` (crypto/internal/hpke's `hkdfKDF`, whose `hash` field is a `crypto.Hash` and `New` a value-receiver method; also crypto/tls's `c.hash.New`) — the emitted method is an extension over a **value** receiver, which likewise has no C# delegate (CS1113), so the converter synthesizes a wrapping lambda carrying the method's own parameters: `() => kdf.hash.New()`. But that lambda **captures the receiver**, and a non-direct-ж pointer-receiver method renders `this ref hkdfKDF kdf` whose `ref var kdf = ref Ꮡkdf.Value` alias cannot be captured by a C# closure (**CS1628** — "cannot use ref/in/out parameter inside a lambda"). So a method whose body contains such a method value is promoted to **direct-ж** by the capture-mode pre-pass (`bodyCapturesReceiverInValueMethodValue`, the value-receiver sibling of `bodyHasPointerMethodValueOnReceiver`), giving it a receiver box `Ꮡkdf` that the synthesized lambda references as a capturable reference: `() => Ꮡkdf.Value.hash.New()`. Two supporting pieces make the receiver render through its box inside the *synthesized* lambda (which has no `*ast.FuncLit` node): the capture-analysis walk now marks the **field-chain root** receiver box-ref, not only a bare-ident receiver (`kdf.hash.New` roots at `kdf`, not a bare ident); and the value-receiver synthesis renders the receiver expression in a lambda-conversion context (`conversionInLambda`) so `convIdent` emits the `Ꮡkdf.Value` box form. Same documented divergence as the other method-value forms — the receiver expression re-evaluates *inside* the lambda (per call), where Go's value-receiver method value binds a copy of it once at creation; acceptable for the compile milestone (the closure sees the same receiver instance, matching the pointer-receiver semantics of the enclosing method). This also cleared the identical latent CS1628 in crypto/tls's `key_schedule.cs` (`expandLabel`/`extract`/`finishedHash` each pass `c.hash.New` to `hkdf`/`hmac`, and the direct-ж fixpoint promoted their callers `deriveSecret`/`trafficKey`/… with call sites adapting to `Ꮡc.expandLabel` / `Ꮡsuite.trafficKey`). (Guarded by the `ReceiverCapturedInClosure` extension — a pointer-receiver method capturing its receiver through a value-receiver method value on a value field-chain (`w.id.render`) and on the bare receiver (`w.tag`), alongside the pre-existing func-literal capture, all invoked and output-compared vs Go; whole-stdlib reconvert diff: exactly hpke + the two crypto/tls files changed, nothing else.)

The **go-statement sibling** of the receiver-capture family: a `go` statement calling a **value-returning** method through the enclosing method's pointer receiver — `go q.conn.HandshakeContext(ctx)` inside `func (q *QUICConn) Start` (crypto/tls quic.go, CS1628) — is FORCED into the synthesized-lambda emission because `goǃ` has only void `Action` overloads (the x/net/nettest CS0407 form): `goǃ(ᴛ1 => q.conn.HandshakeContext(ᴛ1), ctx)`. That lambda references the receiver exactly like the method-value cases above, but neither closure predicate sees it — there is no `*ast.FuncLit` and no method-VALUE expression, only a go-call whose lowering *will* synthesize one. The capture-mode pre-pass therefore also promotes on `bodyHasGoStmtLambdaCapturingReceiver`, which mirrors `visitGoStmt`'s lambda-form decision (a nullary call synthesizes a lambda only for a value-returning or named-func-type callee; a call with arguments does so when the callee returns a value or the arity mismatches — variadic never matches) and fires when the CALLEE expression references the receiver (arguments render outside the lambda, as `goǃ` call arguments). With the method direct-ж, the go-stmt capture analysis' existing box-ref marking of the receiver (`varIsDerefdPointerParam`) takes effect and the lambda renders the chain through the box: `goǃ(ᴛ1 => Ꮡq.Value.conn.HandshakeContext(ᴛ1), ctx)`. The method-group emissions are excluded and unchanged — a void matching-arity callee (os/exec's `go c.watchCtx(resultc)`) binds the receiver chain at delegate-creation time, outside any lambda; a `defer` sibling needs no equivalent because any function-level defer already promotes via `bodyWrappedInDeferContext`. **Known divergence (Phase-4 item):** the synthesized lambda reads the receiver chain (`Ꮡq.Value.conn`) at **goroutine-run time**, whereas Go evaluates the method-value receiver at **go-statement time** — `go q.conn.M(x); q.conn = other` deterministically calls the OLD conn in Go but races toward the NEW one here (arguments are statement-time in both). The same lazy-chain window already exists for every synthesized go-lambda over a non-receiver chain (the CS0407 discard form); the faithful fix for the whole class is hoisting the receiver-chain prefix into a statement-time temp before the lambda, which would also avoid the direct-ж signature flip — direct-ж was chosen here for machinery reuse under the compile-first milestone. (Guarded by `GoStmtReceiverLambda` — `go e.tally.bump(delta)` (argument arm) and `go e.tally.report()` (nullary arm), both value-returning through a pointer field of the receiver, with the goroutine's writes read back through the original receiver chain, output-compared vs Go.)

A conversion to a **named func type** — `metricReader(read)` where `type metricReader func() uint64` — targets a C# **delegate declaration** (`internal delegate uint64 metricReader();`). Distinct delegate types have no cast conversion (a `(metricReader)read` from `Func<ulong>` is CS0030); C# converts via **delegate creation**: `new metricReader(read)`, which accepts a compatible delegate or method group. The general conversion branch special-cases a named target whose underlying is a `*types.Signature`. Composed with the bound-value lambda this renders the full runtime `metrics.go` registration: `d.compute = (ж<statAggregate> p1, ж<metricValue> p2) => new metricReader(read).compute(p1, p2)`. (Guarded by the `MethodExpression` extension — a named-func-type conversion with a bound method invoked through a func field, values vs Go.)

A **GENERIC defined function type** — Go 1.23 `iter`'s `type Seq2[K, V any] func(yield func(K, V) bool)` — emits a **generic delegate**: `public delegate void Seq2<K, V>(Func<K, V, bool> yield);`. Two converter details make this work: the type parameters live on the NAMED type, not the `*ast.FuncType`'s signature, so the delegate declaration derives its generic definition from the TypeSpec's defined type (as the struct/array paths do — deriving from the signature emitted a non-generic `Seq2` whose `K`/`V` were undefined, CS0246/CS0308); and a **conversion to a generic instantiation** (`Seq2Like[string, int](fn)`) peels `IndexListExpr` (multi-parameter — the single-parameter `IndexExpr` already peeled) and resolves the *instantiated* target from the Fun expression's type (the TypeName resolves to the uninstantiated generic, against which convertibility fails), then routes through the same delegate-creation form: `new Seq2Like<@string, nint>((@string k, nint v) => …)`. The instantiated-target override is gated to uninstantiated-generic named targets so pointer conversions (`(*uint64)(p)`, whose Fun type is the full `*T` with the `*` re-applied separately) are untouched. (Guarded by the `GenericTypeInstantiation` extension — a generic defined func type declared, instantiated with two type arguments, and invoked both through a generic function and directly, values vs Go; clears the `iter` package's five wave-1 errors.)

In Go a function is a value; a value-receiver method can be assigned to a variable, and the variable captures **its own copy** of the receiver value at the moment of assignment. This surprises many non-Go programmers:

```go
package main

import "fmt"

type data struct {
    name string
}

func (d data) printName() {
    fmt.Println("Name =", d.name)
}

func main() {
    d := data{name: "James"}
    f1 := d.printName
    f1()
    d.name = "Gretchen"
    f1()
}
```
This prints `Name = James` twice ([run it](https://play.golang.org/p/d-A5re1dfs8)) — `f1` bound a copy of `d`, so the later mutation is not observed. To preserve this semantic, the converter copies the receiver value into the delegate's capture rather than capturing the variable by reference, so the delegate executes against the snapshot taken at assignment time.

### A method value reassigned via `=` hoists its receiver capture
The receiver-snapshot decl above is emitted as a full statement (`var dʗ1 = d;`) before the lambda. In a `:=` **declaration** this hoists naturally, but a plain `=` **reassignment** to a pre-declared variable — database/sql's `checker = nvc.CheckNamedValue` — already wrote the LHS and `=` operator by the time the snapshot is generated, so writing it inline split the assignment into three token-broken pieces (CS1002). The converter routes the snapshot to the statement hoist buffer so it precedes the whole statement:

```go
var checker func(*driver.NamedValue) error
checker = nvc.CheckNamedValue   // reassign a method value
```
```csharp
var nvcʗ1 = nvc;
checker = nvcʗ1.CheckNamedValue;
```

This matches the `:=`-define path (which already hoists) and also covers a reassignment inside a tagless `switch` case. (Guarded by the `MethodValueReassignCapture` behavioral test.) `CheckNamedValue` here is an *interface* method, so the assignment binds a plain method group over the hoisted snapshot (see the interface-receiver rule above); a concrete-receiver method value keeps the param-carrying lambda form, referencing the snapshot the same way.

### A bare function value in `:=` takes its named delegate type, not `var`
Go's short-declaration from a bare function value whose type is a **named** func type — text/template/parse's `state := lexText`, where `lexText` is `func(*lexer) stateFn` and `type stateFn func(*lexer) stateFn` (the classic self-referential state machine) — infers the local as the *unnamed* signature. The converter cannot emit `var state = lexText;` (a C# method group has no `var`-inferable delegate type — CS8917), and typing the local structurally as `Func<ж<lexer>, stateFn>` makes it a **distinct** C# delegate from the `stateFn` the method group produces and that each `state = state(l)` reassignment yields (CS0029). It declares the local with the matching package named delegate instead:

```go
state := lexText
for state != nil {
    state = state(l)
}
```
```csharp
stateFn state = lexText;
while (state != default!) {
    state = state(l);
}
```

A `:=` from a method group whose signature matches no package named func type keeps the existing path. (Guarded by the `NamedFuncTypeStateMachine` behavioral test.)

## Defer / Panic / Recover

### Named-delegate and builtin callees keep the lambda form
A zero-argument deferred/goroutine'd call whose callee is a **named func type** (`defer cancel()`
with `cancel context.CancelFunc`, net dial) cannot take the bare trimmed method-group form —
the named type is a DISTINCT C# delegate with no conversion to the `Action` golib expects
(CS1503) — so the invocation stays wrapped: `defer(() => cancelʗ1())` / `goǃ(() => f())`.
A **builtin** deferred WITH arguments (`defer close(returned)`) is generic with `in` parameters,
so its method group neither infers nor converts to `Action<T>`; the temp-param lambda keeps
deferǃ's eager-argument evaluation: `deferǃ(ᴛ1 => builtin.close(ᴛ1), returned, defer)`.
(Guarded by `DeferCallOrder`'s stopFn + close(drained) shapes, output-compared vs Go.)

### A value-returning goroutine callee is wrapped in a discarding lambda
Go's `go f(…)` discards `f`'s result. Every `goǃ` runtime overload takes a **void** `Action<…>`
delegate, so a value-returning callee passed as a bare method group binds no overload (CS0407 "no
overload matches the delegate" — x/net/nettest `conntest.go`'s `go chunkedCopy(c2, c2)`, where
`chunkedCopy(io.Writer, io.Reader) error` returns `error`). `visitGoStmt` resolves the callee
signature and, when it returns a value, keeps the invocation inside a lambda so the result is
discarded — an expression-bodied lambda over a value-returning call converts to `Action` (the same
form the variadic path, e.g. `go fmt.Println(…)`, already emits):

```csharp
go chunkedCopy(c2, c2)          -> goǃ((ᴛ1, ᴛ2) => chunkedCopy(ᴛ1, ᴛ2), c2, c2);   // param callee
go q.conn.HandshakeContext(ctx) -> goǃ(ᴛ1 => q.conn.HandshakeContext(ᴛ1), ctx);      // selector method
go c.Close()                    -> goǃ(() => c.Close());                             // nullary callee
```

This parallels the **defer** case, with one asymmetry: `deferǃ` additionally carries
`Func<…, TResult>` overloads, so its *param* arm binds a value-returning method group directly and
only its *nullary* arm needs the `() => call()` discard; **every `goǃ` overload is `Action`-only**, so
the goroutine arm needs the discarding wrap for *both* its nullary and param cases (the nullary arm's
`() => call()`, and, for the param case, forcing the temp-param lambda `(ᴛ1, …) => callee(ᴛ1, …)`).
Func-literal callees and `void`-returning method groups are untouched (`goǃ(() => { … })`,
`goǃ(emit, out)`). (Guarded by the `GoStmtValueReturn` behavioral test — value-returning nullary,
single-, multi-param and multi-result goroutine callees, output-compared vs Go.)

A **VALUE-receiver method callee** forces the same lambda forms even when void and
arity-matching: every Go named type emits a C# struct and the method an extension on it, and C#
forbids constructing a delegate from an extension method over a **value-type receiver** (CS1113 —
net/http/httputil's `go spc.copyToBackend(errc)`, `switchProtocolCopier`). So
`goǃ(spcʗ1.copyToBackend, errc)` becomes `goǃ(ᴛ1 => spcʗ1.copyToBackend(ᴛ1), errc)` and a nullary
`go vs.ping()` keeps its invocation (`goǃ(() => vsʗ1.ping())`). The receiver snapshot (`spcʗ1`)
still evaluates at go-statement time. An INTERFACE-receiver method group is excluded (a genuine C#
instance method binds delegates fine), and pointer receivers keep the box-group machinery
(`pointerReceiverBoxMethodGroup` — `ж<T>` is a class, so its group is delegate-legal). (Guarded by
`GoStmtReceiverLambda`'s `valueSender` arms — value-receiver argument and nullary go-statements
with blocking-receive completion proof, output-compared vs Go.)

### A func-literal ARGUMENT of a deferred call hoists its captures before the call
When a deferred call's **callee** is itself a func literal (`defer func() { … }()`), that literal's
lambda-capture snapshots (`var sʗ1 = s;`) are threaded to a builder emitted *before* the `deferǃ(…)`
call. But when the deferred callee is an ordinary call whose **argument** is a capturing func literal —
x/net/nettest `conntest.go`'s `defer once.Do(func() { stop() })` — the argument literal's snapshot
declarations were dumped inline into the deferred call's argument list, an invalid statement
mid-expression (`deferǃ(Ꮡonce.Do,` `var stopʗ1 = stop;` `() => …)` → CS1001/CS1002/CS1003/CS1026).
The hoist sink (`lambdaContext.deferredDecls`) is now provided **unconditionally** in `visitDeferStmt`,
not only for the func-literal-callee case, so `convFuncLit` (reached via convCallExpr → convExprList →
the argument's `LambdaContext`) routes any argument literal's captures to it, and they are emitted before
the call:

```csharp
var stopʗ1 = stop;
deferǃ(Ꮡonce.Do, () => {
    stopʗ1();
}, defer);
```

The empty builder is inert for a deferred call with no capturing func-literal argument (zero golden
churn — the behavioral corpus is byte-identical), and a deferred call whose own arguments are plain
captures keeps its existing pre-call `generateCaptureDeclarations()` emission. (Guarded by the
`FuncLitArgCapture` extension case 14 — a func literal passed as the argument of a deferred `run(…)`
call capturing a local pointer, whose deferred write lands through the shared pointer box, output-compared
vs Go.)

### Defer/go EAGER arguments follow the enclosing closure's capture renames
Go evaluates a deferred (or spawned) call's function value and arguments **at statement time, in the
enclosing scope**. The defer/go emission enters its own lambda-conversion state (its callee snapshots
need a fresh remap set), but that fresh state previously hid the ENCLOSING lambda's capture renames
while the eager arguments rendered: inside an IIFE that snapshot-captured a heap-boxed outer local
(`var baseʗ1 = @base;`), the argument of `defer func(t Tally) { … }(base)` emitted the raw ref-local
`@base` — uncapturable in the IIFE's C# lambda (CS8175) — with the snapshot left declared but unused;
where the raw name IS capturable (a reference-typed local — net/http `transport.go`'s
`defer close(didReadResponse)` inside a `go func() { … }` lambda), it silently bypassed the snapshot
every other read in that body uses. `visitDeferStmt`/`visitGoStmt` now enter through a **seeded**
variant (`enterDeferGoLambdaConversion`) that copies the enclosing lambda's renames into the fresh
state, so the arguments render exactly like any other expression in the enclosing body:

```csharp
var baseʗ1 = @base;
((Action)(() => func((defer, recover) => {
    deferǃ((Tally t) => {
        report("deferred:"u8, t, 4);
    }, baseʗ1, defer);
})))();
```

`prepareStmtCaptures` still OVERRIDES the statement's own captured-callee entries afterward (their
defer-time snapshots), and a function-level defer/go — no enclosing lambda — is untouched (the seed
set is empty). Whole-stdlib footprint: exactly one file, net/http `transport.cs`, where the deferred
`close` argument becomes the goroutine lambda's `didReadResponseʗ1` (semantically neutral there — both
names alias one channel object; the fix matters for ref-local-boxed value locals). (Guarded by the
`DeferArgEnclosingCapture` behavioral test — a heap-boxed struct local passed eagerly to a deferred
func literal, a deferred NAMED callee, and a go-statement literal, each inside an IIFE, with the
mutations landing on the deferred copies and the source read back untouched, output-compared vs Go.)

### A func-literal ARGUMENT inside an `if`/`for` condition hoists its captures before the statement
The same capture-snapshot hazard occurs when a capturing func literal is passed as a call argument
**inside a condition**. `go/types` is dense with this shape — `underIs(t, func(u Type) bool { … })`,
`typeSet().is(func(t *term) bool { … })` — and the literal's snapshot declarations (`var suʗ1 = su;`)
are statements, invalid inside the condition expression. `visitExprStmt` / `visitAssignStmt` already
route such decls to a pre-statement hoist buffer (`v.hoistedDecls`), but `visitIfStmt` and
`visitForStmt` converted the condition with `convExpr(cond, nil)` and no hoist target, so the decls
were dumped inline into the condition (`if (tpar.underIs(` `var suʗ1 = su;` `(ΔType u) => { … }))` →
CS1003/CS1026/CS1002/CS1022/CS1513, ~63 errors across `go/types` alone). Both statement emitters now
convert the condition into a hoist buffer and write any collected decls on their own lines **before**
the `if`/`for`, mirroring `visitExprStmt`:

```csharp
ΔType su = default!;
var suʗ1 = su;
if (tpar.underIs((ΔType u) => {
    …
    if (suʗ1 != default!) { u = match(suʗ1, u); … }
})) { … }
```

The condition is converted **after** an `if`/`for` init clause (preserving capture-counter ordering),
and the `if`-with-init sub-block hoists between the init and the `if`. The traditional `for` reuses the
existing `ForVarInitMarker` slot — the hoisted condition decls are emitted at the same pre-`for` position
as the for-init heap allocations. The hoist buffer is empty for a condition with no capturing func-literal
argument, so the behavioral corpus is byte-identical; the only stdlib deltas are five `go/types` files
(`under.cs`, `builtins.cs`, `expr.cs`, `index.cs`, `instantiate.cs`) and one `crypto/tls`
`slices.ContainsFunc` call. This clears the **syntax-error layer** in those files (`go/types` had ~63
`CS100x`/`CS1026` from this one construct); it does not by itself green `go/types`, which compiles far
enough afterward to surface a deeper layer of latent semantic defects (a `map[token.Token]func()`
mis-lowered to a malformed explicit-interface `IDictionary`/`ICollection` implementation, named-slice
wrappers not satisfying `IArray.Source`, `token` resolution) — the frontier moves from syntax to
semantics, "progress, not regression." (Guarded by `FuncLitCaptureInCondition` — a func literal
capturing an enclosing map, passed as an argument inside a plain `if` condition, an `if` condition with
an init clause, a traditional `for` condition, and a while-style `for` condition, all output-compared vs
Go.)

### A func-literal ARGUMENT inside a return expression hoists its captures before the `return`

The third statement position with the same hazard: a capturing func literal passed as a call argument
**inside a return expression** — net/http's `findHandler` returns
`HandlerFunc(func(w ResponseWriter, r *Request) { … allowedMethods … }), "", nil, nil`, and traceviewer's
`MainHandler` returns `http.HandlerFunc(func(){ … views … })`. A **direct** func-literal result threads
`lambdaContext.deferredDecls` (the go/defer/return channel in `convFuncLit`), but a literal nested as a
call **argument** falls back to the pre-statement hoist sink, which `visitReturnStmt` never provided —
the snapshot declaration was dumped inline inside the return expression (10 syntax errors in `server.cs`,
a 4-error cascade in traceviewer). `visitReturnStmt` now provides the same hoist buffer as
`visitExprStmt`/`visitIfStmt`/`visitForStmt` and splices it before the `return` through its existing
`DeferredDeclsMarker` slot (ahead of any deferred tuple-deconstruction temps):

```csharp
var allowedʗ1 = allowed;
return (wrap((@string msg) => {
    fmt.Println(allowedʗ1[0] + ":" + msg, len(allowedʗ1));
}), "label", default!);
```

The buffer is empty for a return with no capturing-literal argument, so the behavioral corpus is
byte-identical. (Guarded by `ReturnTupleFuncLitArg` — a slice-capturing literal as a call argument inside
a three-result return tuple, and a map-capturing one inside a single-result return, output-compared vs
Go.)

Handling Go `defer` / `panic` / `recover` requires that the converted function run inside a [Go function execution context](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/GoFunc.cs). The context provides the [`defer`](https://golang.org/ref/spec#Defer_statements) call stack and the [`recover`](https://golang.org/pkg/builtin/#recover) handling; `panic` is the global [`panic`](https://golang.org/pkg/builtin/#panic) built-in (a `using static go.builtin`). The body is emitted as a lambda taking two parameters, `defer` and `recover`:

```go
func f() {
    defer func() {
        if r := recover(); r != nil {
            fmt.Println("Recovered in f", r)
        }
    }()
    fmt.Println("Calling g.")
    g(0)
}

func g(i int) {
    if i > 3 {
        panic(fmt.Sprintf("%v", i))
    }
    defer fmt.Println("Defer in g", i)
    g(i + 1)
}
```

converts to:

```csharp
internal static void f() => func((defer, recover) => {
    defer(() => {
        {
            var r = recover();
            if (r != nil) {
                fmt.Println("Recovered in f", r);
            }
        }
    });
    fmt.Println("Calling g.");
    g(0);
});

internal static void g(nint i) => func((defer, recover) => {
    if (i > 3) {
        throw panic(fmt.Sprintf("%v"u8, i));
    }
    defer(() => fmt.Println("Defer in g", i));
    g(i + 1);
});
```

A function that neither directly nor indirectly (through a deferred lambda) uses `defer`/`panic`/`recover` skips the wrapper entirely — the converter scopes the wrapper per function, so e.g. a `main` that just calls `f()` is emitted as a plain method body. A value-returning function returns from inside the wrapper (`=> func((defer, recover) => { …; return x; })`). Two refinements worth noting:

* **Named results + defer.** When a function has named return values *and* uses defer/recover, the named results are declared *outside* the wrapper (closure-captured), the wrapper runs as a `void` action, and the function returns the named results afterward — so the deferred "recover sets the result" idiom is observed.
* **IIFEs.** An immediately-invoked function literal that itself uses defer/recover gets its own wrapper, rendered as a delegate-cast invocation (e.g. `((Func<int>)(() => func((defer, recover) => { … })))()`), so its `recover` scopes to the IIFE and not the enclosing function.
* **A `return` emits against ITS OWN function's results, not the enclosing function's.** A bare `return` in a function with named results emits `return (n, ok);` (the named results). A *nested function literal* must be converted against its **own** signature — otherwise a bare `return` inside a **void** closure would inherit the enclosing function's named results and emit `return (n, ok);` into a `void` lambda (CS8030, "anonymous function converted to a void-returning delegate cannot return a value"). Runtime `mprof.goroutineProfileWithLabelsSync` (named `(n, ok)`) passes `forEachGRace(func(gp1 *g) { …; return; … })` — the void closure's bare returns must stay `return;`. The return signature is tracked separately from `currentFuncSignature` (which stays the *enclosing* function's, so the receiver/parameter detection still resolves a **captured** pointer parameter — an outer parameter — correctly): `convFuncLit` sets a dedicated return-signature to the literal's own signature with save/restore, and `visitReturnStmt` emits results against it. (Guarded by the `ClosureBareReturnNamedResults` behavioral test — a void closure with bare returns nested in a named-results function, output verified vs Go; cleared runtime's 4 CS8030.)
* **All-typeless returns need the explicit wrapper type argument.** C# infers the value-returning wrapper's `T` (`func<T>((defer, recover) => …)`) from the lambda's return statements, and a tuple literal has a natural type only when *all* its elements do — Go `nil` renders as a typeless `default!`. When **every** return in the body contains a nil (`return nil, err` / `return &x, nil` — syscall's `getProcessEntry`, unnamed `(*ProcessEntry32, error)` results), no return contributes a type, inference fails, and overload resolution silently binds the *void* `GoAction` wrapper (CS8030 at each value return). The converter detects that shape and emits the result type explicitly: `=> func<(ж<ProcessEntry32>, error)>((defer, recover) => …)`. Any function with one fully-typed return keeps the inferred form (zero churn). (Guarded by `DeferTypelessReturns`' `find` — unnamed results, a defer, and both returns carrying nil.)

* **Heterogeneous typed returns also need the explicit wrapper type argument.** The same `func<T>` inference fails when a value-returning defer/recover function `return`s expressions of two *unrelated concrete types* that share only the declared interface — go/parser's `parseTypeName` returns `&ast.SelectorExpr{…}` beside a plain `*ast.Ident`, both only `ast.Expr`. Every return is fully typed (so the all-typeless test above does not fire), but C#'s best-common-type of `{ast_SelectorExprжExpr, ast_IdentжExpr}` has no single member the others convert to, so `T` cannot be inferred and overload resolution again binds the void `GoAction` wrapper (CS8030 — 13× in go/parser: parseTypeName, tryIdentOrType, parseSimpleStmt, parseGoStmt, …). `execWrapperReturnsLackCommonType` walks the top-level returns and, at each result position, tests whether *some* return type is identical-to-or-assignable-to by every other; when none is (genuine heterogeneity), the explicit result type is emitted — `=> func<ast.Expr>((defer, recover) => …)`. A single return, or returns that DO share a best-common-type (a concrete beside its own interface — C# infers the interface), keep the inferred form, so the full-stdlib A/B touches only the genuinely-heterogeneous funcs (go/parser, plus one `context.WithDeadlineCause`) and the behavioral corpus stays byte-identical. (Guarded by `DeferInterfaceReturn` — a defer/recover func returning `Shape` via `Circle` vs `Square`, plus a `(Shape, bool)` heterogeneous tuple return, values vs Go.)

### Function-literal named results

A func **literal** with named results declares them at the top of its emitted block, zero-initialized — Go's semantics for `next = func() (v1 V, ok1 bool) { …; return }` (the `iter.Pull` shape): a bare `return` yields the named results as currently assigned, so the lambda emits `() => { V v1 = default!; bool ok1 = default!; …; return (v1, ok1); }`. Without the declarations the emitted tuple referenced undeclared names (CS0103 — the `iter` package's last wave-1 errors). Two interactions: a named-results literal whose *first* statement is a bare `return` must NOT collapse to an expression-bodied lambda (the names exist only as block declarations), and the `namedReturnDefer` path (named results that deferred code mutates) keeps its own arrangement — declarations *outside* the `func((defer, recover) => …)` wrapper, returned after it. Declarations reuse the shadow-aware naming, so a literal result shadowing an outer local renames consistently in both the declaration and the return (`nΔ1`). (Guarded by the `FuncLitArgCapture` extension — bare returns with assigned and zero named results, plus the first-statement-bare-return shape, values vs Go.)

Because a named result lives in the literal's OWN scope, a reference to it in the body is the result, never an outer-scope capture — so named results are excluded from the lambda-capture set (`convFuncLit`) exactly as parameters are. text/template's `readFileFS` returns `func(file string) (name string, b []byte, err error)`, whose closure captures the enclosing `fsys` AND writes `b` via the captured tuple call `b, err = fs.ReadFile(fsys, file)`. Because the closure genuinely captures `fsys`, the capture analysis ran and mis-flagged `b` too — hoisting `var bʗ1 = b;` into the enclosing function, where `b` does not exist (CS0103), and renaming the body's `b` to the captured `bʗ1`. Filtering the named-result names out of the capture set (alongside the parameter names) leaves `b` a plain in-block declaration. (Guarded by `CrossPkgUser`'s `makeScanner` — a captured closure returning named results, one written via a tuple call whose RHS uses the capture, output-compared vs Go; crypto/x509 and html/template shared the same latent shape.)

### Deferred calls whose callee returns a value take the lambda form
The no-arg defer arm passes a bare method group (`defer(k.Close)`) only when the callee returns VOID -- an error-returning method (`defer k.Close()`, registry `Key.Close`) is a `Func<error>` method group that cannot bind the golib `defer(Action)` (CS1503). The lambda form discards the result, exactly Go's deferred-call semantics:
```csharp
defer(() => hʗ1.close());
```
Guarded by `DeferTypelessReturns`.

### Deferred pointer-receiver nullary calls bind the box method group
`defer conf.releaseSema()` with `conf *resolverConfig` (net nss.go / dnsclient_unix.go) trimmed to the deref-alias method group `Ꮡconf.Value.releaseSema` — a struct VALUE against the [GoRecv] `ref` extension, which cannot create a delegate (CS1113). The emission binds the BOX method group instead:
```csharp
defer(Ꮡconf.releaseSema);
```
The `ж<T>` overload is class-typed and delegate-legal, and the method-group conversion captures the receiver when the delegate is created — exactly Go's binding time. Mirrored in the go-statement arm. Gated to methods declared DIRECTLY on the pointee — a PROMOTED method (net interface.go's `defer zc.Unlock()`, declared on the embedded `sync.RWMutex`) has no extension on the outer box (CS1061) and keeps the lambda emission — and to void results (a `Func<>` group binds neither `defer(Action)` nor `go(Action)`). (Guarded by `DeferCallOrder`'s `acquireAndWork`, output-compared.)

The same box-method-group emission also covers a **value receiver** whose type is exactly the pointer-receiver's pointee — `defer b.deck.reset()` (runtime/pprof; also database/sql, log/slog), where `deck pcDeck` is a value FIELD reached through a nested selector and `reset` has a `*pcDeck` receiver, so Go auto-takes `&b.deck`. The original arm required the receiver be an already-pointer *ident*; the value case renders `&receiver` through the shared address machinery (the same `&ast.UnaryExpr{AND}` → `convUnaryExpr` synthesis used elsewhere) — a boxed base gives the aliasing field-ref `Ꮡb.of(profileBuilder.Ꮡdeck)`, an escaping value local gives its box `Ꮡx`, a plain value gives the `Ꮡ(value)` copy — then binds the method: `defer(Ꮡb.of(profileBuilder.Ꮡdeck).reset)`, the ж<pcDeck> overload captured at defer time and mutating the real field. Gated the same way (void result, a NAMED value type whose RecvGenerator box overload exists, matching the pointee exactly so a promoted/embedded method is excluded). (Guarded by the `DeferValueFieldPtrReceiver` behavioral test — a pointer receiver deferring `b.c.reset()` on a value field, and a pointer local deferring the same in a closure, with the reset observed through the same box after return, output-compared vs Go.)

### A deferred pointer-receiver method on an escaping value local captures by-box, not by-copy
The emission above binds the box (`Ꮡstate.free`) for a `defer state.free()` on a value local — but the CAPTURE analysis must cooperate. `defer`/`go`/closure bodies are lambda-conversion scopes: a variable used inside them that escapes to the heap is normally snapshot-copied into a `var stateʗ1 = state;` declaration so the C# closure captures a value, not an uncapturable ref-local. For an escaping value local used **only** as the receiver of a pointer-receiver method call (`state` a `handleState` value, `free` a `*handleState` method — log/slog handler.go's `defer state.free()`), that snapshot is doubly wrong: the address-taking is *implicit* (Go auto-takes `&state`), so the emission still binds the box — but of the *snapshot name* `Ꮡstateʗ1`, which is a plain value with no `Ꮡ` companion:
```csharp
ref var state = ref heap<handleState>(out var Ꮡstate);
state = h.ch.newHandleState(buf, true, " "u8);
var stateʗ1 = state;         // snapshot copy — WRONG
defer(Ꮡstateʗ1.free);        // Ꮡstateʗ1 never declared → CS0103
```
The capture analysis now recognizes this implicit address-of (a value receiver of a pointer-receiver method, matching the pointee exactly and NAMED — the same guard the emission uses) as a reason to treat the local as a **box-ref var**, exactly like an explicit `&state`: it skips the snapshot, and the emission binds the original heap box:
```csharp
ref var state = ref heap<handleState>(out var Ꮡstate);
state = h.ch.newHandleState(buf, true, " "u8);
defer(Ꮡstate.free);          // binds the live variable's box
```
This is not merely a compile fix — a value snapshot is taken at defer time, so it would miss any mutation the body makes to `state` before the deferred call runs; binding `Ꮡstate` matches Go's semantics of deferring against the *live* variable. Gated to an escaping local (a non-escaping one has no `Ꮡ` box and keeps the compiling `Ꮡ(copy)` form) used as a value receiver whose type is exactly the method's pointer-receiver pointee (an already-pointer receiver's box group is the pointer variable itself, whose snapshot name IS declared, so it is excluded). The same generalization silently corrects the closure form (`func(){ x.mutate() }` on an escaping value local previously mutated a lost copy — go/types conversions.go/typeset.go) and removes now-dead `var xʗ1 = x;` snapshots wherever the box was already used. (Guarded by the `DeferHeapLocalPtrMethod` behavioral test — a value local deferring a pointer-receiver method, mutated after the defer, with the deferred method observing the final value, output-compared vs Go.)

The same box-ref treatment covers a **promoted** pointer-receiver method reached through **value embeds** — `lazyCert.Do(…)` on `var lazyCert struct { sync.Once; v *Certificate }` (crypto/x509 `AppendCertsFromPEM`): Go takes `&lazyCert.Once`, an address into the variable's own storage, so the closure must share the original variable. The detection resolves the call through `info.Selections` and walks the selection's embedded-field index path — only **value** embeds along the path root the address at the variable (a **pointer** embed re-roots it at that pointer's target, where the snapshot, which copies the pointer, stays sound). Emission then renders the promoted call through the box's field projection and field uses through the box read; the snapshot form had referenced a never-declared snapshot box (`ᏑlazyCertʗ1`, CS0103) *and* divorced the closure's writes from the original:
```csharp
ᏑlazyCert.of(AppendCertsFromPEM_lazyCert.ᏑOnce).Do(() => {
    (ᏑlazyCert.Value.v, _) = ParseCertificate(certBytesʗ2);
    …
});
return (ᏑlazyCert.Value.v, default!);
```
A variable marked box-ref is also never snapshot-copied by a **nested** literal — the box is a plain reference local that closures at any nesting depth capture directly, so the per-layer `var lazyCertʗ2 = lazyCertʗ1;` chains disappear with it. (Guarded by the `ClosureEmbeddedPromotedPtrMethod` behavioral test — an anonymous-struct local with a value embed whose pointer-receiver method is called from sibling and nested closures interleaved with field writes, cumulative counts observed vs Go.)

The same box-ref treatment covers a pointer-receiver method on a **value-struct FIELD projection** of the escaping local — `defer p.fake.setLines()` (go/internal/gcimporter iimport.go/ureader.go), where `p` is an escaping `iimporter` value local and `setLines` a `*fakeFileSet` method on the value field `p.fake`: Go takes `&p.fake`, an address INTO `p`'s own storage, and the emission renders it through the box's field view — but the snapshot path renamed the base first, referencing a never-declared snapshot box:
```csharp
var pʗ1 = p;                                  // snapshot copy — WRONG
defer(Ꮡpʗ1.of(iimporter.Ꮡfake).setLines);     // Ꮡpʗ1 never declared → CS0103
```
The capture analysis now matches the single field-projection receiver (a FIELD selected on the var's own value-struct storage — the same `&m.field` form `lambdaBoxRefAddressForm` emits — whose type is exactly the method's pointer-receiver pointee and NAMED) as the same implicit address-of, marks the local box-ref, and the defer binds the live box's field view:
```csharp
defer(Ꮡp.of(iimporter.Ꮡfake).setLines);
```
As with the direct case this is a write-visibility fix, not merely a compile fix: gcimporter registers the defer *before* importing (which populates `p.fake.files`), so a snapshot would flush an empty file set. The same generalization corrects deferred **closures** that read such a variable — go/parser's `defer func(){ …; err = p.errors.Err() }()` snapshot-copied `p` at defer time, so the closure read the parser state from *before* parsing (errors always empty); box-ref renders those reads `Ꮡp.Value.errors…` against the live variable. A deeper chain (`p.a.b.m()`), a pointer field hop, or a method promoted through the field's own embeds keeps the existing snapshot handling. (Guarded by the `DeferHeapFieldPtrMethod` behavioral test — a heap-boxed value local deferring a pointer-receiver method on its value field, with lines appended after the defer observed by the deferred flush, output-compared vs Go.)

## Expression Switch Statements
Go expression-based `switch` statements are flexible: cases do not fall through automatically (no `break` needed), and the `fallthrough` keyword runs the next case body bypassing its expression. Based on the [Manual Tour of Go Conversions](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions), converting to `if / else if / else` is the best choice for most cases. When every case label is a C# **compile-time constant** and there is no `fallthrough`, a traditional C# `switch` works. "Constant" here means a C# `const` — a literal, a computed literal expression (`a + b`), or a *typed* basic-type const — not merely a Go constant. A case label that references a plain variable, a struct field (`case frame.fp`), an *untyped* / named-type / cross-package const emitted as `static readonly` (`case goarch.PtrSize`), or an address-of expression (`case &g`) is **not** a C# constant, so a C# `switch` case label there is invalid (CS9135 / CS0150). Such switches fall back to the `if / else if` form comparing the tag with `==` (a temp captures the tag: `var exprᴛ1 = tag; if (exprᴛ1 == frame.fp) …`). The same constant-vs-runtime-value test also chooses `is` (constant pattern) vs `==` for a single-value case within the if-else form. A Go `break` inside a case exits the *switch* (skipping the rest of the case); in the `if / else if` form there is no enclosing C# switch for it to target (CS0139), so a case body that contains such a break is wrapped in a `do { … } while (false)` — the break exits that one-shot loop, i.e. the case. The wrap is emitted only for a case whose body actually has a switch-targeting `break` (one not caught by a nested loop/switch/select), so every other case is unchanged. (A `break` inside a *nested* loop within the case still targets that loop, as in Go.) For cases that use `fallthrough`, the cases are expanded to standalone `if` statements with a local fall-through flag and `goto` to handle break-style exits — the most complex (and least pretty) scenario. In that if-chain form a **trailing `default:` reached via fallthrough** is emitted as a *guarded* `if (fallthrough || !match) { … }` — the guard is needed so the default does not run after a matched-but-non-fallthrough case, but C# cannot prove it always executes. So when such a guarded-default switch is the last statement of a **value-returning** function and every case is terminal, C# reports CS0161 ("not all code paths return a value") even though the Go `default` makes the switch exhaustive (runtime `startpanic_m`). Because a guarded-terminal-default switch cannot be legally followed by reachable Go code (it always returns/exits), the converter emits an unreachable `return default!;` after the if-chain to satisfy C#'s definite-return analysis — gated on the enclosing function/literal actually returning a value (via its own return signature), so a `void` function or a switch that isn't terminal is unaffected. (Guarded by the `SwitchFallthroughDefaultReturn` behavioral test; cleared runtime's CS0161.) A comparison case may use a C# relational/constant pattern (`case {} when x is < 0`) only when the compared-to operand is a C# compile-time constant; for a variable (`case x == y`) or a `static readonly` const (untyped/cross-package), it falls back to a `when` guard (`case {} when x == y`) — a relational pattern there is invalid (CS9135).

### A switch on a `static readonly` constant tag lowers to if-else
A switch TAG that is itself a constant emitted as `static readonly` -- an untyped const's `UntypedInt` wrapper (`switch goarch.PtrSize`, reflect abi.go) or a `uintptr`-struct const -- cannot govern a C# switch: the int case labels are not constants OF the wrapper struct type, and the `is` constant-pattern lowering fails the same way (CS9135). The recorded tag type is no help (go/types records the untyped constant's DEFAULT type in tag position), so the gate is on the object resolution: a constant-valued tag that is not a true C# `const` forces the if-else form (wrapper `==` operators) and disables the `is` pattern:
```csharp
var exprᴛ1 = CrossPkgLib.Precision;
if (exprᴛ1 == 1) {
```
A variable tag stays switchable. Guarded by `CrossPkgUser` / `CrossPkgLib`.

### A leading constant-true case stays opaque to the compiler
Go's `switch { case true: ... case cond: ... }` (time parseStrictRFC3339 deliberately disabling its strict checks) compiles the LATER cases as dead code; a foldable `when true` makes C# reject them outright (CS8120). A constant-true case condition on a NON-LAST clause therefore emits the golib `ᐧᐧ` marker -- a `static readonly bool` the compiler cannot fold:
```csharp
case {} when ᐧᐧ: {
```
The marker is deliberately SEPARATE from the const `ᐧ` switch governor: that const's foldability is itself load-bearing (`case ᐧ when ...` label patterns need a constant, and an infinite `for (...; ᐧ ;...)` relies on the fold for reachability proofs -- CS9135/CS0161 when it was made readonly in place). Guarded by `ExprSwitch`.

### No constant pattern against a named-numeric wrapper
A constant expression whose CONTEXTUAL type is a wrapper struct -- golib `uintptr` or any `[GoType("num:...")]` named numeric (time's `Duration`) -- can never be a C# constant, so no constant/relational pattern can compare against it: `d is >= 0` types the literal 0 as Duration (CS9135). The lowering keeps the plain operator form (`d >= 0`, the wrapper's operators). Guarded by `ExprSwitch` (the `pace` switch).

### An index-expression case label falls back to equality
A case label that INDEXES a package-level array/slice variable (`case Typ[UntypedNil]:` — go/types
operand.go, where `Typ` is the universe `*Basic` array) is a runtime value, never a C# constant. The
single-value `is` form is doubly broken there: C# parses `exprᴛ1 is Typ[UntypedNil]` in pattern position
as an array TYPE (CS0246 + CS0270). `canUsePatternMatch` rejects an `*ast.IndexExpr` label the same way
it rejects a non-constant identifier/selector, so the clause takes the `==`/`AreEqual` comparison the
multi-value arm already produced:
```csharp
if (AreEqual(exprᴛ1, Typ[UntypedNil])) {   // NOT `exprᴛ1 is Typ[UntypedNil]`
```
(Guarded by the `IndexExprCaseLabel` behavioral test — single- and multi-label clauses indexing a
package-level array var, output-compared vs Go.)

### Literal case labels under a named-type tag compare through a cast

A tagged switch whose tag type is a NAMED (non-interface) type — net/http's
`func (code socksReply) String()` switching on `code` — renders the tag as a `[GoType]` wrapper
struct. An untyped-LITERAL label adopts the tag's named type in Go (go/types records it on the label
expression), but its C# render is a bare literal of the UNDERLYING type, which can neither be a
constant pattern (`exprᴛ1 is 0x01` — CS9135, constant pattern against the wrapper) nor compare bare
(`exprᴛ1 == 0x01` is ambiguous between the wrapper's `==` and the underlying's built-in `==`, both
reachable through the wrapper's two-way implicit operators — the same ambiguity family as the
named-string consts). Two converter pieces:

1. the pattern-match decision excludes any named-wrapper tag (`tagIsNamedWrapper`, beside the
   existing `namedTypes`/`tagIsStaticReadonlyConst` gates — those could not catch the mixed
   const-ident + literal switch, because the per-label screening short-circuits once `allConst`
   goes false and never reaches its named-type check);
2. a CONSTANT label that is not an ident/selector/conversion-call (those already render AT the
   wrapper type) casts to the tag type:

```csharp
var exprᴛ1 = code;
if (exprᴛ1 == socksStatusSucceeded) {     // named-const label — no cast
    return "succeeded"u8;
}
if (exprᴛ1 == (socksReply)(0x01)) {       // literal label — cast to the tag type
    return "general SOCKS server failure"u8;
}
```

This also repairs the ALL-literal switch over a named type, which previously emitted the ambiguous
bare `==` form. Full-stdlib footprint: 12 files (socks_bundle, archive/zip, encoding/xml,
go/printer, go/types, internal/poll, syscall zsyscall shims). (Guarded by `NamedNumericSwitchLiteral`
— a mixed named-const + literal switch including a multi-label clause, and an all-literal switch,
output-compared vs Go.)

## Type Switch Statements
For a Go type-switch, C#'s type-pattern `switch` works well. The runtime exposes the dynamic type via `.type()`, and the empty interface is `any`:

```go
func do(i interface{}) {
    switch v := i.(type) {
    case int:
        fmt.Printf("Twice %v is %v\n", v, v*2)
    case string:
        fmt.Printf("%q is %v bytes long\n", v, len(v))
    default:
        fmt.Printf("I don't know about type %T!\n", v)
    }
}
```

converts to:

```csharp
internal static void @do(any i) {
    switch (i.type()) {
    case nint v: {
        fmt.Printf("Twice %v is %v\n"u8, v, v * 2);
        break;
    }
    case @string v: {
        fmt.Printf("%q is %v bytes long\n"u8, v, len(v));
        break;
    }
    default: {
        var v = i.type();
        fmt.Printf("I don't know about type %T!\n"u8, v);
        break;
    }}
}
```

**Go `int`/`uint` cases and the synthetic concrete case.** A Go `int` maps to C# `nint`, but an `int`-valued *literal* boxed into an interface (`do(1)`) has C# dynamic type `int32`, not `nint`. So a `case int:` emits its native form **plus** a synthetic concrete `case int32:` (and `case uint:` adds `case uint32:`) sharing the same body, to catch both boxings. The exception: if the *same* switch also lists an explicit `case int32:`/`case uint32:` (or `case rune:` ≡ int32), the synthetic is **skipped** — emitting it would duplicate the explicit case (CS8120 "unreachable case") and, being emitted first, would steal the explicit case's values and run the wrong body. With the synthetic suppressed, a typed `int` value (`nint`) hits `case int:` and a typed `int32` value hits `case int32:`, distinctly. (Runtime's `printpanicval` switches over `int, int8, …, int32, …, uint, …, uint32, …`; guarded by the `TypeSwitch` behavioral test.)

**Duplicate-mapped cases — the identical-body merge.** Go type distinctions can *vanish* in the C# type map, making a later case unreachable (CS8120). The canonical example was `uint` + `uintptr` under the old `System.UIntPtr` alias — **now moot: `uintptr` is a distinct golib struct** (see [Constant Values](#constant-values)) and both cases emit their own labels, each dynamic type routing to its own body exactly as in Go. The merge machinery remains for any alias pair that still shares a C# type: a duplicate-mapped case merges **only when its Go body is byte-identical** to the first occurrence's — the earlier label already routes both dynamic types to that shared body, so the merge is exact. A marker comment replaces the duplicate label:

```csharp
case uint64 vΔ1: {
    print(vΔ1);
    break;
}
/* case uintptr vΔ1: merged with an earlier case mapping to the same C# type (identical body) */
case float32 vΔ1: {
```

If the bodies **differ**, both labels are kept and the CS8120 stands: a compile error is preferable to silently routing one Go case's values into another case's body. Duplicate detection keys on the resolved C# type (`uintptr`→`nuint`, `rune`→`int32`, `byte`→`uint8`) per switch statement; the synthetic `int32`/`uint32` cases register too, so an explicit later duplicate of a synthetic with the same body also merges. Guarded by the `TypeSwitch` behavioral test (`uint` + `uintptr` with identical bodies, values hitting both Go paths).

**Multi-type cases stack labels and bind at the tag's interface type.** Go binds a type-switch case
variable at the listed CONCRETE type only when the clause lists exactly **one** type; a multi-type clause
(`case *Alias, *Named:`) binds it at the TAG's static (interface) type. The old emission split such a
clause into one concrete-bound C# case per listed type (duplicating the body), so every body use in an
interface-typed context broke — as an argument (`isGeneric(t)` with `t` a `ж<Alias>`, CS1503), an
interface assignment (CS0266), and an extension-method receiver (CS1929 — 18 errors in go/types alone).
A multi-type clause now emits **stacked labels binding only a discard, over one shared body** and
re-binds the variable to the guard expression — the same re-bind the `default` arm uses — so the body
compiles at the interface type exactly as in Go:

```csharp
switch (x.typ.type()) {
case ж<Alias> _:
case ж<Named> _: {
    var t = x.typ;          // t: Type (the tag's interface type), as in Go
    if (isGeneric(t)) { … }
    break;
}
case ж<ΔSignature> t: {     // single-type case keeps the concrete binding
```

The `_` designation is load-bearing, not stylistic: it forces the label into PATTERN context. A bare
`case int8:` label resolves the identifier as an EXPRESSION first, where `using static go.builtin` finds
the same-named conversion FUNCTION (`int8(…)`) — a method group, neither constant nor type — failing
CS8917 (encoding/binary's `Size`/`intDataSize` stacks; caught by the census build, not the behavioral
corpus, whose labels happened not to collide). `case nil` stacks as `case null:`, a dynamic-interface
label stacks in its non-binding `{} ᴛn when ᴛn._<Iface>(out var _)` form, and the synthetic
`int32`/`uint32` companions of `case int:`/`case uint:` stack with the same discard. The
duplicate-mapped-case merge applies per label (a merged label leaves its marker comment above the stack).
An UNBOUND multi-type clause (`switch x.(type)`) stacks the same way with no re-bind — the body is no
longer duplicated per label. The re-bind re-evaluates the guard EXPRESSION at body entry — harmless
for a pure tag (nothing can mutate it between dispatch and entry), and an IMPURE tag is hoisted into
a one-time temporary first (see *The type-switch tag evaluates exactly once* below). (Guarded by the `TypeSwitchMultiCase` behavioral test — bound multi-type
cases over values and pointers with interface-dispatched body uses, `nil` stacked with a concrete type, an
unbound multi-type clause, and the synthetic-int stacking, output-compared vs Go; also rewrote
`TypeSwitch`'s `case int, int64, uint64:` golden with output proven unchanged.)
**Runtime dispatch — `.type()` unwraps the interface adapters.** The case patterns match against
whatever object the switch operand `.type()` returns, so it must surface the Go DYNAMIC value, not
the C#-only wrapper classes the runtime uses to carry it. A non-empty interface value created from
a Go pointer is a generated `IжAdapter` wrapping the receiver box (`var v shape = &c` emits
`new circleжshape(Ꮡc)`; see [Interfaces](#interfaces)), and an interface-to-interface
assignment can wrap the source in an `IInterfaceAdapter`. `.type()` therefore unwraps —
`IInterfaceAdapter.Value` chains first, then `IжAdapter.Box` — mirroring the type-assert machinery
in `_<T>`, so a Go `case *circle:` (emitted `case ж<circle> t:`) matches the adapter-wrapped value
exactly as it matches the raw box that an EMPTY interface (`any`) holds directly. The bound `t`
IS the original receiver box, so writes through it (`t.Value.r += 10`) alias the original object,
matching Go's interface-holds-the-pointer semantics; `case nil` (emitted `case null:`) still sees
the nil interface unchanged. A known edge remains: an interface holding a **nil `*T`** (in Go a
non-nil interface that matches `case *T:` binding a nil `t`) stays wrapped — no C# type pattern
can bind a null — so it falls to `default` rather than wrongly matching `case null:`. (Guarded by
the `TypeSwitchPointerAdapter` behavioral test — pointer-receiver implementations of a non-empty
interface dispatched through single-type, multi-type, no-bind, and write-through cases, plus
value-receiver, `nil`, and raw-box-in-`any` controls.)

**Named-interface case labels dispatch by method set through the adapter registry.** An
INTERFACE-typed case label — named (`case fmt.Stringer:`, `case error:`) or anonymous — must match
by Go METHOD-SET semantics, and after the unwrap above the operand is the raw receiver box, which
never nominally implements a C# interface (the generated pointer adapter does). A plain C# type
pattern (`case Stringer t:`) therefore missed every pointer-sourced implementation. All interface
labels now emit the `when`-guard form the anonymous labels already used —
`case {} Δx when Δx._<Stringer>(out var x):` — routing dispatch through golib's type-assert
machinery, which resolves in order: a **nominal** implementer (value structs made partial, adapter
instances, duck-type wrappers) matches directly; a raw **box** `ж<X>` (or an adapter asserting to a
*different* interface, via its `Box`) re-wraps through `go.AdapterRegistry` — each generated
pointer adapter registers `(typeof(ж<X>), typeof(Iface)) → box => new XжIface(box)` from a
`[ModuleInitializer]`, so the lookup is a dictionary hit and a compiled factory, reflection-free
and Native-AOT-safe (the initializer also roots the adapter against trimming); an *anonymous*
interface still falls back to its generated `ᴛAs` duck-typing conversion. The `out var x` binds at
the CASE interface type exactly as Go binds the case variable, the re-wrapped adapter forwards to
the original box (writes through the binding alias the original object), label order is preserved
(C# tests patterns top-to-bottom, and a `when`-guarded pattern never makes a later label
unreachable), and `case nil` is unaffected (`{}` never matches null). The type-assert core is
non-throwing (`TryTypeAssert`), so a non-matching label — the NORMAL control flow in a type
switch — costs no exception; this also makes a nil-interface `v, ok := x.(T)` return `ok=false`
(Go semantics) instead of faulting, and a named interface with no registered adapter is a MISS
rather than the former missing-`ᴛAs` hard error. Known residuals, all of the same shape (the
adapter type does not exist or its module never loaded, so Go would match where C# misses): a
(struct, iface) pair with **no conversion site anywhere** in the program, a **generic** struct's
adapter (an open registration key is unrepresentable and a generic class cannot host a module
initializer), and FOREIGN **value** adapters (`ᴠ`-composed), which are not yet registered.
(Guarded by the `TypeSwitchNamedInterfaceCase` behavioral test — pointer-adapter value, raw
box-in-`any`, value-struct implementer, `case error:` in both adapter-carried and raw-box forms,
non-matching control, label-order precedence both directions, a multi-type clause of two interface
labels, interface-tag-to-interface-label dispatch, and write-through aliasing, output-compared
vs Go.)

## Struct Types
Go structs are converted to C# `struct` types and used on the stack to optimize memory use and reduce GC pressure; when an instance must escape the stack it is wrapped in a heap box, [`ж<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/%D0%B6.cs) (see [Pointers](#pointers)). Rather than spell out the whole struct body, the converter emits a partial struct carrying a `[GoType]` attribute, and the `TypeGenerator` source generator synthesizes the members (equality, `ISupportMake`, embedding promotion, etc.):

```csharp
[GoType] partial struct Person {
    public @string Name;
    public nint Age;
}
```

The generator also chooses the access modifier from the Go name (exported → `public`, unexported → `internal`), except where the converter emits an explicit modifier — for instance, an unexported type used as the type of an *exported* field is published as `public` to satisfy C# accessibility (the converter emits `public partial struct …` and the generator honors that explicit modifier).

The synthesized value-equality body compares the struct's fields against a parameter named `other` (`public bool Equals(Person other) => this.Name == other.Name && this.Age == other.Age;`). Each comparison's **left operand is qualified with `this.`** so a Go field whose name happens to collide with the parameter — a field literally named `other` — still binds field-to-field. Without the qualification a `type holder struct { mark int; other int }` would emit `other == other.other`, where the left `other` resolves to the *parameter* (a `holder`) rather than the field (an `int`), failing to compile with CS0019 (`==` cannot be applied to `holder` and `int`). `GetHashCode`/`ToString` reference the same field names but have no colliding parameter, so they need no qualification. (Guarded by the `StructFieldNamedOther` behavioral test.)

A combined Go field declaration — `x, y int` — emits a single combined C# line (`internal nint x, y;`) so the output mirrors the Go source's line grouping. The combined form is only used when every name in the group shares the same emitted type and access modifier and none needs per-name special handling; otherwise the converter falls back to one line per name. The fallback applies when any of these hold: a blank field `_` (renamed per occurrence — `_`, `__`, …), a name equal to the enclosing struct type (renamed with the `Δ` collision marker), a per-field array initializer (` = new(N)`), or a mix of exported and unexported names in the same group (`X, y int` → `public nint X;` / `internal nint y;`). Field comments and tags attach to the whole Go field, so they never diverge within a group.

C# does not allow inline or intra-function type definitions, so these are "lifted" out of the function. A **named** local type is lifted with its enclosing function's name as a prefix to avoid collisions — a `type x struct{…}` declared in `main` becomes `main_x`. An **anonymous** struct (or an anonymous struct used as a field/value) is lifted to a synthesized name with a `ᴛ`*N* suffix and marked dynamic, e.g. `[GoType("dyn")] partial struct settingsᴛ1`. Struct "definitions" that match structurally remain usable interchangeably (the generator and implicit conversions handle this). A reference to a lifted type as a bare identifier is renamed to the lifted name, and so is its use as a **slice or array element type** — `[]entry` (where `entry` is a local type) emits `slice<process_entry>`, not the short `slice<entry>` (which is unresolved at package scope → CS0246). The element is resolved through the same lift registry as the bare-identifier and anonymous-struct cases. (Guarded by the `LocalTypeSliceElement` behavioral test, covering both the slice and fixed-array forms; runtime hit this on `printDebugLog`'s `[]readState` and `traceAdvance`'s `[]untracedG`.)

A **map whose VALUE type is an anonymous struct** is lifted the same way. A package-level `var m = map[K]struct{…}{…}` — crypto/internal/hpke's `SupportedKEMs` (`map[uint16]struct{ curve ecdh.Curve; hash crypto.Hash; nSecret uint16 }`) and `SupportedAEADs` — names its value struct through the lift so the map type reads `map<uint16, SupportedKEMsᴛ1>`; without it, `getTypeName`'s map arm stringified the value as raw Go `struct{…}` syntax straight into the C# map signature (`map<uint16, struct{ curve ecdh.Curve; … }>`) — which C# cannot parse (a CS1519/CS1003 syntax cascade). `extractStructType` already lifts a slice/array **element** struct (its `ArrayType` arm) but has no map arm, so a dedicated `extractMapValueStructType` lifts the map **VALUE** struct at the package-level value-spec composite-literal site. The keyed element literals stay the target-typed `new(…)` constructor form — `[0x0020] = new(ecdh.X25519(), crypto.SHA256, 32)` — which binds to the lifted struct's generated constructor; a func-typed value field (SupportedAEADs' `aead func([]byte) (cipher.AEAD, error)`) lifts to a `Func<…>` field that a method-group or func-value element still fills. Both the declaration type (`getCSTypeName` → the map arm) and the literal's own type render (`convMapType` → `getExprTypeName`) resolve the value through the shared `liftedTypeMap` (the lift runs before the initializer is converted). (Guarded by the `MapAnonStructValue` behavioral test — a package-level map with an anonymous-struct value type, including a func-typed field, constructed and read back by key, output-compared vs Go.)

The **empty struct `struct{}` is never lifted** — it maps to the shared golib `EmptyStruct`, so a `struct{}{}` composite literal emits `new EmptyStruct()` and a `map[K]struct{}` ("set") emits `map<K, EmptyStruct>`. Lifting an empty struct would be doubly wrong: it has no fields to model, and the lift mis-attributes its name and identity. When the `struct{}{}` is the value assigned to a map element (`seen[k] = struct{}{}`), the enclosing assignment passes the **LHS ident** (`seen`) into the struct-conversion context to name the lift — so the empty struct was being lifted to `<func>_seen` *and registered under `seen`'s own type, the map* `map[K]struct{}`, in the lifted-type registry. That poisoned every later reference to that map type: the function parameter `seen map[K]struct{}` rendered as the phantom struct instead of `map<K, EmptyStruct>`, and its comma-ok deconstruction (`(_, ok) = seen[k]`) and two-arg indexer vanished (CS8130/CS0021), while real-map call sites mismatched (CS1503). `convStructType` now short-circuits an empty struct to `EmptyStruct` before any lift, mirroring the `!isEmptyStruct` guard that `extractStructType` already applies everywhere else. (Guarded by the `EmptyStructMapSet` behavioral test; runtime hit this on `typesEqual`'s `seen map[_typePair]struct{}` parameter.)

An **empty `interface{}` field is never lifted** either — it maps to `any`, exactly as a bare `interface{}` type does. When `visitStructType` lifts an anonymous struct it walks its fields and lifts any *anonymous interface* field to its own named `[GoType("dyn")]` interface. Those three inline lift sites (a plain `interface{}` field, a `*interface{}` field, and a `[]interface{}`/`[N]interface{}` element) type-asserted `*ast.InterfaceType` directly, diverging from `extractInterfaceType` — the canonical lift gate, which already excludes empty interfaces. So encoding/json's slice-encoder cycle memo — `ptr := struct{ ptr interface{}; len int }{v.UnsafePointer(), v.Len()}` — lifted its `ptr interface{}` field to a named empty **marker** interface `encode_ptr_ptr`. A named empty interface is implemented by nothing, so constructing the struct from the boxed `uintptr` failed (`cannot convert from 'uintptr' to '…encode_ptr_ptr'`, CS1503). The three sites now carry the same `!isEmptyInterface` guard, so an empty-interface field falls through to the normal field-type conversion and renders `any` (`*interface{}` → `ж<any>`, `[]interface{}` → `slice<any>`). (Guarded by the `AnonymousStructs` extension `cycleMemo` — an in-function anonymous struct with an `interface{}` field constructed from a pointer, read back through the field, and used as a map key, output-compared vs Go; fails CS1503 without the guard. Part of greening encoding/json.)

**A returned anonymous-struct composite literal records its implicit conversion AFTER it is lifted.** Two structurally-identical anonymous structs are the *same* Go type but the converter lifts each occurrence to a *distinct* C# name, so a conversion between them must be bridged by a recorded `[assembly: GoImplicitConv<…>]` (the `ImplicitConvGenerator` emits the operators). A closure whose **result type** is an anonymous struct that `return`s a **composite literal** of the identical anonymous struct — `mk := func(…) struct{ptr any; len int} { return struct{ptr any; len int}{p, n} }` — lifts the closure-result type (`…_func_R0`) and the composite-literal type (`…_type`) separately, and `visitReturnStmt`'s `checkForDynamicStructs` records the conversion between them. Each side's C# name is resolved through the per-file lifted-type registry (`liftedTypeMap`), but a function-local composite literal is only *added* to that registry **during its own `convExpr`**. The recording therefore had to move to run **after** the result expression is converted: reading the arg's type earlier found it unlifted and stringified it as raw Go `struct{…}` text — an invalid C# generic argument in the emitted attribute (`[assembly: GoImplicitConv<struct{ptr interface{}; len int}, …_func_R0>]`, CS1031 "Type expected"). Recording after the lift resolves both sides to their lifted names (`[assembly: GoImplicitConv<…_type, …_func_R0>]`). The `dynamicCast` template `checkForDynamicStructs` may return is applied to the already-converted result expression identically either way, so the reorder is otherwise output-neutral (the full-stdlib A/B reconvert is byte-identical). This is latent for the current stdlib (encoding/json builds its identical memo struct once into a variable, avoiding a second same-shape lift). (Guarded by the `ClosureReturnAnonStruct` behavioral test.)

### Lifted anonymous structs embedding an interface
archive/tar's ReadFrom-hiding shape — `io.Copy(struct{ io.Writer }{tw}, r)` — exercises four
coupled rules: a SELECTOR embed's interface check resolves the **Sel** (`Writer`), not the
package ident (`io`), so the cross-package interface embed emits as a plain interface FIELD
(the promoted-struct property form made the generator construct the interface — CS0144); the
composite literal routes the element through the interface conversion **at render**
(`interfaceTypes[i]`, not just the record-only call); a receiver placed into an
INTERFACE-typed composite field triggers **direct-ж** (Go's interface holds the `*T`, so the
pointer adapter wraps the box `Ꮡfr`); and the generator emits ONE value-form impl per
(struct, interface) pair, folding a Promoted duplicate in (CS0111). ARGUMENT-position values
of a mismatched delegate type wrap in the named delegate's constructor exactly like
composite-literal fields (generic delegate params stay native — unsubstituted type params
cannot render). Guarded by `AnonymousInterfaces` (`tally`/`fill`, `byteRepeat`, and the
named-array `quad`/`frame` Range slice).

The same interface-field record+route also fires for a **named non-struct** element that
implements the field's interface. The gate above triggered only when the element's underlying is a
struct (or the field is embedded), so a named scalar with a method set — hpack's
`DecodingError{InvalidIndexError(idx)}`, where `type InvalidIndexError int` has an `Error()` method
satisfying the `error` field — recorded no `GoImplement<InvalidIndexError, error>` and passed the
value bare to the interface-typed constructor parameter (which surfaces as `NilType`, CS1503). The
gate now also fires when a named, non-struct, **non-interface** element type `types.Implements` the
field's interface — mirroring the call-argument path, which routes any argument into an interface
parameter with no struct-only restriction. Recording the `GoImplement` is what clears the error (the
generated implementation makes the scalar implement the interface, so the bare pass then converts
implicitly); the render routing (`interfaceTypes[eltIndex]`) is set too, matching the struct case. An
interface-typed element is excluded — it is already the interface and needs no adapter. (Guarded by
the `InterfaceFieldNamedScalar` behavioral test — a named `int` into an `error` field and a named
`string` into a local interface field, positional and keyed forms, output-compared vs Go.)

**Naming a lift that has no name source — `new(struct{ SomeIface })`.** Every dyn-lift derives its
C# type name from context (the declared var, the struct field, the parameter name). A package-level
`var reserved = new(struct{ types.Type })` (go/internal/gccgoimporter's singleton) had NO source:
the initializer is a CallExpr (the composite-literal up-front lift didn't fire), so the declaration
type fell to the raw `t.String()` mangle (`ж<types.Type}>`), and the lift arrived late from the
call-argument path under builtin `new`'s UNNAMED parameter — an EMPTY lift name, declaring
`partial struct  {` and registering `""` for every reference (`@new<>()`,
`[assembly: GoImplement<, …>]`, `new жΔType(…)` — a whole-package syntax cascade). Two-part fix:
`visitValueSpec` lifts a `new(struct{…})` initializer's struct UP FRONT under the var's name
(mirroring the composite-literal and hpke map-value lifts), so the declaration, the `@new<…>` type
argument, the `GoImplement` recordings, and the pointer-adapter names all resolve through
`liftedTypeMap`:
```csharp
[GoType("dyn")] partial struct reservedᴛ1 {
    public global::go.go.types_package.ΔType Type;
}
internal static ж<reservedᴛ1> reserved = @new<reservedᴛ1>();
p.typeList[n] = new reservedᴛ1жΔType(reserved);
```
and `visitStructType` itself falls back to the generic `"type"` when a lift arrives with an empty
name (the FUNCTION-LOCAL `x := new(struct{…})` form still reaches it through the unnamed-parameter
path → `main_type`), so no caller can produce an unnamed type declaration. (Guarded by the
`NewAnonStructIfaceEmbed` behavioral test — the package-level singleton converted to its embedded
interface through the lifted type's pointer adapter, the embedded field filled and called through
the promotion, plus the function-local form — output-compared vs Go.)

### Astral rune literals
A quoted rune literal beyond the BMP (`'\U0001D504'`) cannot be a C# char literal — it emits
the code point (`(rune)0x1D504`); BMP literals keep their source text verbatim (html's entity
table, CS1012 ×133). Guarded by `StringConvPostfix` (`glyphs`).

### Type-switch default arm binds the interface value
The default clause binds the guard to the ORIGINAL guarded expression (`var x = err;`), whose
static type is the interface — the switch-operand form (`err.type()`) is object and cannot
flow back out (`default: return x`, go/build/constraint's pushNot, CS0266).

### The type-switch tag evaluates exactly once
Go evaluates the TypeSwitchGuard's operand exactly once, but the default-arm and multi-type
re-binds above textually re-emit the tag expression, so a tag containing a **call or channel
receive** evaluated once at dispatch and again at each matched re-bind arm —
`switch p := recover().(type)` re-called `recover()` (which returns nil the second time,
silently losing the recovered value in a `case nil, *bailout:`-style arm that reads `p`;
go/types handleBailout), and a `switch v := (<-ch).(type)` re-received. Such a tag is now
HOISTED into a one-time temporary, and both the dispatch operand and every re-bind read it:

```csharp
var switchᴛ1 = next(x);
switch (switchᴛ1.type()) {
case @string _:
case bool _: {
    var v = switchᴛ1;      // re-bind reads the temp — next() ran exactly once
    …
default: {
    var v = switchᴛ1;
```

The hoist is deliberately GATED — only a tag containing a call (conversions hoist
conservatively; the temp is merely unneeded) or a receive, and only when some arm actually
re-binds (a bound default, or a multi-type clause with a non-blank ident) — so every pure-tag
type switch keeps its direct, byte-identical emission. The temp name comes from the
per-package `getGlobalTempVarName` counter (`switchᴛN`), so nested and sibling hoists never
collide. Single-type concrete labels and the `when`-guard interface labels bind from the
dispatch operand's pattern variable and never re-evaluate the tag regardless. (Guarded by the
`TypeSwitchImpureTag` behavioral test — a counting-function tag whose per-switch eval count is
printed and output-compared vs Go [the pre-fix emission provably prints `calls: 7` for Go's
`calls: 4`], a `recover()` tag in a deferred multi-type switch, and a channel-receive tag that
would deadlock on re-receive.)

### Generated code global::-qualifies root-namespace references
Inside a package whose namespace nests a same-named segment (go/build/constraint emits into
`namespace go.go.build`), C# binds a generated reference's leading `go` RELATIVELY to `go.go`
(CS0234). The generators qualify every type-reference position via `GlobalQualify`
(Common.cs); generated signatures also carry parameter REF KINDS (`in slice<byte>`) and a
canned `System.IFormattable` impl where the interface inherits it (the hand-finished io
stub's dyn machinery).

The **converter** faces the same `go.go` shadowing in the import `using` directives it emits for a
`go/*` package (go/token lands in `namespace go.go`, imports `sync`/`unicode` sub-namespaces): a
rooted `using atomic = go.sync.atomic_package;` / `using go.sync;` binds its leading `go` to the
enclosing `go.go` namespace, resolving `go.sync` to the nonexistent `go.go.sync` (CS0234). `rootQualifyIfAmbiguous` routes its rooting returns through `rootQualified`, which emits `global::go.`
instead of a bare `go.` when the package's namespace second segment is itself `go`:

```csharp
using atomic = global::go.sync.atomic_package;
using global::go.sync;
```

The shadowing is NOT limited to `go/*` packages themselves: any package with a `go/*` package
anywhere in its transitive import CLOSURE compiles with `namespace go.go` in scope (its referenced
assembly makes `go.go` a member of namespace `go`), and C#'s inner-to-outer lookup then binds the
bare leading `go` of a rooted using target to that member from EVERY namespace nested under the
root — internal/fuzz (imports go/ast) emitted `using bits = go.math.bits_package;` inside
`namespace go.@internal`, resolving to the nonexistent `go.go.math` (CS0234 ×16, plus the same
shape in net/rpc's `Δhttp` alias and testing/internal/testdeps). `rootQualified` therefore also
emits `global::go.` when `packageChildNamespaces` carries the `go.go` key (populated from the
transitive import closure by `computeImportAliasRenames`' pre-pass). A package with no `go/*`
anywhere in its closure — every package that was compiling before, and all pre-existing behavioral
tests — keeps the bare `go.` prefix, so there is no golden churn. Cleared go/token, go/doc/comment,
go/build/constraint (own-namespace branch); internal/fuzz's 18 CS0234 and net/rpc's latent pair
(closure branch). Guarded by the `GoNamespaceShadow` behavioral test, which covers BOTH branches
through a nested local module literally named `go/nsshadow` (emitting `namespace go.go`, the shape
a single-file behavioral test cannot express): the nested lib imports `math` + `math/rand` so its
own rooted using exercises the own-namespace branch, and the importing `main` package (namespace
`go`, with `go.go` in its closure) exercises the closure branch.

**Referencing a `go/*`-package TYPE loses a root segment because the path's own `go` collides with the root namespace.** A `go/ast` type reference renders correctly as `go.go.ast_package.X` (root `go` + the path's `go.ast` → namespace `go.go`, class `ast_package`), but `convertToCSTypeName` then strips the *leading* `go.` as a redundant root (bodies live inside `namespace go`), leaving `go.ast_package.X` — namespace `go`, which has no `ast_package` (CS0234/CS0426 in the go/* consumers go/doc, go/printer, go/internal/typeparams, whose GoImplement attributes and `using` aliases both carry the stripped form). The two rooting helpers now recognise this: `isStrippedGoPathPackageRef` splits the ref at its first `_package` class segment and tests the *namespace* portion against `packageChildNamespaces` (the current package's rooted import-closure namespaces): the ref is stripped iff that namespace is NOT already a real rooted namespace but *becomes* one when the root `go.` is prepended. This is a **membership** test, not a string-shape test, so it recognises a stripped go/*-package ref at any depth — `go.ast_package` (ns `go`✗ → `go.go`✓), `go.build.constraint_package` (ns `go.build`✗ → `go.go.build`✓, three-segment `go/build/constraint`), `go.doc.comment_package` (ns `go.doc`✗ → `go.go.doc`✓) — while leaving a genuinely-rooted ref alone (`go.io.fs_package` — ns `go.io` is already real). (The earlier two-segment string heuristic — "the class segment sits immediately after `go.`" — recognised only the depth-one `go.ast_package` shape and silently missed the three-segment `go/build/constraint` and `go/doc/comment` sub-package refs, which are string-indistinguishable from a correctly-rooted `go.io.fs_package`; the membership test is what disambiguates them.) `rootQualifySubNamespaceTypeRefs` (the assembly-scope GoImplement/GoImplicitConv attributes) re-roots the stripped form to a bare `go.go.ast_package`; `rootQualifyIfAmbiguous` (the in-namespace `using` aliases) re-roots to `global::go.go.ast_package` — always `global::`, because a bare `go.go.<pkg>_package` re-binds its leading `go` to the nearest enclosing `go` from *any* importer (a go/*-package's own `go.go.*` namespace, and equally `internal/pkgbits` at `go.internal.pkgbits` resolving the second `go` inside `go.go`, CS0234). This un-blocks the whole go/* chain at the rooting level (go/doc's own-errors 17 → 1); each go/* package still needs its remaining per-package residuals (e.g. a methodless-func-type's `[GoTypeAlias]` still names an inline-rendered `ΔFilter`) to fully compile. The depth-one shape is now guarded by `GoNamespaceShadow` (its `go/nsshadow` nested module's import renders through `isStrippedGoPathPackageRef` → `using nsshadow = global::go.go.nsshadow_package;`); the multi-segment sub-package depth (`go/build/constraint`) remains census-verified only — the A/B reconvert-diff showed only the four `go/build/constraint`- and `go/doc/comment`-importing packages, go/build, go/doc, go/parser, go/printer, gaining the corrected double-`go` rooting, with the depth-one `go.go.ast_package` refs unchanged and zero collateral.

**BCL names in generator templates are global::-qualified too — a Go type can shadow any bare BCL
name.** The generated partials sit inside the package class, where every Go type in the package is
a sibling member that wins name lookup over `System.*`: internal/trace/traceviewer declares
`type Range struct`, so the named-string wrapper's sub-slice indexer `this[Range range]` bound the
Go `Range` instead of `System.Range` (CS1503 inside its own `ViewType.g.cs`). This is a *class* of
collisions, not one bug — any package declaring a type named `Range`, `Index`, `Type`, `Span`, … is
exposed — so the audit qualified every BCL reference the TypeGenerator templates emit:
`global::System.Range` (string/slice/array indexers), `global::System.Span<T>`/`ReadOnlySpan<byte>`,
the `IEnumerator`/`IEnumerable` members, `ICloneable`, `IEquatable` and the `System.Numerics`
operator interfaces on numeric wrappers, `System.Type`/`Reflection.MethodInfo`/`Activator`/
`NotImplementedException`/`[DebuggerNonUserCode]` in the dynamic-interface machinery, and the
`GeneratedCode` attribute stamped on every generated declaration (`Common.cs`, shared by all
generators). golib names (`slice<T>`, `NilType`, `IChannel`, …) stay bare — they live in the `go`
namespace the generated code owns. Converter-emitted visible code is not part of this rule (it
renders BCL names by the file-scoped conventions above). (Guarded by `BclTypeNameShadow` —
a package declaring `type Range struct` alongside a named string type and a named slice type, both
sub-sliced with the Go `Range`'s fields as bounds, output vs Go.)

### Generic embedded fields
A GENERIC embed (`entry[K,V]` embedding `node[K,V]`, internal/concurrent) arrives in the AST as
an `IndexExpr`/`IndexListExpr` over the base type; the anonymous-field walk unwraps it (plain,
pointer, and selector forms) and the member emits under the **base name** with type arguments
stripped **before** the selector dot-strip — the arguments may contain qualified types whose
dots otherwise win the LastIndex (`*concurrent.HashTrieMap[T, weak.Pointer[T]]` misnamed its
member `Pointer` instead of `HashTrieMap`). The TypeGenerator's promoted accessors carry the
type parameters on the instance param (`ref Δentry<K, V> instance`) and strip them from the
member access (`instance.node.isEntry`). A promoted method call through a raw ж **box local**
hops `X.Value` ahead of the cross-package pointer-embed hop
(`m.Value.HashTrieMap.Value.Load(value)`, unique). BANKED: unqualified promoted METHOD calls
through a generic embed (`w.show()`) — receiver wrappers resolve the embedded type by exact
name; qualified calls work. Guarded by `GenericStructFields` (`wrapped[T]`/`tag[T]`) and
`CrossPkgUser` (`holder[T]` embedding `*CrossPkgLib.Cache[T]`).

### A methodless named func type renders as its base delegate

Go treats a named func type as freely interconvertible with its underlying `func(...)` when the
type has **no methods** — the name is purely documentary. `type releaseConn func(error)`
(database/sql) and `type CancelFunc func()` (context) are assigned to and from anonymous
`func(...)` values without conversion: `grabConn` returns `releaseConn`, `queryDC` takes
`func(error)`, and Go passes one to the other. Emitting the named type as a *distinct* C#
delegate (`ΔreleaseConn`) broke this — the base `Action<error>` its underlying renders to has no
implicit conversion to it (CS1503/CS0029), and the mismatch even excluded the `ж`-receiver
overload of methods taking such a param, so `db.pingDC(...)` on a boxed `*DB` failed with CS1929.

A **non-generic** named func type with **no methods** is therefore rendered AS its base C#
delegate (`Action`/`Func<…>`) everywhere it is referenced (`getTypeName`/`getFullTypeName` return
the underlying signature), and its declaration is skipped (`visitFuncType` emits only a marker
comment). Every named↔underlying conversion becomes identity, exactly as Go models it:
```csharp
// type releaseConn is a methodless func type — rendered inline as its base delegate
internal static (ж<driverConn>, Action<error>, error) grabConn(this ж<ΔConn> Ꮡc, context.Context _) { … }
internal static error queryDC(this ref DB db, …, Action<error> release, …) { … }
```
Three exclusions keep the collapse sound — a type is left as a named delegate if any holds:
- it **has methods** (its method set is meaningful — the `FirstClassFunctions`/`hashFunc` wrap case
  below still applies);
- it is **generic** (it is referenced as `Seq<V>`, and the type parameter must stay in scope — see
  the generic-`Seq` range-over-func case);
- its **signature references another named func type, including itself**. A *self-referential* func
  type — `type stateFn func(*machine) stateFn` (a Go state machine, `NamedFuncTypeStateMachine`) —
  has no finite base-delegate form (`Func<M, Func<M, …>>` is infinite); and a reference to another
  named func type (`strategy func(score) action`) would leave that name undefined after collapse.
  Only the *leaves* of the func-type reference graph collapse; a referencing type stays named and
  renders the collapsed leaf inside its own signature.

Because the collapse applies at both the declaration and every reference, and to foreign types too
(context's `CancelFunc` collapses in context's own conversion, so database/sql sees `Action`),
consistency holds across packages. One position needed a companion fix: a variadic `...Option`
element is package-class-qualified (`main_package.Option`) for a package-local named type, which
would mangle a collapsed delegate to `main_package.Action` (CS0426) — `variadicElementType` now
skips the qualifier when the element collapsed. Cleared 13 of database/sql's 17 errors (the whole
named-func family + the CS1929 it masked). Guarded by `MethodlessFuncType` (a function returning
the named type, one taking the anonymous underlying, a struct field, and a tuple-deconstruction
seam across the two); regression-checked against the self-referential (`NamedFuncTypeStateMachine`,
unchanged), nested-reference (`FirstClassFunctions`), and variadic-param (`PublicizedFuncTypeParam`)
cases.

**A collapsed methodless func type must NOT export a `[GoTypeAlias]`.** When such a type is *also*
collision-renamed — `type Filter func(...)` alongside a method `Filter` (go/ast's `Filter` vs
`(CommentMap).Filter`; the `ReservedTypeMethodCollision` shape) — the rename records an exported
`[assembly: GoTypeAlias("Filter", "ΔFilter")]` so consumers can name the renamed type. But because
the type collapses to its base delegate, **no `<pkg>_package.ΔFilter` type is ever emitted** — so a
consumer that loads the alias generates `global using astꓸFilter = go.go.ast_package.ΔFilter;`
naming a nonexistent type (go/doc referencing `ast.Filter`, CS0426). `visitFuncType` now records
each collapsed methodless func type's name in `packageInlineFuncTypeNames`, and the exported-type-
alias emission skips any alias whose key *or* value matches (the collision path stores the alias
under the renamed value `ΔFilter`, the plain path under the raw name) — so the alias is never
exported and the consumer renders `ast.Filter` inline as `Func<nint, bool>` through the normal
collapse. (Guarded by the `CrossPkgUser` extension — a cross-package `CrossPkgLib.Sift` methodless
func type colliding with a `Sift` method, named as a var type and rendered inline, output vs Go; and
by `ReservedTypeMethodCollision` whose `[GoTypeAlias]` is now correctly absent.)

When such a collapsed delegate's signature carries a parameter whose type lives in a **sub-package**
(an import path with a slash), the `Func<…>`/`Action<…>` rendering must qualify that type as the
package **class**, not the namespace. The collapsed signature is produced from the Go signature's
`t.String()`, which keeps the canonical import PATH inline — `func(*sync/atomic.Int32) int32`,
`func(string, io/fs.DirEntry, error) error` (path/filepath's `WalkDirFunc`) — losing the file's import
alias. `convertToCSFullTypeName` converted the whole slash-bearing string as one import path, dotting
the type straight into the namespace: `sync.atomic.Int32` / `io.fs.DirEntry` — CS0234, since `atomic`
is not a namespace of `go.sync` (the type lives in class `atomic_package`). It now splits the trailing
`.TypeName` off at the first `.` after the last path `/`, converts the package path with the class
suffix, and re-appends: `sync.atomic_package.Int32`, `io.fs_package.DirEntry`. The suffix is only added
when the path segment does not already carry it — some callers (a recorded `[GoType]` underlying,
`sync/atomic_package.Uint32`) hand a pre-suffixed path, which would otherwise double to
`atomic_package_package` (a `DefinedTypeOverPkgType` regression, caught and gated). The behavioral
corpus is byte-identical except the intended change, and an A/B reconvert of net+go/types (same package
set) is byte-identical — only the func-type-subpackage-param shape moves. (Guarded by the
`SubpackageFuncTypeParam` behavioral test — a methodless `applyFunc func(*atomic.Int32) int32` whose
collapsed delegate carries the `sync/atomic` sub-package parameter, output-compared vs Go; the same
shape drives path/filepath's `WalkDir`/`Walk` referencing `io/fs.DirEntry`/`FileInfo`.)

A collapsed func type's **parameter list must not be double-converted**. `convertToCSFullTypeName`'s
`func(` handler split the parameter string with `extractTypes`, then re-ran `convertToCSTypeName` over each
result — but `extractTypes` already renders a NAMED parameter in C# form (it strips the Go name and converts
the type). Re-feeding an already-C# `map<@string, ж<Object>>` through the `map<` arm's `splitMapKeyValue`
mis-parsed it into `map<@string, ж<Object>, >` — a spurious trailing empty type arg (CS1031 "Type expected",
go/ast's `NewPackage` taking `type Importer func(imports map[string]*Object, path string) (…)`). The fix makes
`extractTypes` **always** return C#-form (the bare-type/unnamed branch now converts in place too, matching the
named branch), and the caller trusts that output directly instead of a second pass. This is byte-identical
everywhere except named-parameter func types — bare-type func types (`func(int, string)`) were already
converted once and stay so, just at the `extractTypes` site rather than the caller. (Guarded by the
`NamedFuncTypeMapParam` behavioral test — `type Importer func(imports map[string]*Node, path string) (pkg
*Node, err error)` used as a function parameter, output-compared vs Go; CNR byte-identical across the corpus,
and an A/B reconvert of go/ast shows only that one collapsed-delegate parameter shape moving.)

**A collapsed methodless named func type's DELEGATE TYPE renders through `iifeDelegateType`, not the string
path.** The double-conversion fix above kept the collapsed delegate on `convertToCSFullTypeName`'s `func(`
string handler — but that string domain naively slash→dots a cross-package element's import PATH. go/doc
passes `simpleImporter` to `ast.NewPackage` (whose `importer` is `ast.Importer`, a methodless func type), so
the converter wraps the method group in the collapsed base delegate
`new Func<map<@string, ж<go.ast.Object>>, @string, (ж<go.ast.Object> pkg, error err)>(simpleImporter)` —
`go/ast.Object` mangled to `go.ast.Object` (no `_package` class, no file alias), so `ast` is not a namespace
of `go` (CS0234 ×2), and the resulting error-typed delegate then fails the method-group→delegate conversion
(CS0123). `getCSTypeName` now routes a methodless named func type through the SAME structural
`iifeDelegateType` path an ANONYMOUS signature already takes (that path exists precisely because the string
path mangles slash-bearing package paths), naming each element via `aliasedElementTypeName` — so the
cross-package `ast.Object` keeps its `ast` alias (and a Δ-renamed foreign element its recorded `ꓸ`-alias):
`new Func<map<@string, ж<ast.Object>>, @string, (ж<ast.Object>, error)>(simpleImporter)`. The only visible
change for a SAME-package/single-segment element is that a multi-result signature's delegate type drops its
Go result-tuple element NAMES (`(ж<Node> pkg, error err)` → `(ж<Node>, error)`), matching how anonymous
signatures already render — cosmetic, both compile. An A/B full-stdlib reconvert moves **11 files, all
equal-or-better**: the go/doc mangle fixed, plus `go/parser`/`go/scanner` (`go.token_package.ΔPosition` →
`tokenꓸPosition`), `go/internal/gccgoimporter` (a malformed `(io.ReadCloser>, error)` → valid),
`internal/trace/traceviewer` (`net.http_package.Request` → `http.Request`), and `path/filepath`
(`io.fs_package.DirEntry` → `fs.DirEntry`) all cleaned up, with `bufio`/`go/ast`/`nettest` only dropping
cosmetic tuple names; CNR touches only three existing goldens (`NamedFuncTypeMapParam`,
`SubpackageFuncTypeParam`, `FirstClassFunctions`), all the same pattern. (Guarded by the `CrossPkgUser`
extension — a package-level `simpleResolve` passed as a METHOD GROUP to `CrossPkgLib.Resolve`, whose
`Resolver` is a methodless func type naming the cross-package `*CrossPkgLib.Node`, so the wrapped delegate
renders `ж<CrossPkgLib.Node>` via the alias, output-compared vs Go. The single-segment producer compiles
either way, so the byte-golden — unnamed vs Go-named result tuple — is what guards the routing; the exact
slash-bearing CS0234/CS0123 needs a multi-segment producer like go/ast, verified by the go/doc source A/B.
go/doc's own remaining block is the SHARED generated-adapter forwarding of go/ast's unexported interface
marker methods — a separate root.)

A companion root cleared path/filepath fully: a **cross-package type ALIAS whose target lives in yet
another package** — `os.FileInfo = fs.FileInfo` (os/types.go, target in `io/fs`) — is emitted as an
assembly-scoped `global using FileInfo = go.io.fs_package.FileInfo;` in **os's own** conversion, never as
a member of the os package's C# class, so a cross-package reference `os_package.FileInfo` does not resolve
(CS0426, path/filepath's `lstat = os.Lstat` func value). `getTypeName` now renders such an alias as its
**target** — `os.FileInfo` → `fs.FileInfo` (→ `io.fs_package.FileInfo` via the file's `fs` using). Gated
to a **different-package target**: an alias to a SAME-package type (`CrossPkgLib.Temperature = Celsius`)
already resolves through the existing `ꓸ` global-using alias (`CrossPkgLibꓸTemperature`) and is left
untouched — narrowing here reverted a `CrossPkgUser` churn the blanket form caused. CNR byte-identical;
an A/B of os+io/ioutil (same package set) shows only that one intended resolution (io/ioutil's `ReadDir`
sort lambda moved `osꓸFileInfo` → `fs.FileInfo`, matching the file's other `fs.FileInfo` refs — still
compiles). **GUARD OWED** — the shape needs three packages (B declares `Y`, A aliases `type X = B.Y`, C
references `A.X`), which neither the single-package baseline nor the 2-package `CrossPkg` harness
expresses; validated by the `go-src-converted/path/filepath` build (1→0) + io/ioutil build.)

**A func type renders structurally in EVERY type-name path — the signature never stringifies.**
`getTypeName` now carries a `*types.Signature` arm (`signatureTypeName`, beside `iifeDelegateType`)
mirroring the slice/map/chan composite arms: Go syntax — `func(name type, …) results` — with every
parameter/result type resolved **recursively**, so a cross-package element keeps the file's short
import alias exactly like the neighboring map/slice fields. Previously only *some* positions routed
through the structural `iifeDelegateType` (var declarations; variadic or slash-bearing struct
fields); every other position — a struct field of a **named** methodless func type (go/importer's
`importer gccgoimporter.Importer`), a MAP field's func **value** type (net/http's `TLSNextProto`
maps), a same-package named func field (traceviewer's `f MutatorUtilFunc`) — reached
`convertToCSFullTypeName` as `t.String()` text with import PATHS inline, and the slash heuristics
mangled those one of **three ways** depending on the string's shape: the whole-string
path-conversion arm fires when no dot-after-slash precedes the first `[` (a leading `map[` bracket),
naively dotting every path — `ж<go.types.Package>`, `ж<crypto.tls.Conn>` (no `_package` class, and
under a `go.go`-nested namespace the leading segment binds the child namespace — CS0234) — while the
split-at-dot arm mangles a mid-signature path to a classed-but-unrooted form
(`@internal.trace_package.UtilFlags`, traceviewer mmu.cs). With the structural arm the string
reaching the parser is slash-free (`func(*Server, *tls.Conn, Handler)`) and each element converts
through the normal alias route. Result NAMES are preserved, so a named multi-result field keeps its
named C# tuple (the display-path advantage the old struct-field routing existed to protect); a
same-package/builtin signature renders byte-identically to the old `t.String()` path (zero churn —
CNR confirmed across all 331 behavioral projects). A variadic tail renders `...elem` (which the old
path's `..`-strip reduced to the unparseable `.elem`) and lowers through the parser to the golib
`ꓸꓸꓸ` delegate family (next paragraph). One side effect: element recursion passes through
`getTypeName`'s foreign-ALIAS arm, so a signature naming a cross-package alias (`os.FileInfo` →
`io/fs.FileInfo`) now registers the **target's** package for a file-local using — a few stdlib files
gain a benign `using fs = …;` alias line (`collectTypePackages`' Named case does not match a
`*types.Alias`, so the old path never registered it). Whole-stdlib A/B footprint: 20 files — the
go/importer field fixed, `ж<tls.Conn>` in net/http server/transport/h2_bundle (field + composite
literals), traceviewer's `Func<trace.UtilFlags, (slice<slice<trace.MutatorUtil>>, error)>`,
go/scanner's `err` field moving to the canonical `tokenꓸPosition` alias (the old
`go.token_package.ΔPosition` resolved only by go.go-namespace luck), the variadic type-assert
target below, one comment-alignment shift, and the benign using-line additions. Cleared the
IMP-2/HTTP-3 CS0234 cluster (net/http ×8 + go/importer ×6 + traceviewer). (Guarded by the
`SynthesizedDelegateChildPkg` behavioral test — a nested CHILD subpackage (slash-bearing import
path) whose `*inner.Record` rides a named methodless func-type field with a nested-tuple lookup
param AND a `map[string]func(*inner.Record, string)` field, both invoked at runtime vs Go.)

**The func-type string parser splits parameters at TOP-LEVEL commas only — and a variadic tail
lowers to the `ꓸꓸꓸ` delegate family.** `extractTypes` split the parameter list with a naive
`strings.Split(signature, ",")`, so a nested func param returning a TUPLE — `lookup func(string)
(io.ReadCloser, error)` (go/internal/gccgoimporter's `Importer`, surfacing as go/importer's
`gccgoimports.importer` field) — shredded at the tuple's interior comma, unbalancing the assembled
delegate: `Func<@string, (io.ReadCloser>, error)` (the inner `>` closes before the tuple's second
element — a 6-error syntax cascade, IMP-1). `splitTopLevelParams` tracks `<>`/`()`/`[]`/`{}` depth
(with the channel-arrow `<-` guard `splitMapKeyValue` already carries) and splits only at depth 0.
On top of that, a variadic tail (`...elem`, from the structural render above) converts its ELEMENT
type in `extractTypes` and carries an ellipsis-family marker that the `func(` assembler hoists into
the delegate FAMILY name — `Actionꓸꓸꓸ<@string, any>` — mirroring `iifeDelegateType`'s lowering
exactly. That fixed the variadic func type as a type-ASSERTION target as a rider:
`.(func(string, ...any))` (net/http transport.go's `tLogKey` logger) previously emitted the
unparseable `._<Action<@string, .any>>(ᐧ)` and now renders `._<Actionꓸꓸꓸ<@string, any>>(ᐧ)`.
(Guarded by the `FuncFieldNestedTupleParam` behavioral test — builtin-typed struct fields with
nested-func-returning-tuple params in both the anonymous and named-collapse forms plus a
named-tuple-result sibling, all invoked at runtime vs Go.)

A **type ASSERTION** whose target is a methodless func type must assert against the **collapsed
delegate**, not the (never-emitted) name. `ci.(Compressor)` where
`type Compressor func(io.Writer) (io.WriteCloser, error)` (archive/zip's compressor/decompressor
registries) rendered `ci._<Compressor>()` — `convTypeAssertExpr` converts the target via `convExpr`,
which emits the bare ident, and after collapse `Compressor` is undefined (CS0246). When the asserted
target is a methodless named func type, the assertion now renders its `getCSTypeName` (the collapsed
`Func<…>`): `ci._<Func<io.Writer, (io.WriteCloser, error)>>()` — matching how the stored value was
emitted (a collapsed delegate). Other assertion targets are unchanged. (Guarded by the
`MethodlessFuncTypeAssert` behavioral test — `i.(Compressor)` on a matching and a non-matching dynamic
type, output-compared vs Go; CNR byte-identical and an A/B of archive/zip shows only the two intended
`_<Compressor>`/`_<Decompressor>` → `_<Func<…>>` lines.)

An **UNINITIALIZED local `var` of a methodless named func type** renders its declared type through the
same structural path. `visitValueSpec`'s no-initializer branch computed the type from
`convertToCSTypeName(getTypeName(...))` (the string path) and only re-routed a bare *anonymous*
`*types.Signature` through `getCSTypeName`; a methodless NAMED func type is a `*types.Named`, so it kept
the string render — and that render mangles a slash-bearing cross-package element. go/parser's `parseDecl`
declares `var f parseSpecFunction`
(`type parseSpecFunction func(doc *ast.CommentGroup, keyword token.Token, iota int) ast.Spec`), which
emitted `Func<ж<go.ast.CommentGroup>, go.token.Token, nint, go.ast_package.Spec> f = default!;` — the
`go.ast`/`go.token` elements re-root to the nonexistent `go.go.ast`/`go.go.token` (CS0234), and the
declared delegate then mismatched the lambdas assigned to `f` and the `parseGenDecl(keyword, f)` parameter,
which render the SAME Go types structurally as `ast.CommentGroup`/`token.Token` (CS1661/CS1678/CS1503 — 12
errors, all this one declaration). The no-initializer branch now routes a func-typed var (anonymous
signature OR methodless named func, via `methodlessNamedFuncSignature`) through `getCSTypeName` →
`iifeDelegateType`, whose `aliasedElementTypeName` keeps each element's `pkg.Type` alias:
`Func<ж<ast.CommentGroup>, token.Token, nint, ast.Spec> f = default!;`. This precedence matches
`getCSTypeName`'s own — the func render wins over the foreign-alias route (which for a methodless named
func would point at the SKIPPED delegate declaration); a non-func foreign-renamed local keeps its alias
unchanged. An A/B full-stdlib reconvert moves exactly one file (go/parser/parser.cs), greening go.parser
outright. (Guarded by the `MethodlessFuncType` extension — an uninitialized `var find lookup` where
`type lookup func(string) (path string, ok bool)`; the byte-golden captures the structural render
`Func<@string, (@string, bool)>` — dropping the Go result NAMES the string path keeps — output-compared vs
Go. As with the delegate-routing sibling above, a single-segment/same-package producer compiles either way,
so the unnamed-vs-Go-named result tuple is what guards the routing; the exact slash-bearing CS0234 needs a
multi-segment producer like go/ast, verified by the go/parser source A/B.)

### Named delegate types wrap mismatched initializers
A NAMED func-type field initialized with a value of a DIFFERENT delegate type has no implicit
C# conversion: internal/concurrent's `keyHash: mapType.Hasher` feeds a `hashFunc` field from a
`Func<…>` field. The composite-literal walk resolves each element's field BY NAME (keyed-aware)
and wraps mismatched delegate values in the target delegate's constructor —
`keyHash: new hashFunc((~mapType).Hasher)` (the wrap splits a C# named-argument label first).
FuncLit and nil initializers stay bare. Guarded by `FirstClassFunctions`
(`handler`/`provider`/`registry`).

### A named delegate value passed to a structural func parameter re-wraps
The MIRROR of the argument-position named-delegate wrap: a **structural** (written-anonymous) func
parameter receiving a value of a **named** delegate type — net/http h2_bundle's
`sc.scheduleHandler(…, handler)`, where `handler` is `HandlerFunc` and the parameter is
`func(ResponseWriter, *Request)` (CS1503). Go converts named→structural implicitly; C# needs the
same delegate re-wrap, targeting the synthesized structural delegate:

```go
type Handler func(int, string) string   // has a method → distinct C# delegate
func invoke(f func(int, string) string, n int, s string) string { return f(n, s) }
var h Handler = describe
invoke(h, 1, "a")
```
```csharp
invoke(new Func<nint, @string, @string>(h), 1, "a"u8);
```

Two argument shapes render named and take the wrap: a value whose **Go type** is a named func type
(with methods), and a `:=` local **declared from a method group**, which the declaration emission
types with the matching package named delegate (`HandlerFunc handler = Ꮡsc.Value.handler.ServeHTTP;`
— the bare-function-value `:=` rule above) even though go/types keeps it structural — the exact
h2_bundle shape. A **methodless** named func type already *renders* as the structural delegate
(`methodlessNamedFuncSignature` collapses it — same C# type), so it stays bare; method groups and
func literals themselves convert natively. A generic structural parameter (unsubstituted type
params) also stays native. (Guarded by the `NamedDelegateStructuralParam` behavioral test —
named-with-method and method-group-declared locals wrapped, methodless/method-group/func-literal
controls bare, values vs Go.)

### Func-typed fields with a cross-package (slash-path) type render structurally
A func-typed struct field whose signature names a type from a **multi-segment** import path —
testing/quick's `Config.Values func([]reflect.Value, *rand.Rand)`, where `rand` is `math/rand` —
must render as a structural `Action`/`Func<…>` delegate via `getCSTypeName`, not through the
string display path. The display path stringifies the signature as `func([]reflect.Value,
*math/rand.Rand)` and splits the slash-bearing import path on `/`, emitting the dotted
`math.rand.Rand`; but `math` aliases to `math_package`, so `math.rand` resolves to the nonexistent
`math_package.rand` (CS0426). The structural renderer recurses per signature element and qualifies
each named type by its package **name**:

```go
type Config struct {
    Values func([]reflect.Value, *rand.Rand)   // rand is math/rand
}
```
```csharp
public Action<slice<reflectꓸValue>, ж<rand.Rand>> Values;
```

The re-routing is gated on the signature string containing `/` **or the signature being variadic**:
the string path cannot render a variadic signature at all — `getTypeName`'s `..` strip reduces the
ellipsis of go/build's `JoinPath func(elem ...string) string` (Context, build.go:84) to `.string`,
emitting the unparseable `Func<.@string, @string>` (CS1031 + CS1003 ×2, all three go.build errors),
and even unstripped it has no variadic lowering. Structurally such a field renders the golib
variadic delegate family (`public Funcꓸꓸꓸ<@string, @string> JoinPath;` — see the variadic-lowering
section below), which loose-arg, empty and spread calls through the field all bind. Every other
func field keeps the display path: `func(string) (importPath string, ok bool)` preserves its named
tuple elements that the structural renderer drops. (Guarded by the `FuncTypeParam` behavioral
test's `runner.gen` field, and by `VariadicFuncFields` — a struct with variadic func-typed fields
assigned from a named func and func literals, called loose/empty/spread — for the variadic arm.)

### A variadic func type lowers to the golib `Actionꓸꓸꓸ`/`Funcꓸꓸꓸ` delegates

A **variadic function TYPE used as a value** — a parameter, variable, struct field, or collapsed
methodless named type such as go/types' `reportf func(format string, args ...interface{})` — used
to have three mutually incompatible lowerings: the delegate type rendered `Action<@string,
slice<any>>` (no `params` — the BCL `Action` cannot express one), a variadic func LITERAL emitted
the named-function convention `(@string format, params ꓸꓸꓸany argsʗp) => …` (CS1661/CS1678
against that `Action`), and calls through the value passed loose Go-style args as if `params`
existed (`reportf("…"u8, (~f).typ)` — CS1503; `reportf("empty type set"u8)` — CS7036).

The lowering now targets a golib delegate family carrying a real C# 13 `params Span<T>` tail
(`src/core/golib/variadic.cs`; fixed-arity prefixes up to eight mirror the BCL Action/Func family,
and the `ꓸꓸꓸ` suffix reads as Go's `...`):

```csharp
public delegate void Actionꓸꓸꓸ<T1, TArg>(T1 arg1, params Span<TArg> args);
public delegate TResult Funcꓸꓸꓸ<T1, TArg, out TResult>(T1 arg1, params Span<TArg> args);
```

`iifeDelegateType` — the single structural lowering every `getCSTypeName(*types.Signature)` and
collapsed methodless named func type routes through — names the family when `sig.Variadic()` and
passes the variadic **element** type as the last type argument. Everything else then agrees with
**zero changes** to the other emissions, because the parameter types match by identity
(`ꓸꓸꓸT` *is* `Span<T>`):

- the named-function convention (`internal static @string gather(@string prefix, params ꓸꓸꓸnint
  valsʗp)`) converts as a method group — `apply(gather)` stays bare;
- a variadic func literal (`(@string prefix, params ꓸꓸꓸnint valsʗp) => …`, C# 13 params lambda)
  converts natively — go/types' `comparable(typ, true, default!, (@string format, params ꓸꓸꓸany
  argsʗp) => {…})` now binds its `Actionꓸꓸꓸ<@string, any>` parameter;
- calls through the value pass loose args or an empty tail via C# `params` expansion, and a Go
  spread (`f(nums...)`) binds the slice's `.ꓸꓸꓸ` Span in normal form;
- a C# consumer calls a transpiled printf-style callback naturally (`ctx.Logf("…", a, b)`) — the
  library use case that ruled out the pack-into-a-`slice<T>` alternative.

A `:=`-declared variadic func literal is untouched: it keeps C#'s natural (params-capable) lambda
type under `var` (the `VariadicClosureSpread` shape). One deliberate residue: `deferǃ`/`goǃ` of a
call **through a variadic func value** would need to capture the `Span` tail, which a ref struct
cannot be — no stdlib occurrence; pack into a slice at such a site if one ever appears. Full-stdlib
A/B footprint: go/types predicates.cs/expr.cs plus every file that renders a variadic func type
structurally (inspected file-by-file at introduction). (Guarded by `VariadicFuncValues` — a named
func AND a func literal satisfying a variadic func-typed param, loose/empty/spread calls through
it, and a nil-compared variadic func-typed var — output-compared vs Go.)

**A type-ASSERTION target routes through the same structural lowering.** `convTypeAssertExpr` rendered
the asserted type by converting the TYPE EXPRESSION through the string-based type-name path, which
skips the variadic lowering above — net/http transport.go's
`cw.(func(string, ...any))` emitted `._<Action<@string, .any>>(ᐧ)` with a literal `.any` (CS1001, the
`...` mangled instead of lowered). An anonymous-signature assert target now renders through
`getCSTypeName` → `iifeDelegateType`, exactly like the collapsed methodless NAMED func target already
did: `._<Actionꓸꓸꓸ<@string, any>>(ᐧ)`. Non-variadic signatures render identically on both paths, so the
only full-stdlib delta is the transport.cs site. (Guarded by `VariadicFuncTypeAssert` — a positive
variadic assert invoked through the asserted value, a negative assert on a non-func value, and a
non-variadic anonymous func assert, output-compared vs Go.)

### Major-version import directories
A `/vN` import path segment (math/rand/v2) hosts a package named for the PARENT segment, and
the emitted class follows the package NAME: consumers reference `math.rand.rand_package`, not
the path-derived `v2_package` (CS0234). Centralized in `convertImportPathToNamespace`, so
every consumer-side derivation (using aliases, attribute references, FQ renderings) agrees.

### A non-canonically-aliased import renders foreign types via the file's alias
A file that imports a package under an **explicit alias that differs from the canonical package
name** must render that package's types through the alias, not the canonical name. cryptobyte's
`asn1.go` imports `encoding/asn1` as `encoding_asn1` — because the sibling vendored subpackage
`.../cryptobyte/asn1` already claims the canonical `asn1` — so a `*asn1.BitString` parameter must
emit `ж<encoding_asn1.BitString>`. `getTypeName` had rendered the canonical `asn1.BitString`
(`importQualifier(pkg.Name())`), which the file's `using asn1 = …cryptobyte.asn1_package` resolves
to the *subpackage* (no `BitString`) — CS0426, and the RecvGenerator faithfully propagated the
wrong qualifier into its `.g.cs`. A `types.Type` carries no source alias, so a per-file
`importPathAliases` map (import path → the alias the file's `using` bound) is threaded into
`getTypeName`; a foreign type whose import path the file aliased renders through that alias. Only
**explicitly-aliased** imports populate the map — unaliased / blank / dot / Δ-collision-renamed
imports are absent and keep the `importQualifier(pkg.Name())` fallback, so nothing else changes
(value references were already correct — they come from the AST import name via `convIdent`; only
type references, sourced from `types.Type`, lost the alias). Cleared cryptobyte's CS0426 (which had
masked deeper `Builder.add`/`slice.Value` roots, now banked). GUARD OWED — the shape needs two
packages whose names collide so one import is forced non-canonical, not expressible in the
single-library behavioral corpus.

## Struct Type Embedding
Go structs use "[type embedding](https://go101.org/article/type-embedding.html)" instead of inheritance. Since converted structs are C# `struct`s (no inheritance), the `TypeGenerator` manages the equivalent: it adds a field for the embedded type and promotes the embedded type's fields and methods (selection shorthand). Both field and method promotion are **transitive through every embedding level**: when `top` embeds `mid` which embeds `inner`, `top` gets an accessor for `inner`'s field `n` (`top.n => ref mid.n`) and a forwarding receiver for `inner`'s method `describe` (`top.describe() => target.mid.describe()`), each resolving through `mid`'s own one-level promotion. The generator collects an embedded struct's members and methods recursively (following each field whose name equals its type's simple name — Go's embedding marker), with the closest declaration of a name winning, matching Go's promotion rules. **Pointer embeds promote too.** Go also embeds by pointer (`*traceBuf`), whose C# field type is `ж<traceBuf>`; its methods and fields are promoted exactly like a value embed (the field's ref-property is dereferenced — `target.traceBuf.Value.method()` — which binds the pointer-receiver method via the `[GoRecv]` `ж<T>` overload). The embedding-marker comparison dereferences the field type first, because a pointer field's simple name carries a `.Value` suffix (`traceBuf.Value`) that would never match the bare embed field name. This matters most *transitively*: `traceExpWriter` embeds `traceWriter` (value) which embeds `*traceBuf` (pointer), and `traceBuf`'s `varint`/`byte` must promote all the way up — without the deref-aware marker the nested pointer embed is skipped and the upper struct silently loses the method (CS1929). (Guarded by the `NestedEmbeddingPromotion` behavioral test for value embeds and the `PointerEmbeddingPromotion` test for one-level and two-level-transitive pointer embeds; runtime relies on the field case for `stackWorkBuf` → `stackWorkBufHdr` → `workbufhdr.nobj` and the pointer case for the trace writers.) Because the promotion is performed at conversion time by the generator, methods added later in hand-written C# are not automatically promoted; keeping the source in Go and re-converting (or using explicit interfaces) is the maintainable path.

**Zero values of promoted-embed structs construct through a generated constructor — never `default`.** The generator stores each promoted embed in a `private readonly ж<T>` box that only the type's constructors allocate, so a `default`-valued instance has null boxes and the first promoted-member access throws `NullReferenceException`. Both halves close this: the **converter** renders every *uninitialized* declaration of such a struct through the NilType constructor instead of `default!` — `var s shadowed` emits `shadowed s = new(nil);`, an uninitialized package-level `var g shadowed` emits `internal static shadowed g = new(nil);` (the addressed-global box wraps the same, `new(new shadowed(nil))`), and a named result `(r shadowed)` declares `shadowed r = new(nil);` — while the **generator** allocates the boxes in the *parameterless* constructor too, so the `new S()` zero values materialized by `heap(new S(), out var Ꮡs)` (an address-taken local) and golib's `@new<T>()` (`p := new(shadowed)`, which constructs via `Activator.CreateInstance<T>()`) are equally usable. The detection (`structHasPromotedEmbeds`, `visitStructType.go`) mirrors the embedded-field emission: an embed takes the promoted-box path unless it is a same-package interface, a builtin non-named embed (`int`), or a pointer to a non-named type; a cross-package embed (selector type) always promotes. Residual gap: an instance materialized as `default(T)` *outside* a declaration — a missing-key map read, a freshly `make`d slice's elements — still has null boxes; golib cannot run a constructor generically there. (Guarded by the `NamedTypeOverStruct` behavioral test — `var s shadowed` with explicit `s.ctxt.fn` and promoted `s.fn` access, plus `new(shadowed)`, vs Go.)

**A C#-keyword-named embed composes generated names from the unescaped member name.** A Go struct named for a C# keyword (`type base struct{…}`) is emitted with the `@` escape (`@base`), and embedding it makes `@base` the member name. Standalone identifier positions keep the escape (the `partial ref @base @base` accessor, the constructor parameter, member accesses like `instance.@base.id`), but every *composed* generated name must strip it, because `@` is only valid leading an identifier: the promoted-struct box field and its constructor assignments emit `Ꮡʗbase` (`Ꮡʗ@base` is CS1002), matching the already-stripped `Ꮡ`-prefixed field-reference statics and the converter's `structFieldBoxName`. (Guarded by the `NamedTypeOverStruct` extension — a keyword-named embed with promoted field/method access, a keyword-keyed composite literal, and a write through `&p.id` promoted through the embed, all vs Go.)

**Cross-package embeds resolve through the semantic model.** The member-collection above resolves the embedded struct's *syntax* (`GetStructDeclaration`) — same-package or via `CompilationReference`s. In a real [MSBuild](Glossary.md#msbuild) build, project references arrive as **metadata** references (never `CompilationReference`), so a cross-package embed — `type rtype struct { *abi.Type }` (runtime `type.go`) or a user package embedding a library struct — silently promoted **nothing**: the generated "Promoted Struct Field Accessors" section was empty and every `t.TFlag`/`t.Str`/`t.Kind_` was CS1061. The field collection now falls back to the **type's metadata symbol** (`GetTypeByMetadataName` on the normalized nested name, e.g. `go.internal.abi_package+Type`) and enumerates its public instance fields; the emitted accessors are unchanged in form — true refs through the embed (`public ref abi.TFlag TFlag => ref Type.Value.TFlag;` for a pointer embed), so writes through a promoted name reach the embedded target. Transitive promotion through a *metadata* type's own embeds is not chased (no corpus site needs it). **Promoted POINTER-RECEIVER method calls through a cross-package *pointer* embed are routed at the call site**: the generator emits no method forwarder for a metadata embed (method promotion is syntax-resolved), so `t.Uncommon()` on `Δrtype` (embeds `*abi.Type`, runtime `type.go`) was CS1929; the converter now emits the explicit hop through the embed field's box — `t.Type.Value.Uncommon()` — where the deref'd `.Value` is a ref return, binding the `[GoRecv] ref` extension addressably. A *same-package* pointer embed keeps its generated forwarder (no churn), and a promoted **value-receiver** method call (`p.Hot()`) remains a documented open gap — call through the embed explicitly. (Guarded by the `CrossPkgUser` Phase-4b extension — a promoted pointer-receiver `Calibrate` through the cross-assembly pointer embed, write-through observed via the target.) (Guarded by the `CrossPkgUser` Phase-4 extension — pointer-embed and value-embed field promotion across the assembly boundary, write-through observed via the embedded target, vs Go; cleared runtime `type.go`'s 4 CS1061, 68 → 64.)

Two refinements complete the cross-package pointer-embed story (2026-07-03, internal/reflectlite's last 4): **(a) the hop names the FIELD, which is struct-scoped** — an embed field named like a Δ-renamed package type (rtype's embedded `Type` vs reflectlite's `Type` interface, Δ-renamed `ΔType` by its type-vs-method collision) is *declared* unrenamed, so the hop emission must not apply the package-level rename (`t.ΔType.Value.Uncommon()` was CS1061); both hop arms now route through `structFieldBoxName`, the same struct-scoped naming the box accessors use. **(b) A generated interface implementation forwards through the hop too**: when an interface member has NO direct struct method and is satisfied purely by Go promotion through a single embedded-pointer field (`GoImplement<rtype, ΔType>` — `Size`/`Kind` live on `*abi.Type`), the `InterfaceImplTemplate` emits `this.Type.Value.Size()` instead of the unbindable `this.Size()` (CS1929); the `IжAdapter` template forwards the same members `m_box.Value.Type.Value.M()`. Detection is syntax-level — the converter's embed marker is the `public partial ref ж<X> F {{ get; }}` property (`GetEmbeddedPointerHopNames`) — and gated to a SINGLE hop (Go's promotion-ambiguity rules make multi-embed interface satisfaction rare; extend when the corpus surfaces one). (Guarded by the `CrossPkgUser` Phase-5 extension — a local Δ-renamed `Meter` interface colliding with the embed field name, satisfied purely by promotion through `*CrossPkgLib.Meter`, with all bump paths aliasing one shared object, vs Go.)

**A pointer-receiver method promoted through a VALUE embed is routed at the call site, not by a generator forwarder.** When `timeTimer` embeds `timer` *by value* and `timer` has a pointer-receiver method (`func (t *timer) modify(…)`), the generator emits **no** `modify` forwarder on `timeTimer` (a `target.timer.modify(…)` forwarder body would copy the value field, losing the write, and would not bind the `ж<timer>` overload) — so a promoted call `t.modify(…)` on a `*timeTimer` would leave the receiver as the whole `ж<timeTimer>` box, which the promoted method's ж/`[GoRecv]`-ref overload cannot bind (CS1929). The converter instead routes the promoted call through the embedded field's box, exactly as the *explicit* `t.timer.modify(…)` already renders: `t.of(timeTimer.Ꮡtimer).modify(…)` for a pointer local, `Ꮡt.of(timeTimer.Ꮡtimer).modify(…)` for a deref'd pointer parameter (the `&receiver.field` &-machinery supplies the correct box per receiver form). Because it field-refs the real embedded storage — never a `Ꮡ(copy)` — the mutation writes through. This is detected via the method's `types.Selection.Index()` having a single embedded-field hop (`[embeddedField, method]`); it is gated to a **value** embed (a *pointer* embed already yields the box as its field value and is left to the generated forwarder — taking its address would double-box to `ж<ж<T>>`), and to a single hop (deeper chains fall through).

**The pointer-interface ADAPTER projects through VALUE embeds the same way — chained.** A `GoImplement<T, Iface>(Pointer = true)` whose interface members are satisfied only by promotion through value embed(s) — dwarf's `type UintType struct { BasicType }`, `type BasicType struct { CommonType }`, `func (c *CommonType) Common()` — cannot forward `m_box.M()` (nothing binds on `ж<UintType>`, CS1929 ×18). The `ImplementGenerator` resolves each unbound interface member by walking the single-value-embed chain (syntax marker: the `public partial ref X X {{ get; }}` property whose name equals its type's simple name — `GetEmbeddedValueHopNames`; bounded to 4 hops), composing the box projection hop by hop via the TypeGenerator's static ref accessors: `m_box.of(UintType.ᏑBasicType).of(BasicType.ᏑCommonType).Common()`. At each level a direct-ж method binds on the projected box and anything else binds through its deref'd `.Value` (ref extensions bind on the ref-returning `Value`) — the same dichotomy as the pointer-embed hop. Mutations write through (the projection field-refs the real embedded storage in the receiver box). (Guarded by `StructPointerPromotionWithInterface`'s `counterKind → kindBase → meta` chain: `st.Stamp()` twice through the interface, `Hits()` reading the count mutated through the same boxes, vs Go.)

**A FOREIGN value embed's direct-ж method binds through METADATA.** When the embedded type lives in another assembly — database/sql's `driverConn` value-embeds `sync.Mutex`, cast `*driverConn → sync.Locker` — its direct-ж method (`Lock`/`Unlock`, emitted by the converter as `this ж<Mutex>` extensions) is visible only in the compiled sync assembly's METADATA, never this compilation's syntax trees. The syntax-based box scan (`GetBoxReceiverMethodNames`) therefore misses it and the chain-walk fell through to the unbindable `m_box.Lock()` (CS1929 ×2). The walk now also resolves the embed field's TYPE SYMBOL and probes its containing package class's static `this ж<T>` members via metadata (`GetForeignBoxReceiverMethodNames`, mirroring the foreignStruct arm's boxBound scan — only a PUBLIC ж-extension binds cross-assembly, since unexported `RecvGenerator` twins are internal); when found it forwards the box hop `m_box.of(driverConn.ᏑMutex).Lock()`, exactly the converter's own call-site form (`Ꮡdc.of(driverConn.ᏑMutex).Lock()`). The `.Lock()` resolves in the generated adapter because `sync_package` sits in the enclosing `go` namespace — the same reason the converter's own call sites bind without a `using static`. (No single-baseline behavioral guard expresses this — it needs a foreign package's ж-method type value-embedded AND implementing that package's interface, the `sync.Mutex`+`sync.Locker` shape — so **GUARD OWED**; verified by a minimal two-assembly reproduction of that exact shape, 2×CS1929 → 0.)

The **exception is the enclosing method's own `[GoRecv] ref` receiver**: a non-direct-ж pointer-receiver method renders `this ref T recv` with **no box** (`Ꮡrecv` exists only for direct-ж), so the box descent referenced a nonexistent name (CS0103 — runtime `mgcscavenge.go`, `(*scavChunkData).alloc/free` calling the promoted `sc.setEmpty()`/`setNonEmpty()` from the embedded `scavChunkFlags`). No box is needed either: the embedded field of a `ref` receiver is *addressable*, so the promoted method's `[GoRecv] ref` overload binds on the **explicit field call** — `sc.scavChunkFlags.setEmpty()` — with faithful write-through. (A *direct-ж* target on the bare receiver would have promoted the enclosing method via the capture-mode fixpoint, so this arm's target always has the `ref` overload.) The receiver name-match is guarded **rendered==raw**: an inner binding that shadows the receiver name is Δ-renamed by the shadow pass, declines the arm, and keeps the descent — the same hardening applied in `convUnaryExpr`'s `&recv.field` branch, where a pointer *local* shadowing the receiver name previously took the receiver arm and emitted `Ꮡ`+raw (a nonexistent box) instead of falling to the pointer-variable arm (`cΔ1.of(chunk.Ꮡflags)`). The fix also pre-cleared the same latent shape in `archive/zip` (`f.FileHeader.hasDataDescriptor()`), `go/internal/gcimporter`, `go/types`, and `image` (whole-stdlib reconvert diff: exactly those sites changed, nothing else). (Guarded by the `EmbeddedValuePointerMethod` behavioral test — value embed + mutating pointer-receiver methods called via a pointer local, a deref'd param, AND the enclosing `[GoRecv] ref` receiver, plus a shadowing-pointer-local control, all with write-through verified against Go; runtime relies on it for `timeTimer`'s `modify`/`stop`/`reset` and `scavChunkData`'s `setEmpty`/`setNonEmpty`.)

**A POINTER embed's BOX-receiver primary promotes through the box hop, not the deref'd value.** The promoted-receiver harvest (`GetExtensionMethods` → `IsExtensionMethodForStruct`) matched only VALUE-receiver forms (`T`/`ref T`/…), so a **direct-ж** primary (`this ж<T>`, emitted when a method takes the address of a receiver field) on an embedded type had no promoted forwarder — sha3's `cshakeState` embeds `*state`, whose `Write` is `this ж<state>`, so `Ꮡc.Write(…)` was CS1929. Such a method IS promotable through a **pointer** embed: the converter renders the hop `target.<embed>` as a `ж<T>`, so the forwarder `target.<embed>.Write(…)` binds the box receiver directly (no box construction). The `TypeGenerator` now collects those box primaries separately (`GetBoxReceiverExtensionMethods`, keyed off `GetEmbeddedPointerHopNames` so it fires ONLY for pointer embeds — a value embed's `target.<embed>` is a value that cannot bind a ж-receiver, which would need the box-hop form the sibling `GoImplement` adapter uses above) and marks each `MethodInfo.IsBoxRecv`, so the emission drops the `.Value` a value-receiver forwarder appends (`target.<embed>.M(…)` for a box primary vs `target.<embed>.Value.M(…)` for a value method). The pointer-receiver forwarder delegates to the value form unchanged, and the shared box means write-through reaches the real embedded storage. (Guarded by the `PointerEmbedBoxReceiver` behavioral test — `Outer` embedding `*Inner` whose `Add` takes `&n.total` (a box primary), the promoted `o.Add(…)` mutating through the shared box, output-compared vs Go. Full behavioral suite green; a whole-corpus confirmation on the real sha3 is deferred to the next census, as with the sibling foreign-embed fix.)

### A promoted field whose name equals the enclosing type is Δ-renamed

Go lets an embedded struct carry a field whose name equals the type doing the embedding —
debug/gosym's `type Func struct{ *Sym }` where `type Sym struct{ Func *Func; … }`, so `Sym.Func`
promotes onto `Func`. The generator's promoted-field accessor would then emit a `Func` member
inside struct `Func`, which C# rejects (CS0542 — a member cannot share its enclosing type's name).
The `TypeGenerator` now Δ-prefixes just that accessor's NAME when its simple name equals the
`NonGenericStructName` (the field ACCESS on the right keeps the original name), matching the
`ΔGoType`/`Δslice` collision-rename precedent:
```csharp
public ref ж<Func> ΔFunc => ref Sym.Value.Func;   // was: `Func => …`, CS0542
```
The promoted field is read on the embedded struct directly (`sym.Func = fn`), never via the outer
value, so no converter reference to the renamed accessor needs coordinating; a package that *did*
read `outerFunc.Func` would surface CS1061 in the gate (none does). Cleared debug/gosym's lone
CS0542. Guarded by `PromotedFieldNameIsType` (a `Node` embedding a `*sym` whose `Node` field
collides — accessed through the explicit embedded path, values vs Go).

### Promoted pointer methods descend multi-hop value-embed chains
A pointer-receiver method promoted through two or more embedded VALUE structs descends hop by hop: the first hop through the `&`-machinery (box-vs-parameter distinction), then one `.of(<Owner>.<field-box>)` view per additional hop -- the `ж<T>` field views compose onto the method's receiver box (reflect's `sliceType` embeds `abi.SliceType` embeds `abi.Type`, whose `Common()` extension binds `ж<abi.Type>` -- CS1929):
```csharp
Ꮡ(rg).of(rig.ᏑDevice).of(CrossPkgLib.Device.ᏑSensor).Calibrate(3);
```
The own-receiver bare form joins the hop path (`recv.E1.E2.method(...)`); a chain broken by a pointer embed falls through unchanged. Guarded by `CrossPkgUser`.

## Interfaces
Go interfaces are duck-typed: a type implements an interface simply by having the methods. The converter emits each **user-defined** interface as a partial interface with a `[GoType]` attribute, and the **`ImplementGenerator`** source generator discovers which concrete types satisfy it and emits the implementing glue plus the implicit conversions. As a result, assigning a concrete value to an interface variable is direct — no reflection lookup or `.As(...)` call is needed:

```go
type Stringer interface {
    String() string
}

type point struct{ x, y int }

func (p point) String() string {
    return fmt.Sprintf("(%d, %d)", p.x, p.y)
}

func describe() Stringer {
    return point{1, 2}    // point implements Stringer -> assignable directly
}
```
```csharp
[GoType] partial interface Stringer {
    @string String();
}

[GoType] partial struct point {
    internal nint x, y;
}

[GoRecv] internal static @string String(this ref point p) {
    return fmt.Sprintf("(%d, %d)"u8, p.x, p.y);
}

internal static Stringer describe() {
    return new point(1, 2);   // implicit conversion emitted by ImplementGenerator
}
```

The well-known built-in interfaces (`error`, `fmt.Stringer`, etc.) are hand-written in `golib`/the baseline rather than `[GoType]`-generated, but concrete types implement them the same duck-typed way. (Earlier strategies used a generic `As`/reflection mechanism; that has been superseded by the compile-time source generators.)

Each discovered "concrete type implements interface" pairing is recorded as an assembly-level attribute in the package's `package_info.cs`, e.g. `[assembly: GoImplement<point, Stringer>]`, which `ImplementGenerator` consumes.

Two refinements to the recording pipeline (io's `NopCloser`/`eofReader`, 2026-07-03): **(a) the interface-inheritance prune drops only COMMON implementations, and only from the LOWER interface.** When an interface embeds others (`ReadCloser` = `Reader` + `Closer`), a type recorded on both the derived and an embedded interface needs only the derived pair — C# interface inheritance covers the embedded one. The prune intersects the two sets and removes the overlap from the *embedded* interface's set; it previously intersected **in place on the derived set** (the HashSet mutates its receiver), which *emptied* the derived interface's recordings whenever the overlap was empty — `GoImplement<nopCloser, ReadCloser>` vanished and the `return nopCloser{r}` failed CS0029. **(b) An INDEX-expression assignment target records against its ELEMENT type.** `mr.readers[0] = eofReader{}` assigns to a `[]Reader` element; the interface-detection previously tested the container's root identifier (`mr` — never an interface; Go forbids indexing one), so no pair was recorded and the concrete literal emitted bare. The check now types the whole index expression, and the conversion-recording path keeps the element type rather than redirecting to the container. (Guarded by the `InterfaceCasting` extensions — `rdCloser`, an inheriting interface returned concretely while the embedded `rdr` has its own recording, plus an interface-slice element assignment.)

The prune matches a FOREIGN base by its **canonical name**: the inheritance tracking stores both the alias render the declaration emits (`fs.FileInfo`) and the `getFullTypeName` render (`go.io.fs_package.FileInfo`) — the implementation-map keys are canonical, so the alias form alone never matched a foreign embed and both the derived and base impls emitted the same explicit members (zip's `headerFileInfo : fileInfoDirEntry` + `fs.FileInfo`, CS8646 ×6/CS0111 ×2). Structural bases track their canonical names the same way. (Guarded by `CrossPkgUser`'s `stamped` — a local interface embedding the foreign `CrossPkgLib.Labeled`, with `seal` recorded against both; only the derived record survives.)

**An embedded INTERFACE FIELD forwards the members it declares in the pointer adapter.** zip's `type nopCloser struct { io.Writer }` satisfies `io.WriteCloser` with `Write` living on the embedded interface VALUE (Go promotes the field's method set) and `Close` on the struct. The `IжAdapter` forwards still-unbound members that the field's interface declares through the field itself — `m_box.Value.Writer.Write(…)` (CS1929 with no forward). Detection is semantic (a non-static field whose name equals its interface type's simple name — the converter emits embeds as real fields), gated to a SINGLE embedded interface field, and filtered to members the field's interface (including its inherited interfaces) actually declares. (Guarded by `InterfaceCasting`'s `wrapSink{Animal}` cast by pointer to the wider `speakShutter` — both the promoted and the own member called through the interface, runtime-verified vs Go.)

**Promoted forwarders through a Δ-renamed embedded interface use the markerless FIELD name.** The converter names an embedded field after the **Go embed name**, so a struct value-embedding an interface whose C# TYPE was collision-renamed (see [Type-vs-Method Name Collisions](#type-vs-method-name-collisions)) declares `public log.slog_package.ΔHandler Handler;` — the marker lives on the type only. testing/slogtest's `type wrapper struct { slog.Handler; mod func(*slog.Record) }` (slog has both a `Handler` type and a `Logger.Handler()` method, so the type is `ΔHandler`) broke in BOTH generated wrapper forms because the `ImplementGenerator` derived the promoted-forwarder field name from the interface TYPE's simple name: the value partial struct emitted bare `ΔHandler.Enabled(…)` (CS0103 cross-package, CS0120 same-package where the bare name binds the nested interface type), and the pointer adapter emitted `m_box.Value.ΔHandler.Enabled(…)` (CS1061). The field name is now the `Δ`-stripped simple name (`GetSimpleName(…, dropCollisionPrefix: true)`, the same derivation `StructTypeTemplate` already used for embedded-field accessors) in all three places: the value template's promoted arm, the pointer arm's promoted fallback, and the pointer arm's semantic embedded-interface-field detection (which compares field name to type name and otherwise never matches `Handler` vs `ΔHandler`):

```csharp
// value partial struct — promoted members forward through the field:
public bool Enabled(nint level) => Handler.Enabled(level);
// pointer adapter — through the box value's field:
bool global::go.main_package.ΔHandler.Enabled(nint level) => m_box.Value.Handler.Enabled(level);
```

An overridden member is untouched (it forwards to the struct's own method, `this.Handle(…)` / `m_box.Value.Handle(…)`), and a non-renamed interface's simple name has no marker to strip, so every other promotion emits byte-identical code. (Guarded by `ShadowedInterfaceEmbed` — a `Handler` interface Δ-renamed by a `Logger.Handler()` method collision, value-embedded in a `wrapper` struct that overrides one of its three methods, cast to the interface by BOTH value and pointer, promoted and overridden members runtime-verified vs Go; cleared testing/slogtest's 6 errors.)

**A FOREIGN struct's adapter forwards a PROMOTED interface method through the box value, not a phantom static.** When the pointer adapter is generated in an assembly OTHER than the struct's — the struct's package class lives in a different namespace segment, so its extension methods are invisible to extension-method lookup — the `ImplementGenerator` forwards each interface member through a package-class STATIC call (`xcoff_package.ReadAt(m_box, …)`). But this only works for a method the struct **declares directly** (whose `RecvGenerator` ж/ref static exists). A **promoted** interface method — debug/buildinfo's `*xcoff.Section` → `io.ReaderAt`, where `Section` embeds the `io.ReaderAt` interface so `ReadAt` is promoted, not declared — has no such static, so the forward targets a nonexistent overload (CS1501, "no overload takes 3 arguments"). The static forward is now gated on a real box/ref-bound static existing; when absent, the adapter forwards through the box VALUE — `m_box.Value.ReadAt(…)` — invoking the struct's own PUBLIC promoted method (the same promotion `File.ReadAt` etc. rely on). A directly-declared method keeps the static forward unchanged (no churn). (Validated by the `go-src-converted/debug/buildinfo` build — its `*os.File → io.ReaderAt` [direct `ReadAt`, static] and `*xcoff.Section → io.ReaderAt` [promoted, box-value] adapters — plus the full behavioral suite + tar/math-big/net corpus; a single-assembly behavioral guard cannot host it — the shape needs a struct embedding a THIRD package's interface, cast cross-assembly, so **GUARD OWED**.)

**Pointer-sourced interface values use a generated ADAPTER, not the value-boxing partial struct (2026-07-03).** A Go interface value created from a pointer — `var s Iface = &t`, `New(new(lockedSource))`, `Rand{src: &runtimeSource{}}` — holds the **pointer**: every call through the interface mutates the original object, `s.(*T)` recovers that same pointer, and interface equality is pointer identity. The old emission deref'd the box into the C# interface (`~box`, boxing a **copy**) — aliasing divergence — and could not serve **direct-ж** receiver methods at all (a method that takes the address of a receiver field is emitted with the box AS its receiver, `this ж<T>`, which a struct's `this` can never bind — math/rand `lockedSource` CS1929). The converter now records such casts as `[assembly: GoImplement<T, Iface>(Pointer = true)]`, and `ImplementGenerator` emits a sealed **adapter class** instead:

```csharp
internal sealed class runtimeSourceжSource : go.math.rand_package.Source, IжAdapter
{
    private readonly ж<runtimeSource> m_box;
    public runtimeSourceжSource(ж<runtimeSource> box) => m_box = box;
    public object? Box => m_box;
    long go.math.rand_package.Source.Int63() => m_box.Int63();  // direct-ж / ж-twin binds the box
    // Equals/GetHashCode delegate to box identity (Go pointer-interface equality)
}
```

Cast sites emit the adapter around the box (`Incrementer inc = new CounterжIncrementer(c);`, `src: new runtimeSourceжSource(Ꮡ(new runtimeSource()))`), covering call arguments, keyed composite-literal fields, and `var` declarations; a pointer-typed operand in these positions renders as the box (isPointer ident context), not the deref'd receiver ref-local. Member forwarding picks the receiver form per method: direct-ж and `[GoRecv]` ref-extensions (whose `RecvGenerator` ж-twin exists) forward to `m_box.M(...)`; plain value-receiver methods forward to `m_box.Value.M(...)` (Go copies the value at the call). The golib type-assert machinery (`_<T>()`) unwraps `IжAdapter.Box` so `s.(*T)` yields the original `ж<T>`, and `AreEqual` unwraps both operands so interface-vs-interface and interface-vs-pointer comparisons are box identity (`ж<T>.Equals` is already identity-based); `iface == ptr`/`iface != ptr` comparisons emit `AreEqual(...)` with the pointer operand kept as the box (the old `iface == ~p` deref form compared a copy). Because each adapter is a distinct class, the interface-inheritance de-duplication (dropping `GoImplement<T, Source>` when `GoImplement<T, Source64>` exists and `Source64` embeds `Source`) exempts pointer-form pairs — a `Source`-targeted cast site references `runtimeSourceжSource` even though `runtimeSourceжSource64` also implements `Source`. VALUE-sourced casts (`var s Iface = t`) keep the partial-struct implementation — Go copies the value into the interface there, which is exactly C#'s struct-boxing semantic. Known limits (documented, not yet needed by the corpus): a cross-package pointer cast keeps the old deref-copy form (the adapter class only exists in the impl type's assembly — `isLocalImplType` gate), and asserting an adapter-held interface to a *different* interface (`s.(Source64)` on a `Source`-created value) is not yet unwrapped. (Guarded by the `InterfaceCasting` extension — pointer-receiver `Counter` with a direct-ж member cast to an interface, mutations verified through BOTH the interface and the original pointer, assert-back recovering the same box, and `back == c` pointer equality, run-verified vs Go; and by `InterfaceImplementation`'s output comparison — `zoo[0] == f` interface-vs-pointer identity.)


**Non-empty interface-to-interface conversions use a forwarding adapter.** A Go interface value may be assigned or passed to another non-empty interface when the source interface method set satisfies the target (`var local localLabel = foreign`, where `foreign` is `CrossPkgLib.Labeled`). C# has no structural conversion between unrelated interfaces, so the converter records the interface pair as `[assembly: GoImplement<CrossPkgLib_package.Labeled, localLabel>]` and emits the cast site as a generated adapter:

```csharp
CrossPkgLib.Labeled foreign = new CrossPkgLib.Sensor(Name: "adapter"u8, Temp: 21);
localLabel local = new CrossPkgLib_LabeledᴠlocalLabel(foreign);
fmt.Println(labelOf(new CrossPkgLib_LabeledᴠlocalLabel(foreign)));
```

`ImplementGenerator` emits a sealed adapter implementing the target interface and `IInterfaceAdapter`, stores the source interface value, and forwards each target member to that value. The golib assertion/equality helpers unwrap `IInterfaceAdapter.Value` before type assertions, `Implements<TInterface>`, and `AreEqual`, so the wrapper behaves as an interface view over the original Go interface value rather than a new concrete payload. Guarded by `InterfaceToInterfaceAdapter`, which imports `CrossPkgLib.Labeled`, assigns it to a local compatible interface, passes it as a parameter, and output-compares the forwarded calls.

Two rules govern how concrete implementation records are emitted:

* **Only impl types declared in the *current* package are recorded.** `ImplementGenerator` realizes the attribute by emitting a `partial struct <Impl> : <Interface>` into the **current package's** namespace and class — so it can only add an interface to a type defined in the *same assembly*. A pairing whose impl type is *imported* from another package (e.g. `image/color/palette` building `[]color.Color{ color.RGBA{…} }`) is therefore **not** re-emitted in the consumer: that relationship is already established in the impl type's own package (`image/color` records `[assembly: GoImplement<ΔRGBA, Color>]`). Re-emitting it in a consumer would generate a broken cross-assembly partial (a fresh empty `palette_package.ΔRGBA` rather than the real `color_package.ΔRGBA`), so the converter skips any pairing whose impl type is not local.
* **Multi-segment interface references are root-qualified.** The `GoImplement` attributes are emitted before the file's `namespace` with only `using go;` in scope; that directive imports the *types* of namespace `go` (so a top-level `io_package.Writer` resolves unqualified) but **not** its nested namespaces. A multi-segment package class such as `container.heap_package.Interface` is therefore root-qualified to `go.container.heap_package.Interface` so it resolves; single-segment refs (`io_package`, `sort_package`) are left unchanged.

### Cross-package pointer-to-interface conversions use the foreign adapter
A pointer-sourced cast to an interface implemented by a FOREIGN type references the foreign assembly's PUBLIC adapter class - os's `err = &PathError{...}` emits `new fs.PathErrorжerror(Ꮡ(new PathError(...)))`, io/fs having generated the adapter from its own `GoImplement<PathError, error>(Pointer = true)` record. The record's existence is read from the imported package's package_info (`parseExportedPointerImplements`, the same imported-records pattern as GoTypeAlias). The existence key's INTERFACE side is the **canonical qualified name** (`canonicalRecordIfaceName` — a dotless record name qualifies with the recording package's class): the simple name alone let image's `Paletted→image.Image` record satisfy a `Paletted→draw.Image` cast, referencing the foreign adapter that implements the WRONG interface (CS1503); the value-implement records key the same way. The reference goes through the file-local package ALIAS (`fs.PathErrorжerror`, user-ruled style) via getTypeName, which also registers the using. Guarded by `CrossPkgUser` (`rep = mtr` -> `new CrossPkgLib.MeterжReporter(mtr)`; `&CrossPkgLib.Alarm{}` -> error; and the same-simple-name LOCAL `Labeled` — `var localLb Labeled = sp2` takes the LOCAL `CrossPkgLib_SensorжLabeled`, never the lib's exported `SensorжLabeled`).

An **EXPLICIT pointer-to-interface conversion** — Go's `image.Image(dst)` with `dst *image.RGBA` (image/draw) — is the same interface cast in conversion clothing and routes through the same machinery: `isTypeConversion` probes the ORIGINAL pointer type against an interface target (the value type alone does not implement it — the elem-only probe misread the conversion as a constructor call, `new image.Image(dst)`, CS0144), and the emission re-renders the argument in its BOX form and CASTS the adapter to the interface — `((image.Image)new image_ΔRGBAжImage(Ꮡdst))` — because the adapter implements its members explicitly, and a chained member access on the conversion result (`CrossPkgLib.Labeled(sp).Label()`) cannot bind on the adapter class itself (CS1929). (Guarded by `CrossPkgUser`'s `CrossPkgLib.Labeled(sp2).Label()` / `LabeledOf(sp2)` pair, output-compared vs Go.)

The **VALUE mirror of the explicit conversion** — `crypto.SignerOpts(sigHash)` with `sigHash crypto.Hash` (crypto/tls, CS0030 ×4) — routes a **FOREIGN named non-interface VALUE source** through the same `convertToInterfaceType` machinery, keeping the outer interface cast: `((crypto.SignerOpts)new crypto_HashᴠSignerOpts(sigHash))` plus the local value-form `GoImplement<crypto_package.Hash, crypto_package.SignerOpts>` record. A plain cast cannot bind here: a foreign value type implements its interfaces via **extension methods** (never structurally), and the converting assembly cannot `partial` a foreign type — the same reason the implicit both-foreign value arm exists (`syscall.Signal`→`os.Signal`). When the defining assembly already implements the pair (its package_info carries the value-form record), `convertToInterfaceType` falls through and the emission stays the plain cast spelling. LOCAL value sources deliberately keep the plain-cast/partial-impl route (no churn, no redundant records). The outer cast is load-bearing exactly as in the pointer arm — and additionally because `var signOpts = …` must type as the INTERFACE: each tls site reassigns `signOpts` to a different adapter two lines later (CS0029 hazard if the var typed as the adapter class). (Guarded by the `CrossPkgLib`/`CrossPkgUser` extension — `Verdict` implements `Scored` via a value receiver with deliberately NO witness in the lib, `CrossPkgLib.Scored(CrossPkgLib.Verdict(4))` converts explicitly in the user package and the same var is then reassigned a local `*tallies` implementation, output-compared vs Go; whole-stdlib reconvert diff: exactly the four crypto/tls sites plus its package_info record.)

**No exported adapter — the LOCAL adapter for a foreign pair.** When the defining package never
converts the pair itself (os never casts `*File` to `io.Reader`, so no record exists to
reference), the converting package records `GoImplement<os_package.File, io_package.Reader>(Pointer
= true)` **locally** and the generator emits a **local adapter class** for the foreign struct
(`internal sealed class os_FileжReader`; the `m_box` field is fully qualified). The class name is
**package-qualified** (`{pkg}_{Struct}ж{Iface}`): two same-named foreign structs adapting to one
interface otherwise compose a single colliding class — math/big records both `bytes.Reader` and
`strings.Reader` against `io.ByteScanner` (CS0102/CS0111/CS8646 ×8). The local VALUE adapters for
foreign structs qualify the same way (`syscall_ΔSignalᴠΔSignal`); a LOCAL delegate's value adapter
stays bare (`funcValueᴠValue`). Forwarding decisions come from **metadata** — the compiled foreign
assembly exposes every converter and sibling-generator form as real symbols, so an extension on
`ж<T>` binds the box (`m_box.Read(p)`) and everything else binds the deref'd value
(`m_box.Value.M()`, ref extensions bind through the ref-returning `Value`). This replaces the old
deref-COPY fallback, so aliasing is faithful: fmt's `Fscan(os.Stdin, …)` emits
`Fscan(new os_FileжReader(os.Stdin), …)` (CS1503 ×3, the last fmt family). Guarded by
`CrossPkgUser` (`*Probe → Sampler` via `CrossPkgLib_ProbeжSampler`, mutation read back through the
original pointer).

### A cross-package interface's unexported sealing marker is stubbed
Go seals an interface to its defining package with an **unexported marker method** — `ast.Expr`'s
`exprNode()`, `ast.Stmt`'s `stmtNode()`, `ast.Decl`'s `declNode()`, `text/template/parse.Node`'s
`tree()`/`writeTo()`. The method's C# implementation is an **internal** extension in the interface's
own assembly (`internal static void exprNode(this ref IndexExpr _)`), so an adapter generated where
the interface is CONSUMED — `go/internal/typeparams` casting go/ast's `*IndexExpr` to `ast.Expr`, or
`text/template` casting `*parse.RangeNode` to `parse.Node` — cannot see it: forwarding
`m_box.Value.exprNode()` is CS1061. The C# interface member itself is public (unexported Go methods
render without a modifier), so it is still *required* — dropping it is CS0535, and an *internal*
interface member cannot be implemented cross-assembly at all. Because Go never lets a sealing marker
be called from outside its package, the adapter satisfies the member with a **no-op / `default!`
stub** instead of forwarding:
```csharp
void global::go.go.ast_package.Expr.exprNode() { }                       // void marker
global::go.…parse_package.Tree global::go.…parse_package.Node.tree() => default!;   // non-void marker
```
The `ImplementGenerator` flags a method as an inaccessible marker when its Go name is unexported
(`GetScope == "internal"`) **and** its declaring assembly differs from the one the adapter is
generated into (`MethodInfo.IsInaccessibleMarker`); a SAME-assembly impl keeps forwarding (the
internal extension is accessible there). Both the pointer (`AdapterImplTemplate`) and value
(`ValueAdapterImplTemplate`) adapters emit the stub. This greens `go/internal/typeparams` (whose only
errors were the two `exprNode` forwards) and is a prerequisite for `text/template`/`go/doc`. (Guarded
by `CrossPkgLib`/`CrossPkgUser`: the sealed `Emitter` interface with an unexported `emitNode()`, a
`*Leaf` implementing it, cast to `Emitter` in the consumer assembly — CS1061 without the stub.)

### A dynamic interface's runtime conversion class re-escapes a keyword method name
An anonymous or type-asserted interface is lifted to a `[GoType("dyn")]` partial interface (see
[Anonymous interfaces used as an adapter target](#anonymous-interfaces-used-as-an-adapter-target-are-lifted-package-wide)),
and for the dynamic form `go2cs-gen`'s `InterfaceTypeTemplate` additionally emits a **runtime
conversion class** — `ΔI<ᴛTTarget> : I` — that duck-types a target at run time by reflection-binding
each interface method to the target's extension methods (the fallback for a duck-typed assertion the
compile-time `ImplementGenerator` could not resolve). When such a dynamic interface **embeds** an
interface carrying an unexported *sealing* method whose name is a C# reserved keyword — internal/testenv's
`interface{ testing.TB; Deadline() (time.Time, bool) }`, where `testing.TB` has `private()` — that name
must be `@`-escaped in the generated class. The converter already escapes it in the interface itself
(`void @private();`), but the sealing method reaches the conversion class through the base-interface walk
(`interfaceSymbol.AllInterfaces`), and a symbol name read from Roslyn (`IMethodSymbol.Name`) arrives
**UNescaped** — unlike a syntax `Identifier.Text`. Emitting it raw yields `void private()` and
`nameof(private)`; that syntax error corrupts the class body, and because the conversion class is nested
inside the `public static partial class …_package` container the parse recovery ejects every subsequent
operator into the *static* container — **CS0715** ("static classes cannot contain user-defined operators")
×25 plus a **CS0246** cascade (~54): 84 errors from one keyword method. (The nesting itself is legal — a
non-static class nested in a static class holds instance members and operators fine, as every non-keyword
dynamic interface proves; only the broken body triggers the eject.)

The fix re-escapes the name only where it is emitted as its own identifier token — the method declaration
(`MethodInfo.GetSignature`) and each `nameof(...)` in the reflection-binding static constructor. The
compound delegate/field names (`{Name}ByPtr`, `s_{Name}ByPtr`) stay on the raw name: a keyword + suffix
is never itself a keyword, and `@` cannot appear mid-token. `EscapeCsKeyword` is a no-op for every
non-keyword method, so all other dynamic-interface output is byte-identical. Emitted form:
```csharp
internal class ΔcommandContext_type<ΔTTarget> : commandContext_type
{
    private delegate void privateByPtr(ж<ΔTTarget> targetʗ);              // compound name — raw
    public void @private() { … }                                         // declaration — escaped
    // static constructor:
    extensionMethod = targetType.GetExtensionMethod(nameof(@private));   // nameof — escaped
}
```
Greens internal/testenv (its only errors were this one method's cascade). Guarded by the
`DynamicInterfaceKeywordMethod` behavioral test — a named `TB` interface with a `private()` sealing
method, embedded in a type-assertion's anonymous interface so the lifted `[GoType("dyn")]` target's
conversion class must implement the escaped `@private()`; it does not compile without the fix.

### A keyword-named type's interface adapters escape declarations and compose class names unescaped
A Go **type** whose name is a C# reserved keyword (`type fixed struct{…}`, `type lock interface{…}`) is
`@`-escaped by the converter everywhere it stands as its own identifier token (`[GoType] partial struct
@fixed`, `ж<@fixed>`, `@lock l = f`). Two other name paths mishandled such types:

1. **`ImplementGenerator`'s emitted type positions.** A LOCAL struct's name reaches the generator as a
   bare Roslyn SYMBOL name — UNescaped, unlike display strings (`ToDisplayString()` uses
   `CSharpErrorMessageFormat`, which escapes, so `go.main_package.@lock` arrives correct). Emitting the
   raw name produced `partial struct fixed : sizer` — which the C# parser reads as a *fixed-size-buffer*
   declaration, ejecting mangled members into the static `…_package` container (**CS0708**
   `'main_package.'` "cannot declare instance members in a static class" plus a CS1642/CS1663/CS7092
   buffer cascade) — and the same raw name inside the pointer adapter's `ж<fixed>`. The generator now
   applies `EscapeCsKeyword` at those emission sites (`InterfaceImplTemplate.StructName`, the pointer
   adapter's wrapped `StructName`, and the value-embed hop's class qualifier); it is a no-op for every
   non-keyword name.

2. **Adapter class-name composition, BOTH sides.** `@` is only legal at the START of a C# identifier
   token, so a keyword part cannot carry its marker into a composed adapter name: the converter emitted
   `new @fixedж@lock(Ꮡf)`, which lexes as TWO tokens (`@fixedж` + `@lock` — CS1526). Both composers now
   build from UNESCAPED simple names — the converter's `adapterTypeRef`/`valueAdapterTypeRef` via
   `stripSanitizationMarkers` (which also clears a pre-qualified `os_@fixed`-style interior marker), and
   the generator's `AdapterName` compositions via `GetUnsanitizedIdentifier` — producing
   `fixedжlock`/`fixedᴠlock`. The composed name always contains the `ж`/`ᴠ` infix or a package prefix,
   so it is never itself a keyword and needs no marker (the same rule the keyword-method compound names
   above rely on: a keyword + suffix is never a keyword).

Emitted form (from the `KeywordNamedTypes` goldens and its generated adapters):
```csharp
sizer p = new fixedжsizer(Ꮡf);                          // converter cast site — composed, no marker
@lock lp = new fixedжlock(Ꮡf);

partial struct @fixed : global::go.main_package.@lock   // generator value-form — escaped declaration

internal sealed class fixedжlock : global::go.main_package.@lock, IжAdapter
{
    private readonly ж<@fixed> m_box;                   // escaped type reference
```
`TypeGenerator` and `RecvGenerator` were already correct — they read syntax `Identifier.Text`, which
keeps the `@fixed` spelling. Guarded by the `KeywordNamedTypes` behavioral test: struct `fixed` value-
and pointer-implementing `sizer` plus a keyword-named interface `lock`, with a pointer-receiver `grow`
exercising the RecvGenerator ж-twin on the keyword-named receiver.

### A keyword-named addressed global's heap-box field strips the escape after the Ꮡ prefix
An address-taken package-level var is backed by a heap-box FIELD plus a ref-returning property
(`writeAddressedGlobalDecl`). A keyword-named such global (`var null = json.RawMessage([]byte("null"))`,
net/rpc/jsonrpc) arrives keyword-escaped (`@null`), and composing the box as `Ꮡ` + `@null` places the
escape INTERIOR to the identifier token — `Ꮡ@null` lexes as two tokens (a whole-file syntax cascade).
The `Ꮡ` prefix already de-keywords the composed name (the keyword + affix rule the adapter compositions
above rely on), so the field declaration strips the escape — matching every `&null` use site, which
already composed `Ꮡnull` through `boxBaseName`:
```csharp
internal static ж<slice<byte>> Ꮡnull = new(slice<byte>((@string)"null"));
internal static ref slice<byte> @null => ref Ꮡnull.ValueSlot;   // the var itself keeps its escape

var p = Ꮡnull;                                                  // use site, unchanged
```
Guarded by `HeapKeywordVar` (a package-level `var null` written through its pointer and read back both
ways), alongside its existing keyword-named LOCAL coverage.

### A foreign struct's promoted method forwards through its value embed
When the adapter's struct is FOREIGN (defined in another assembly) it binds forwarding from METADATA
(the boxBound / refBound scan above); a member neither on its box nor a ref-static falls to
`m_box.Value.M()`. But an interface member the foreign struct PROMOTES through a VALUE-embedded field
has no extension on the struct's OWN package class, so `m_box.Value.M()` is CS1929 — `text/template`
casting `*parse.RangeNode` to `parse.Node`, where `RangeNode` embeds `BranchNode` and the exported
`String` lives on `BranchNode`, not `RangeNode`. The generator now discovers the foreign struct's
value embeds from metadata (`GetForeignValueEmbeds`: a member whose name equals its type's simple
name) and, for a still-unbound member the embed's package class declares as a public value/ref-receiver
extension (`GetForeignValueReceiverMethods`), forwards through the embed's package-class STATIC:
```csharp
global::go.@string global::go.…parse_package.Stringer.String() =>
    global::go.…parse_package.String(ref m_box.Value.BranchNode);
```
The static form is required because the embed's namespace is not imported in the adapter file (only
`using go;`), so an instance-form `m_box.Value.BranchNode.String()` cannot resolve the extension —
exactly as the foreign struct's own extensions route through `staticClass`. The receiver argument
carries the extension's ref-kind (`ref`/`in`/value). Rerouting is gated to a genuinely promoted member
(the struct binds it neither directly nor via a box/ref hop), so a struct that declares the method
itself is unaffected. This clears `text/template`'s last CS1929. (Guarded by `CrossPkgLib`/`CrossPkgUser`:
`*Branch`, which promotes the exported `Emit` through its `EmitBase` value embed, cast to `Emitter` —
CS1929 without the reroute.)

### A promoted box-receiver method through an UNEXPORTED value embed is called cross-package via a public forwarder
An EXPORTED, pointer-receiver method that takes the address of a receiver field is emitted as a
**direct-ж (box-receiver) primary** `M(this ж<T> …)`. When such a method is *promoted* through an
**unexported** VALUE embed — `testing.T.Errorf`, promoted from the embedded `common` (`type T struct{
common; … }`), or go/types' `TypeName`/`Var`/`Func`, which embed `object` — an IN-PACKAGE caller
renders the descent through the embed's box-field accessor (see *Promoted pointer methods descend
multi-hop value-embed chains* above):
```csharp
Ꮡt.of(testing.T.Ꮡcommon).Errorf("…"u8, …);   // in-package
```
`Ꮡcommon` is the `TypeGenerator`'s `FieldReferences` box accessor for the embed, and — matching the
embed's unexportedness — it is `internal`. So a caller in **another package/assembly** (crypto/internal/
cryptotest, testing/slogtest, x/net/nettest, go/internal/gcimporter) cannot see it: a cross-assembly
reference to an `internal` member reads as **CS0117** ("`testing_package.T` does not contain a definition
for `Ꮡcommon`"), not CS0122. Every path through the unexported embed (`common`, `Ꮡcommon`, its promoted
members) is `internal`, so no converter-only descent can reach it. The fix is two-sided:

- **`go2cs-gen` (`StructTypeTemplate`)** — for a direct, non-generic, **unexported VALUE embed**, harvest
  the embed's box-receiver primaries (`GetBoxReceiverExtensionMethods`, previously collected only for
  POINTER embeds) and, for each **exported** one, emit a single box-only shim (`IsValueEmbedBoxRecv`) that
  performs the descent internally, where the `internal` accessor is reachable:
  ```csharp
  public static void Errorf(this ж<T> Ꮡtarget, @string format, params Span<object> argsʗp)
      => Ꮡtarget.of(T.Ꮡcommon).Errorf(format, argsʗp);
  ```
  No `this ref T` overload (a box receiver cannot bind on a value). The shim scope is the shared
  `methodScope` — the STRUCT's exportedness, downgraded for a non-public return type — so it is `public`
  only for an exported method on an EXPORTED struct returning void/public (the genuinely reachable case),
  and `internal` on an UNEXPORTED enclosing struct (context's `afterFuncCtx`, reflect's
  `structTypeUncommon`), whose `ж<T>` receiver is itself internal — a `public` shim there is CS0051. It is
  gated to an exported promoted method (an unexported one is never reachable across packages, so it needs
  no shim; its in-package callers keep the inline descent). The value embed is discriminated by
  `!promotedStructType.Contains("<")` (a plain value embed's type name never carries `<`, whereas the
  pointer-box form `ж<…>` and generic embeds do) — a more robust test than the `@`-keyword-escaped
  `pointerEmbedTypeNames` membership, whose `ж<@file>`-shaped names mismatch and mis-fired the shim onto
  os.File's `*file` POINTER embed (a stray `File.Ꮡfile.Value`, CS0119).
- **the converter (`convSelectorExpr`)** — when the promoted-method descent is reached through an
  unexported embed of a FOREIGN package (single hop), it drops the inaccessible `.of(…)` view and calls
  the promoted method DIRECTLY on the receiver box, binding the public shim:
  ```csharp
  Ꮡt.Errorf("…"u8, …);   // cross-package (Ꮡt for a deref'd param, tΔ1 for a lambda box param)
  ```
  The box is recovered from the first-hop `&embed`-address the `&`-machinery already computes (the text
  before its last `.of(`), so it is correct for every receiver kind without re-deriving it.

(Guarded by `PromotedEmbedLib`/`PromotedEmbedUser`: `Counter` value-embeds an unexported `common` whose
exported `Add`/`Report` take `&c.sum` (box-receiver); the user package calls them on a `*Counter` local
and through a parameter, plus reads the exported `Label` field for contrast — output-compared vs Go.)

**Plain-return-type addendum — a PLAIN (non-box) promoted method returning a public builtin.** The
box-shim above covers a method emitted as a `ж<T>` primary (it takes `&receiver.field`). A method that
merely READS a field — `testing.common.Name()` (`func (c *common) Name() string { return c.name }`) — is
emitted as an ordinary `Name(this ref common)` extension, so the promotion machinery emits the usual
value + box forwarders `Name(this ref T)` / `Name(this ж<T>)` with body `target.common.Name()`. But their
scope was downgraded by the RETURN type: `@string` (and `error`, `bool`, `nint`, … — every golib builtin)
is a PUBLIC C# type whose Go-lowercase name the name-based `GetScope` heuristic reads as unexported, so
the forwarder was emitted `internal` and thus invisible cross-assembly. Cross-package the converter emits
the same bare `Ꮡt.Name()` (the foreign-unexported-value-embed arm fires for EVERY promoted
pointer-receiver method, not just box ones), which then bound a same-named FOREIGN extension —
`x/net/nettest`'s `timeoutWrapper` reads `t.Name() == "…"`, and the only visible `Name` was
`flag.Name(ref flag.FlagSet)` (flag is imported by testing) → **CS1929**. The fix keeps the forwarder
public when its return type is GENUINELY accessible: `go2cs-gen` captures the return type's ACTUAL C#
accessibility (`MethodInfo.ReturnTypeIsPublic`, computed by `IsEffectivelyPublicType` — the type and
every type argument / tuple element / array-or-pointer element is `public`, treating builtin special
types and use-site-bound type parameters as public) and, for the direct-unexported-value-embed case,
trusts it over the lowercase name (`directEmbedIsUnexportedValue && method.ReturnTypeIsPublic`):
```csharp
public static @string Name(this ж<T> Ꮡtarget) { ref var target = ref Ꮡtarget.Value; return target.common.Name(); }
```
Every OTHER promotion keeps the conservative name heuristic (so no golden/compile churn), and an
UNEXPORTED enclosing struct still yields an internal forwarder (its `ж<T>` receiver is internal — a public
forwarder there is CS0051, and my change only prevents a downgrade below the struct's own scope). This
greens `x/net/nettest` (census 271 → 272/302, zero regressions). (Guarded by `PromotedValueEmbedLib`/
`PromotedValueEmbedUser`: `Widget` value-embeds an unexported `common` whose plain `Name() string` is read
in an expression cross-package, alongside an unrelated `Gadget.Name()` — the foreign same-named extension
— output-compared vs Go; CS1929 without the fix.)

**Pointer-expression-receiver addendum.** The converter arm above recovers the box from the `.of(…)`
strip of the first-hop `&embed` address — which assumes the receiver has an addressable base (an
ident: a raw-box local, a deref'd param's `Ꮡx`). A pointer receiver **expression** — a type-assert or
call chain like go/internal/gcimporter's `pkg.Scope().Lookup(name).(*types.TypeName).Type()` — has no
such base: the `&`-machinery boxes a COPY (`Ꮡ(x.@object)`, no `.of(` anywhere), so the arm silently
fell through to the spelled embed hop, `internal` cross-assembly (**CS1061**). A follow-up sub-arm
recognizes a pointer-typed receiver expression that renders as the raw box (pointer-typed and not
deref-aliased) and calls the promoted member straight on it — the box IS the receiver:
```csharp
pkg.Scope().Lookup(name)._<ж<types.TypeName>>().Type();   // binds the public Type(this ж<TypeName>)
```
(Guarded by `PromotedValueEmbedExprRecv`: the promoted `Name()` called on a `map[string]any`
assert-chain receiver and on a constructor-call receiver, output-compared vs Go; CS1061 without the
fix.)

### Cross-package value-to-interface conversions use the local VALUE adapter
A VALUE conversion of a FOREIGN named type to a LOCAL interface (os's `Signal` interface is
DOWNSTREAM of `syscall.Signal` — neither assembly can partial the other) records
`GoImplement<foreign, localIface>` locally; the `ImplementGenerator` detects the foreign struct
(different containing assembly, no local declaration) and emits a **value adapter class**
`{pkg}_{Struct}ᴠ{Iface}` (composed with `Symbols.ValueAdapterInfix`; package-qualified for a
FOREIGN struct — see the pointer-adapter collision note above) wrapping a **COPY** of the struct
— exactly as a Go interface holds a value — with value equality. The conversion site emits
`new syscall_ΔSignalᴠΔSignal(sig)`. The adapter's struct field is **fully qualified**
(`GetFullTypeName(true)`): the bare name resolved to the LOCAL same-named type when os's
`ΔSignal` interface shadowed syscall's `ΔSignal` struct.

Method forwarding uses the **container-qualified static form** —
`global::go.encoding.binary_package.Uint32(m_value, b)` rather than `m_value.Uint32(b)`:
converted Go methods are extension methods on the package class the struct nests in, and the
instance form only resolves when the generated file has a `using` for that namespace (`using go;`
covers root-namespace packages like io/os, but a sub-namespace package like `encoding/binary`
never resolved — debug/plan9obj CS1061 ×6). The static form is exactly equivalent and needs no
using at all.

**BOTH-FOREIGN value pairs take the same route.** When the interface is foreign too
(debug/plan9obj passes `binary.BigEndian`, an `encoding/binary` value, as `binary.ByteOrder`),
the converter first consults the imported package_info records (`parseExportedValueImplements`,
plain or `Promoted` `GoImplement` forms): if the defining assembly already implements the pair,
the bare value converts implicitly and nothing is recorded. Otherwise the pair is recorded
locally and the conversion site wraps in the locally generated value adapter
(`new binary_bigEndianᴠByteOrder(binary.BigEndian)`) — the value sibling of the both-foreign pointer
adapter above.

### An exported func type publicizes the unexported types in its signature
An EXPORTED named func type becomes a `public` C# delegate; an unexported type in its signature —
x/text/unicode/bidi's `type Option func(*options)`, where `options` is package-private — is then
less accessible than the delegate (CS0059, "inconsistent accessibility"). The type-accessibility
pass, which already publicizes the unexported types exposed by an exported struct field / package
var / method signature, also walks an exported named type whose underlying is a `*types.Signature`
and publicizes the unexported named types in its parameters and results:

```go
type options struct{ … }        // unexported
type Option func(*options)       // exported -> public delegate
```
```csharp
[GoType] public partial struct options { … }   // publicized to match the delegate
public delegate void Option(ж<options> _);
```

Only a package with an exported func type over an unexported type is affected (no golden churn). (Guarded by the `PublicizedFuncTypeParam` behavioral test.)

**A func-TYPED exported field or var publicizes the unexported types in the func signature.** The
accessibility walk that publicizes an unexported type exposed by an exported field / package var
(`collectUnexportedNamedTypes`, CS0052) peels `pointer`/`slice`/`array`/`map`/`chan` wrappers to reach
the element type — but stopped at a `*types.Signature`, so an unexported type reachable ONLY through a
func-typed field's signature was left `internal`. crypto/internal/hpke's

```go
type hkdfKDF struct{ … }                          // unexported
var SupportedKDFs = map[uint16]func() *hkdfKDF{…}  // exported var -> public field
```

emits `public static map<uint16, Func<ж<hkdfKDF>>> SupportedKDFs`, whose type embeds `hkdfKDF` through
the func RESULT — but `[GoType] partial struct hkdfKDF` defaulted to `internal`, less accessible than
the public field (CS0052). `collectUnexportedNamedTypes` now has a `*types.Signature` case that recurses
into the signature's PARAMS and RESULTS through the same named-only walk (which handles a nested func
result in turn), so `hkdfKDF` is publicized to `[GoType] public partial struct hkdfKDF` (and its exported
methods go public via the receiver-access cascade). Both sides of the signature are covered — a func
PARAMETER exposes an unexported type just as a func RESULT does (`var Appliers = []func(*cfg)` →
`public static slice<Action<ж<cfg>>> Appliers`, publicizing `cfg`). This routes through the named-only
`collectUnexportedNamedTypes`, NOT the signature-context `collectSignatureTypes`: a lifted anonymous
struct/interface written in the func signature stays the CS0050/CS0051 signature domain, so only genuinely
func-reachable NAMED types publicize here. (Guarded by the `FuncFieldUnexportedType` behavioral test — a
public `map[uint16]func() *hkdfState` var whose func result exposes an unexported type, plus a
`[]func(*cfg)` var whose func parameter exposes another, output-compared vs Go; both fail CS0052 without
the publicize.)

**A publicized wrapper reaches through an UNNAMED composite RHS to its element type.** A defined type whose `[GoType]` wrapper is emitted `public` (exported, or unexported-but-publicized) exposes its written RHS through the wrapper's `Value`/ctor/indexer/operators, so an unexported RHS type must be publicized too. This holds not just for a NAMED RHS (`type EncoderBuffer encoder`) but for an UNNAMED composite RHS whose ELEMENT is an unexported named type: `type ringElement [256]fieldElement` exposes `fieldElement` through the array-wrapper's indexer/`Value`/`ToSpan`, so `fieldElement` must be publicized (crypto/internal/mlkem768, CS0050/CS0051/CS0053/CS0054/CS0056/CS0057). `collectPublicizedWrapperRHS` therefore feeds the RHS unconditionally to the pointer/slice/array/map/chan-peeling walk (`collectUnexportedNamedTypes`) rather than gating on a named RHS. The walk has no `*types.Struct` case, so a struct RHS stays a no-op — an exported field of an unexported struct-field type is the CS0052 domain and is intentionally left internal. (Guarded by the `NamedArrayWrapper` extension — an exported `Grid [3]unit` over an unexported `unit`, output vs Go.)

### A publicized unexported interface is emitted `public`
The accessibility pass records an unexported **interface** used in an exported surface exactly like a
struct or func type — testing's `type testDeps interface { … }` reached through `func MainStart(deps
testDeps, …) *M` is interned into `packagePublicizedTypes`, and `visitTypeSpec` sets
`pendingTypeAccess = "public "`. But on the EMISSION side, every top-level type-kind emitter consumes
`v.pendingTypeAccess` (struct, array, map, ident, the inline selector/star cases) *except*
`visitInterfaceType`, which dropped it — so the interface always emitted `[GoType] partial interface
testDeps`, defaulting to C# `internal`, less accessible than the `public` member that references it
(CS0051). `visitInterfaceType` now reads-and-clears `pendingTypeAccess` at entry (so the lifted/anonymous
interfaces it visits recursively see an empty value) and folds the modifier into the post-attribute slot,
emitting `[GoType] public partial interface testDeps`. Non-publicized interfaces are unchanged (no churn).
(Guarded by the `PublicizedInterfaceParam` behavioral test — an exported function taking an unexported
interface whose method returns a built-in type, output-compared vs Go.) The **transitive** cascade also
walks a publicized interface's method signatures: the `collectMethodSignatureUnexportedTypes` fixpoint step
walked a type's `named.NumMethods()` (declared receiver methods) but that is **0 for a defined interface**
— an interface's methods live on its underlying `*types.Interface`. It now also iterates
`iface.NumMethods()` for a publicized interface, so an unexported NAMED type in a public interface member's
parameter/result signature is publicized in turn (CS0051/CS0050).

A public callable's signature can also reference a **lifted anonymous** type, which the NAMED-only cascade
above cannot reach — testing's `testDeps.CoordinateFuzzing(… corpusEntry …)` / `RunFuzzWorker` / `ReadCorpus`,
where `type corpusEntry = struct{…}` is an ALIAS to an anonymous struct. The signature type is not a
`*types.Named` but a lift (`corpusEntryᴛ1`), a synthesized name over a raw `types.Type` with no
`*types.Object`, so `packagePublicizedTypes` (keyed by object) cannot hold it. A parallel set
`packagePublicizedLiftedTypes` (keyed by the alias-stripped anonymous `types.Type`) fills the gap: a
SIGNATURE-context walker `collectSignatureTypes` — used by the exported-func, exported named-func-type, and
method/interface-method signature paths — records any lifted anonymous struct/interface it reaches, and the
lift emission in `visitStructType` consults `isPublicizedLiftedType` and emits `public`. This is deliberately
**signature-scoped** and does *not* fold into the shared named-only `collectUnexportedNamedTypes`: an exported
**field/var** of an anonymous struct is the CS0052 domain (a public struct/var over an internal anon field
type is legal while its own enclosing type is internal), so only signature positions lift — keeping golden
churn to the one genuinely-affected shape. (Guarded by the `PublicizedInterfaceAnonAlias` behavioral test — an
unexported interface publicized through an exported function, whose method both takes and returns a
`type = struct{…}` alias, output-compared vs Go; it fails to compile with CS0050/CS0051 without the lift
publicize.)

### Publicized unexported types make their exported methods public
An unexported Go type reachable through an exported surface (an exported var — `var BigEndian
bigEndian` — an exported field, or an exported function's signature) is emitted `public`
(`packagePublicizedTypes`, CS0052/CS0050). Its **exported methods** must then be public too —
Go callers hold such values through the exported var and call the methods cross-package, but the
receiver-based access rule alone rendered them `internal` (extension methods invisible outside
the assembly: `binary.BigEndian.Uint32(...)` CS1061). The receiver-access checks in
`visitFuncDecl` treat a publicized receiver as public, and `collectPublicizedTypes` **cascades**
through the publicized types' exported method signatures to a fixpoint (a newly public method's
unexported parameter/result types get publicized in turn, or the public method would be CS0050).
Unexported methods stay `internal` regardless.

### Structural interface satisfaction emits C# interface inheritance
Go converts `fs.File` to `io.Reader` implicitly because the method set suffices; C# interfaces
are nominal. When a declared interface's method set **strictly contains** an EXPORTED method
interface from a **directly imported** package (checked with `types.Implements`), the converter
emits real C# inheritance at the declaration and **skips re-declaring the covered members**
(redeclaring would HIDE the base member — implementers would need both):

```csharp
[GoType] partial interface File :
    io_package.ReadCloser
{
    (FileInfo, error) Stat();
}
```

Every downstream interface-to-interface conversion then becomes an implicit reference
conversion — identity-preserving (the dynamic value flows through type asserts, unlike an
adapter wrapper) and zero-cost (os's `CopyFS` passes an `fs.File` to `io.Copy`, CS1503).
Details: only the **minimal covering set** is listed (`ReadCloser` subsumes `Reader`/`Closer`);
the strict-subset guard rules out inheritance cycles (equal method sets never inherit);
candidates covered by a declared **embed** are skipped (the embed emission handles those);
bases reference the **file-local package alias** (`io.ReadCloser`, user-ruled style) via
getTypeName, which also registers the using — needed because the declaring Go file may not
import the candidate's package (`fs.go` declares `File` without importing `io`); lifted/dyn
and constraint interfaces are excluded. **Multiple non-subsuming bases sharing a method**
(`CrossPkgLib.Sealed` and `.Rated` both carry `Label`): both are inherited, and the shared
member is **re-declared** — a member covered by exactly one listed base is inherited/skipped,
but one covered by two or more is re-declared so it hides both inherited slots and member
lookup through the derived interface stays unambiguous (CS0121). Go needs only one method to
satisfy all; the C# implementers satisfy every slot with the same public method. Consequently the converter **never records an interface-to-interface
`GoImplement`** — the generator's impl types are structs, and an interface-typed record kills
its whole run. Bounds (banked): candidates come from direct imports only — same-package
structural pairs, the universe `error`, and non-imported-package pairs would still surface as
compile errors and would need the adapter complement. Guarded by `CrossPkgUser` (`namedLabel :
CrossPkgLib_package.Labeled`, passed to `CrossPkgLib.Describe`).

### Embedded-pointer hop receivers split per method
An interface member satisfied by promotion through an embedded POINTER field forwards through
the hop — but the receiver form depends on the target method: a `[GoRecv]` ref extension (or
struct method) binds the deref'd value (`this.File.Value.Name()`), while a **direct-ж primary**
(an extension on `ж<X>` emitted when the receiver escapes — os's `File.Read`/`Write`) binds the
box FIELD itself (`this.File.Read(p)`; deref'ing first strands the receiver, CS1929). The
generator discriminates by scanning the compilation for `this ж<X>` extensions — only
converter-emitted primaries are visible to the single-pass scan (sibling-generator ж-twins are
not), which is exactly the needed split. Applied to both the value-form partial and the pointer
adapter's hop arm. Guarded by `StructPointerPromotionWithInterface` (`Describer` over
`deviceHandle{*Device}`).

### A forwarded multi-value call deconstructs when tuple elements need interface conversion
`return newRawConn(f)` forwards a `(*rawConn, error)` tuple into a `(syscall.RawConn, error)`
result list — C# tuple conversions do not consult user conversions element-wise (CS0266). The
converter deconstructs into temps and converts each element through the usual interface
machinery (which also records the `GoImplement` pairing):

```csharp
var (ᴛ1, ᴛ2) = makeRelay();
return (new relayжReporter(ᴛ1), ᴛ2);
```

Elements whose actual type is itself an interface are left alone (structural inheritance
covers those). Guarded by `CrossPkgUser` (`getReporter` forwarding `makeRelay`).

### A multi-value call spread into a call's parameters in an assignment hoists into temps
Go lets a MULTI-VALUE call fill the parameters of an enclosing call — `r := t.newRange(t.parseControl("range"))`,
where `parseControl` returns five values feeding `newRange`'s five parameters. C# has no splat, so the inner
call is deconstructed into markers and passed expanded:

```csharp
var (ᴛ6, ᴛ7, ᴛ8, ᴛ9, ᴛ10) = Ꮡt.parseControl("range"u8);
var r = Ꮡt.newRange(ᴛ6, ᴛ7, ᴛ8, ᴛ9, ᴛ10);
```

`convExprList` already performs this expansion, but only when the call's `deferredDecls` hoist target is
non-nil — passing the whole tuple as one argument is otherwise CS7036 (text/template/parse's `rangeControl`).
The return-form threads that target (visitReturnStmt); the assignment forms do too, on BOTH lowering
branches: the single-declare block and the mixed/escaping block (a pointer-result local that is heap-boxed is
not counted in `declaredCount`, so it takes the latter — the `newRange` case above). A **statement-level**
`f(g())` (a bare expression statement, not an assignment) carries no `deferredDecls` of its own, so the
expansion now falls back to the enclosing `ExprStmt`'s `v.hoistedDecls` buffer — testing's
`registerCover2(deps.InitRuntimeCoverage())`, where `InitRuntimeCoverage` returns three values:

```csharp
var (ᴛ1, ᴛ2, ᴛ3) = deps.InitRuntimeCoverage();
registerCover2(ᴛ1, ᴛ2, ᴛ3);
```

The hoisted `var (…) = …;` lands in the statement's existing hoist buffer, emitted before the statement.
Byte-identical corpus-wide except where the pattern occurs (and a harmless renumber of any later temps, since
the per-file marker index is monotonic). Guarded by `TupleSpreadIntoCall` (a value result, an escaping pointer
result, and a statement-level spread).

A **PACKAGE-LEVEL var initializer** has no statement sink at all — `var debug = template.Must(
template.New("RPC debug").Parse(debugText))` (net/rpc debug.go; also internal/trace/traceviewer) passed the
whole `(ж<Template>, error)` tuple as `Must`'s one argument (CS7036). There the spill becomes a hidden
once-evaluated static tuple FIELD (`v.globalDeclHoist`, flushed by visitValueSpec before the var's own
field — C# static field initializers run in textual order, the same holder shape `visitPackageTupleVarSpec`
emits for `var a, b = f()`), and the arguments read its components:

```csharp
internal static (nint, nint) tupleᴛ1ʗ = parts();
internal static nint g = combine(tupleᴛ1ʗ.Item1, tupleᴛ1ʗ.Item2);
```

Guarded by the `TupleSpreadIntoCall` extension (a package-level `var` spreading a two-value call into a
wrapping call, value read back in main).

### A range over a pointer-typed type conversion parenthesizes before the deref
Ranging over a pointer to an array implicitly dereferences it — the converter appends `.Value` to the
range expression. When the range expression is itself a pointer-typed TYPE CONVERSION it renders as a C#
cast (`(ж<array<byte>>)(uintptr)(p)`, crypto/internal/nistec's p256 init over
`(*[43*32*2*4][8]byte)(*p256PrecomputedPtr)`). A cast binds LOWER than member access, so a bare append
`(ж<…>)(p).Value` parses as `(ж<…>)((p).Value)` — the deref lands on the operand, not the cast result
(CS1579 "no GetEnumerator" on the box type, CS8130). `visitRangeStmt` now wraps the range expression in
parentheses — `((ж<…>)(p)).Value` — whenever the pointer-unwrap deref is active and `rangeStmt.X` is a
`*ast.CallExpr` whose `Fun` is a type expression (`info.Types[Fun].IsType()`, which catches the
unsafe.Pointer conversions `isTypeConversion` deliberately excludes). Byte-identical corpus-wide (the
pattern only occurs on a pointer-producing conversion in range position, which never compiled before).
Guarded by `RangePointerArrayConversion` (transpile+compile+target only — the exact cast shape needs an
`unsafe.Pointer` source, whose runtime round-trip golib does not reproduce, so it is not output-compared).

### Adapter accessibility: symbol-OR-name on both sides
The adapter class scope cannot be derived from Go name casing alone (`error` is lowercase yet the golib interface is public METADATA - the name rule made io/fs's PathErrorжerror internal, CS0122 x40) nor from symbols alone (sibling generators' `public partial` modifiers are invisible to a single-pass generator - the symbol rule broke same-assembly interfaces like `CrossPkgLib.Reporter`). The ImplementGenerator takes symbol-OR-name on the struct AND the interface.

### GoImplement records de-duplicate at attribute emission
os converts dirEntry to fs.DirEntry both through its own alias (`type DirEntry = fs.DirEntry`) and through the io/fs name - two records for ONE interface made the generator emit the explicit implementation twice (CS8646/CS0111). The de-duplication happens at ATTRIBUTE EMISSION with the ALIASED record winning (its simple name resolves via the package usings); normalizing the RECORD KEY instead was twice wrong - qualified attr names break generator name resolution and flip the alias-locality gate. **Measurement lesson:** those declaration-phase errors had SUPPRESSED all of os's method-body diagnostics (Roslyn phase gating) - a package is not truly measured until its declaration errors are zero.

**The interface-inheritance PRUNE exempts pairs that generate their own adapter CLASS.** The same
attribute-emission stage also drops a "lower" GoImplement record when the SAME implementing type is
recorded against a derived interface that C#-inherits it (elf's errorReader against both io.ReadSeeker
and io.Reader — the two value-form partial-struct implementations would implement `Read` twice,
CS0111/CS8646). That prune is only valid for the value-boxing PARTIAL-STRUCT form (one type, one
interface list). A pair whose implementation is a DISTINCT generated adapter class must survive, since
each cast site references the adapter for the EXACT interface it targets — the ж<T> pointer form was
already exempt, and the same now holds for the value-form adapter classes (`<src>ᴠ<iface>`): an
**interface-sourced** conversion (net/http wraps `net.Conn` values as `io.Reader`/`io.Writer` — the
prune dropped both pairs under the also-recorded `Conn→ReadWriteCloser`, so every
`new net_ConnᴠWriter(…)` referenced a class the generator never emitted, CS0246 ×17 in net/http and
recurring in net/rpc and httputil) and a **foreign-struct value** conversion
(`<pkg>_<T>ᴠ<iface>`) are marked at recording time (`adapterClassImplementations` in
`convertToInterfaceType`) and skipped by the prune. (Guarded by `IfaceToIfaceNarrow` — one source
interface converted to a full-surface embedded-interface target AND to its narrower bases at
argument, assignment, and return positions, dispatch output-compared vs Go.)

### Anonymous interfaces used as an adapter target are lifted package-wide

An inline anonymous interface used as a `GoImplement` target — internal/trace's `readBatch(r
interface{io.Reader; io.ByteReader})`, whose concrete `*bufio.Reader` argument is cast to the
inline interface — must resolve to a NAMED C# type on every side, or the raw Go structural
literal is emitted into the `package_info.cs` assembly attribute and into the adapter class
name (`bufio_ReaderжByteReader}` — the stray `}` breaks the C# parse and cascades ~75 syntax
errors across the file). `visitInterfaceType` already lifts the inline interface to a named
type (`readBatch_r`) in the visitor's per-file `liftedTypeMap`, but a cast at a DIFFERENT
file's call site (generation.go) has its own visitor and its own map, so `convertToInterfaceType`
saw only the raw `*types.Interface` and emitted the literal.

The lift is now also recorded in the package-level `packageDynamicTypeNames` registry — for
FUNCTION-scoped lifts too, since a function-parameter anon interface hoists to file level and is
referenced cross-file — exactly as anonymous structs already register (`visitStructType`).
`convertToInterfaceType` resolves an anonymous `*types.Interface` through the same three steps
`dynamicStructTypeName` uses: this file's `liftedTypeMap`, then the shared registry, then a
deferred `«DYNTYPE:…»` marker resolved after the file-visit barrier. The marker survives the
adapter-name composition (`adapterTypeRef`/`valueAdapterTypeRef` skip the simple-name strip when
it is present — the embedded signature contains dots), and the `GoImplement` attribute writer
resolves or drops it (mirroring the implicit-conversion writer). Because file visits are
concurrent, `registerDynamicTypeName` keeps the lexically smallest name for a signature so the
result is deterministic. Emitted form:
```csharp
// batch.cs (declaring file):
[GoType("dyn")] partial interface readBatch_r : /* io.Reader */ … { … }
// generation.cs (cross-file cast site):
(b, gen, var err) = readBatch(new bufio_ReaderжreadBatch_r(Ꮡr));
// package_info.cs:
[assembly: GoImplement<bufio_package.Reader, readBatch_r>(Pointer = true)]
```
Clears internal/trace's 75-error syntax cascade (the residual CS0315 — a named-numeric wrapper
not satisfying a lifted operator constraint — is a distinct, deeper root). Guarded by
`AnonInterfaceCrossFile` (a two-file package: file A declares `describe(thing interface{ Sizer;
Namer })`, file B casts a concrete `*box` to it — the lifted name must flow into the attribute,
the adapter, and the signature).

### Function-literal parameters share the body scope
Go declares parameters in the function block, so a body-level `fpath, err := ...` REUSES a literal's `err` parameter. The variable analysis gives literals ONE merged scope (params + body declarations) mirroring real function declarations; a separate param scope had made the `:=` a shadow declaration beside later reuses (CS0841/CS0128, os CopyFS's WalkDir literal). Guarded by `LambdaFunctions` (`probe`).

### System-colliding local type names are root-qualified in assembly attributes
A Go package can name one of its own exported types after a top-level C# `System` type — internal/profile's `ValueType`, go/ast's `Object`, bytes' `Buffer`. The `GoImplement`/`GoImplicitConv` assembly attributes generated in `package_info.cs` sit at **file scope**, before the `namespace` line, where both `using System;` (a csproj global using) and `using static go.<pkg>_package;` are active — so a bare `ValueType` is ambiguous between `System.ValueType` and the package type (CS0104). The emitter root-qualifies any bare, dotless type name matching a curated set of `System` top-level names at the package class:

```csharp
[assembly: GoImplement<go.@internal.profile_package.ValueType, message>]
[assembly: GoImplicitConv<go.@internal.profile_package.ValueType, ж<go.@internal.profile_package.ValueType>>(Indirect = true)]
```

Foreign types are always package-qualified already (dotted) and are left untouched; no non-colliding name changes, so every non-colliding attribute emits byte-identically. (Guarded by the `SystemCollidingTypeName` behavioral test.)

## Pointers
Pointer conversions use the golib heap box [`ж<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/%D0%B6.cs) (read "zhe"). Taking the address of a value uses the address-of operator `Ꮡ` (e.g. `Ꮡx`); an escaping local is allocated via `heap(...)`, and addresses of a struct field or array element are taken through `.of(Type.ᏑField)` / `.at<T>(index)`.

The box's value accessors follow one naming scheme (unified 2026-07-02; the checked accessor was previously `val`): **`Value`** is the strict dereference (`ref`-returning; panics on a nil pointer, as Go does), **`ValueSlot`** is its no-check twin (the identical real slot — for reads/writes of a held value that may legally be nil), and **`DerefOrNil()`** is the null-box-tolerant extension used by nil-terminated walks (an *extension method* is the only ref-returning form C# permits on a possibly-null receiver; it returns a throwaway slot when nil). The same `Value` name is used by the generated named-type wrappers for their underlying-value accessor and by the golib `uintptr` struct for its raw word — converted code has exactly one spelling for "the value behind this thing". A Go struct **field** named `val` still emits as `.val` (it is the user's identifier, not the accessor):

```csharp
ref var a = ref heap(new array<@string>(2), out var Ꮡa);  // escaping local
var p = Ꮡa.at<@string>(0);                                 // &a[0]
var pField = Ꮡsettings.of(settingsᴛ1.ᏑRetries);            // &settings.Retries
```

**A heap-boxed *range variable*** needs the box allocated **per iteration**. When a `for i := range s` (or `for _, f := range s`) variable has its address taken, it escapes — but the foreach already declares that name, so a single `ref var i = ref heap(…)` before the loop would clash (CS0136). The converter iterates a *temp* and, inside the body, allocates a fresh box each pass and copies the temp into it:

```go
for i := range s {
    p := &i      // i escapes
    use(p)
}
```
```csharp
foreach (var (iᴛ1, _) in s) {
    ref var i = ref heap(new nint(), out var Ꮡi);   // a FRESH box each iteration
    i = iᴛ1;
    var p = Ꮡi;
    use(p);
}
```

The per-iteration box is required for Go 1.22 loop-variable semantics: each iteration's variable is distinct, so a stored `&i` must point to a different box each pass (`for i := range s { ptrs = append(ptrs, &i) }` yields `0 1 2`, not `2 2 2`). A non-escaping companion variable still declares directly in the foreach. (Guarded by the `RangeVarHeapBox` behavioral test — both a within-iteration `&i` and the stored-pointer distinctness case; runtime exercises it in `for i := range stackpool` and `for _, f := range s.Fields`.) A heap-boxed `for i := …; cond; post` **clause** variable takes the same per-iteration box through its carrier rewrite — see [*For-clause variables are per-iteration*](#for-clause-variables-are-per-iteration-go-122-loop-variable-semantics).

The `at<T>(index)` element-address accessor takes a `nint` index. Go permits **any** integer type as an array/slice index and converts it to `int` for the access, but C# has no implicit `nuint`/`uint`/`ulong`→`nint` conversion, so a non-`int` index is narrowed explicitly to match Go's index-to-int conversion (CS1503 otherwise). An `int` index, or an untyped int constant (which renders as a plain int literal), is emitted as-is:

```csharp
var pi = Ꮡa.at<nint>((nint)(i));        // &a[i]      where i is a uintptr
var pe = Ꮡa.at<nint>((nint)(g % 2));    // &a[g%2]    where g is a uint (g%2 widens to long in C#)
```

This is the element-address analogue of the indexed-literal key cast (`SparseArray<T>`, above) and the `IBinaryInteger<T>` width-agnostic length params on `unsafe.Add`/`Slice`/`String`. (Guarded by the `ArrayWideIndexAddress` behavioral test.)

A **string** indexed by a wide/unsigned integer takes the same `(int)` cast: a string LITERAL renders as a `ReadOnlySpan<byte>` (`"…"u8`) whose indexer takes `int`, so a `uintptr` index is CS1503 — runtime `heapdump.go`'s `"0123456789abcdef"[pc&15]` emitted `"…"u8[(uintptr)(pc & 15)]`. The index-expression emission routes a wide-kind index on any string-typed base through the cast — `"…"u8[(int)((uintptr)(pc & 15))]` — and an `@string` *variable*'s indexer binds an `int` argument too, so both renders are covered; an int/small index is unchanged. (Guarded by the `ArrayWideIndexAddress` extension — literal and variable string bases with `uintptr`/`uint64` indexes, byte values vs Go.)

The address of a **slice** element uses the call form `Ꮡ(slice, index)` (golib overloads `Ꮡ<T>(IArray<T>, int)` and `(…, nint)`) rather than `at<T>`. Go `int` (→ `nint`) and the small integer types that implicitly widen to `int` bind directly, but an unsigned-32-or-wider or 64-bit index (`uint`/`uint32`/`uint64`/`uintptr`/`int64`) binds neither overload, so it is cast to `int`: `Ꮡ(s, (int)(i))`. Only those wide/unsigned types are cast — an `int`/`nint` or small-int index is emitted as-is to avoid churn. (Mirrors the runtime's `&datap.pclntable[funcoff]` / `&filetab[fileoff]`, indexed by `uint32` offsets. Guarded by the `ElementAddressUnsignedIndex` behavioral test.)

The `Ꮡ(slice, index)` form applies to **any slice-typed base expression**, not just a named slice variable — a method-**call** result (`&b.stk()[0]`, runtime `mprof.go`; `&StringByteSlice(s)[0]`, `syscall`), a builtin/`make` result, an `unsafe.Slice(…)` result (`reflect`), or a **slice-expression** base (`&x[0:cap(x)][cap(x)-1]`, `math/big`). Such bases have no bare identifier, so they previously fell out of the (identifier-gated) slice arm into the *array* branch — a slice's type name also starts with `[` — whose naive fallback textually prefixed `Ꮡ` onto the postfix chain: `Ꮡb.stk().at<uintptr>(0)` binds as `(Ꮡb).stk()…`, referencing a box that does not exist (CS0103), or copy-boxed the slice header (a lost-write latent). The element address of the returned slice **view** reaches the shared backing array per Go aliasing, so a write through the pointer is visible via the original storage. (Guarded by the `NestedFieldElementAddr` extension — `&st.stk()[0]` through a pointer local, write-through vs Go.)

The same `(int)` narrowing (the shared `castWideIntegerToInt` helper) applies to the bounds of a **3-index (full) slice** `s[low:high:max]`, which lowers to the golib `.slice(nint low, nint high, nint max)` method: a `uintptr`/`uint`/`uint32`/`uint64`/`int64` bound is cast — `stk[:b.nstk:b.nstk]` (b.nstk a `uintptr`) → `stk.slice(-1, (int)(b.nstk), (int)(b.nstk))`. Go's own slice bounds are `int`, so the narrowing matches Go. A plain `int`/small-int bound is left uncast. (The 2-index range forms `s[lo:hi]` narrow through `getRangeIndexer` for the C# `[..]` range operator; only the 3-index `.slice()` form needed this.) (Guarded by the `Slice3IndexWideBound` behavioral test — `uintptr`/`uint`/`uint64` full-slice bounds on an array and a slice + an int control, values verified vs Go; runtime hits this in `mprof`'s `stk[:b.nstk:b.nstk]`.)

**Address of an element of an array *field* reached through a pointer or boxed struct.** When the array being indexed is a field of a heap-boxed value — `&mp.future[i]` where `mp` is a `*memRecord`, or `&g.future[i]` where `g` is an address-taken global — the *array field's* address goes through the box-field accessor first, then the element index: `Ꮡmp.of(memRecord.Ꮡfuture).at<cycle>(i)` (pointer parameter), `mp.of(...)` (pointer local), `Ꮡg.of(rec.Ꮡfuture).at<cycle>(i)` (boxed global). A naive `Ꮡ` prefix on the field read (`Ꮡ(~mp).future`) instead binds `.future` to the box value `Ꮡ(~mp)` (a `ж<memRecord>`, which has no `future` member) → CS1061. This requires a matching golib detail: `ж<T>.at<TElem>(index)` resolves the array through the `Value` property, **not** the raw `m_val` field — for a field-reference pointer produced by `of(...)`, `m_val` is an empty default and the real array lives behind `Value` (the same resolution `of(...)` itself uses). Reading `m_val` would miss the array → null-deref at runtime even though the C# compiled. (`array<T>` is a readonly struct over a shared backing `T[]`, so the value `Value` yields still aliases the real elements; writes through the returned element pointer land.) (Guarded by the `PointerFieldArrayElementAddress` behavioral test — pointer parameter and pointer local both taking `&p.future[i]` and mutating through it.)

The same `Value`-not-`m_val` rule applies to the **dereference operator** `~`. A value read through a pointer — `(~c).field`, the form the converter emits for `c.field` where `c` is a `*T` — must resolve through `Value`. For a *field-reference* pointer (`c := &b.w` → `Ꮡb.of(box.Ꮡw)`) or an array-element pointer, the real storage lives behind `Value` and `m_val` is an empty default, so `operator ~` returning `m_val` would read a **zero-valued copy** (`(~c).a` → `0`) — it compiles but is silently wrong. `ж<T>.operator ~` therefore returns `value.Value` (which resolves struct-field / array-element references and, for a standard pointer, is exactly `m_val`), matching the `IPointer<T>.operator ~` that already did. This surfaced when a defined-type-over-struct's forwarded fields were read back through a `*wrapper`, but it is general to any `*x.field` value read. (Guarded by the `NamedTypeOverStruct` behavioral test's read-back path.)

The `at<E>(i)` **element type `E` is rendered fully-qualified** — `at<sync.atomic_package.Int32>`, not the file-local alias `at<atomic.Int32>`. A namespace-rooted type resolves inside `namespace go;` without any `using <pkg>` alias, whereas the alias form needs the file to import that package. A file can index a cross-package-typed array field of a struct without ever naming the element type (so Go requires no import, and the converter emits no `using atomic`), which would leave the alias unresolved (CS0246, e.g. runtime's `tracecpu.go` indexing `trace.cpuLogWrite`). A current-package or basic element renders identically either way, so this is churn-free. (Guarded by the `ArrayOfCrossPackageType` behavioral test's `&x.c[i]` element-address case.)

Using `ж<T>` rather than the C# `ref` keyword avoids the escape-analysis complications of passing a `ref` into code that expects a heap-allocated pointer. This is a simplification that can cost an unnecessary heap allocation when an address is taken; a future escape-analysis pass could keep such values on the stack when it is provably safe, similar to how [Go does this](https://golang.org/doc/faq#stack_or_heap) at compile time.

> Note: a package-level global whose address is taken is backed by a real heap box so that writes through `&global` (and `&global.field`) are observed, rather than mutating a copy.

### Pointer-typed globals and double-pointer walks (`&head`, `*pp`, `ValueSlot`)
A package-level global of **pointer type** whose address is taken — `var head *node` with `pp := &head` — is heap-boxed like any addressed global, yielding a **double box**: `ж<ж<node>> Ꮡhead`. Three rules make the classic linked-list walk (`for pp := &head; *pp != nil; pp = &(*pp).next { … *pp = n }`) faithful:

1. **One star is ONE deref.** `*pp` on a `**T` yields a `*T` — a single `.Value`/`.ValueSlot` hop, never two. (An older arm added an extra `.Value` per pointer *depth*, double-dereferencing every single-star of a double-pointer field — runtime `mheap.go`'s `specialsIter` walk failed CS0029 in both assignment directions.) A genuine `**pp` is two nested `StarExpr`s, each contributing its own hop. Likewise a *field read through an explicit single star* on a `**T` — `(*outer.ptr).Value` — keeps the base pointer-typed after one star, so normal pointer-base field handling supplies the remaining auto-deref: `(~(outer.ptr.Value)).Value`.
2. **A deref whose *result* is still reference-like reads `ValueSlot`, not `Value`.** Go's `*pp` may legally yield nil (`*pp != nil` is the loop condition); only *dereferencing* that nil panics. golib's strict `Value` accessor nil-checks the slot, so a deref (or boxed-global property) producing a pointer/slice/map/chan/func/interface value routes through `ж<T>.ValueSlot` — the **identical real slot with no nil check** — and reads *and writes* both persist: `pp.ValueSlot = n` lands in the original global storage. A deref producing a plain **value** keeps the strict `Value` (a nil `*node` deref must panic, as in Go). The boxed global's ref-property follows the same split: `internal static ref ж<node> head => ref Ꮡhead.ValueSlot;` for the pointer-typed global, `=> ref Ꮡg.Value;` for a value-typed one.
3. **`&global` on an addressed global is the identity box, never a copy.** `&allm` (where `var allm *m` is boxed) emits `Ꮡallm` — the existing box — not `Ꮡ(allm)`, which would heap-allocate a *copy* and silently disconnect writes. And `&(*pprev).alllink` (address of a field behind one explicit star) peels the star and goes through the field-box accessor: `pprev.Value.of(m.Ꮡalllink)`.

The full emitted walk:

```csharp
internal static ж<ж<node>> Ꮡhead = new(default(ж<node>));
internal static ref ж<node> head => ref Ꮡhead.ValueSlot;

for (var pp = Ꮡhead; pp.ValueSlot != nil; pp = (pp.ValueSlot).of(node.Ꮡnext)) {
    if ((~(pp.ValueSlot)).val == v) {
        pp.ValueSlot = (pp.ValueSlot).Value.next;   // *pp = (*pp).next — write lands in real storage
        ...
```

This is exactly the runtime's `allm`/`itabTable` shape (`for pprev := &allm; *pprev != nil; pprev = &(*pprev).alllink`). (Guarded by the `GlobalPointerWalk` behavioral test — ordered insertion, head/middle removal, and a method call through the pointer global, all via `**node` writes, output-compared against Go.)

### Capturing the address of a heap-boxed local in a closure
A local whose address is taken (`&m`) is heap-boxed: the converter emits `ref var m = ref heap(new T(), out var Ꮡm)`, where `Ꮡm` is the box and `m` is a `ref`-local alias of `Ꮡm.Value`. When a **function literal captures such a local and takes its address inside the closure**, the variable must be referenced through the box, not snapshot-copied. A C# `ref`-local cannot be captured by a lambda (CS8175), and the older snapshot capture (`var mʗ1 = m;`) is wrong twice over: it copies the *value* out of the box (so writes through the captured `&m` are lost), and the copy declaration is a statement that has nowhere valid to land when the literal sits in an expression position — e.g. a func literal passed as a **call argument** (`run(func(){ use(&m) })`) or a local initializer (`f := func(){ use(&m) }`).

The fix: a heap-boxed local whose address is taken inside a lambda is marked *box-ref* and the snapshot is suppressed. The box `Ꮡm` is a plain local (a capturable reference), so the C# closure captures it by reference — matching Go's capture-by-reference semantics. Inside the closure the converter then renders every form through the box:

```csharp
ref var m = ref heap(new box(), out var Ꮡm);
run(() => {
    set(Ꮡm);                       // &m  → Ꮡm
    Ꮡm.Value.y = Ꮡm.Value.x + 1;       // value use of m → Ꮡm.Value
});
// &m.field (value struct field) → Ꮡm.of(box.Ꮡfield)
```

This also covers `&m.field` (a value-struct field address inside the closure: `Ꮡm.of(box.Ꮡfield)`). The detection is scoped to the bare `&m` and value-struct `&m.field` forms (the ones with a box-ref emission form); an element address `&m[i]` keeps the existing snapshot path. The behavioral test `FuncLitArgCapture` guards the call-argument, value-use, field-address, and initializer cases.

### A capture that is WRITTEN after the capture point routes to shared storage, not a snapshot
Go closures share the ONE variable with the enclosing function. The value snapshot the converter uses for captured structs/arrays/slices/maps/chans (`var tʗ1 = t;` hoisted before the lambda, in-lambda references renamed to `tʗ1`) is therefore only *observationally* correct while **neither side writes the variable after the snapshot point**. Once anything does, the snapshot silently diverges — the program compiles and runs, with wrong values:

- a closure's writes land in a divorced copy: `bump := func() { t.total += 100 }` never affects `t` (probe-proven: Go 106, snapshot 6);
- body writes after the lambda's creation are invisible to a read-only closure: `get := func() int { return t.total }; t.total += 10` reads the stale copy (Go 15, snapshot 5);
- a **deferred** literal observes the variable's registration-time value instead of Go's final value (`defer func(){ fmt.Println(t.total) }(); t.total = 42` — Go 42, snapshot 5);
- two closures over one variable each get their own copy, so a writer and a reader stop communicating entirely.

The converter now detects **written-after-capture** per variable during analysis (`varShareFacts`, one cached scan of the enclosing declaration) and routes such captures to shared storage. Writes counted, conservatively by syntax: an assignment or `++`/`--` whose target roots at the variable's own storage (`t = …`, `t.f.g = …`, array `a[i] = …` — but *not* through a deref, an implicit pointer deref `p.f = …`, or a slice/map element, which a snapshot copy shares anyway); a pointer-receiver method call on the variable held as a value (Go's implicit `&t`); a `for t = range` clause; an explicit `&t` anywhere or an uncalled pointer-receiver method value (an **alias** — later writes through it are syntactically invisible, so it counts at any position, e.g. `p := &t; get := func(){…}; p.total = 50`); and any of these inside any func literal (the literal may run at any time). A plain body write counts only if positioned after a referencing literal or sharing a `for`/`range` loop with one (a later iteration's write follows an earlier iteration's creation).

The routing, by variable shape:

```csharp
// Heap-boxed variable (escaping struct local, aliased int, …) → by-box (boxRefVars):
ref var t = ref heap<Tally>(out var Ꮡt);
t = new Tally(5, "s");
var bump = () => {
    Ꮡt.Value.total += 100;      // value use → Ꮡt.Value: writes the ONE box the body reads
};

// Unboxed variable (value parameter; slice/map/chan local, whose copy diverges on
// reassignment) → NATIVE C# capture — no snapshot, no rename; the display class
// shares the local exactly as Go shares the variable:
internal static void probeB1(Tally t) {
    var bump = () => {
        t.total += 100;          // captures the parameter itself
    };
    bump();
    t.total++;                   // 106, matching Go
```

This applies only to genuine **closure-body** references (a func literal's body, directly or as a go/defer statement's literal callee). A go/defer statement's non-literal callee/receiver expression and its call arguments keep their statement-time evaluation — `defer fmt.Println(t.total)` still prints the registration-time value, which IS Go's argument semantics. Read-only-after-capture variables keep the snapshot (observationally identical, zero churn — the vast majority of stdlib captures). A **loop-statement-defined** variable (for-init or range clause) also keeps it: its per-lambda snapshot approximates Go 1.22's per-iteration variable, which shared routing would break. A literal's own parameters/results are not captures and are excluded. (The remaining known gap, deliberately out of scope here: a `for`-init variable captured by closures diverges from Go 1.22 per-iteration semantics — the C# `for` control variable is shared across iterations — tracked as its own defect.)

The native route makes the emitted C# read exactly like the Go for the parameter case; the by-box route reuses the box-ref machinery above (including `ValueSlot` for inherently-heap locals: a captured slice that the closure reassigns emits `Ꮡs.ValueSlot = append(Ꮡs.ValueSlot, …)` against a materialized `heap<slice<T>>` box). (Guarded by the `ClosureWriteVisibility` behavioral test — 19 probes: boxed/plain × local/param × closure-writes/body-writes-after × plain/defer/go/IIFE/loop-created contexts, plus slice/map reassignment, alias writes, two-closure sharing, and the read-only/defer-argument/range controls that must KEEP snapshot semantics.)

**A NAMED RESULT routed to shared storage declares its box too.** The `defer func(){ hook(written, err) }()` idiom is exactly the written-after-capture shape above with the captured variable being a *named result* — Go's deferred closure must observe the FINAL named-result values. When the escape analysis marks such a result (an interface-typed result is blanket-marked the first time it is reused on a mixed `v, err := …` define; a value-type one when `&x` is taken), the render sites duly go through the box (`Ꮡerr.ValueSlot` inside the deferred literal) — but the named-result declaration prologue emitted only the plain `error err = default!;`, leaving `Ꮡerr` undeclared (CS0103 — internal/poll `SendFile`'s deferred `TestHookDidSendFile`, the single error skip-cascading ~80 os-dependent packages). A box-backed named result (`identHasHeapBox`, the same gate plain locals use) now declares the box, in three shapes:

- **No defer wrapper** (plain function, or a closure writing the result): the full escaping-local form at the declaration site — `ref var err = ref heap<error>(out var Ꮡerr);` — body and bare returns keep reading the plain alias, nested closures read/write `Ꮡerr.ValueSlot`. A value-type result with `&x` gets `ref var x = ref heap(new nint(), out var Ꮡx);`, making the write through `&x` visible to the bare return (previously that shape was also CS0103).
- **namedReturnDeferMode** (function body wrapped in `func((defer, recover) => …)`): the decls sit OUTSIDE the wrapper, and a lambda cannot capture a `ref` local (CS8175), so the outside line creates only the box — `heap<error>(out var Ꮡerr);` — the wrapper re-derives the value alias inside (`ref var err = ref Ꮡerr.ValueSlot;`, exactly like a deref'd pointer parameter's `ref var fd = ref Ꮡfd.Value;`), and the final post-defer return reads through the box: `return (written, Ꮡerr.ValueSlot);`.
- **Func-literal sibling** (a literal with named results + defer + post-capture writes): same split, except the literal's body is itself a lambda conversion, so every in-wrapper use already renders through the box — including the explicit-return rewrite's assignment targets (`(var v, Ꮡe.ValueSlot) = pair(n);`) — and the literal's trailing `return (w, Ꮡe.ValueSlot);` reads the box.

The box-read accessor follows the box-ref rule above: `.ValueSlot` for an inherently-heap result (reading the held reference is not a dereference), `.Value` for a value-type box. Results NOT escape-marked are untouched — `written` in the same defer stays a plain local captured natively by the C# closure, which already observes the final value. (Guarded by the `NamedResultDeferCapture` behavioral test — value + error named results logged by a deferred closure with post-capture writes and bare returns, the `&x` value-result, the func-literal sibling, and a non-defer closure write; output-compared vs Go, proving the deferred observation of FINAL values. Stdlib footprint: 12 functions across 10 files — internal/poll, net/http, go/parser, crypto/tls, internal/fuzz, debug/buildinfo, both go importers, net/textproto.)

**A PARAMETER routed to shared storage declares its box too** — the third position of the same family (plain locals, named results, parameters). A parameter can be escape-marked without any capture-mode method call: a body-top-level mixed `:=` REDECLARES the parameter object (the spec's redeclaration rule includes the parameter lists when the block is the function body), so the define walker escape-analyzes it — and an interface-typed one is blanket-marked. When such a parameter is also captured by a closure and written after the capture point, the routing above sends it by-box (`Ꮡctx.ValueSlot` inside the lambda) — but the parameter prologue only boxed for the capture-mode (direct-ж) trigger, leaving the box undeclared (CS0103): database/sql `beginDC`'s `ctx` (redeclared by `ctx, cancel := context.WithCancel(ctx)` after `withLock`'s closure captured it) and go/types `nify`'s `x, y` (swapped by `x, y = y, x` and redeclared by `xorig, x := x, Unalias(x)` after the trace defer captured them). `paramNeedsHeapBox` (and its func-literal analogue `funcLitHeapBoxParamIdents`) now also fires for a box-ref-routed parameter, emitting the exact capture-mode form — the signature takes the incoming value as `ctxʗp` and the preamble declares `ref var ctx = ref heap(ctxʗp, out var Ꮡctx);` (inside the `func((defer, recover) => …)` wrapper when the function has one, where the box is an ordinary capturable local). Body statements keep reading/writing the plain ref alias — the redeclare emits `(ctx, var cancel) = …` against it — so both sides hit the ONE box, and a deferred observer sees Go's FINAL values. The check rides the declaring-ident lookups, so a box-ref'd value RECEIVER (never `ʗp`-renamed by the signature paths) can never take the param form. (Guarded by the `WrittenCaptureParam` behavioral test — the beginDC redeclare shape, the nify deferred-observer shape (named result + defer wrapper), a closure-write read back by the body, the func-literal sibling, and an inherently-heap slice param; all output-compared vs Go. Stdlib footprint: exactly `database/sql/sql.cs` + `go/types/unify.cs`.)

### A nested closure's capture snapshot reads the enclosing closure's snapshot
When a heap-boxed **ref-local is used by VALUE** (its address is not taken) and captured by NESTED closures, it is not box-ref'd — it is snapshot-copied: the converter declares `var mʗ1 = m;` before the closure and the closure uses `mʗ1`, so the uncapturable `ref`-local `m` is never referenced inside the lambda. The snapshot chain must be threaded through each level. A capture generated for an **inner** closure that lands inside an **outer** closure's body must read the outer closure's snapshot, not the enclosing method's ref-local — testing/fuzz.go's `run` closure captures `fn := reflect.ValueOf(ff)` (a heap-boxed `reflect.Value`), and run's inner `go tRunner(t, func(t){ … fn.Call(args) })` snapshots run's `fnʗ1`, not the method-level `fn` (a ref-local uncapturable inside a closure → CS8175):

```csharp
ref var fn = ref heap<reflectꓸValue>(out var Ꮡfn);
var fnʗ1 = fn;                 // run's snapshot (before the run closure)
var run = (…) => {
    var fnʗ2 = fnʗ1;           // the goroutine's snapshot reads run's fnʗ1, NOT fn
    goǃ(tRunner, t, (…) => func((defer, recover) => { … fnʗ2.Call(args); }));
};
```

`generateCaptureDeclarations` finds the RHS by walking the conversion stack outward past pass-through levels (a `go`/`defer` statement's own `enterLambdaConversion`, which carries an empty rename map) to the first enclosing lambda that renamed the variable. It skips the capture's OWN owner state — `pendingCaptures` is shared across a function's lambdas, so an outer lambda's snapshot can be generated while converting an inner func-literal argument (`go dnsWaitGroupDone(ch, func(){})`, net/lookup.go), leaving the owner's state on the stack with a rename equal to the name being declared; adopting it would emit a self-reference `var fʗ1 = fʗ1;` (CS0841). Byte-identical corpus-wide except where a nested closure re-captures a heap-boxed local. Guarded by `FuncLitArgCapture` (a heap-boxed struct re-captured in an inner goroutine — CS8175 without the fix — and the `go f(x, func(){})` self-reference shape) and by `DeferValueFieldPtrReceiver` (a defer inside a lambda).

A **pointer (or other inherently-heap) local** captured by a closure that takes its address needs the box too, but reaches it by a different route. A local of an *inherently heap-allocated* type — a pointer, slice, map, channel, interface, or func — is already a reference, so it normally gets **no** heap box (the `convertToHeapTypeDecl` path returns nothing for such types). But when one is captured by a closure that takes its address (`mToFlush := &node{…}; run(func(){ prev := &mToFlush; … *prev = mToFlush.next })`), the closure needs a *shared* box so writes through `&mToFlush` inside it reach the outer function's storage. The converter detects this as the same *box-ref* mark used above (an inherently-heap local whose address is taken inside a lambda), and for a box-ref local it now emits the heap box even though the type is inherently heap — `ref var mToFlush = ref heap<ж<node>>(out var ᏑmToFlush)` — so the box `ᏑmToFlush` (a `ж<ж<node>>`, i.e. a `**node`) exists for the closure to reference. Without it the closure emitted `ᏑmToFlush` for `&mToFlush` against a never-declared box (CS0103); a same-function `&ptr` with **no** closure still takes the `Ꮡ(ptr)` copy form (a copy is fine there — no shared storage is needed), so that case is unchanged.

Reading such a box needs care, because for a box-of-pointer the held value can legitimately be nil while the box itself is a real allocation. `Ꮡm` here is a `ж<ж<node>>` (a `**node`), so `Ꮡm.Value` reads the *held pointer value* — not a dereference of `Ꮡm` — and in Go reading `*(&p)` when `p` is a nil `*T`/slice/map yields the nil value, with no dereference and no panic. The strict `ж<T>.Value` getter (which panics on a null stored value by design, so a genuine `*p` on a nil pointer still throws) would wrongly panic on that read. So the converter emits the golib `ж<T>.ValueSlot` accessor for these box-of-pointer reads — identical to `.Value` but without the nil-pointer-dereference check, returning the *real* slot so reads and writes both persist (unlike `DerefOrNil`, which yields a throwaway slot for a genuinely-nil box). `ValueSlot` is gated to a box-ref **local** of inherently-heap type (a deref'd pointer *parameter* keeps the strict `.Value`, since its box wraps the pointed-to value and `Ꮡp.Value` is a genuine dereference). The `heap(out …)` / `heap(target, out …)` helpers likewise return `ref pointer.ValueSlot`: a freshly allocated box is structurally non-nil, so the getter's nil check there is always spurious (identical to `.Value` for a value-type box; it just avoids a spurious panic when establishing the `ref var mToFlush = ref heap<ж<node>>(out var ᏑmToFlush)` alias). A genuine dereference of the held pointer (the second `.Value` in `ᏑmToFlush.ValueSlot.Value.v`) stays strict and still panics on nil — preserving Go's "panic ⇒ panic" semantics, and complementing the deliberate strict-`.Value` design behind `DerefOrNil`. (Guarded by the `ClosureCapturedPointerAddress` behavioral test — a closure that takes the address of a captured pointer local, walks a linked list by reassigning *through* that address and mutating each node, with the outer function observing both the reassignment-to-nil and the persisted mutations, proving the box is shared rather than copied. Mirrors runtime's `trace.go` `mToFlush := allm; systemstack(func(){ prev := &mToFlush; … mToFlush = mToFlush.next })`, ~4 CS0103.)

A pointer-receiver method called **through a FIELD of such a boxed pointer local**, inside the closure, field-refs through the **held pointer**, not the box. The receiver of `c.flushGen.Store(…)` (runtime `mcache.go`'s `allocmcache`, inside `systemstack`) is taken via the &-machinery, and inside a lambda the box-ref address form substitutes the capturable box for the uncapturable ref-local alias. For a *value*-struct local (box `ж<T>`) and a deref'd pointer *parameter* (box `ж<T>` — the Go pointer itself) the bare box is the correct `.of()` receiver — but a boxed pointer LOCAL's box is `ж<ж<T>>`, one level above the `ж<T>` the field accessor projects from, and feeding it to `.of` fails inference (CS0411 — the one error that skip-cascaded ~237 packages behind `runtime`). Such a base declines the bare-box form and falls through to the pointer-variable field arm, whose ident render reads the box the same way every other in-lambda value use does: `Ꮡc.ValueSlot.of(mcache.ᏑflushGen).Store(…)` — `.ValueSlot` because reading the held pointer out of the box must not nil-check (the dereference happens in `.of`, preserving panic semantics), and because that slot IS what the enclosing `ref var c = ref heap<ж<mcache>>(out var Ꮡc)` alias reads. When such a local is **named after its own type** (`gauge := newGauge()`), the accessor's owning-type name additionally qualifies with the package class (`Ꮡgauge.ValueSlot.of(main_package.gauge.Ꮡv)`): the enclosing `ж<gauge>`-declared local stays visible inside the lambda, so the bare type name binds the uncapturable ref-local (CS8175) with no identical-simple-name fallback — the declared type differs from the type name. (Guarded by the `ClosurePtrLocalFieldMethod` behavioral test — the `allocmcache` shape: a pointer local written inside a closure and immediately method-called through a value field, read back after the closure, plus the named-after-type variant; output-compared vs Go, proving the write-through and the field-method call both bind the one shared box.)

A **deref'd pointer parameter or pointer receiver** captured by a closure is box-ref'd the same way, even when only its *value* is used inside the closure (not its address). Such a parameter is emitted as the box `ж<T> Ꮡp` with `ref var p = ref Ꮡp.Value`, and the `ref`-local alias cannot be captured (CS8175). Inside the closure a value use becomes `Ꮡp.Value.field` and an address use `Ꮡp`, so the closure captures the box by reference — matching Go capturing the pointer. (Guarded by the behavioral test `PointerParamCapturedInClosure`; the runtime captures `*maptype` / `*m` parameters this way pervasively.)

A pointer **receiver** captured by a closure needs an extra step the parameter case does not: the box `Ꮡp` only exists if the method is emitted **direct-ж** (the box passed *as* the receiver, `this ж<T> Ꮡp`). A normal pointer-receiver method is `[GoRecv] this ref T p` (a value-ref receiver, with the `ж<T>` companion generated separately), which has no box for the closure to reference. So "the receiver is referenced inside a function literal" is a **direct-ж trigger** — a fourth one alongside taking a field's address (`&p.field`), returning the receiver (`return p`), and using the receiver as a bare pointer value (`p.next = p`, `p != q`). Mirrors runtime's `func (p *_panic) nextFrame() { systemstack(func(){ … p.lr … }) }`. A closure parameter that shadows the receiver name resolves to a distinct object, so it does not falsely trigger the promotion. (Guarded by the `ReceiverCapturedInClosure` behavioral test — receiver captured by an immediately-invoked closure that reads/writes through it, by one that takes a field's address, and by one that is *returned* so the box must outlive the call.)

Once a method is direct-ж, its receiver is the box `Ꮡc`, but the deref'd value alias `ref var c = ref Ꮡc.Value` is what most uses see. When such a receiver is passed **whole** as a pointer argument — `stackcache_clear(c)` in `func (c *mcache) prepareForSweep()` — the argument must be the box `Ꮡc`, not the value alias `c` (a value cannot bind a `ж<mcache>` parameter → CS1503). A deref-aliased pointer *parameter* is already handled (it is an `identIsParameter`), but a direct-ж *receiver* is not a parameter, so the call-argument conversion recognizes it explicitly and emits the box. (Guarded by the `DirectBoxReceiverPassedWhole` behavioral test.)

The receiver placed whole into a **composite-literal element** whose field is a pointer — `func (f *_func) funcInfo() funcInfo { …; return funcInfo{f, mod} }` (runtime `symtab.go`; `funcInfo`'s first field is the embedded `*_func`) — needs the same box, and is itself a **direct-ж promotion trigger** (`bodyUsesReceiverAsPointerValue`'s composite arm): a boxless `[GoRecv] ref` receiver has no `Ꮡf` to place in the field (CS1503). Once promoted, the composite renders the box through the existing pointer-field element machinery: `new ΔfuncInfo(Ꮡf, mod)`. Both positional and keyed elements trigger, gated on the **field's declared type being a Go pointer** (resolved positionally or by key from the composite's struct type — the element expression's own type is always `*T` for a pointer receiver): a receiver placed into an *interface*-typed field also typechecks in Go, but that emission compiles today, and promoting for it would re-route every such method stdlib-wide (the field gate trims the first-cut 73-file audit to 68 — the shape is genuinely pervasive: go/types' Checker methods, net/textproto's dotReader{r: r}, zstd readers — every audited site the same signature+box re-routing) — its pointer-identity semantics are logged as a separate question. (Guarded by the `DirectBoxReceiverPassedWhole` extension — positional + keyed composites, identity verified by writing through the wrapped pointer and reading the original.)

The same composite arm also fires when the receiver is stored **as an element of a SLICE or ARRAY literal whose element type is a pointer** — `func (s *UserTaskSummary) Descendents() []*UserTaskSummary { descendents := []*UserTaskSummary{s}; … }` (internal/trace `summary.go`). Without promotion the boxless `[GoRecv] ref` receiver renders the value alias `s` into a `ж<T>[]` slot (CS0029); once promoted direct-ж, the element renders the box: `new ж<UserTaskSummary>[]{Ꮡs}.slice()` (and `[2]*T{s, other}` → `new ж<T>[]{Ꮡs, Ꮡother}.array()`). Gated on the **slice/array element type being a pointer** (the `*types.Slice`/`*types.Array` arms of `bodyUsesReceiverAsPointerValue`), mirroring the struct-field pointer gate. (Guarded by the `ReceiverPointerValue` extension — the receiver stored into a `[]*ring` and a `[2]*ring` literal, `chain[0]` identity verified by mutating through the stored pointer and reading back through the receiver.)

The same pointer-element boxing must also fire for an **ELIDED (type-inferred) nested composite** — the inner `{c}` of `[][]*Certificate{{c}}` (crypto/x509 `Verify`). The inner literal has no `Type` node; its inferred element type is `*Certificate`, and its sole element `c` is the deref-aliased `*Certificate` receiver. The typed composite path boxes a bare pointer-typed ident element (`argTypeIsPtr`), but the untyped-elided slice/array path rendered its elements with a **nil** context, so that treatment never ran and `c` emitted the value alias into a `ж<Certificate>[]` array (CS0029). The elided path now supplies a context that boxes a bare pointer-typed ident when the element type is a pointer — `new ж<Certificate>[]{Ꮡc}.slice()` — returning nil (unchanged nil-context rendering) when the element type is not a pointer or no element is a bare pointer ident, so non-pointer elided literals stay byte-identical. (Guarded by the `ElidedNestedPtrComposite` behavioral test — `[][]*Node{{n}}` where `n` is a pointer receiver.)

A **MAP** composite literal whose value or key type is a pointer boxes its element the same way — but through `convKeyValueExpr` (the `[key] = value` form), not the slice/array element loop above. `map[K]*T{k: c}` where `c` is a deref'd pointer parameter renders the value alias `c` into a `ж<T>` map slot (CS0029); the map-source branch of `convKeyValueExpr` now sets the `isPointer` ident context for the VALUE when the map's declared element type is a pointer, so a bare-ident pointer value emits its box `Ꮡc` — `new map<@string, ж<node>>{["a"u8] = Ꮡa}`. A pointer-KEY map (`map[*T]V{c: 1}`) boxes the key the same way (`new map<ж<node>, nint>{[Ꮡa] = 1}` — the `ж<T>` dictionary key matches by box identity). Gated on the map's declared **element/key type being a pointer** (not an interface — an interface-valued map still routes through the interface conversion) *and* the element expr's own type being a pointer, so a value already rendered as a box (`&x`, a pointer local) is unaffected. (Guarded by the `MapPointerElementLiteral` behavioral test — a pointer-value map and a pointer-key map built from `*node` parameters, aliasing verified by mutating through a stored value and looking up by pointer-key identity.)

**Reassigning a pointer parameter to a new pointer.** A `*T` parameter that walks memory by reassignment — `bits = addb(bits, n)` (a `*byte` step in the runtime's bitmap scanners) or `p = p.next` (a list walk) — cannot write through its value alias: `ref var bits = ref Ꮡbits.Value` makes `bits` the pointed-to *value*, and a pointer RHS (`ж<byte>`) does not fit it (CS0266/CS0029). The reassignment instead repoints the **box** and re-aliases the value var — `Ꮡbits = addb(Ꮡbits, n); bits = ref Ꮡbits.Value;` — reusing the same box-reassignment path that handles a direct-ж receiver's `r = r.prev` (the RHS already emits the box form). (Guarded by the `PointerParamWalk` behavioral test, a circular-list walk that reassigns the parameter and reads the pointed-to value each step.) Reassigning a *pointer local* (not a parameter) is unaffected — a local already holds the box.

**Reassigning a captured pointer parameter inside a closure.** The repoint-and-re-alias above (`Ꮡp = …; p = ref Ꮡp.Value;`) rebinds a `ref`-local. Inside a CLOSURE that captured the parameter that is illegal: the re-aliased value var is an ENCLOSING `ref`-local, and C# forbids referencing an outer `ref` local inside a lambda (CS8175 — crypto/x509 `buildChains`'s `considerCandidate` closure does `if sigChecks == nil { sigChecks = new(int) }` on the captured `*int` parameter). The box reassignment `Ꮡp = …` is legal (it writes the captured box field, hoisted to a closure field), so only the ref-local refresh is dropped inside a lambda:
```csharp
if (ᏑsigChecks == nil) {
    ᏑsigChecks = @new<nint>();          // was: … ; sigChecks = ref ᏑsigChecks.DerefOrNil();  (CS8175)
}
ᏑsigChecks.Value++;
```
Every in-lambda and post-lambda dereference of a repointed captured pointer routes through the box `Ꮡp.Value`, so the now-stale value alias is never read — an accepted modeling gap (like the nil-terminated walk's), not a miscompile. The suppression is sound because no LEGITIMATE re-alias ever occurs inside a lambda: a lambda's OWN pointer parameter is passed as the box `ж<T>` (never deref-aliased), and a heap-boxed value local is written THROUGH its box (`Ꮡb.Value = …`, never box-repointed). Guarded by `ClosureReassignsPtrParam` (a closure that reassigns a captured `*int` parameter; a non-nil runtime argument keeps the reassignment branch unreached so output stays deterministic).

The same repoint-and-re-alias applies when the parameter is reassigned **from a tuple** — `(left, x, idx) = binarySearchTree(x, idx, n/2)` (runtime `mgcstack.go`) or `pp, _ = pidleget(0)` (`proc.go`). The box-reassignment triggers matched the RHS **element-wise**, so a tuple *deconstruction* (one call RHS, several LHS) never fired them — the ж<T> tuple component was assigned into the deref'd value alias (CS0029) — and element 0's raw expression type is the whole `*types.Tuple` (never a pointer), so even a first-position pointer element missed. The per-element RHS type now comes from the call's result tuple, and the emitted form is the single-assign form verbatim: `(left, Ꮡx, idx) = binarySearchTree(Ꮡx, idx, n / 2); x = ref Ꮡx.Value;` — with the nil-safe accessor when the parameter is nil-compared (`(Ꮡpp, _) = pidleget(0); pp = ref Ꮡpp.DerefOrNil();`). The triggers are gated to a **reassigned** element: a `:=`-declared pointer element binds the tuple's ж<T> component into a fresh pointer local — which *is* the box — directly, and an inner `:=` local shadowing a parameter's name must not repoint the parameter's box (crypto/x509's `c, _, err := …cert(i)`). (Guarded by the `PointerParamNilWalk` extension — a nil-compared tuple-reassign walk plus a reassign-then-mutate-through probe, values vs Go.)

**Nil-terminated walk.** A pointer-parameter walk that stops at a nil terminator — `func sumList(p *node) int { for p != nil { total += p.val; p = p.next } }` — needs two extra pieces, *modeled together*:

1. **Compare the box, not the value alias.** The loop guard `p != nil` must emit `Ꮡp != nil` (the box). Each binary operand's pointer context is otherwise taken from the *other* operand's pointer-ness, and `nil` is not a pointer type — so the param would convert in value form (`p != nil`, comparing a `node` struct value, the wrong thing). The converter forces the box form for a deref'd pointer *parameter* in a `==`/`!=` comparison. This is safe only for a parameter: a pointer *local* is already the box, and forcing it would emit a non-existent `Ꮡlocal`.
2. **Nil-safe re-alias.** On the final step `p.next` is nil, so `Ꮡp = p.next` repoints the box to nil; re-aliasing through the plain `Ꮡp.Value` getter would then throw a nil-pointer dereference before the guard is re-checked. The deref/re-alias instead routes through the golib `ж<T>` extension `Ꮡp.DerefOrNil()`, which returns a `ref` to a shared `default(T)` slot when the box is nil (never read while nil — the `Ꮡp != nil` guard excludes it) rather than throwing. The entry alias uses it too, so an empty-list call (`sumList(nil)`) is nil-safe at entry.

```csharp
internal static nint sumList(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNil();
    nint total = 0;
    while (Ꮡp != nil) {
        total += p.val;
        Ꮡp = p.next; p = ref Ꮡp.DerefOrNil();
    }
    return total;
}
```

`DerefOrNil()` is **not** a substitute for a genuine dereference: reading or writing `*p` on a nil pointer (`~Ꮡp` / `Ꮡp.Value`) still panics, preserving Go semantics — only the re-alias, which captures a reference without reading it, uses the nil-safe form. Both pieces are gated on a pointer parameter that is compared with `==`/`!=` anywhere in the body: a comparison signals that nil is a *legal argument* (Go panics only at an actual deref, never at entry), so the eager entry alias must not throw for it. This covers both the reassigned walk above and a nil-testing body invoked with a literal-nil argument (`defer closeIt(nil, 3)` → `p == nil` — the eager `Ꮡp.Value` alias otherwise panics before the body runs). The accepted trade-off, shared with the walk case: an *unguarded* value deref of an actually-nil argument reads the throwaway slot instead of raising Go's nil-deref panic — observable only by a program already panicking in Go. A parameter that is never nil-compared keeps the plain `.Value` form, so other code is unchanged. (Guarded by the `PointerParamNilWalk` behavioral test — a nil-terminated sum, a mutate-through-the-parameter pass, and an empty-list call — plus `DeferTypelessReturns`' deferred nil-argument call. The never-nil circular walk stays on the plain `.Value` path via `PointerParamWalk`.)

A **package-level global** referenced inside a closure is *not* captured at all — it is a C# static, accessed live. A value snapshot (`var gʗ1 = g`) would copy the struct (so `&gʗ1` has no box → CS0103, and writes through the global from inside the closure would be lost) and is semantically wrong, since Go reads/writes the live global. For an address-taken (heap-boxed) global the closure references the static box `Ꮡg` directly — a method call routes as `Ꮡg.method()` and a field address as `Ꮡg.of(T.Ꮡfield)`. (Guarded by `GlobalCapturedInClosure`; the runtime does this in every `systemstack(func(){ … mheap_ … })`.)

### Capture-mode methods called through a value field of the receiver
A pointer-receiver method that takes the address of one of its own fields (`func (c *Counter) Add(d int32) int32 { return bump(&c.n, d) }`) is *capture-mode*: it is emitted with the heap box **as** its receiver (`this ж<Counter> Ꮡc`) so `&c.n` can field-reference the real storage as `Ꮡc.of(Counter.Ꮡn)`. When another struct embeds such a type as a **value field** and drives it through that field — `func (f *Flag) Incr() int32 { return f.c.Add(1) }` — the call needs a `ж<Counter>` aliasing the real `f.c`. The enclosing method is therefore itself promoted to capture-mode (direct-ж), and `f.c.Add(1)` is emitted as `(&f.c).Add(1)`:
```csharp
public static int32 Incr(this ж<Flag> Ꮡf) {
    ref var f = ref Ꮡf.Value;
    return Ꮡf.of(Flag.Ꮡc).Add(1);   // f.c.Add(1) — nested field-address box
}
```
The nested `Ꮡf.of(Flag.Ꮡc).of(Counter.Ꮡn)` chain resolves each level through `ж<T>.Value` (which honors a parent that is itself a field/array reference), so writes land on the real embedded field rather than a copy. A plain (non-capture) value method called through the same field — `f.c.Get()` — is left as a normal `f.c.Get()` value call.

This field-address routing applies only to **value** fields. When the field is itself a **pointer** — e.g. cpuProfile's `log *profBuf`, accessed as `cpuprof.log` where `cpuprof` is a heap-boxed global — its C# value is *already* a `ж<profBuf>` box, so a direct-ж method binds to it directly: `cpuprof.log.close()`. Taking the field's address (`Ꮡcpuprof.of(cpuProfile.Ꮡlog)`) would double-box to `ж<ж<profBuf>>` (CS1929). The heap-boxed-receiver routing recognizes that a field selector or indexed element whose own type is a Go pointer is already a box and skips the `&`-machinery for it. This discriminates a pointer *field* of a boxed global (already a box) from a deref'd pointer *parameter* (`s` in `s.Prev()`, a value alias whose box is `Ꮡs`): the latter is a bare identifier, not a selector/index, so it is correctly still routed through `Ꮡs`. The same exclusion applies when the pointer field is reached through a pointer **local** rather than a boxed global — `s := sl.mspan; s.gcmarkBits.bytep(…)` where `s` is a `*mspan` local — which otherwise routed through the pointer-local-field address path (`s.of(mspan.ᏑgcmarkBits)`); the field value `(~s).gcmarkBits` is already the `ж<gcBits>`. (Guarded by the `PointerFieldOfBoxedGlobal` behavioral test, covering both the boxed-global `cpuprof.log.write`/`.close` form and the pointer-local `s.log.push` form; runtime exercises both pervasively, e.g. `mspan.sweep`.)

The same applies when the value field belongs to a **package global** rather than a receiver — `ctrl.total.Add(5)` where `var ctrl controller` and `total` is an atomic field. The method's box address goes through the field-address machinery, `Ꮡctrl.of(controller.Ꮡtotal).Add(5)`, not a bare `Ꮡ` prefix on `ctrl.total` (which would bind to the box variable `Ꮡctrl`, whose value type has no `total` member → CS1061). This is the form runtime uses pervasively for `gcController`, `sched`, `memstats`, etc. The **method call itself** triggers heap-boxing the global: when a pointer-receiver method is called on a (possibly nested) value field of a package value global, the escape pass marks that global address-taken so its box exists — the call site needs the box even when the global is never explicitly `&`-addressed elsewhere. This is gated on the method being ж-only (a pointer receiver): a same-package method known to be capture-mode, **or** any pointer-receiver method whose package's capture-mode set is not locally available — the latter covers cross-package atomic methods (`func (x *Uint32) Store`), which are likewise emitted with only a box receiver, so a plain value/ref of the field cannot bind them (CS1929). The walk to the global root bails at any pointer hop (a field reached through a pointer already has a real address and is handled by the pointer-local / receiver paths), so a receiver/parameter field such as `f.c` is never disturbed. (Guarded by the `AtomicValues` behavioral test's global-atomic-field case; runtime exercises this for `prof.signalLock`, `trace.seqlock`, `scavenge.gcPercentGoal`, etc.)

It also applies when the receiver is an **indexed element** of such a field — `trace.stackTab[i].dump()` (boxed global) — where the element's address goes through the box-field accessor: `Ꮡ(trace.stackTab, i).dump()` for a slice field, or `Ꮡtrace.of(T.ᏑstackTab).at<E>(i).dump()` for an array field. The same routing covers an indexed element of an array/slice reached through a **pointer** — `bh.Value[i].Load()`, where `bh` is a pointer and the element is an atomic value — emitted `bh.of(T.Ꮡval).at<E>(i).Load()`. This is gated on the called method being **direct-ж** (a box receiver): an ordinary `[GoRecv] ref` method binds to an addressable element directly, so it is left as `container[i].method()` and only a direct-ж method (which truly needs the box) is routed — avoiding needless churn on the common case. (Guarded by the `IndexedElementDirectBoxMethod` behavioral test — a direct-ж method on an array-element-through-a-pointer-parameter, with mutation persistence verified; runtime hits this on `mprof`'s `bh.Value[i].Load()`/`.StoreNoWB()`.)

A capture-mode method called on a **value local of an inherently-heap type** — a named *slice*/*map*/*chan* — also forces the box, which `identHasHeapBox` otherwise refuses. An inherently-heap type is already a reference, so a var of it is normally *not* boxed even when it "escapes" (the escape pass marks every inherently-heap local escaping and returns early). But a capture-mode pointer-receiver method — internal/trace/internal/oldtrace's `orderEventList` (a named `[]orderEvent`) with heap.Interface `Push`/`Pop` that forward the receiver to `heapUp(h, …)`/`heapDown(h, …)` — is emitted with a `ж<orderEventList>` receiver, so a plain value cannot bind it (CS1929 — `var frontier orderEventList; frontier.Push(…)`). The escape pass therefore records the capture-mode reason **in that inherently-heap early-return branch** (the only place these vars are seen, before the general address-of scan), and `identHasHeapBox` honors it — emitting `ref var frontier = ref heap<orderEventList>(out var Ꮡfrontier)` so the calls route `Ꮡfrontier.Push(…)`/`Ꮡfrontier.Pop()` through the box. A named slice/map/chan with no capture-mode method called on it stays unboxed (already a reference — no churn). (Guarded by the `NamedSliceCaptureMethod` behavioral test — a named-slice value local with `*stack` `push`/`pop` that forward the receiver to helpers, mutated and read through the same box, output-compared vs Go.)

A capture-mode method called on a **value PARAMETER** boxes the parameter **at entry** — go/format's `format(…, cfg printer.Config)` calling `cfg.Fprint(&buf, fset, file)`, where `(*printer.Config).Fprint` is transitively direct-ж (its body calls the defer/recover-wrapped `fprint` on its own receiver), so its only emitted receiver form is the box `ж<Config>` and the raw value parameter cannot bind it (CS1929 ×2). Parameters are deliberately **never** fed through the full escape analysis (their `&param` forms use the `Ꮡ(value)` copy-box), so the escape pass runs exactly one narrow parameter check per function — `bodyCallsCaptureModeMethodOn`, the same predicate the local-var arms use (`markCaptureModeBoxedParams`) — and marks only the params it fires for. For a marked param the **signature renames the incoming value to the `ʗp` form** (the variadic-prologue rename convention) and the parameter preamble declares the boxed alias:

```csharp
internal static (slice<byte>, error) format(…, printer.Config cfgʗp) {
    ref var cfg = ref heap(cfgʗp, out var Ꮡcfg);
    …
    cfg.Indent = indent + indentAdj;        // body writes hit the boxed storage…
    var err = Ꮡcfg.Fprint(…);               // …the same storage the callee mutates through the receiver
```

Entry-time boxing is the load-bearing choice: Go auto-addresses the parameter (`cfg.Fprint(…)` ≡ `(&cfg).Fprint(…)`), so a body write **before** the call (`cfg.Indent = …`) must be seen by the callee, and the callee's writes through the receiver pointer must be seen by the rest of the body — while the **caller's** argument stays untouched (by-value parameter). A call-site `Ꮡ(cfg)` copy-box compiles but silently drops the callee's writes for the rest of the function. An ARRAY param folds its Go by-value clone into the box init (`ref var b = ref heap(bʗp.Clone(), out var Ꮡb);` — the plain `b = b.Clone();` preamble line is skipped), and an inherently-heap-typed param records the capture-mode box reason exactly like the value-local arm above. The trigger is strictly the capture-mode call: a param that leaks into `identEscapesHeap` some other way — a mixed `data, pc, line := …` define re-uses the param object, so the define walker escape-analyzes it (debug/gosym's `slice`) — keeps its historical unboxed emission (`paramNeedsHeapBox` re-verifies the predicate against the declaring ident). Whole-stdlib reconvert diff: exactly go/format's `internal.cs` changed, nothing else. (Guarded by the `CaptureModeValueParam` behavioral test — a defer-promoted direct-ж method plus a transitively-promoted one called on a value parameter, with a pre-call write observed by the callee, callee writes read back after, and the caller's copy proven untouched, output-compared vs Go — and by the `CaptureModeValueParamLib`/`CaptureModeValueParamUser` cross-package pair mirroring the format→printer shape: a foreign `Config` value param, `Fprint` → defer/recover `fprint` transitive promotion, trace accumulation across two calls proving write-visibility through the foreign `ж<Config>` extension.)

When the same function **also contains a func literal or defer that references the boxed parameter**, the in-lambda references must route **through the box** — the capture analysis marks such a param box-ref (the same arm family as a deref'd pointer parameter, whose `ref var p = ref Ꮡp.Value` alias shares the exact shape). The boxed param's Go name is a `ref`-local alias, which a C# lambda cannot capture (CS8175), and the general capture-snapshot fallback (`var tʗ1 = t;` before the lambda) compiles but **divorces the closure from the boxed storage** Go shares between the closure and the direct-ж callee: a closure read misses the callee's writes through the receiver pointer, a closure write is invisible to the callee, and a deferred closure observes entry-time values instead of return-time state. With the box-ref mark, a closure read emits `var get = () => Ꮡt.Value.total;`, a closure write `Ꮡt.Value.total += 100;`, and a deferred observer `defer(() => { (result, log) = (Ꮡt.Value.total, Ꮡt.Value.log); });` — the box `Ꮡt` is a plain `ж<T>` local, captured by reference, so every reference (body, closure, callee) hits the one boxed storage, matching Go's one-parameter-variable semantics. A **deferred direct-ж method value on the param itself** (`defer t.Add(n)`) needed no change — it already routes through the box (`deferǃ(Ꮡt.Add, n, defer)`), binding the receiver address at defer time exactly like Go. Whole-stdlib reconvert diff: **zero files** — no stdlib function composes a capture-mode-boxed param with a closure today, so the composition is user-code-facing and was guard-discovered. (Guarded by the `CaptureModeParamClosure` behavioral test — four compositions with write-visibility checks in both directions: a closure read that must see the callee's later write, a closure write the callee must observe (and vice versa), a deferred closure reading return-time state, and a deferred method value whose writes a sibling deferred observer reads; each output-compared vs Go, with the caller's copy proven untouched. Under the pre-fix snapshot emission all four compiled and produced wrong values.)

Entry-time boxing extends to a **function literal's own value parameter** — the original coverage walked only `*ast.FuncDecl` params, so `f := func(t Tally, m int) {…; t.Add(m); …}` rendered the raw `Tally` value against `Add`'s only `ж<Tally>` receiver form (CS1929). The escape pass marks literal params with the same one-narrow-predicate check as declaration params (walking `FuncLit` nodes **before** the define walk, so a mixed `t, y := …` re-use cannot pre-empt the verdict; a leaked-but-not-capture-mode param keeps its historical unboxed emission via the same declaring-ident re-verification). The literal's signature takes the incoming value under the `ʗp` name and its **first block statement** is the boxed re-declaration — the exact preamble form, injected before the single-return collapse (which it thereby suppresses, correctly keeping the body a block):
```csharp
var f = (Tally tʗp, nint m) => {
    ref var t = ref heap(tʗp, out var Ꮡt);
    t.total++;                    // body writes hit the boxed storage…
    Ꮡt.Add(m);                    // …the same storage the callee mutates
    return (t.total, t.log);
};
```
This applies uniformly to every literal form: an assigned literal, a call argument, a `defer func(t Tally) {…}(x)` / `go …` argument-passing target (each deferred/goroutine run boxes its own copy at entry), and — unlike the variadic prologue, which excludes them — an **IIFE**, whose names-only parameter list emits the `ʗp` name so the rebinding composes with the delegate cast. A literal with both a variadic tail and a boxed param stacks the two `ʗp` prologues (variadic slice first, matching the declaration preamble order). A **nested closure** over the literal's boxed param takes the box-ref route (never a value snapshot, which compiled but orphaned the callee's writes — `var tʗ1 = t; tʗ1.Add(9)` lost both directions of write-visibility), while the literal's **own body** keeps the plain ref-alias renders above (`t.total++`, not `Ꮡt.Value.total++`): a box-ref var whose declaring literal is the lambda currently being converted renders plain, since its box and alias are locals of that very lambda — only genuinely nested lambdas read through the box. Whole-stdlib reconvert diff: **zero files** — no stdlib literal calls a capture-mode method on its own value param today, so this is user-code-facing and guard-discovered. (Guarded by the `CaptureModeFuncLitParam` behavioral test — assigned, IIFE, deferred-argument, nested-closure, and variadic-composition shapes, each with write-visibility checked in both directions and the caller's copy proven untouched, output-compared vs Go.)

And it applies when the field belongs to a **pointer local** — `h.s.inc()` where `h` is a `*holder` local and `inc` has a pointer receiver. A pointer local holds the box `ж<holder>` directly, so the value `~` dereference of the field (`(~h).s`) is an rvalue; the `[GoRecv]` method needs an addressable receiver (CS1510 on the generated `ref`). The field's box address is taken instead — `h.of(holder.Ꮡs).inc()` — binding the `ж` overload. (A pointer *parameter* is deref-aliased to a value, so `p.s.inc()` already works without this and is left alone. This is the form runtime uses for `(*c).gp.set(…)` / `.cas(…)` in coro.)

Finally, the same rvalue problem occurs when the field belongs to a pointer reached through *another field* — `o.h.wait.add(…)` where `o.h` is a `*holder` field and `wait` is a value (atomic) field. `o.h` dereferences to an rvalue, so `(~o.h).wait` is not addressable. The receiver is routed through the box-field accessor `o.h.of(holder.Ꮡwait)`, which aliases the **real** field storage — *not* a `Ꮡ(value)` copy, which compiles but silently boxes a copy so the atomic write is lost (a behavioral bug, not a compile error). Both the explicit address form (`&o.h.wait`) and a pointer-receiver method call on the field are routed this way. This is deliberately scoped to a base that is itself a *field selector*: a bare-ident base is the method's own receiver or a deref'd pointer *parameter* (both emitted as an addressable `ref`, so `f.c.Get()` binds directly — routing them through `&` would emit `Ꮡf.of(…)` but a value-ref receiver has no `Ꮡf` box) or a pointer *local* (handled above). (Guarded by the `AtomicFieldThroughPointer` behavioral test — a mutate-then-read proves the real field is updated, not a copy; runtime exercises this for atomic fields reached through pointer chains such as `sgp.g.selectDone.CompareAndSwap` and `gp.m.mLockProfile.recordLock`.)

The base may also be a pointer **rvalue** — a pointer-returning **call** (`getg().schedlink.set(…)`, `q.tail.ptr().schedlink.set(…)`, `Δp.chunkOf(ci).scavenged.setRange(…)`, `getg().m.p.ptr().wbBuf.get2()`) or a pointer **element index** (`batch[i].schedlink.set(…)`). Go auto-derefs the pointer to reach the value field, so the converter renders the read as `(~rvalue).field`; the `~` deref is an rvalue, so a pointer-receiver method on it cannot bind (`CS1510` on the generated `ref`). Unlike a deref-aliased *parameter* (whose box is `Ꮡp`) or a *field* deref (handled above), the call/index value **already is** the `ж<T>` box, so the receiver is materialized straight through it via the box-field accessor — `getg().of(g.Ꮡschedlink).set(…)`, `batch[i].of(g.Ꮡschedlink).set(…)` — never a `Ꮡ(value)` copy (which would lose the write). The routing is scoped to a base that is **not** an ident and **not** a field selector (those are the param/receiver/local/field cases above) and is **not a type conversion**: a conversion `(*T)(p)` renders as a C# *cast* (`(ж<T>)(uintptr)(…)`), a low-precedence form on which a trailing `.of(…)` would mis-bind to the inner operand, so a pointer-reinterpret keeps its existing `Ꮡ(…)` form (the runtime-unsafe S1 territory). (Guarded by the `PointerRvalueFieldReceiver` behavioral test — a pointer-receiver method on a value field reached through a returning call, a method-call chain, and a pointer-element index, each with write-through verified; runtime exercises this for `guintptr.set` via `getg()`/`batch[i]`/`q.tail.ptr()`, `pallocData.setRange` via `chunkOf`, and `wbBuf.get2`/`discard` via `getg().m.p.ptr()`.)

The bare-ident-base exclusion above holds **only for `[GoRecv] ref` methods** (which bind on the addressable value alias directly). A **direct-ж** (box-receiver) method — `func (s *scavengeIndex) find(…)` and the like, emitted with a `ж<T>` receiver — needs the *box*, so calling it on a value field-chain rooted at a deref-aliased pointer **parameter or (direct-ж) receiver** is `CS1929`: `Δp.scav.index.find(force)` (root `p`, a `*pageAlloc` receiver), `mp.trace.seqlock.Load()` (root `mp`, a `*m` parameter), `h.userArena.readyList.remove(s)`. These are routed through the box-field accessor too — `Ꮡp.of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).find(force)` — never a `Ꮡ(value)` copy (which would lose an atomic write). The `&`-machinery recurses through the value field-chain to the param/receiver box: `&Δp.scav.index` builds `Ꮡp.of(…).of(…)`, where the box base is the **raw** parameter name (`Ꮡp`, not the shadow-renamed `ᏑΔp` — a deref param `p`→`Δp` is `ref var Δp = ref Ꮡp.Value`, box `Ꮡp`). The routing is gated to direct-ж so a `[GoRecv] ref` method on the same chain keeps binding directly (no churn); a receiver root additionally requires the *enclosing* method to be direct-ж (only then does its receiver box `Ꮡrecv` exist). (Guarded by the `FieldChainBoxReceiver` behavioral test — a direct-ж method on a value field-chain rooted at a pointer parameter and at a direct-ж receiver, both with write-through verified; runtime exercises this pervasively for `scavengeIndex`/`mSpanList`/`timers` methods and `m.trace` atomic fields.)

For the **receiver-root** case, the enclosing method only *becomes* direct-ж through the capture-mode pre-pass's transitive fixpoint: a pointer-receiver method that calls a direct-ж method on a value field-chain of its own receiver — `func (p *pageAlloc) free(…) { … p.scav.index.free(…) }` — is promoted to direct-ж so its receiver box `Ꮡp` exists for the routing above. This detection walks the **full** value field-chain `recvName.f1.…fn.method` (every hop a value, non-pointer field), not just one level: `p.scav.index.free(…)` roots `free` at the receiver `p` through two value fields (`scav`→`index`). A one-level chain (`b.u.Load()` on an embedded atomic) was already detected; the multi-level walk generalizes it. A pointer field anywhere in the chain stops the walk — that subexpression is already a box and roots the call elsewhere (the pointer-field paths above), so it must not trigger promotion. The promotion is transitive: once `pageAlloc.free` is direct-ж, its caller `func (h *mheap) freeSpanLocked(…) { … h.pages.free(…) }` is in turn promoted (now calling a direct-ж method on `h.pages`), and so on up the call graph until a root holding the value through a real box/pointer. (The multi-level receiver-root promotion is covered by the `FieldChainBoxReceiver` test's `deep.bumpDeep` case — `d.mid.c.inc()`, a direct-ж `inc` on a two-level value field-chain of a receiver with no other direct-ж trigger, write-through verified; runtime exercises it on `pageAlloc.free`/`freeSpanLocked`.)

### Converting a Go pointer to `unsafe.Pointer`
`unsafe.Pointer` is the golib class `unsafe_package.Pointer : ж<uintptr>` (a numeric address wrapper). A `uintptr`/`unsafe.Pointer` argument converts through the implicit `uintptr ↔ Pointer` operators, but a **Go pointer** argument (`*T`, emitted as the managed box `ж<T>`) has no such conversion — a plain cast `(@unsafe.Pointer)(ж<T>)` is `CS0030` (when `T` is unrelated to `uintptr`) or a runtime `InvalidCastException` (the base→derived downcast `(@unsafe.Pointer)(ж<uintptr>)` compiles but the object is a plain `ж<uintptr>`, not a `Pointer`). So `unsafe.Pointer(ptr)` for a pointer `ptr` is emitted through the golib helper that pins the pointed-to storage:
```go
func (u *UnsafePointer) Load() unsafe.Pointer { return Loadp(unsafe.Pointer(&u.value)) }
```
```csharp
public static @unsafe.Pointer Load(this ж<UnsafePointer> Ꮡu) {
    ref var u = ref Ꮡu.Value;
    return (uintptr)Loadp(@unsafe.Pointer.FromRef(ref (Ꮡu.of(UnsafePointer.Ꮡvalue)).Value));
}
```
> The resulting numeric address is **not GC-stable** — the same caveat that applies to every `unsafe.Pointer`-as-`uintptr` use; the runtime intrinsics that consume it (e.g. `Loadp`, `StorepNoWB`) are assembly stubs, so this conversion is about producing compilable C#, not GC-correct pointer arithmetic. (The reinterpret pattern `*(*U)(unsafe.Pointer(&x))` is handled separately and is not affected.)

**A NIL pointer converts to address 0, not a throw.** golib's `ж<T> → uintptr` (and `ж<T> → void*`) operator takes the pointed-to storage's address via a `fixed` block — but a **nil** box has no storage to pin, so `&value.Value` dereferences it and throws. Go's `uintptr(unsafe.Pointer(nil))` is simply **0**, and the syscall wrappers pass nil pointers exactly this way: `syscall.Write` hands `writeFile` a nil `*Overlapped` for a synchronous write, then passes `uintptr(unsafe.Pointer(overlapped))` (= 0) to the `SyscallN` trampoline. The operators now return `0`/`null` for a nil box before pinning — so any converted `os.Stdout.Write` (hence `fmt.Println`) whose stdout is a pipe reaches the OS `WriteFile` and prints, instead of crashing on the nil-`overlapped` argument. (Guarded by the `NilPointerUintptr` behavioral **output** test — `uintptr(unsafe.Pointer(nilPtr)) == 0` and a non-nil control, vs Go.)

**Atomic pointer ops on a MANAGED pointer field read/write the reference, not a `uintptr`.** The lock-free-cache idiom `atomic.LoadPointer((*unsafe.Pointer)(unsafe.Pointer(&x.field)))` / `atomic.StorePointer(…, unsafe.Pointer(v))` — where `x.field` has type `*T` and so holds a `ж<T>` reference — cannot go through the literal conversion: `new @unsafe.Pointer(v)` round-trips the managed reference through its (transient) address, and `(ж<@unsafe.Pointer>)(uintptr)(FromRef(ref …field))` dereferences raw memory, losing GC identity (it NRE'd on the very first read — x/sys/windows's `LazyDLL`/`LazyProc` proc caches at package-init). `convCallExpr.managedAtomicPointerIdiom` recognizes the idiom (the callee is `sync/atomic.LoadPointer`/`StorePointer` and the argument is `(*unsafe.Pointer)(unsafe.Pointer(&Z))` with `Z` of pointer type) and emits golib's managed-referent overloads on the **field box** directly: `atomic.LoadPointer(Ꮡx.of(T.Ꮡfield))` → `ж<ж<T>>` → `Volatile.Read` returning `ж<T>`, and `atomic.StorePointer(Ꮡx.of(T.Ꮡfield), v)` → `Volatile.Write` of the plain `ж<T>` (the stored value unwrapped from its `unsafe.Pointer(…)` conversion). The overloads are additive — a `ж<ж<T>>` argument never matches the existing `ж<@unsafe.Pointer>` (`= ж<Pointer>`) signature, so ordinary `unsafe.Pointer` atomics are untouched. The load stays `unsafe.Pointer`-typed to Go, so a caller's `== nil` still renders `(uintptr)… == nil`; the `ж<T> → uintptr` operator (above) yields 0 for a nil box, so the nil test is correct with no change to the surrounding emission. Blast radius is only the packages using the idiom (x/sys/windows and a handful of stdlib sites), each a pure re-shaping to the managed overload; CNR byte-identical across the behavioral corpus. (Guarded by the `ManagedAtomicPointer` behavioral **output** test — a `*proc`-field lock-free cache initialized once and re-read, vs Go; it NRE'd before the fix.)

The `ref` the helper takes depends on how the pointer argument **renders**. A genuine box — an address-of expression, a local pointer variable, a pointer field, a call result — is the `ж<T>` object, so the ref goes through its boxed value: `FromRef(ref (box).Value)`. But a **deref-aliased** pointer — a pointer *parameter* or pointer *receiver*, which the body renders as the pointed-to value alias (`ref var p = ref Ꮡp.Value`) — is not a box; `.Value` on it is `CS1061` (`nuint` has no `Value` — runtime `select.go` `unsafe.Pointer(pc0)` and `heapdump.go` `unsafe.Pointer(pstk)`, both `*uintptr` parameters). The alias is itself a ref-local into the boxed storage, so the converter takes its ref directly: `FromRef(ref p)`. Detection reuses `exprIsDerefAliasedPointer` (the same discriminator the pointer-reinterpret block uses). This also let the `guintptr`/`muintptr` receiver family (`runtime2.go` `(*uintptr)(unsafe.Pointer(gp))` inside `guintptr.cas`) compile — previously `ref (gp).Value` bound the `[GoType]` wrapper's `Value` *property* (CS0206); the CAS it feeds (`atomic.Casuintptr`) is a `partial` asm stub, so the copy-box semantics match the established reinterpret precedent (compile-milestone bar; the faithful managed-referent `ж<T>` model for those types remains a separate effort). (Guarded by the `UnsafePointerParamPin` behavioral **output** test — the parameter and receiver shapes read through the pin and match Go, plus a field-address control that keeps the `(box).Value` form.)

**Returning an `unsafe.Pointer` parameter whole is a plain value return.** The return path boxes a *pointer parameter* returned whole (`return p` → `return Ꮡp` — the value alias cannot bind the pointer result), and the pointer-result check counts the `UnsafePointer` basic as a pointer. But an `unsafe.Pointer` parameter renders as a plain **value** param (`@unsafe.Pointer zero`) with *no* box, so the prefix referenced a nonexistent `Ꮡzero`/`Ꮡv`/`Ꮡfd` (CS0103 — runtime `map.go` `mapaccess1_fat`/`mapaccess2_fat`'s `return zero`, `mem_windows.go`, and `panic.go` `readvarintUnsafe`'s tuple return). The box form now applies only when the returned parameter's own type is a **genuine `*T`** (deref-aliased, so `Ꮡp` exists); an `unsafe.Pointer` param returns as-is. (Guarded by the `UnsafePointerParamPin` extension — the whole-return, tuple-return, and genuine-`*T`-control shapes, values vs Go; cleared 4 runtime CS0103, 63 → 59.)

The **reverse** direction — reinterpreting a raw address *as* a pointer, `(*T)(p)` where `p` is an `unsafe.Pointer` (or `uintptr`) — is the reinterpret pattern referenced above. Its result is the pointer type `ж<T>`. A plain `(ж<T>)p` cast is `CS0030`: because `unsafe.Pointer` is `Pointer : ж<uintptr>`, reaching `ж<T>` needs the two chained user-defined conversions `Pointer → uintptr → ж<T>`, and C# performs at most one user-defined conversion in a cast. The converter routes explicitly through `uintptr` — `(ж<T>)(uintptr)(p)` — which reads the `T` at `p`'s address via golib's `explicit operator ж<T>(uintptr value) => new ж<T>(*(T*)value)` (with `uintptr(Pointer) => Value`, the address the pointer holds). The deref `*((*unsafe.Pointer)(k))` then adds `.Value`: `((ж<@unsafe.Pointer>)(uintptr)(k)).Value` — Go's read of the `unsafe.Pointer` stored at `k`. This is the identical routing the *dereference* path (`(*int)(p)` inside `*(...)`) already used via its `isPointerCast` flag; the fix extends it to the two shapes that did **not** set that flag: a bare call **argument** `atomicwb((*unsafe.Pointer)(ptr), new)` (runtime `atomic_pointer.go`) and an **extra-paren** deref `*((*unsafe.Pointer)(k))` (runtime `map.go`'s indirect key — `convStarExpr`'s dereference branch sees a `ParenExpr`, not the `CallExpr`, so it never marks the cast). Gated to a **pointer-result** conversion whose **argument** is a raw address (`unsafe.Pointer`/`uintptr` basic); the pointer-to-*named*-type value conversion `(*Base)(defPtr)` (below) has a `*T` argument, is handled earlier, and is not affected. Like every reinterpret through the `uintptr` round-trip, the golib operator reads/boxes a **copy** from a `fixed` address, so this is memory-layout-dependent code whose runtime values are **not the contract** — golib's own `map<K,V>` is what actually runs; the converted `runtime/map.go` only needs to compile. (Guarded by the `UnsafePointerReinterpret` behavioral **Compile + Target** test — both the extra-paren deref and the bare-argument shapes; cleared all 21 `unsafe.Pointer → ж<unsafe.Pointer>` CS0030 in `runtime`, 137 → 114.)

A deref whose **starred inner is a func type** (or any non-identifier type) — `*(*func())(add(…))`, runtime `panic.go`'s deferred-slot read `return *(*func())(add(p.slotsPtr, i*…)), true` — misses the identifier-gated cast-deref branch and falls to the default deref path, which must **wrap the cast before `.Value`**: C# postfix binds tighter than a cast, so a naked `.Value` re-binds onto the cast's *inner* operand (`(ж<Action>)(uintptr)(add(…)).Value` reads the inner `@unsafe.Pointer`'s `uintptr` — CS0029 `ж<Action>`→`Action` in the tuple return). The default deref now wraps any type-conversion operand: `(((ж<Action>)(uintptr)(add(…))).Value, true)`. This is the fourth instance of the cast-precedence/extra-paren family, and **indexing** a reinterpret result directly is the fifth: `(*[2]uint64)(x)[0] = 0` (runtime `malloc.go`) appended the pointer-to-array auto-deref `.Value` and the index to the cast render — `(ж<array<uint64>>)(uintptr)(x).Value[0]` read the inner `@unsafe.Pointer`'s `uintptr` and indexed a `nuint` (CS0021); the index emission now wraps a type-conversion base the same way: `((ж<array<uint64>>)(uintptr)(x)).Value[0]`. (Guarded by the `UnsafePointerReinterpret` extensions — the func-type deref in a tuple return and the indexed reinterpret write/read.)

The unsafe builtins `unsafe.Add`, `unsafe.Slice`, and `unsafe.String` accept a length/offset of **any integer type** (Go's `IntegerType` constraint, which includes `uintptr`/`uint`). golib's implementations therefore take a generic `IBinaryInteger` length, truncated to the `int` offset — so `unsafe.Slice(p, uintptrLen)` binds without an explicit cast (a plain `nint` parameter rejected a `uintptr`/`uint` argument with CS1503). (Guarded by `UnsafeBuiltinIntegerLen`.)

Passing an `unsafe.Pointer` **argument to an `unsafe.Pointer` parameter** keeps the `@unsafe.Pointer` struct value — `add(p, x)`, not `add(p.Value, x)`. The struct is an exact match for the parameter. (Guarded by `UnsafePointerArgPassing`.)

**Array-backed defined types reinterpret through storage-sharing `Value` refs, not value copies.** The fiat field-arithmetic shape (crypto/internal/edwards25519 `scalar.go`) reinterprets `&s.s` (a `fiatScalarMontgomeryDomainFieldElement`, written directly over `[4]uint64`) as `(*[4]uint64)` — and as its *sibling* `(*fiatScalarNonMontgomeryDomainFieldElement)` — then **writes element-wise through the reinterpreted pointer** (`fiatScalarFromBytes` parses INTO `&s.s` on a virgin receiver). Neither the copy-boxing named↔named route (each `[GoType("[N]elem")]` wrapper converts only to `array<E>`; a sibling cast needs two chained user conversions — CS0030) nor a plain `ж<>` cast (distinct instantiations) works, and any copy-based route would materialize the wrapper's **lazy** backing on a temp and orphan every write. The emission derefs through the ref-returning `ж<T>.Value` and invokes the wrapper's `Value` property in place — `Ꮡ((Ꮡs.of(Scalar.Ꮡs)).Value.Value)` (underlying-array form) / `Ꮡ((nonMont)((…).Value.Value))` (sibling form, one implicit conversion from `array<E>`) — materializing the backing on the ORIGINAL storage and boxing an `array<E>` struct that shares its `T[]`: element reads and writes flow through. Gating consults the type's **written RHS** (a new per-package pre-pass records each `TypeSpec`'s declared right-hand side, which `Named.Underlying()`'s full resolution loses): only types written *directly* over an unnamed array take this route, so chain-defined view wrappers (`type pallocBits pageBits`) keep the existing copy-box route byte-identically; the same written-RHS gate lets `isTypeConversion` claim the pointer-to-type-literal target `(*[4]uint64)(…)` (no `types.Object` exists for a composite type) without disturbing the pointer-cast slice form (`(*[1<<20]Method)(p)[:n:n]`, internal/abi). Caveat (documented, no stdlib site): a *whole-value* write through the reinterpreted box (`*p = q`) rebinds only the boxed struct. (Guarded by the `NamedArrayWrapper` extensions — a virgin-field write through the underlying reinterpret, a sibling reinterpret aliasing the same storage read-during-write, and a heap-boxed local, all output-compared vs Go.)

**The `uintptr → ж<T>` raw-address reinterpret operator is `explicit` by design.** It boxes a **copy** of the value read at an arbitrary address (the runtime-unsafe reinterpret seam) — never something to happen silently, and every converter-emitted reinterpret already uses explicit cast syntax (`(ж<T>)(uintptr)(p)`). As an *implicit* conversion it also poisoned overload resolution: a `uintptr` argument converted to **both** an `@unsafe.Pointer` parameter (via the numeric `uintptr ↔ Pointer` operators, which stay implicit) and any `ж<T>` parameter, so a **free function and a same-named pointer-receiver method** — runtime's `func add(p unsafe.Pointer, x uintptr)` (stubs.go) vs `func (p *notInHeap) add(bytes uintptr)` (malloc.go), both emitted as static `add` overloads in the package class — were ambiguous (CS0121) at every free-call site whose argument is a **pin of a boxless receiver**: inside a `[GoRecv] ref` method, `unsafe.Pointer(b)` emits the `uintptr`-typed `(uintptr)@unsafe.Pointer.FromRef(ref b)` (runtime `map.go` `b.keys()`/`b.overflow()`/`b.setoverflow()`, `mprof.go`'s stack-record walkers — 6 sites). With the operator explicit, the `uintptr` argument binds only the `@unsafe.Pointer` overload. The reverse `ж<T> → uintptr` (box → address) operator remains implicit — producing a number is not a silent deref. (Guarded by the `FuncVsMethodOverload` behavioral **output** test — the free `add` + direct-ж method `add` overload pair with the boxless-receiver pin call shape, plus both method-call forms, values vs Go; cleared all 6 runtime CS0121, 59 → 53.)

**A cross-package type reference emits its `using <alias> = <namespace>;` even when the file did not import the package under a usable name.** A foreign type renders in short-alias form — `pkg.Type` (`time.Duration`, `abi.Kind`) for a named type, `@unsafe.Pointer` for the `unsafe.Pointer` basic — which resolves only through a file-local alias (`using time = time_package;`, `using @unsafe = unsafe_package;`). That alias is normally generated from a *canonical* (unaliased) `import`, but a file can reference a foreign type with no such import through three routes: **type inference** — a *same-package* function returns a foreign type, so the caller infers a local of that type but never writes `pkg.` and need not import the package (runtime `preempt.go`: `fd := funcdata(f, i)`, where `funcdata` returns `unsafe.Pointer`); a **blank import** (`_ "pkg"`, side-effects-only — **no `using` is emitted for it at all**: the old `using _ = <ns>;` emission hijacked C#'s `_` DISCARD for the whole file, so a deconstruction discard (`(w, _) = w.ensure(…)`, runtime `tracetime.go`) bound the namespace alias instead (CS0118 + CS0029); the import is recorded as a comment, and a genuine type reference still gets its canonical alias from this machinery — e.g. `symtabinl.go`'s `_ "unsafe"` for `//go:linkname`); or an **aliased import** (`import u "unsafe"`, whose alias `u` differs from the canonical `pkg.Name()` prefix the type reference uses). All previously yielded CS0246. The converter now walks every emitted type (`collectTypePackages`, called from `getTypeName` — named types by `pkg.Path()`, an `unsafe.Pointer` basic by the pseudo-path `"unsafe"`, recursing through pointer/slice/array/map/chan/generic/func-signature so a `[]time.Duration` element registers too) and, at file close (`visitFile`), supplies the canonical `using <alias> = <namespace>;` for every referenced foreign package the file did not already import canonically. It is idempotent-safe — a canonical import records its path in `canonicalAliasImported`, so `visitFile` never re-emits (duplicates) it — and a non-canonical alias (`using u = unsafe_package;`) coexists with the added canonical one without conflict. It is also **collision-guarded**: the synthesized `using <alias> = <namespace>;` is skipped when its canonical `<alias>` was already bound to a *different* namespace by a real import — cryptobyte's `asn1.go` imports both `encoding_asn1 "encoding/asn1"` (referenced by type, so it reaches this loop) and the subpackage `.../cryptobyte/asn1` (unaliased → alias `asn1`), so synthesizing `using asn1 = encoding.asn1_package` would duplicate the subpackage's `using asn1` (CS1537). The real imports' emitted aliases are tracked per file (`importAliasesEmitted`); the parent stays reachable through its `encoding_asn1` alias, so skipping the canonical one is safe (a non-colliding canonical alias is still supplied — no churn). (The *separate* defect that the type reference itself renders `asn1.ObjectIdentifier` rather than the file's `encoding_asn1.ObjectIdentifier` — `getTypeName` uses the canonical alias, not the file's non-canonical one — is tracked independently.) This is the *type-reference* analog of the method-call `addMethodPackageNamespaceUsing`. (Guarded by `UnsafePointerInferredNoImport` — the `unsafe.Pointer` basic arm, scalar/composite/blank-import variants — and `InferredForeignTypeNoImport` — the generic named arm, an inferred `*strings.Reader` in an `fmt`-only consumer.)

### Reinterpreting a pointer to a defined type with identical underlying — `(*Base)(p)`
A Go conversion `(*Base)(p)` where `p` is a `*Def` and `Base`/`Def` share an *identical underlying* type (one is a defined type over the other, e.g. `type pinnerBits gcBits`, or both over the same type) reinterprets the pointer. C# has no conversion between the two distinct generic instantiations `ж<Def>` and `ж<Base>`; only the `[GoType]` wrapper's **value** conversion `Def ↔ Base` exists. So the converter performs the reinterpret on the value and re-boxes it:
```go
func (s *mspan) newPinnerBits() *pinnerBits { return (*pinnerBits)(newMarkBits(s.nelems * 2)) }   // newMarkBits returns *gcBits
```
```csharp
internal static ж<pinnerBits> newPinnerBits(this ref mspan s) {
    return Ꮡ((pinnerBits)(~newMarkBits(((uintptr)s.nelems) * 2)));   // deref the ж<gcBits> box, value-convert, re-box
}
```
The argument is **dereferenced first** (`~box`) when it renders as a genuine pointer box — a call result, a local box, or a pointer field — because the value conversion operates on the underlying value, not on `ж<Def>` (a plain `(pinnerBits)(ж<gcBits>)` is `CS0030`). A deref-aliased pointer **parameter/receiver** already renders as the pointed-to value (`Δp`, not a box), so it value-converts directly with no `~` — the original `(*atomic.Uint32)(p)` receiver case (runtime/mprof `goroutineProfileStateHolder`). Both forms box a **copy** (`Ꮡ`): the shared underlying is the wrapped value, and a defined-over-struct wrapper holds it in a `readonly` field, so there is no write-through to lose; this matches the long-standing copy semantics of this branch (the runtime intrinsics behind these are assembly stubs). Both ships stay in managed `ж<>` land — no raw-address round-trip. (Guarded by `NamedPointerReinterpret`.)

The **third direction** — a pointer to a BASIC type reinterpreted to a defined type over that
basic — takes the same value-convert-and-re-box route: fmt's `(*stringReader)(&str)` (`type
stringReader string`) emits `Ꮡ((stringReader)(str))` — the address-of collapses with the value
deref, restricted to this arm so the long-guarded emissions stay byte-identical. Writes through
the box hit the copy, which is faithful for the pattern (the source string is never re-read).
Guarded by `NamedPointerReinterpret` (`tail`/`consume`). The **defer-wrapper receiver rule** is a
sibling of these box-form decisions: any function-level defer/recover wraps the whole method body
in the synthesized execution-context lambda, so a `ref T` receiver referenced inside is CS1628 —
`bodyWrappedInDeferContext` flips the method to the direct-ж receiver, whose deref alias emits
inside the wrapper (fmt `ss.Token`; guarded by `DeferCallOrder` `acc.add`).

The same block also covers a **named-numeric pointer reinterpreted to its underlying *basic* type** — `(*uint64)(head)` where `head` is a `*lfstack` (`type lfstack uint64`). This is the runtime's atomic-on-a-named-integer pattern: `atomic.Load64((*uint64)(head))` / `atomic.Cas64((*uint64)(head), …)` on the named atomic types **`lfstack`** (uint64, `lfstack.go`), **`sweepClass`** (uint32, `mgcsweep.go`), **`profAtomic`** (uint64, `profbuf.go`), and **`sysMemStat`** (uint64, `mstats.go`). `ж<lfstack>` and `ж<uint64>` are distinct generic instantiations with no conversion (`CS0030`), so the same value-convert-and-re-box applies — `atomic.Load64(Ꮡ((uint64)(head)))` — using the `[GoType("num:uint64")]` wrapper's `lfstack → uint64` value conversion. The reinterpret condition is generalized from *Named↔Named* to also fire when the **result** elem is a **basic** type whose underlying equals a **named** argument elem's (`namedToBasic`); the result C# type name comes from the result elem directly (`uint64`/`uint32`). Because it boxes a copy, a **read** through the reinterpret is faithful (golib `Load64` reads `Ꮡptr.Value` = the copy = the value), which is verified against Go; a **write** through it (`atomic.Store64`/`Cas64`/`Xadd64`) targets the copy, but those intrinsics are asm stubs in the converted runtime, so there is no faithful write-through to lose. Cleared all 13 `lfstack`/`sweepClass`/`profAtomic`/`sysMemStat` `→ ж<primitive>` CS0030 (runtime 114 → 101). (Guarded by `NamedNumericPointerReinterpret` — the read path across uint64/uint32 named types, values verified vs Go.)

### The club-41 mop-up batch (flag/flate/binary/syntax roots)
Nine coupled rules from the shallow-stack campaign:
- **Named func types implementing interfaces** (flag's `funcValue`): a delegate cannot be a
  partial struct — the generator routes Delegate records to the VALUE adapter
  (`new funcValueᴠValue(v)`), whose Go methods are package extensions binding on the wrapped
  copy; non-struct record kinds SKIP rather than throw (a throw kills the package's entire
  generator run). Guarded by `FirstClassFunctions` (`handler.tag`).
- **Ref receivers never take box renders**: a `[GoRecv] ref` receiver has NO box — the
  escape-heap and lambda-capture convIdent arms fall through to the value alias (flate init's
  `d.fill = (*compressor).fillStore` emitted a nonexistent `Ꮡd`, CS0103). Guarded by
  `FirstClassFunctions` (`worker`).
- **Func-field callees drive argument treatment**: `getFunctionSignature` resolves a
  FUNC-typed field's signature (`d.fill(d, b)` — the receiver arg renders as the box for a
  `ж<T>` slot, CS1503).
- **Integer wrappers carry the UntypedInt bridge** (`(token)(endBlockMarker)` — C# never
  chains two user conversions, CS0030). Guarded by `SortArrayType` (`levelToken`).
- **Package aliases shadowed by method names** qualify through the `_package` class
  (`sort_package.Sort(…)` — flate's `byLiteral.sort` bound the method group, CS0119).
  Guarded by `SortArrayType` (`PeopleByAge.sort`).
  A **Δ-renamed foreign CONST reached through that fallback** must still substitute the
  renamed member: the composed lookup key (`time_package.Second`) misses the alias map
  (keyed on the plain package name, `time.Second`), so `getAliasedTypeName` retries with
  the `PackageSuffix` stripped and, on a CONST hit, keeps the `_package` qualifier while
  substituting the alias — `time_package.ΔSecond` (crypto/tls's `Config.time` method ×
  time's `Second` const-vs-`Time.Second()` collision; the raw name bound the
  `Second(this Time)` extension method group, CS0019 ×2). Gated to consts: const entries
  exist only for collision-renamed members, while type entries cover every exported type,
  whose raw `_package`-qualified renders already bind. Guarded by
  `ShadowedImportConstLib`/`ShadowedImportConstUser` (the lib Δ-renames `Peak` for its own
  `Meter.Peak` collision; the user's `gauge.ShadowedImportConstLib` method shadows the
  import and `Span(2) * ShadowedImportConstLib.Peak` reaches the renamed const through the
  fallback, output-compared vs Go).
- **Blank params synthesize names when the body discards** (`_ = b[7]` bound the blank
  `littleEndian` receiver, CS0029) — encoding/binary's bounds-check hints. Guarded by
  `TypeSwitch` (`marker.tag`).
- **Defined-over-named-struct composites wrap the underlying** (`decoder{order: o}` →
  `new decoder(new coder(order: o))`, CS1739). Guarded by `NamedPointerReinterpret` (`view{}`).
- **Labeled switches declare their break target** (`break_BigSwitch:;` after the switch —
  both switch visitors now mirror visitForStmt, CS0159). Guarded by `SwitchBreakInCase`
  (`pick`).
- **Short declarations keep the named-numeric cast** (`p := printFlags(0)` re-imposes
  `((printFlags)0)`, CS1503) and **empty-interface switch tags compare via AreEqual**
  (`switch err := recover(); err { case ErrLarge: }`, CS0019).

### Slice-to-array conversions route golib's copy constructor
Go's slice-to-array conversions both route through `array<T>(slice<T> source, nint length)` —
a COPY constructor that panics Go-style on a short slice:
- the Go 1.20 **value** form `[4]byte(slice)` emits `new array<byte>(s, 4)` (netip
  `AddrFromSlice`, CS1955) — Go's conversion copies, so this is exactly faithful;
- the Go 1.17 **pointer** form `(*[32]byte)(slice)` emits `Ꮡ(new array<byte>(x, 32))`
  (edwards25519 `fiatScalarFromBytes`'s input, CS0030) — Go aliases the slice's backing here,
  the boxed copy does not; reads back through the same pointer stay faithful, and the corpus
  sites are read-only inputs. A NAMED-over-array target falls through unchanged (banked).
Guarded by `NamedPointerReinterpret` (`sliceToArray`).

### A direct-ж method on a value field-chain boxes through the &-machinery
A direct-ж (box-receiver) method called on a field of a plain VALUE param — netip's
`ip.addr.halves()`, where Go auto-addresses `&ip.addr` — routes the receiver through the
&-machinery: `Ꮡ(ip).of(ΔAddr.Ꮡaddr).halves()`. This boxes a COPY, which is faithful because
the enclosing Go value param is itself a copy: writes through the method could only ever reach
the local copy in Go too. (Pointer-rooted chains and indexed elements take their own
long-standing arms; this is the remaining value-rooted case.) Guarded by
`StructPointerPromotionWithInterface` (`rig`/`probeRig`).

### Field address of a collision-renamed heap-boxed local uses the raw box name
A heap box always keeps the RAW Go identifier (`ref var Δslice = ref heap<T>(out var <box>slice)`), so taking the address of a FIELD of a collision-renamed boxed local routes through `boxBaseName` -- the raw-name box, never the Δ-renamed alias (CS0103; reflect `SliceOf`'s `&slice.Type`). This matches the whole-value `&p` form and the renamed receiver/parameter boxes:
```csharp
internal static void bump(ж<nint> Ꮡnp) {
```
Guarded by `CollisionRenamedLocalBox` (`bump(&p.n)` on the renamed local `p`).

### A capture-mode method on a shadow-renamed heap-boxed local uses the rendered box name
A capture-mode method — one that escapes its receiver's address, e.g. `cryptobyte.Builder.AddASN1`, which hands `&b` to a callback — called on a heap-boxed VALUE local routes through the receiver box: `var b Builder; b.AddASN1(…)` → `Ꮡb.AddASN1(…)`. Unlike a deref-aliased pointer *parameter* (whose box keeps the RAW name, `Ꮡp`), a heap-boxed value LOCAL keeps its box under the RENDERED name — an escaping local is `ref var b = ref heap(new T(), out var Ꮡb)`, so when the local is SHADOW-renamed its box takes the renamed name. crypto/x509 `marshalCertificate`'s inner `serialiseConstraints` closure declares `var b cryptobyte.Builder`, renamed `bΔ1` to dodge the enclosing method's own `var b` declared LATER (a C# lambda cannot re-declare an enclosing-scope local, CS0136); its box is `ᏑbΔ1`. Emitting the raw-name box `Ꮡb` there both mis-references the outer `b`'s box (declared later in the method → CS0841/CS0103) and, where a same-named outer box does resolve, calls the method on the wrong operand — go/types `conversions.go` called `x.convertibleTo` on the receiver box `Ꮡx` instead of the inner operand box `ᏑxΔ2`:
```csharp
ref var bΔ1 = ref heap(new cryptobyte.Builder(), out var ᏑbΔ1);
…
ᏑbΔ1.AddASN1(cryptobyte_asn1.SEQUENCE, (ж<cryptobyte.Builder> bΔ2) => { … });   // was Ꮡb (CS0841)
```
The receiver-box render resolves the box base through `boxBaseName` with the lambda capture-remap DISABLED, so it yields: the shadow-rendered *declaring* name (`bΔ1`) for an escaping local; the raw name (`Ꮡp`) for a pointer parameter; and — critically — the *declaring* name for a variable CAPTURED by the closure, not its value-snapshot capture name. A heap-boxed local captured by a closure has its box captured directly (`Ꮡonce` in sync `OnceFunc`'s returned closure), so the capture-remapped `Ꮡonceʗ1` (a non-existent box) must not appear. Guarded by `ShadowedHeapBoxReceiver` (an inner closure's `var b` capture-mode method, shadow-renamed against an outer same-named `var b` declared later).

### Nested dereferences parenthesize before the outer `.Value`
A deref whose operand is ITSELF a deref renders with the prefix `~` form, on which a naked postfix `.Value` mis-binds (postfix beats unary: `~X.Value` is `~(X.Value)`). The outer deref wraps the inner one -- reflect `MapOf`'s `**(**mapType)(unsafe.Pointer(&imap))`:
```csharp
var back = (~(ж<ж<array<nint>>>)(uintptr)(@unsafe.Pointer.FromRef(ref (Ꮡ(ip)).Value))).Value;
```
Guarded by `PointerCastSliceRange` (compile-shape).

### Function literals returning `unsafe.Pointer` state their return type
A literal with a single `unsafe.Pointer` result can mix return arms of DIFFERENT C# types (reflect `deepEqual`'s `ptrval`: `(uintptr)v.pointer()` on one arm, the raw `v.ptr` on the other), which defeats C# lambda return-type inference (CS8917). The emitted lambda states its return type explicitly; each arm then converts implicitly through the golib operators:
```csharp
var pick = @unsafe.Pointer (bool u) => {
```
Guarded by `PointerCastSliceRange` (compile-shape).

### Interface-returning literals with distinct arm types state their return type too
The same inference gap hits an interface result whose arms return DIFFERENT concrete types — net ipsock.go's `inetaddr := func(ip IPAddr) Addr` returns three pointer-adapter classes (`TCPAddrжΔAddr` / `UDPAddrжΔAddr` / `IPAddrжΔAddr`), which share only the interface (CS8917). When a single non-empty-interface result's return arms carry two or more distinct types, the lambda states the return type explicitly (`Addr (IPAddr ip) => …`); each arm then converts implicitly. Single-typed literals keep the inferred form (zero churn). (Guarded by the `InterfaceCasting` extension `makeAnimal` — an adapter arm plus a value arm, runtime-verified.)

### Multi-value literals with no fully-typed arm state their return type — named results included
The single-result inference gaps above generalize to any MULTI-result literal where EVERY return arm carries a typeless element — `return nil, nil, nil, nil, err` on the error arms and `return dnsNames, ips, emails, uriDomains, nil` on the success arm (crypto/x509 `parseNameConstraintsExtension`'s `getValues := func(subtrees) (dnsNames []string, ips []*net.IPNet, emails, uriDomains []string, err error)`). A C# tuple literal with any untyped element has no natural type, so no arm fixes the lambda's return type and delegate-type inference fails (CS8917). The lambda states its tuple return type explicitly, and each `nil` then takes its target element type:
```csharp
var getValues = (slice<@string> dnsNames, slice<ж<net.IPNet>> ips, slice<@string> emails, slice<@string> uriDomains, error err) (cryptobyte.String subtrees) => { … };
```
NAMED results are now included (they were previously excluded): the trigger — a multi-result literal with a return arm but NO fully-typed arm — is identical whether the results are named or not. A bare `return` (which returns the named results) never matches the result arity, so it neither marks has-return nor a false fully-typed arm; a named literal that DOES have a fully-typed explicit arm keeps inferred typing (no return-type prefix, no churn). Guarded by `NamedResultLambdaInfer` (a five-result named-result closure whose error arms return `nil,nil,err` and success arm `e,o,nil`).

### String-returning literals in assignment position state their return type
A literal with a single Go `string` result can mix return arms of DIFFERENT C# types even though every arm is a Go string: a bare string literal is a `"…"u8` `ReadOnlySpan<byte>`, a literal+variable concat binds golib's `operator +(@string, @string)` (so it is `@string` regardless of u8 suppression), and a call into a hand-written stub can return C# `string` (the baseline `fmt.Sprintf` does). `@string` and `string` convert implicitly in BOTH directions, so a lambda mixing those arms has no unique best common type and its delegate type is not inferable — CS8917 on `pick := func(v any) string {…}` whose `case string:` arm returns `"string:" + t` alongside `fmt.Sprintf` arms. In assignment position (`var pick = …`, where C# must infer the delegate type), the lambda states its return type explicitly and each arm then converts to `@string` in place:
```csharp
var pick = @string (any v) => {
```
Argument/return/composite-element literals are target-typed by their receiving delegate type (no inference to fail — and an explicit return type could only add an identity-match constraint against stub delegate types), so they keep the plain form; the Go `var` declaration form emits an explicit delegate type (`Func<@string, bool, @string> pad = …`) and is likewise immune. Gated to the basic string kind — a named string type would need its own conversions. Guarded by `FuncLitStringConcatReturn` (`:=` literals mixing concat, u8-literal, and stub-`Sprintf` arms — including a type-switch body and a right-side literal concat — plus the `var` form; runtime-verified).

The TUPLE-ELEMENT sibling: in a MULTI-result literal, a bare string literal element is worse than typeless — it is *wrongly* typed. Inside a tuple the literal emits as a bare C# `string` (u8 spans cannot be tuple elements), so an arm with no `nil` and no string variable — internal/fuzz `fuzzOnce`'s `return dur, coverageSnapshot, ""` (`func(entry CorpusEntry) (dur time.Duration, cov []byte, errMsg string)`) — counted as "fully typed" in the multi-result scan above and suppressed the explicit tuple return type, letting inference *succeed with the wrong element type*: the destructured `errMsg` was C# `string`, which has no `!=` against a `"…"u8` span (CS0019 rather than CS8917). A basic-string constant literal element whose declared result element is Go `string` now also marks its arm not-fully-typed, so the same explicit-tuple emission fires:
```csharp
var fuzzOnce = (time.Duration dur, slice<byte> cov, @string errMsg) (CorpusEntry entry) => { … };
```
and each `""` converts to `@string` in place via target typing. Same assignment-position gate; a literal whose string elements are all variables keeps inferred typing (the full-stdlib A/B footprint was exactly internal/fuzz worker.cs plus two latent-identity repairs, internal/coverage/decodecounter `sget` and net ipsock.go `addrErr`, both re-proven green). Guarded by the `FuncLitStringConcatReturn` extensions `fuzzish` (named results, `!= ""` on every destructured element) and `sget` (unnamed `(string, error)`); the pre-fix converter fails them with exactly CS0019 ×4.

The NUMERIC sibling: an untyped numeric constant literal element against a differently-SIZED declared result element is wrongly typed the same way. The literal emits bare, so the arm infers the literal's natural C# type — an INT literal is C# `int`, a FLOAT literal C# `double` — where the Go result is e.g. `int64`: net/http ServeContent's `sizeFunc := func() (int64, error) { …; return 0, errSeeker }` had no `nil`/string-literal element on its error arms, counted "fully typed", and inferred `Func<(int, error errSeeker)>` — rejected at the `serveContent(…, sizeFunc, …)` call because delegate types are invariant (CS1662/CS0029/CS1503, and the leaked `errSeeker` element name rides the inferred tuple). Such an element now also marks its arm not-fully-typed, so the same explicit-tuple emission fires:
```csharp
var sizeFunc = (int64, error) () => { …; return (0, errSeeker); };
```
A declared element the literal's natural type already matches (`int32` for an INT literal, `float64` for a FLOAT literal) infers correctly and stays inferred, and Go `int` (C# `nint`) is deliberately exempt — `return 0, err` against `(int, error)` results is pervasive and green today (the element converts implicitly at every use site), so marking it would churn stdlib-wide for no observed defect, the same reasoning that keeps `lambdaConstReturnCastType` away from signed single results. A SUB-negated literal (`return -1, …`) is unwrapped and marked the same way. (Full-stdlib A/B footprint: net/http fs.cs `sizeFunc` — the target — plus three latent same-shape repairs, internal/coverage/decodecounter `rdu32` ×3 and net/http h2_bundle `allocatePromisedID` (both `(uint32, error)`) and internal/zstd `fetchHuff` (`(uint16, error)`). Guarded by the `FuncLitNumericTupleReturn` behavioral test — the sizeFunc shape and a float64 shape both PASSED to typed function parameters, the `-1` arm, and the int/float64-identical controls that must keep inferred typing; the pre-fix converter fails it with exactly the fs.cs trio CS1662/CS0029/CS1503 ×2.)

### A GoImplement record is gated on the method set actually satisfying the interface
Every `[assembly: GoImplement<T, Iface>]` record makes the `ImplementGenerator` emit implementation glue whose members forward to T's like-named methods — so a record whose Go method set does NOT satisfy the interface generates a forwarder to a method that does not exist. The corpus case: net/http's `err = http2GoAwayError{LastStreamID: …, ErrCode: cc.goAway.ErrCode, …}` — the keyed composite's sparse-array `ident` context leaks the `error`-typed LHS onto each FIELD value, and the `ErrCode` field's value recorded `GoImplement<http2ErrCode, error>` even though `http2ErrCode` has only `String()`/`stringToken()` (its generated `Error() => this.Error()` was CS1929). `convertToInterfaceType` now folds a `types.Implements` check over the recorded form's method set (T for a value record, `*T` for a `ж<T>` record) into `recordableBase`, which gates both the record and the matching adapter-wrapping emissions. A conversion the Go checker admitted always passes the check, so the gate can only drop pairs a caller composed from mismatched types; a type-param-carrying target skips the check (`types.Implements` is undefined for uninstantiated generics, and the open-generic conversion emission must stay). The full-stdlib A/B for this change is exactly one removed line — the false `http2ErrCode` record. (Guarded by the NEGATIVE `KeyedLiteralIfaceAssign` behavioral test: a keyed literal assigned to an `error` variable whose field-value type has `String()` but no `Error()` — a reintroduced record fails the compile phase.)

### Promoted methods through a FOREIGN pointer embed forward via the embedded box
A struct embedding a FOREIGN pointer (`net/http`'s `http2timeTimer struct { *time.Timer }`, `net/http/internal`'s `FlushAfterChunkWriter struct { *bufio.Writer }`, `bufio.ReadWriter { *Reader; *Writer }`) promotes the embed's pointer-receiver methods, but those land as ж-extensions visible to the consuming assembly only through METADATA — direct-ж primaries (`Reset(this ж<Timer> …)`) and the public `RecvGenerator` twins of `[GoRecv]` methods (`Write(this ж<Writer> …)`). The `ImplementGenerator`'s syntax-tree hop scan saw none of them, so the value-form partial deref'd to the value (`this.Timer.Value.Reset(d)` — CS1929, the extension receiver strands) and the FOREIGN-struct pointer adapter fell back to the struct itself (`m_box.Value.Write(p)` — CS1061, `ReadWriter` declares nothing). Both arms now probe the embed's type SYMBOL: the single-hop paths union the foreign element's metadata box methods into the hop's box-method set (`this.Timer.Reset(d)`; `m_box.Value.Writer` in the local pointer arm), and the foreign-struct pointer arm routes each member still on the plain fallback through the UNIQUE pointer embed whose metadata box methods declare it (`m_box.Value.Writer.Write(p)`; Go's depth-one promotion ambiguity rules make the unique-embed requirement faithful). An embed package class outside extension-lookup reach (not the emitting namespace, the shared root `go`, or an enclosing segment) forwards through its package-class static with the box as the receiver argument, mirroring the foreign-extension arm. (Guarded by the `ForeignPtrEmbedIfaceLib`/`ForeignPtrEmbedIfaceUser` pair — a local struct embedding a foreign pointer adapted by value AND by pointer, plus a foreign two-pointer-embed struct adapted by pointer, output-compared vs Go.)

### Promoted methods through embedded INTERFACE fields route per-member to the declaring field
A struct whose embeds are INTERFACE fields (httputil's `dumpConn struct { io.Writer; io.Reader }` adapted to `net.Conn`) satisfies interface members through the fields' method sets. The pointer adapter's embedded-interface-field arm was gated to a SINGLE field, so a multi-field struct got no forwarding at all (`m_box.Read(…)` — CS1061). The arm now resolves per member: each still-unbound interface member forwards through the UNIQUE embedded field whose interface declares it (`m_box.Value.Reader.Read(p)` / `m_box.Value.Writer.Write(p)`); a member declared by several fields is left unbound (Go's promotion ambiguity rules reject it unless the struct overrides, and a struct override is already resolved earlier). The single-field behavior is unchanged (zip's `nopCloser`, slogtest's Δ-renamed `Handler` field). (Guarded by the `IfaceFieldEmbedAdapter` behavioral test — a two-interface-field struct adapted by pointer to a third interface needing members from both fields plus one declared on the struct, output-compared vs Go.)

### A pointer parameter used only through its box gets no deref VALUE alias
Every named pointer parameter is emitted as its box `ж<T> Ꮡp` with an entry-time value alias
`ref var p = ref Ꮡp.Value` (so a value use `p.field` reads through `p`). But a parameter that the body
touches **only** through its box — `unsafe.Pointer(p)` → `new @unsafe.Pointer(Ꮡp)`, `p == nil` →
`Ꮡp == nil`, or passing `p` on as a `*T` argument → `Ꮡp` — never references that value alias, so the
alias is a dead local that nonetheless **dereferences the box** at function entry. When the argument is
nil this NREs even though Go never touches the pointee: `syscall.writeFile(…, overlapped *Overlapped)`,
called with a nil `overlapped` and using it only as `unsafe.Pointer(overlapped)`, crashed at
`ref var overlapped = ref Ꮡoverlapped.Value` — the failure of any converted `fmt.Println` whose stdout is
a pipe (`syscall.Write` → `writeFile`). The converter already skips the alias for an **unnamed/blank**
pointer param (never referenced); this extends it to a **named** param whose value alias is likewise
unreferenced. After the body is converted, `bodyReferencesIdentAsValue` scans the emitted body text for
the value-alias name as a **standalone identifier** — the address marker `Ꮡ` is a Unicode *letter*, so
the box form `Ꮡp` is excluded by the preceding-letter boundary, while a genuine value use always emits
the bare name and matches. The scan only ever ADDS spurious matches (a field selector `x.p`, a string, a
comment), which keep the alias, so a live alias is never dropped (no CS0103) — it removes strictly unused
locals. Because a param with no alias leaves `implicitPointers` empty (which otherwise triggers the
signature rebuild that renames pointer params to the box `Ꮡ<name>` the body references), a
`skippedDeadPointerAlias` flag forces that rebuild. Full-stdlib/behavioral blast radius is broad (72
behavioral files — many functions forward a pointer param onward as a pointer), all pure alias removals.
This shares its root with the receiver case below (an eager deref alias NREs on a nil pointer); the
converse **live**-alias nil case — invoking a pointer method/function that *does* dereference a nil
receiver/param — still NREs at the alias, awaiting the nil-safe `DerefOrNil` extension. (Guarded by the
`DeadPointerParamAlias` behavioral test — a pointer param used only in `p == nil`, one forwarded as a
pointer, and one dereferenced (alias kept), each called with nil and non-nil, output-compared vs Go; the
nil calls NRE'd before the fix.)

### A pointer RECEIVER compared to `nil` compares its box, not its deref'd value
A method with a pointer receiver — `func (f *File) checkValid() error { if f == nil { … } }` — is emitted
as `checkValid(this ж<File> Ꮡf, …)` with the body alias `ref var f = ref Ꮡf.Value`. Go's `f == nil` is a
**pointer** comparison (nil-pointer check — and Go legitimately calls methods on nil-pointer receivers), so
it must compare the box `Ꮡf == nil`, not the deref'd struct value `f`. Emitted in value form, `f == nil`
binds the generated `File.operator==(File, NilType)` — which compares against `default(File)` and, for a
**promoted-embed** struct (`File` embeds `*file`), dereferences that zero value's null embed box → a
`NullReferenceException` (the first crash of a converted `fmt.Println`, via `os.Stdout.Write` →
`checkValid`); even for a plain struct it is the wrong answer (`&box{}`, a non-nil pointer to a zero struct,
compares *equal* to `default(box)` → `true` where Go gives `false`). The converter already forced the box
form for a deref'd pointer **parameter** (`isDerefdPointerParamIdent`, driving the `==`/`!=` operand
context in `convBinaryExpr`); the receiver is deliberately not a "parameter" in that model
(`paramNames` excludes the receiver), so it needs its own recognizer, `isDerefdPointerReceiverIdent`
(object-identity match via `isPointerReceiver`/`identResolvesToReceiver`, so a local shadowing the receiver
name keeps its own render). It is scoped to the comparison operands only — unlike a pointer parameter the
receiver is **not** folded into `nilSafePtrParamNames`, so its deref-alias form is unchanged (only the
receiver's `==`/`!=` operand switches to the box); `convIdent` renders `Ꮡf` for the receiver through its
existing direct-ж-receiver arm. Like a deref'd pointer parameter, the box form applies to **every**
`==`/`!=` comparison, not just against nil: a pointer receiver can only be `==`-compared to another
pointer or to nil, so the box is always the correct (pointer-identity) operand — `func (b *Reader)
Reset(r io.Reader) { if b == r … }` (bufio) becomes `AreEqual(Ꮡb, r)`, comparing the pointer to the
interface's held pointer, where the pre-fix `AreEqual(b, r)` compared a *struct value* to the interface
(never equal — a latent recursion-guard bug the fix also closes). The full-stdlib A/B is small and
mechanical — 161 receiver operands across 77 files gain a `Ꮡ` box (155 nil-checks, 6 pointer-identity),
every changed line adding only the box.
A still-open **separate** gap: invoking a pointer method on an actually-nil receiver NREs at the deref alias
`ref var f = ref Ꮡf.Value` before the guard runs (the nil-safe `DerefOrNil` accessor covers this for pointer
*parameters* walked to nil but is not yet extended to receivers). (Guarded by the `PointerReceiverNilCompare`
behavioral test — a plain-struct `*box` where `&box{}` must compare `!= nil`, `== nil`/`!= nil` receiver
methods, and a promoted-embed `*embedder` whose pre-fix value comparison NRE'd, output-compared vs Go; and
by the re-baselined `RingPointerMethods` whose `r != nil` receiver walk now renders `Ꮡr != nil`.)

## Implicit Pointer Dereferencing
**Deciding whether a selector base is *already* dereferenced.** A field selector on a pointer-valued base auto-derefs in Go, so the converter must insert the deref (`(~x).field` / `x.Value.field`) — *unless* the base is itself an explicit dereference (`(*p).field`) or a pointer conversion whose dedicated branch appends its own `.Value`. That "is the base already deref'd" test was a whole-subtree scan for **any** `StarExpr`, which mistook a conversion star buried in a call **argument** for a dereferenced base — `stringStructOf((*string)(unsafe.Pointer(p))).n` (runtime `arena.go`): the `(*string)` star belongs to the argument's conversion, the *call result* (`ж<stringStruct>`) is not deref'd, and skipping the auto-deref left `.n` on the box (CS1061). The test now inspects only the base's own outermost shape (unwrapping parens; a pointer-conversion base still routes to the conversion branch), and the conversion-branch dispatch also unwraps **enclosing parens**, so an extra-paren conversion base — `((*specialWeakHandle)(unsafe.Pointer(…))).handle` (runtime `mheap.go`) — reaches it (the same extra-paren blind spot the reinterpret routing had). Reads through a conversion base are faithful; a **write** through one hits the copy box, the documented reinterpret-seam limitation shared by the whole `(ж<T>)(uintptr)` family (the runtime sites are reads). The corpus was byte-identical across all behavioral projects after the change — only previously-non-compiling shapes gained emissions. (Guarded by the `PointerSelectorDeref` behavioral test — both shapes, read values vs Go; cleared 3 runtime CS1061, 74 → 71.)

In Go, pointer types automatically dereference; these `age` assignments are equivalent:
```go
var s struct{ age int }
var ps = &s
(*ps).age = 20
ps.age = 20
```
This also applies to receiver methods — a value-receiver method works on the type *and* on a pointer to it. In practice, the converter handles implicit dereferencing of a pointer parameter by binding a `ref` local to the box's value. For example:
```go
func PrintValPtr(ptr *int) {
    fmt.Printf("Value available at *ptr = %d\n", *ptr)
    *ptr++
}
```
becomes:
```csharp
public static void PrintValPtr(ж<nint> Ꮡptr) {
    ref var ptr = ref Ꮡptr.Value;

    fmt.Printf("Value available at *ptr = %d\n"u8, ptr);
    ptr++;
}
```

A pointer **local** that holds a `ж<T>` box (e.g. `x := list.head`, where `head` is a `*node`) dereferences on field access through the box — a read becomes `(~x).field` and a write `x.Value.field = …`. This applies to **promoted** fields too: when `T` embeds another struct, a selector naming an embedded field (`x.next` where `next` is promoted from an embedded header) must still dereference. The converter decides this by checking field membership recursively through embeds, so a promoted-field access on a pointer local is not left as a bare `x.next` on the box (which has no such member, CS1061). This mirrors the Go runtime's `scanstack`, which walks `x := state.head; … x.nobj` where `nobj` is promoted into `stackObjectBuf` from an embedded header.

When the field access is the **LHS of an assignment** and the chain is *nested* — `o.stack.hi = …` where `o` is a pointer local and `stack` is a value-struct field — every dereference in the base must use the assignable `.Value` form, not `~`: `(~o).stack` yields a value (an rvalue), so assigning to a field through it is not a variable/property (CS0131). The converter propagates the assignment context down the selector chain, emitting `o.Value.stack.hi = …`. This mirrors runtime/cgocall.go's `g0.stack.hi = sp + 1024` where `g0` is a `*g` local.

The same applies to **`++`/`--`** on a field reached through a pointer local — increment/decrement reads *and* writes its operand, so `(~mp).ncgocall++` (a field of an rvalue) is CS1059. The converter emits the assignable `mp.Value.ncgocall++`.

**An INDEX-expression assignment target takes the same `.Value` write path.** When the assignment LHS
is an index over a field reached through a pointer local — net/http client.go's redirect loop
`req.Header[k] = vv`, `Header` a named-map wrapper field — the read form `(~req).Header[k] = vv`
indexer-sets a wrapper-STRUCT field of an rvalue copy (CS0131; httptest's recorder hit the same shape).
The assignment threads a dedicated `IndexExprContext.isAssignmentTarget` flag (distinct from the
general assignment context, which also rides along RHS conversions where an index READ must keep the
deref form), and `convIndexExpr` converts its BASE in assignment context: `req.Value.Header[k] = vv;`.
Compound assignment (`m[k] += 2`), `++`/`--` on an index operand (`m[k]++` via `visitIncDecStmt`), and
tuple-deconstruction elements (`(config.Value.Certificates[0], err) = …`, net/http server.cs) take the
same path. The write form is emitted for EVERY boxed index-assignment base (75 stdlib files) — for
reference-backed fields (plain maps, slices, golib arrays) both forms compile and write through the
same backing store, so this is churn-free semantically; the named-wrapper fields are the shapes that
did not compile. (Guarded by `BoxedMapFieldWrite` — named-map-wrapper writes, plain-map writes,
compound/inc-dec writes, and an array-field element write, all through a pointer local, values
output-compared vs Go.)

**Dereferencing a pointer FIELD reached through a parameter — `*p.field`.** A `*p` where `p` is a pointer parameter is emitted as the value alias `p` itself (the `ref var p = ref Ꮡp.Value` local already denotes the pointed-to value), so the converter has a parameter-deref shortcut. That shortcut must fire only when the operand *is* the parameter (`*p`, or `**p`): for `*p.field` — a deref of a pointer *field* reached through `p` (`*gp.ancestors`, where `ancestors` is a `*[]ancestorInfo`) — the operand `p.field` is a distinct lvalue that still needs its own dereference. The shortcut keyed off the *root* identifier (`getIdentifier` digs through the selector to `p`), so it wrongly dropped the field deref, emitting `gp.ancestors` (the `ж<…>` pointer) instead of `gp.ancestors.Value`. That silently fed a pointer where the pointed-to value was expected — `for _, a := range *gp.ancestors` ranged the box (CS8130, since a `ж<slice<…>>` is not enumerable as tuples), and `x := *p.cnt` typed a pointer as a value (CS0029). The shortcut now excludes a **selector** operand, so `*p.field` falls through to the selector-deref path and renders `p.field.Value`. (Guarded by the `DerefPointerToField` behavioral test — a `for _, x := range *h.xs` over a deref'd pointer-to-slice field and a `*h.cnt` value read, both through a pointer parameter; runtime hit this on `traceback.go`'s `range *gp.ancestors`.) An **index** operand rooted at a parameter (`*temps[depth]`, math/big's slice-of-pointers element deref) is excluded the same way.

The **receiver** flavor of the same shortcut (`*u` inside `func (u *unifier)` → the deref-aliased `u`) had the identical overreach: it keyed off the root identifier, so `*u.handles[x]` — a deref of a **pointer-valued map element** reached through the receiver (go/types unify.go's `return *u.handles[x]` and `*u.handles[x] = t`, `handles` a `map[*TypeParam]*Type`) — dropped the element deref entirely, returning/assigning the raw `ж<ΔType>` (CS0266 in both directions). The receiver shortcut is now gated on the operand *being* the receiver ident (object identity, like every other receiver-specific render), and the non-direct operand falls through to the tail deref: `u.handles[Ꮡx].ValueSlot` (`ValueSlot` because the element's pointee is an interface — reference-like reads and writes both persist through the real slot). (Guarded by the `RecvMapElementDeref` behavioral test — element deref read and write through the receiver, the write observed through the shared pointer, alongside the genuine `return *r` receiver copy, output-compared vs Go.)

## Break / Continue Labels
Go restricts a label to immediately precede the enclosing statement (e.g. a `for`). Equivalent behavior is produced with a placed label and a `goto`:

### Break Label
```go
OuterLoop:
    for i = 0; i < n; i++ {
        for j = 0; j < m; j++ {
            switch a[i][j] {
            case nil:
                state = Error
                break OuterLoop
            }
        }
    }
```
becomes (the label is emitted as `break_OuterLoop:`):
```csharp
    for (i = 0; i < n; i++) {
        for (j = 0; j < m; j++) {
            switch (a[i][j].type()) {
            case nil:
                state = Error;
                goto break_OuterLoop;
            }
        }
    }
break_OuterLoop:;
```

### Continue Label
```go
RowLoop:
    for y, row := range rows {
        for x, data := range row {
            if data == endOfRow {
                continue RowLoop
            }
            row[x] = data + bias(x, y)
        }
    }
```
becomes (`continue_RowLoop:` placed at the end of the *labeled* loop's body — so `goto continue_RowLoop` from the inner loop lands there and the **outer** loop proceeds to its next iteration; `break_RowLoop:` would go after the outer loop):
```csharp
    foreach (var (y, row) in rows) {
        foreach (var (x, data) in row) {
            if (data == endOfRow) {
                goto continue_RowLoop;
            }
            row[x] = data + bias(x, y);
        }
continue_RowLoop:;
    }
```
Both the `break_<label>`/`continue_<label>` labels are emitted for a labeled `for` **and** a labeled `range`/`foreach` loop (the label target is placed regardless of loop kind; a missing one is CS0159 "no such label"). Guarded by the `ForVariants` behavioral test (labeled `range` with nested `continue`/`break`).

### A user label on an empty statement emits an explicit empty statement

A Go label can attach to an **empty statement** — `keep:` as the last line of a block, a
`goto`/`break`/`continue` target with nothing between the label and the closing brace
(internal/trace gc.go's `goto keep` target at the tail of a for-loop body). A C# label must
precede a statement, so a bare `keep:` before `}` is CS1525/CS1002. `visitLabeledStmt` detects
an `*ast.EmptyStmt` target and emits the explicit empty statement `keep:;` — the same shape the
`break_<label>:;`/`continue_<label>:;` synthesis already uses. A label on a non-empty statement
is unchanged (`big:` followed by its statement stays bare). Guarded by `LabeledEmptyStmt` (a
`goto` to an end-of-loop-body label, a `goto` to an end-of-function label, and a `goto` to an
end-of-inner-block label — values vs Go).

### Reassigned or ref-bound range variable
A C# `foreach` iteration variable is **read-only**, but Go lets a `range` key/value variable be reassigned inside the body (it is a per-iteration copy). When the converter detects such a reassignment (`=`, `+=`, `-=`, `++`, …) of a newly-`:=`-defined range variable — or a **pointer-receiver method selected on the value-typed range var** (`q.GoString()`, whose emitted `[GoRecv]` form takes `this ref T`; a foreach var cannot bind `ref` — CS1657, dnsmessage's four `Message.GoString` loops) — it iterates a temp and declares the variable as a mutable local copy in the body, rather than binding it directly. The per-iteration `var q = vᴛ1;` copy preserves Go's semantics exactly: the pointer-receiver mutates the copy, as Go's implicit `(&q)` on the range copy does. A pointer-*typed* range var is excluded (it dereferences; no ref bind), as are value-receiver and interface-method selections. The machinery covers string AND slice/array/map ranges:

```go
for _, r := range s {  // r is a rune
    if r >= 0x10000 {
        r -= 0x10000   // reassigns the range variable — CS1656 on a foreach var
        …
    }
}
```
```csharp
foreach (var (_, rᴛ1) in s) {
    var r = rᴛ1;       // mutable local copy
    if (r >= 65536) {
        r -= 65536;
        …
    }
}
```
A range variable that is only *read* keeps binding directly to the `foreach` tuple (no temp, no churn). This reuses the same temp-var/`innerPrefix` machinery as the `for k, v = range` (re-assign-into-existing-vars) form. (Guarded by the `RangeVarReassign` behavioral test; runtime hits this in `os_windows`'s UTF-16 surrogate-pair encoder.)

### For-clause variables are per-iteration (Go 1.22 loop-variable semantics)

Go 1.22 gives each iteration of a `for i := …; cond; post` loop its **own copy** of the clause-declared variables, initialized from the previous iteration's final value. A C# for-clause variable is ONE variable shared by every iteration, so a closure captured in the body would observe the shared post-mutated final value (`3 3 3` instead of Go's `0 1 2`), and a stored `&i` would alias one shared box — compiling code that is silently wrong. When a clause-declared variable is **captured by a func literal in the body** or is **heap-boxed**, the converter rewrites the clause to drive a renamed *carrier* (`iᴛ1`) and re-declares the real variable fresh from the carrier at the top of the body; when the body can write the variable, its value is copied back to the carrier at every transfer to the post clause — the end of the body, before an unlabeled `continue` (a C# `continue` skips the end of the body), and after the `continue_<label>:` target (which the copy-backs deliberately follow, so a `goto continue_<label>` flows through them):

```go
var fs []func() int
for i := 0; i < 3; i++ {
	fs = append(fs, func() int { return i })
}
fmt.Println(fs[0](), fs[1](), fs[2]())   // Go 1.22+: 0 1 2
```
```csharp
for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
    var i = iᴛ1;               // fresh per-iteration variable — each closure captures its own
    fs = append(fs, () => i);
}
```

A variable the body (or a closure in it) *writes* adds the copy-backs:

```csharp
for (nint iᴛ1 = 0; iᴛ1 < 6; iᴛ1++) {
    var i = iᴛ1;
    if (i % 2 == 0) {
        iᴛ1 = i;               // an unlabeled continue copies back at its own site
        continue;
    }
    i++;
    fs = append(fs, () => i);
    iᴛ1 = i;                   // end-of-body copy-back feeds the post clause
}
```

A **heap-boxed** clause variable allocates a *fresh box* each pass — the same rule as the [per-iteration range-variable box](#pointers): a stored `&i` must be a distinct pointer per iteration — and always copies back (writes through the pointer are not syntactically detectable):

```csharp
for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
    ref var i = ref heap<nint>(out var Ꮡi);
    i = iᴛ1;
    ps = append(ps, Ꮡi);       // three DISTINCT pointers: 0 1 2
    iᴛ1 = i;
}
```

Loops whose clause variables are neither captured nor boxed emit exactly as before — the shared clause variable is then unobservable, so there is no churn. A read-only captured variable skips the copy-backs. Every clause reference (init/cond/post) renders the carrier, so the body keeps the variable's Go name — nested same-name loops compose with shadow renames (`iΔ1` gets carrier `iΔ1ᴛ1`), and a multi-variable clause transforms only the variables that need it. One legacy fallback: a heap-boxed variable that a **clause** func literal references keeps the old hoisted whole-loop box, since the body-scoped box would not be in scope at the clause. (Guarded by the `ForLoopPerIterationVars` behavioral test — read-only capture, body write + unlabeled `continue`, stored `&i` distinctness, boxed + captured closures, labeled continue with a write, nested same-name loops, a multi-variable clause, a struct-typed clause variable, and an immediately-invoked writing closure — values vs Go. `EscapedLoopVarSiblingIndex`, `ForVariants`, and `RingPointerMethods` re-baselined to the per-iteration shape.)

## The `go.golib` support namespace

golib's hand-written support types (`SparseArray<T>`, `PinnedBuffer`, `TypeExtensions`, `HashCode`, `FatalError`, …) live in the **`go.golib`** child namespace — deliberately NOT `go.<any Go package name>`. The namespace was originally `go.runtime`, which collides with the real `runtime` package: converted code imports runtime as `using runtime = runtime_package;` inside `namespace go`, and a child namespace `go.runtime` visible from any referenced assembly (golib is referenced by *every* project) wins simple-name lookup over the alias — CS0576 at every `runtime.X` use (surfaced by `iter`/`internal/weak` in wave 1). The same reasoning forbids `go.internal`, `go.sync`, etc.; `golib` is not a Go stdlib package name, so the child namespace can never collide with an import alias. Emitted code references these types via the child namespace (`new golib.SparseArray<T>{…}`), which resolves inside `namespace go` with no using directive.

The general form of this collision — a REAL parent/child package pair — is handled by **Δ-renaming the import alias**. A C# using alias declared inside a namespace conflicts with a same-named child namespace visible from ANY transitively referenced assembly (CS0576 at every use), and transitivity makes this common: `runtime.csproj` itself references `runtime/internal/math|sys` (namespace `go.runtime.@internal`), so *every* package importing `runtime` sees a `go.runtime` child namespace — `iter` and `internal/weak` surfaced it in wave 1 (`weak`, in namespace `go.@internal`, collides with `go.@internal.runtime` from `internal/runtime/*` instead). A pre-pass computes the package's transitive Go import closure (exactly mirroring MSBuild's transitive ProjectReference visibility), derives every child-namespace chain it contributes, and Δ-renames any import alias the current package's namespace would capture: `using Δruntime = runtime_package;` with uses `Δruntime.Goexit()` — the established collision marker. The rename propagates through one lookup to the using emission, package-qualifier identifiers, and cross-package type-name prefixes; a package with no collision emits byte-identically. (The behavioral corpus sees this on `io` — the real Go closure contains `os → io/fs`, hence `go.io` — captured in the `AnonymousInterfaces` golden as `Δio`.)

### Foreign renamed types reference the recorded imported-type alias
A cross-package type that is renamed (or Go-aliased) inside its own package -- `syscall` declares `ΔHandle` for its type-vs-method-colliding `Handle` -- must be referenced through the recorded imported-type alias (`global using syscallꓸHandle = go.syscall_package.ΔHandle`): the raw qualified render (`Δsyscall.Handle`) names a type that does not exist (CS0426 x26, internal/poll). The substitution lives at the C#-NAME layers -- `getCSTypeName` (delegate elements, parameters, results) and `getDisplayTypeName` (named struct fields) -- and deliberately NOT in `getTypeName`: the Go-shaped name layer also feeds promoted-embed MEMBER naming, where the substitution renamed and rescoped the generated accessors (reflect CS8799 regression on the first cut). The GoImplicitConv assembly attributes record type names under the file-local import qualifier, so the resolving `using` in package_info.cs declares that same qualifier (`using Δsyscall = go.syscall_package;`).

A **pointer/box (or other composite) element** — `*time.Location` as a func result (archive/zip's `timeZone`), a `*syscall.Handle` parameter, a slice/map element — is renamed too, but by a *different route* that does not need `getTypeName` (so the CS8799 landmine is untouched): `getTypeName` renders the Go-shaped `*time.Location` (unrenamed, per above), then the downstream `convertToCSFullTypeName` applies `getAliasedTypeName` to the FINAL string identifier — substituting `time.Location → timeꓸLocation` before boxing — yielding `ж<timeꓸLocation>`. So the alias reaches every position that flows through the C# type-name conversion (values, pointers, boxes, composite elements alike), **provided `importedTypeAliases` is populated**. That map is loaded from the imported package's `package_info.cs` (the `[GoTypeAlias]` round-trip), so a *fresh full reconvert* renders the alias everywhere; a **stale/partial overlay** that lacks the up-to-date `package_info.cs` renders the raw name and mis-reports CS0426 — the failure is in the measurement tree, not the converter (internal/trace/testtrace's `trace.Time`/`Event`/`Stack` and archive/zip's `*time.Location` were both bank-diagnosed as converter roots, then shown by a clean reconvert to already render `traceꓸTime`/`ж<timeꓸLocation>`).

The map is now populated for the WHOLE package before any file converts. Even within a fresh reconvert,
`importedTypeAliases` was loaded INCREMENTALLY — `visitImportSpec` loads a package's aliases only when it
visits an import of that package, and files convert in sorted-filename order. So a foreign renamed type
reached TRANSITIVELY — through a value whose package the current FILE does not itself import — rendered its
raw (nonexistent) name if that file converted before any file that DOES import the package. go/printer's
`comment.go` (`slash := list[0].Slash`, a `token.Pos` read through `ast.Comment`, importing only `go/ast`)
sorts first, so its `slash` heap box emitted `heap<go.token_package.Pos>` instead of `heap<tokenꓸPos>`
(= `go.go.token_package.ΔPos`) — CS0426, the sole such site in the stdlib. A package-level pre-pass
(`preloadImportedTypeAliases`, run before the file-conversion loop) now loads the exported aliases of every
package ANY file imports, up front. The load is deduped per imported package, so it only FRONT-LOADS what
`visitImportSpec` did incrementally; the alias set is file-order-independent and, because it only ADDS
aliases previously missing for a transitive-use file, it can only turn a currently-WRONG render right (a
compiling package has no wrong-rendered renamed type) — CNR byte-identical across the behavioral corpus, and
an A/B full-stdlib reconvert changes exactly one file (go/printer/comment.cs), greening go.printer alongside
the append-disambiguation root above. (Guarded by the three-package `TransitiveAliasPreload` fixture:
`CrossPkgBox.Box` carries a field of `CrossPkgLib`'s Δ-renamed `Status`; the test's `a_boxed.go` (sorts
first) reads it transitively — `return &s` heap-boxes `s`, rendering `heap<CrossPkgLibꓸStatus>` — while
importing only `CrossPkgBox`, and `z_main.go` (sorts last) is the only file importing `CrossPkgLib`. Without
the preload the box renders the nonexistent `CrossPkgLib_package.Status` (CS0426); output-compared vs Go, 4
phases green. This is the three-package shape the 2-package `CrossPkg` harness could not previously express —
cf. the `os.FileInfo` alias root, still GUARD OWED above for that reason.)

The preload still covers only packages **some file imports**. A foreign renamed type reached ONLY through
ANOTHER package's signature — go/types renders go/ast's `FieldFilter` (`func(string, reflect.Value) bool`)
when it passes `ast.NotNilFilter` to `ast.Fprint`, and **no go/types file imports `reflect`** — had no alias
loaded at all, so the synthesized delegate wrap rendered the raw name: `new Func<@string, reflect.Value,
bool>(ast.NotNilFilter)` — `Value` resolved inside `reflect_package` (CS0426) and the mismatched delegate
then failed the method-group conversion (CS0123). `aliasedElementTypeName` (the delegate-element rename
route) now loads the owning package's exported aliases **on demand** when a foreign named element has no
registered alias — `loadImportedTypeAliases` is deduped per package, so a miss costs one probe — and the
resolving `global using reflectꓸValue = go.reflect_package.ΔValue;` rides the normal package_info emission
(the consumer sees the type through its importer's **transitive** assembly reference). For LOCAL modules the
resolver map (`importPackageDirs`) is now captured over the **transitive** import closure rather than direct
imports only, so the same on-demand load works outside GOROOT. (Guarded by `SynthesizedDelegateCrossPkg`:
`CrossPkgFuncLib.Picker func(CrossPkgLib.Status) bool` + exported `Hot` matching it; the consumer imports
only `CrossPkgFuncLib` and passes `Hot` where a `Picker` is expected — the wrap must render
`new Func<CrossPkgLibꓸStatus, bool>(CrossPkgFuncLib.Hot)`; output-compared vs Go, 4 phases green.)
```csharp
public static Func<CrossPkgLibꓸStatus, nint> CheckFunc = (CrossPkgLibꓸStatus st) => st.Code * 2;
internal static (CrossPkgLibꓸStatus, nint) gauge(CrossPkgLibꓸStatus st) {
internal static ж<CrossPkgLibꓸStatus> statusPtr(ж<CrossPkgLibꓸStatus> Ꮡst) {  // *Status → box of the alias
```
Guarded by `CrossPkgUser` (`CheckFunc`/`gauge`/`meterBox` -- delegate, signature, and field positions; `statusPtr`/`ledger` -- a `*CrossPkgLib.Status` pointer as a func parameter, result, and struct field, each boxed as `ж<CrossPkgLibꓸStatus>`).

## Source Generators
Several Go semantics cannot be written directly in C#, so the converter emits compact, attributed partial declarations and lets a set of Roslyn source generators (`src/gen/go2cs-gen/`, referenced as an analyzer by every converted project) synthesize the rest at compile time. This keeps the visible converted code close to the Go original. The principal generators and attributes:

* **`TypeGenerator`** — driven by `[GoType]`. Emits the body of a converted type: a struct's members and equality, a named numeric/slice/array/map/channel type's wrapper and operators (see [Untyped Constants and Named Numeric Types](#untyped-constants-and-named-numeric-types) and [Slices and Arrays](#slices-and-arrays)), and struct-embedding field/method promotion.
* **`ImplementGenerator`** — wires up Go's duck-typed [interfaces](#interfaces): finds the concrete types that satisfy each `[GoType] partial interface` and emits the implementation glue and implicit conversions.
* **`RecvGenerator`** — emits pointer-receiver overloads for receiver methods (`[GoRecv]`), so a method written against a value (`this ref T`) is also callable through the pointer/box form. A **variadic** method keeps its `params` in the generated overload: cryptobyte's `func (b *Builder) add(bytes ...byte)` emits the value form `add(this ref Builder b, params Span<byte> bytesʗp)`, but the `ж<Builder>` overload had dropped `params` (a bare `Span<byte>`), so a call passing individual elements through a box (`c.add(0xff)`, `c` a `ж<Builder>` closure parameter) could not bind it and fell back to the ref-receiver value method — CS1929. `GetMethodInfo` now preserves the `params` modifier (the Go variadic is always the last, non-receiver parameter, so it never lands on the `this ж<T>` receiver). Guarded by `VariadicBoxReceiver` (a `*sink` with `add(bytes ...byte)` called on a box — via a closure and directly — with zero, one, several, and spread arguments, values vs Go).
* **`ImplicitConvGenerator`** — emits the implicit conversion operators that let a [named type](#type-definitions) and its underlying types be used interchangeably.
* **`PartialStubGenerator`** — emits a throwing `partial` implementation for any bodyless `partial` method that has no other implementing part (e.g. assembly/cgo functions with no convertible body), while leaving real hand-written companion implementations untouched.

Common attributes the converter emits for the generators (and tooling) to consume: `[GoType]` (type bodies), `[GoRecv]` (receiver methods), `[GoTag]` (struct field tags), `[GoPackage]` (package info), and the test-only `[GoTestMatchingConsoleOutput]`.

## Manually-Converted Declarations (managed-referent pointers)

Some Go declarations cannot be faithfully auto-converted because their semantics depend on hiding a managed pointer inside an integer. The canonical family is runtime's `guintptr`/`puintptr`/`muintptr` (`type guintptr uintptr` holding a `*g` the Go GC must not see): the CLR has the *opposite* constraint — a managed reference stored as a number is invisible to the .NET GC, so the referent can be collected or moved and the number is garbage. The managed conversion stores the `ж<T>` box **directly** and the numeric form never exists (model precedent: `core/sync/atomic`'s hand-rewritten `Pointer<T>`).

Two mechanisms deliver this, chosen by granularity:

* **Whole-file** (pre-existing): a hand-finished file marked `[module: GoManualConversion]` is skipped by the converter when it exists in place (`containsManualConversionMarker`) and restored over auto output by the overlay on fresh reconversions. Right when the whole file is hand-owned (sync/atomic `type.cs`).
* **Type-level** (`go2cs/manualTypeOperations.go`): the `manualConversionTypes`/`manualConversionFuncs` registry (keyed by package path and raw Go names) makes the converter skip emitting the listed **type declarations**, every **method on those types**, listed **adjacent free functions** (`setGNoWB`), and **`GoImplicitConv` assembly attributes** referencing the types — each replaced by a marker comment pointing at the package's `*_impl.cs`. Right when the types live in a large file (runtime2.go) that must otherwise keep receiving converter improvements.

The hand implementation (`src/core/<pkg>/<file>_impl.cs`, e.g. `core/runtime/runtime2_impl.cs`) declares the same type/extension surface the auto call sites bind: value-receiver methods as `this T` extensions, pointer-receiver methods as `[GoRecv] this ref T`, and the conversion operators call sites need. For the guintptr family that surface is: `.ptr()` returns the stored box, `.set()` stores it, `.cas()` is a real `Interlocked.CompareExchange` on the reference slot (the Go original's `atomic.Casuintptr` maps to a throwing asm stub — the managed model makes it *work*), `== 0`/`= 0` bind zero-comparison/nil operators, and numeric escapes are deliberate and loud: converting a non-zero integer **panics** (a number can never faithfully become a managed reference), and converting *to* a number (print/`hex` diagnostics) yields a stable object-identity hash — an opaque token, never an address.

One call-site emission cooperates (`convCallExpr.go`): a conversion **to** a manual type from an `unsafe.Pointer` — `guintptr(unsafe.Pointer(newg))` — unwraps the inner conversion and emits the referent-preserving ctor form `new Δguintptr(newg)` instead of the numeric cast chain `(Δguintptr)(uintptr)new @unsafe.Pointer(newg)`, which would lose the referent at the `(uintptr)` hop.

**The runtime lock/note model (`core/runtime/lock_sema_impl.cs`).** Go's `mutex.key` is a tagged atomic slot — 0 unlocked, `locked` (1) held, or an `*m` address|locked heading a waiter chain through `m.nextwaitm`, parked on OS semaphores. The managed model hand-owns `mutexContended`/`lock2`/`unlock2`/`notewakeup`/`notesleep`/`notetsleep_internal` (via the same registry; thin wrappers and consts stay auto) and keeps the **same key protocol restricted to `{0, locked}`**: the mutex is an `Interlocked` spinlock on the real `key` storage with `SpinWait` escalation standing in for the spin→yield→park ladder; the note is a signaled/clear latch (double-wakeup throw preserved; timeout at millisecond granularity). Deliberately not modeled, documented in place: the waiter queue (fairness), lock profiling, and the `m.locks`/preempt bookkeeping — `getg()` is a Go compiler intrinsic with no managed realization yet (a `[ThreadStatic]` g/m model is the future root that unlocks runtime-operational semantics; the bookkeeping returns to these bodies when it lands).

**`sync/atomic.Value` (`core/sync/atomic/value.cs`, whole-file).** Go's `atomic.Value` stores and loads an `any` atomically by reinterpreting the interface's internal two-word `(type, data)` layout: `(*efaceWords)(unsafe.Pointer(&v))`, then `atomic.LoadPointer`/`StorePointer`/`CompareAndSwapPointer` on the `typ` and `data` slots, with a `firstStoreInProgress` sentinel guarding the first store. That layout is a Go runtime detail with **no managed equivalent** — an `any` here is a single `System.Object` reference (one word), and reinterpreting a managed reference as a raw address to poke type/data words simply NREs (the same managed-referent-through-`unsafe.Pointer` wall as the guintptr family). The first *operational* hit was `internal/testlog`'s package-level `var logger atomic.Value`, loaded during `os.Getenv` — so `atomic.Value.Load()` NRE'd on the zero value before any store. The whole file is hand-rewritten (marked `[module: GoManualConversion]`) to store the `any` **directly** in the `Value.v` field and use `Volatile.Read`/`Interlocked.CompareExchange` for the acquire/release ordering and CAS the literal conversion cannot provide; the nil-store and inconsistent-type panics, and `CompareAndSwap`'s by-value comparison (`AreEqual`, matching Go's `i != old`), preserve the spec. Guarded by the `AtomicValue` behavioral test (Load-nil / Store / Swap / CompareAndSwap over typed string values, output-compared vs Go).

### A cross-package `//go:linkname` PULL emits a forwarder, not a throwing stub

A bodyless function carrying `//go:linkname <local> <pkgpath>.<func>` (a three-field directive naming another package) is a **PULL** — the function has no body of its own and links to another package's (often unexported) symbol. `golang.org/x/sys/windows`'s `LazyDLL`/`LazyProc` reach the Go runtime's DLL loaders this way:

```go
//go:linkname syscall_loadlibrary syscall.loadlibrary
func syscall_loadlibrary(filename *uint16) (handle Handle, err Errno)
```

Left as an ordinary bodyless declaration, it would emit a `partial` that the [`PartialStubGenerator`](#source-generators) turns into a **throwing** stub — dead DLL loading. The converter (`visitFuncDecl.go`) instead recognizes the directive and emits a **forwarder body** that calls the target, bridging any nominal `num:uintptr` type difference through `uintptr` (the linked signatures are structurally identical, so a mismatch is only between two such types):

```csharp
internal static (ΔHandle handle, Errno err) syscall_loadlibrary(ж<uint16> filename) {
    var (ᴛ1, ᴛ2) = syscall.loadlibrary(filename);
    return ((ΔHandle)(uintptr)ᴛ1, (Errno)(uintptr)ᴛ2);
}
```

Pointer/slice/string parameters (`filename`) pass through unchanged (the same golib type on both sides); an integer/uintptr parameter is passed `(uintptr)p` and an integer/uintptr result returned `(LocalType)(uintptr)r`. The target alias is the last path segment of the linkname's package (`syscall.loadlibrary`), resolved through the importing file's own `using syscall = syscall_package;`.

**Forwarding is gated on an explicit whitelist of hand-implemented targets** (`linknameForwardTargets` — currently `syscall.loadlibrary`/`loadsystemlibrary`/`getprocaddress`, the native P/Invokes in `core/syscall/dll_windows.cs`). This is not optional prudence: a linkname target is **indistinguishable at conversion time** from any other bodyless assembly/intrinsic Go function — `syscall.loadlibrary` and `runtime.reflectcall` are *both* bodyless `//go:` asm in Go — so only the whitelisted targets are known to have a real C# implementation to call. Every other linkname pull stays a bodyless stub, the pre-forwarder behavior: a method-receiver PUSH (`//go:linkname X reflect.(*rtype).Align`, reflect's `badlinkname.go` "pushes linknames of the methods"), a same-package pull (`//go:linkname unusedIfaceIndir reflect.ifaceIndir` inside reflect), and an unimplemented intrinsic (`//go:linkname call runtime.reflectcall`) would each otherwise emit an uncompilable forwarder (a nonexistent `reflect.(*rtype)`/`runtime.reflectcall` member, or a package alias that doesn't exist for the package's own name). Extend the whitelist when a new native linkname target gains a hand-written C# implementation. Guarded by `TestRecurseLinknameForwarder` (asserts the whitelisted `syscall.loadlibrary` forwarder body + the uintptr result bridge, and that a non-whitelisted `runtime.reflectcall` target stays a stub).

## Deterministic Output

Converter output is **byte-reproducible**: converting the same Go source with the same converter build produces byte-identical C# every run. This is a hard guarantee the goldens, the full-conversion error measurements, and any future release tag all rest on. Three mechanisms enforce it (all landed 2026-07-01, proven by two consecutive full-stdlib conversions diffing to zero across 305 packages):

* **Files convert sequentially, in sorted-filename order.** Per-file conversion previously ran in concurrent goroutines sharing package-level state claimed at visit time — `initΔN` module-initializer indices, blank-identifier temp numbering (`_ᴛN`, an unsynchronized map — a genuine data race), and imported collision-rename visibility: a file could mark an imported `package_info.cs` "parsed" *before* the parse finished, so a concurrently-converting file skipped the wait and emitted an imported renamed const **bare** (`abi.String` instead of `abi.ΔString` — a compile error that came and went with goroutine scheduling). Sequential conversion costs nothing measurable: a full-stdlib conversion is 3m42s concurrent vs 3m39s sequential (the cost is `go/packages` type-graph loading, not emission).
* **The stdlib conversion queue is deterministic and dependency-complete.** The topological sort now walks sorted roots (map-iteration roots made unrelated packages' order flip run-to-run), and a GOROOT-**vendored** dependency (imported as `golang.org/x/…` but keyed on disk as `vendor/golang.org/x/…`) is resolved to its vendored key — previously the edge was silently dropped, so an importer (e.g. `x/text/secure/bidirule`) could convert *before* its dependency (`x/text/unicode/bidi`), whose `package_info.cs` — the source of imported collision-rename aliases like `bidiꓸClass` — did not exist yet at that point.
* **Multi-box re-alias emission is sorted.** A multi-assignment that repoints several pointer boxes (`(Ꮡx, Ꮡy) = (Ꮡy, Ꮡx)`) emits its independent `n = ref Ꮡn.Value` refreshers in sorted name order (the set backing them is a map).