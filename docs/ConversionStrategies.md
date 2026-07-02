# Conversion Strategies

> **Updated 2026-06-27 for the "go2cs2" generation of the converter.** This is a living
> document; as more use cases are converted these strategies are refined. The current converter
> is written in **Go** (using the official `go/ast` + `go/types` toolchain, under `src/go2cs/`)
> and emits C# that leans on two things the visible code does not show in full: a hand-written
> runtime library, **`golib`** (`src/core/golib/`), and a set of **Roslyn source generators**
> (`src/gen/go2cs-gen/`) that synthesize the Go semantics which cannot be written directly in C#.
> Notes that previously referenced the retired ANTLR4/C# converter or the old `gocore` library
> have been updated to reflect this. See also [`Architecture.md`](Architecture.md) and
> [`../CLAUDE.md`](../CLAUDE.md).

The guiding goal: the generated C# should be both *behaviorally* and *visually* similar to the
original Go, so that a Go developer can read the output and follow it. The runtime library and
the generators exist to keep the visible converted code close to the Go original.

## Topics

* [Package Conversion](#package-conversion)
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

**Exported structs and interfaces cross packages.** An exported struct's fields and methods are reachable on the consumer side exactly as the producer emits them — `CrossPkgLib.Sensor{Name: …, Temp: …}` lowers to a C# constructor call and `s.Name` / `s.Hot()` to field/method access on the imported struct — because the struct and its `[GoRecv]` extension methods live in the (referenced) library assembly.

A cross-package **interface satisfaction** is subtler. Go is structurally typed, so a consumer may assign any value with the right method set to an interface; C# requires the *nominal* `partial struct T : I` implementation glue, which the [`ImplementGenerator`](#source-generators) can only add to `T` **in T's own assembly** (`isLocalImplType`). The converter records a `[assembly: GoImplement<T, I>]` for each concrete→interface conversion it *witnesses while converting T's package* — so for a consumer to use `Sensor` as `Labeled` across the assembly boundary, the satisfaction must be witnessed in the **library** that declares `Sensor`. The idiomatic Go interface-satisfaction assertion does exactly this:

```go
var _ Labeled = Sensor{}   // in CrossPkgLib — records GoImplement<Sensor, Labeled> in this assembly
```

With that, the library emits `[assembly: GoImplement<Sensor, Labeled>]`, `Sensor : Labeled` is realized in the library assembly, and a consumer's `var l CrossPkgLib.Labeled = s` / `CrossPkgLib.Describe(s)` compile as ordinary upcasts. (A library that returns the interface from a constructor — `func New(...) Labeled { return Sensor{…} }` — witnesses it the same way.) A type that satisfies an interface but is *never* used as it within its own package is not yet auto-realized cross-package; proactively recording every local concrete→local interface structural match is a possible future enhancement, weighed against the extra generated glue it would add to every package. Also guarded by the `CrossPkgLib`/`CrossPkgUser` pair (Phase 3: struct field access + interface satisfaction).

### Standard-library solution file (`.slnx`)

A whole-standard-library run (`go2cs -stdlib`) also emits a Visual Studio solution — **`go-src-converted.slnx`** — at the output root (`-go2cspath`), so the freshly converted stdlib is openable / buildable as **one unit** immediately after a run, rather than depending on a hand-maintained solution that drifts. It is the auto-generated counterpart of `src/go-src-converted.sln`, and its XML mirrors the format of `src/go2cs.slnx` (a `<Configurations>` block plus `<Folder>`/`<Project>` entries, CRLF line endings, no BOM). It references:

* every converted stdlib project it emitted (`core/<pkg>/<projectName>.csproj`, grouped under a `/core/` folder),
* any per-package **test** projects (`*_test.csproj`, grouped under a `/tests/` folder — inert until Phase 4 emits them, and the folder is omitted entirely when there are none),
* the shared **`golib`** runtime (`core/golib/golib.csproj`), and
* the **`go2cs-gen`** source-generator/analyzer project (`gen/go2cs-gen/go2cs-gen.csproj`, under a `/generators/` folder).

The stdlib project list is gathered by walking the emitted `core/` output tree (so it also picks up future test projects with no code change), and every project is emitted in stable **alphabetical** order for deterministic output. All paths are **solution-relative** (forward slashes) so the generated solution is portable — no absolute, machine-specific paths. The `golib` and `go2cs-gen` references use the same `core\golib` / `gen\go2cs-gen` layout the converted `.csproj` files already assume via `$(go2csPath)` (which resolves to `$(SolutionDir)`), so the solution locates them wherever those csproj references already resolve. The file is only rewritten when its content changes, so repeated runs are a no-op.

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

**Constant-literal return inside a lambda with an unsigned result (delegate-type inference, CS8917).** A Go closure assigned to a local — `casePC := func(casi int) uintptr { if pcs == nil { return 0 }; return pcs[casi] }` (runtime `select.go`) — is emitted as `var casePC = (nint casi) => { … };`, whose delegate type C# must **infer from the return-expression types**. The literal `return 0` is typed `int`; `return pcs[casi]` is typed `nuint` (`uintptr`). C#'s best-common-type algorithm uses the expression types (not constant convertibility), and `int` has no common type with `nuint`/`uint`/`ulong` (there is no implicit `int`→unsigned conversion for a non-constant), so the `var` assignment fails with CS8917 ("no best type found for the lambda"). The converter casts the literal to the result type so both returns share it: `return (uintptr)(0)`. Gated tightly to avoid churn and new errors: only **inside a lambda body** (`conversionInLambda` — a *named* func's `return 0` to a `nuint` result compiles as an ordinary constant conversion and needs no cast), only for a bare **integer literal** (the sole shape that trips the `int`-vs-unsigned inference gap — `byte`/`uint16` widen to `int`, and the signed/`nint`/`long` kinds share a common type with `int`, so those never hit CS8917), and only when the result is a **basic** `uint`/`uint32`/`uint64`/`uintptr` (a *named* type over an unsigned kind is left alone — `(gclinkptr)(0)` would only compile if that type defined an int conversion, so casting it could introduce a new error). Runs after the narrow-arithmetic return cast, with which it is disjoint (that handles binary/unary arithmetic on sub-`int` types; this handles a bare literal to a wide unsigned type). (Guarded by the `ClosureMixedReturnUnsigned` behavioral test — `uintptr`/`uint64`/`uint32`/`uint` mixed-return closures plus a signed control that stays uncast, values verified vs Go; cleared the `select.go` `casePC` CS8917.)

> One sticking point: not all C# indexing constructs accept a `nint`. Explicit indexers support `nint`, but [implicit index support](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-index-support) (the `Index`/`Range` syntax) currently only works with `int`, so range-operation indices are cast to `int` where needed. (The earlier strategy of compiling to `long`/`ulong`, or of custom `@int`/`@uint` structs selected by a `TARGET32BIT` directive, has been superseded by `nint`/`nuint`.)

## Untyped Constants and Named Numeric Types

This area is where Go's flexible numeric model meets C#'s stricter one, and it has a few moving parts worth calling out.

**Untyped constants.** As noted under [Constant Values](#constant-values), an untyped Go constant becomes a golib `UntypedInt`/`UntypedFloat`/`UntypedComplex`. These wrappers define implicit conversions to **and from** every numeric type so the value can slot into whatever context uses it, just like an untyped Go constant. The trade-off is that mixing an `UntypedInt` directly into heavily-typed arithmetic (e.g. `someUint64 * untypedConst`) can become ambiguous to C#'s overload resolution, since the wrapper is convertible in either direction. Context-typing of untyped *local* constants (emitting them with the concrete type their use demands instead of the wrapper) is an area of ongoing refinement.

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
* **Identity conversions are not double-cast.** `arenaIdx(x)` where `x` is already `arenaIdx` is a Go no-op. This arises for an untyped-constant shift that adopts the target type from context (`arenaIdx(1 << bits)`, whose operand go/types already types as `arenaIdx`, so the inner conversion has already emitted `(arenaIdx)((nuint)1 << bits)`), and for a plain `arenaIdx(yArenaIdx)`. Wrapping the already-typed expression in a second `(arenaIdx)` cast just doubles it, so the converted argument is returned as-is — `arenaIdx a = (arenaIdx)((nuint)1 << (int)(bits));`, not `((arenaIdx)((arenaIdx)(…)))`.

(Guarded by `NamedNumericConversion`, `NamedNumericShiftConv`, `NamedTypeBitwiseConst`, `IotaEnum`, `FuncTypeParam`, and `CrossPkgUser`; the `string`-target exception is guarded by `StringConvPostfix` and `UnsafeOperations`; verified by the full behavioral suite — output comparisons confirm the precedence is unchanged.)

**Generated conversion operators between named numerics of *different* assemblies.** The two paragraphs above are the *converter's inline* casts. Separately, when the converter sees a conversion *between two named numeric types* it records a `[assembly: GoImplicitConv<…>]` and the `ImplicitConvGenerator` emits a user-defined `implicit operator` for it. The emitted body constructs one named type from the other's underlying value: `new Target((ValueType)src.val)`. When both named types live in the **same** assembly this is fine (e.g. runtime's `muintptr ↔ Δhex`), but when the operator must **construct a *foreign* named numeric** — one declared in another C# assembly — two problems appear that only manifest cross-assembly:

* A direct cast to the foreign named type has no route. `(NameOff)src.val` where `src.val` is `ulong` and `NameOff` (`internal/abi`) is a *different assembly* is **CS0030** — C# does not select the foreign type's `int32`-based user conversion for a `ulong` source across the assembly boundary (the same cast to a *local* named type compiles). It must go **through the foreign type's underlying basic**: `new …NameOff((int)src.val)`.
* The default host can be a phantom. The operator is hosted in `partial struct {sourceType}`; if that source is the *foreign* type (reached here via a local alias, e.g. runtime's `global using nameOff = abi.NameOff`, so the cross-package dot is hidden and the conversion records as `Inverted`), the `partial struct NameOff` declares a new *empty local* type rather than extending the foreign one — **CS1729** (no constructor). The operator is relocated into the **local** type instead.

So for a foreign *constructed* type the generator emits, fully-qualified and hosted in the local type:

```csharp
// runtime, dur↔hex style: foreign abi.NameOff constructed from local Δhex
partial struct Δhex {
    public static implicit operator global::go.@internal.abi_package.NameOff(global::go.runtime_package.Δhex src)
        => new global::go.@internal.abi_package.NameOff((int)src.val); // through the underlying int32
}
```

The override fires **only** when the `new`-constructed side (the LH type: the *source* when the conversion is `Inverted`, else the *target*) is foreign; same-assembly operators are emitted byte-identically as before (no churn). Because the trigger is inherently cross-assembly, the behavioral-test harness (single-assembly, and unable to import a foreign named numeric — `internal/*` types are un-importable from a test module and the baseline stubs expose none) cannot host it; the guard is the **`go-src-converted/runtime` build**, where `NameOff`/`TypeOff`/`TextOff` ↔ `Δhex` naturally occur (this fix cleared 3×CS0030 + 3×CS1729 there).

The same underlying routing applies when an untyped-constant **shift** is re-typed to a named numeric. An untyped shift `1 << k` is re-typed to the type it assumes from context (so it can combine with typed operands); when that resolved type is a *named* numeric, the re-type must go through the underlying — `(arenaIdx)((nuint)1 << k)`, not a bare `(arenaIdx)(1 << k)` (CS0030). The shift's *width* is likewise decided by the underlying (a `nuint`/`uint64`-backed named type shifts the left operand in that width to avoid the `int`-overflow seen for `1 << 63`). Non-named shifts are unchanged. (Guarded by the `NamedNumericShiftConv` behavioral test — wide `uint`/`uint64`-backed and narrow `uint8`-backed named types; runtime hits this on `arenaIdx(1 << arenaBits)`.)

The unsigned named-numeric path above gets a width-cast operand, but a **signed** constant operator expression whose target is a plain builtin `int64` has no such cast, so C# would compute it in `int32` and overflow at compile time in checked mode (CS0220): `int64(1<<63 - 1)`, `var d int64 = 1<<40 + 7`, or `12345 * 1000000000 + 54321` passed to an `int64` parameter. Go evaluates each as a constant in its `int64` type. For a signed constant binary/shift expression whose folded value is **outside the C# `int32` range**, the converter emits the **folded 64-bit literal** (`9223372036854775807L`, `1099511627783L`, `12345000054321L`) instead of the operator form — correct, and self-contained. In-range constants and unsigned constants are unchanged (they keep the readable `1 << k` form). (Guarded by the `UntypedConstArithmetic` behavioral test; runtime hits this in `mgcmark`/`netpoll`/`runtime1`.)

The same coercion is needed where the converter itself inserts a C# `(int)` cast on a named-numeric value — a **slice bound** (`summary[sc+1:ec]` with `sc`/`ec` of type `chunkIdx`), a **shift count** (`1 << (d % 64)` with `d` of type `statDep`), or the **length of an `unsafe.Pointer`-to-array slice** (`(*[N]T)(ptr)[:n]` → `new slice<T>(new ReadOnlySpan<T>(ptr, (int)n))`, since the `ReadOnlySpan<T>` constructor takes a C# `int` — see *Slicing a pointer-to-array*). A bare `(int)(sc + 1)` is CS0030 for the same reason, so the converter emits `(int)(nuint)(sc + 1)` / `(int)(nint)(d % 64)` — through the named type's underlying basic; a plain `nint`/`nuint` length is narrowed `(int)(n)`. Plain basic operands keep the bare `(int)(x)` form. (Guarded by the `NamedNumericIntCast` behavioral test; the Span length by `StdLibInternalAbi`.)

**Defined types over a struct — forwarded fields.** A Go type definition over a *struct* — `type winlibcall libcall` — makes the underlying struct's fields accessible on the named type (`w.fn`), without promoting its methods. The named type is emitted as `[GoType("libcall")] partial struct winlibcall;` and the `TypeGenerator` wraps the underlying value (`private libcall m_value;`). For the underlying's fields to be reachable, the generator **forwards each as a get/set property** over `m_value`:
```csharp
private libcall m_value;                 // NOT readonly — see below
public nuint fn { get => m_value.fn; set => m_value.fn = value; }
public nuint n  { get => m_value.n;  set => m_value.n  = value; }
// … args, r1, r2, err
```
The underlying struct is resolved with `GetStructDeclaration` (same package, or a *source*-referenced package — a metadata-only struct is not resolved, so its fields are not forwarded), and its members come from `GetStructMembers`. Crucially `m_value` is **mutable** (not the wrapper's usual `readonly`), so a write through a pointer — `c.val.fn = fn`, where `c` is a `ж<winlibcall>` and `c.val` is `ref winlibcall` — invokes the setter on the real storage and persists. (The `readonly`→mutable choice is decoupled from the nullable-`m_value` form that only the lazily-allocated `array` backing needs.) Forwarding is skipped for a non-struct underlying (a named type over an interface or another named type) and for an underlying that contributes no fields, so those wrappers are unchanged. *Composite-literal construction* of such a type (`winlibcall{fn: x}`) is a separate, not-yet-handled case (the runtime accesses these only by field). (Guarded by the `NamedTypeOverStruct` behavioral test — write-through and read-back of forwarded fields through a pointer; runtime hits this on `winlibcall` over `libcall`, `syscall_windows.go`.)

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
h.val.flags &= unchecked((uint8)~hashWriting);
```

An LHS type that `int` widens to implicitly (`int`/`int32`/`int64`) needs no cast and stays `a &= ~b`. (Guarded by the `AndNotAssignNarrow` behavioral test, which exercises both an ident LHS and a struct-field LHS — they route through different assignment-emission paths.)

## The "nil" Value
In Go, `nil` is the equivalent of C# `null`. Where possible, converted code uses the golib [`NilType`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/NilType.cs) with a default instance called `nil` (defined in [`go.builtin`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/builtin.cs)). `NilType` provides comparison operators so `x == nil` / `x != nil` work across the runtime types (slices, maps, channels, pointers, interfaces), each of which defines what "nil" means for it (e.g. a `map<K,V>` whose backing dictionary is null is the nil map: reads return the zero value, `len` is 0, ranging yields nothing, and a write panics — matching Go).

The same null-safe-zero-value principle applies to value types whose backing store is a reference. A zero-value `string` converts to `@string s = default!`, which runs no constructor, so the backing `byte[]` is null. Rather than NRE on the first read, `@string` treats a null backing as Go's empty string `""` for every read — length 0, no bytes to index/range, `== ""` is true, prints empty, and concatenation yields the other operand (`var s string; s += "x"` → `"x"`). Constructors still allocate, so only the `default(@string)` zero value relies on this. (Guarded by the `StringZeroValueConcat` behavioral test.)

## Empty Interface
In Go, every type satisfies the method-less interface `interface{}`, now spelled `any`. This operates fundamentally like .NET's `System.Object`, so the converter maps the Go empty interface to `any` (a global alias for `object`). For example, a Go `func(i interface{})` becomes `void f(any i)`, and a `map[any]string` becomes `map<any, @string>`.

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

A **block-scoped `const`** that shadows an enclosing parameter or variable is renamed the same way. `func f(ns int64) { …; const ns = 10e6; use(ns) }` (runtime `notetsleep_internal`) is legal in Go but the inner `const ns` and the param `ns` both emit as `ns` in C# (CS0136). A const is tracked separately from variables — its `go/types` object is a `*types.Const`, not the `*types.Var` the scope stack records — so the shadow-rename pass had ignored it; it now records a shadowing const (detected by the same by-name enclosing-scope check) and rewrites its declaration and every use to `nsΔ1`, leaving the enclosing `ns` untouched. Only a *shadowing* const is renamed (a plain block const keeps its name, no churn). (Guarded by the `ConstShadowsParam` behavioral test — the inner uses bind the const value, the outer uses bind the param.)

Renaming depends on correctly identifying which declarations are *function-level* — the set a nested variable of the same name must avoid (C# forbids the nested one even when the function-level one is declared *later*). A `for init; …` loop's `:=` variable, and a range `:=` key/value, are scoped to their own statement, **not** the function body, so they are deliberately excluded from that set. Recording a for-loop variable as function-level (it is encountered first, in source order) would mask the real function-level variable of the same name declared afterward — `for b := …{} for b := …{} … b := newBucket(…)` — leaving *all three* emitted as `b` and colliding (CS0136). With the for-loop variables correctly treated as inner scopes, they are renamed `bΔ1`/`bΔ2` while the function-level `b` keeps its name. (Guarded by the `ForVarMasksFuncLevel` behavioral test; runtime hit this in `stkbucket`.)

The same forward-collision rule applies at **every block level, not just the function body**. C# CS0136 fires whenever a name is declared in two scopes where one encloses the other, *regardless of declaration order* — so a nested variable must be renamed if the same name is declared anywhere in an enclosing block, whether that declaration appears before or after it in source. The scope-stack walk only records declarations already seen (backward), and the function-level forward set covers only the function body; a variable declared **later in an intermediate enclosing block** would otherwise be missed. To close that gap, each block scope (function body, `if`/`for`/`range`/`switch`/`select` bodies, bare blocks, and `case`/comm-clause bodies) is pre-scanned for its directly-declared names (`:=` and `var`, excluding a control statement's own init `:=`, which is scoped to that statement) when the scope is pushed, so forward declarations are visible to the shadow check. For example, the runtime's `runGCProg` has two `for off := …` loops followed by `off := n - nbits` *in the same enclosing `for {}` body* — the block-level `off` encloses both loops, so the loop variables are renamed `offΔ1`/`offΔ2` while the block-level `off` keeps its name. (Guarded by the `ForVarMasksBlockLevel` behavioral test — distinct from `ForVarMasksFuncLevel`, where the later same-named variable is function-level; this cleared 5 runtime CS0136 in `runGCProg`/`mprof`/`runtime1`/`time`.)

The mirror image — a local shadowing a package-level **GLOBAL** — is resolved the other way: the *global reference* is qualified rather than the local renamed. C# locals are function-scoped, so a local `trace := traceAcquire()` shadows a same-named global `var trace` throughout the function, and an *earlier* read of the global binds to the not-yet-declared local (CS0841; the wrong variable regardless). Renaming the local is the fragile, entangled path (it interacts with collision renames and the shadow-rename counter); instead a use whose ident resolves to a package-level var **of this package** — while a same-named function-level local is declared — is emitted qualified with the package static class: `runtime_package.Δtrace.minPageHeapAddr`, which a local can never shadow. This is the same package-class qualifier the box-field accessor uses for a shadowed owning type (below). Runtime's `traceallocfree.traceSnapshotMemory` reads the global `trace.minPageHeapAddr` before its local `trace := traceAcquire()` (both collision-renamed `Δtrace`); the qualifier is gated so an ordinary global (no shadowing local) and the local's own uses (which resolve to the local, not the package scope) keep their bare, Go-like form — no churn. (Guarded by the `GlobalShadowedByLocal` behavioral test — a collision-renamed global and a plain global each read before a same-named local; cleared runtime's last CS0841.)

Two subtleties complete this for the runtime's `runqputslow` shape (three `for i := …` loops that reuse `i`). **An ESCAPING loop variable is function-scoped in C#.** A function-body-level `for i := …` whose variable escapes to the heap is emitted as a `ref var i = ref heap<…>(out var Ꮡi)` declaration at the *function-body* scope (not the loop) — see [Pointers](#pointers) — so the sibling loops that reuse `i` genuinely collide with it (CS0136), unlike the ordinary all-loop-scoped case above. The escaped loop variable is therefore collected as function-level (so the siblings are renamed `iΔ1`/`iΔ2` and the escaped one keeps `i`), but *only* when it escapes, is at function-body level (a nested loop's box hoists to its own block, not the function body), and its name is not already a real function-level declaration — preserving the `ForVarMasks…` invariant above (a genuine function-level variable is never masked). **And a renamed variable used as an LHS index/map key is rewritten there too.** An assignment `a[i] = …` / `m[ns] = …` / `p.f[k] = …` reassigns the *root* (`a`/`m`/`p`); the index/key expression is a separate value, so a shadow-renamed variable used there (`a[iΔ1]`, `m[nsΔ1]`) must be rewritten by descending the target's index/selector/deref chain and renaming each index. Missing this is a *silent* bug — the LHS key kept the enclosing variable's name, so `m[ns] = nsΔ1*100` wrote to the wrong key with no compile error — as well as a CS0136/CS0165 once the loop variable itself is renamed. (Both guarded by the `EscapedLoopVarSiblingIndex` behavioral test — the array case would not compile and the map case would silently return the wrong value without the pair; cleared the 2 `runqputslow` CS0136 plus a CS0841.) The target-chain descent also visits a **method-call receiver** in the chain — `x.ptr().val.next = …` (runtime `stackpoolalloc`, where the loop `x` is renamed `xΔ1` because a func-body `x` is declared after the loop). The `x` is buried inside the `x.ptr()` call, past the selector/index steps, so without visiting the call the use kept the raw `x` — read before its (later) declaration → CS0841, or a silent wrong bind. Visiting the whole call renames its receiver and argument identifiers (the call's result is the navigated base, so the descent stops there). (Guarded by the `ShadowedVarMethodCallLHS` behavioral test — write-through through the method verified vs Go; cleared the `stack.cs` CS0841.)

The reverse collision — a package **method named like a built-in** — needs the opposite treatment. In Go a method `func (b *pageBits) clear()` and the universe `clear` built-in coexist: the method is only ever reached as `b.clear()`, while a free `clear(s)` is always the built-in. But the method is emitted as a `clear(this ref pageBits)` extension on the package's static class, and C# member lookup binds that same-class member for an *unqualified* free `clear(s)` call — shadowing the using-static `go.builtin.clear` and failing (`CS1620`/`CS1503`). So a built-in call whose name the package also declares as a method/function is emitted **qualified** — `builtin.clear(s)` — which resolves to the golib built-in regardless of the same-class shadow; the method call stays `b.clear()`. (This also required golib to gain the Go 1.21 `clear` built-in itself, in slice/span/map forms. Guarded by the `ClearBuiltinShadow` behavioral test; runtime hit this on `pageBits.clear`/`sweepClass.clear`, ~11 errors.)

For a **function-literal parameter** that shadows an enclosing local, the rename must reach the parameter *declaration* itself, not just the body: `run(func(n int){ … n … })` where an outer `n` is in scope emits `run((nint nΔ1) => { … nΔ1 … })`. The body's uses already resolve to `nΔ1`; if the signature still declared the bare `n` (the raw name), the body's `nΔ1` would be undeclared (CS0103). The parameter name in the emitted lambda signature therefore comes from the same shadow-aware identifier mapping as the body (the raw name when nothing is shadowed, so plain function types and non-shadowing parameters are unchanged). (Guarded by the `ClosureParamShadow` behavioral test; the runtime hit this pervasively on `mcall`/`systemstack(func(gp *g){…})` where the closure's `gp` shadows an outer `gp`, ~40 CS0103.)

Conversely, a **local that shadows a *pointer parameter*** must not inherit the parameter's special emission. A deref-aliased pointer parameter is `ж<T> Ꮡp` with `ref var p = ref Ꮡp.val`, so passing it whole to a `*T`-expecting function emits its box `Ꮡp`. But a *local* `t` shadowing a `t *T` parameter (`func mapKeyError2(t *_type, …){ … var t *_type; … }`) is a plain pointer local — passing it should stay `use(tΔ2)`, not `use(ᏑtΔ2)` (the spurious `&` references an undefined `ᏑtΔ2` box → CS0103). The bug was that the "is this a parameter?" check matched by *name*, so the shadowing local was misclassified; it now verifies the resolved object is genuinely one of the function's parameter objects, not just a name match. (Guarded by the `ShadowedPointerParam` behavioral test; runtime hit this on `mapKeyError2`/`interhash`'s inner `var t *_type`, ~11 CS0103.)

### Type-vs-Method Name Collisions

Go keeps types and methods in separate namespaces, so a package may legally declare both a type `foo` and a method `foo` on some receiver. In C# both land in the same package class — the nested type and the `[GoRecv]` extension method — where a type and a method cannot share a name (CS0102). The converter resolves this by `Δ`-prefixing the **type** (`Δfoo`) while the method keeps its core-sanitized name (`foo`), so they no longer collide.

This needs an extra step when the colliding name is also a **golib reserved word** (`slice`, `array`, `channel`, `map`, …). Such a name is `Δ`-prefixed *anyway* — to avoid the golib runtime type (`slice<T>` etc.) — so the method too becomes `Δslice`, and the plain `Δ` no longer separates type from method. In that case the converter appends the type marker `ᴛ` to the **type** only, giving it a name distinct from the method:

```csharp
[GoType] partial struct Δsliceᴛ { … }                          // Go `type slice struct{…}`
[GoRecv] internal static Δsliceᴛ Δslice(this ref builder b, …) // Go `func (*builder) slice(…)`
```

Only the type side is renamed; the method (and every call site and go2cs-gen-generated pointer-receiver overload) stays `Δslice`. This is deliberate: the go2cs-gen generators compute method names independently, so renaming the *method* would desync them — renaming the *type* keeps the converter and generators in agreement (the generators read the type name from the emitted C# syntax/attributes). This mirrors the Go runtime's `type slice struct{…}` (the GC slice header) versus `func (*userArena) slice(…)`.

A **struct field** named like a colliding package-level identifier is *not* renamed: a field is struct-scoped (`g.trace` does not collide with a package type/method `trace` in C#), so the field declaration keeps its core-sanitized name (`trace`). The box-field accessor static the `TypeGenerator` emits for it is therefore `g.Ꮡtrace` (the `Ꮡ`-prefixed declared member name). The converter's `&g.field` address form (`Ꮡg.of(g.Ꮡtrace)`) must use that **declared** field name — it derives the accessor member from `getCoreSanitizedIdentifier` plus the type-colliding rename, *not* from the general identifier path that applies the package-level collision `Δ`-rename. Using the latter would emit `g.ᏑΔtrace`, which has no matching generated static (CS0117). Reserved-word fields keep their `Δ` (the field really is declared `Δarray` for a field named `array`), so the accessor is `Ꮡ`+the declared name in every case. (Guarded by the `CollisionFieldBoxAccessor` behavioral test; runtime hit this on `g`/`m`/`p`'s `trace`/`stack`/`p` fields, ~20 CS0117.) The generated accessor's **accessibility** matches the *field's* (its exportedness), not the field type's name — an exported field `Fun [1]uintptr` (C# `array<nuint> Fun`) yields a **public** `ᏑFun`, so another package's `other.of(ITab.ᏑFun)` can reach it; deriving the scope from the type's simple name (`array` → lowercase → `internal`) would make the cross-package accessor unreachable (CS0117 in runtime's `iface.go` walking `abi.ITab.Fun`).

The same struct-scoped rule applies to a **keyed composite-literal field name**. `Frame{funcInfo: f}`, where the field `funcInfo` is named like a colliding package type/method (declared unrenamed as `funcInfo`), must emit the C# initializer key `funcInfo:` — the package-level `Δ`-rename that `convExpr` would apply yields `ΔfuncInfo:`, which is not a parameter name of the generated constructor (CS1739). `convKeyValueExpr` therefore emits a struct-field key whose name collides at package level via `getCoreSanitizedIdentifier` (the declared name), not the general identifier path. (Same `CollisionFieldBoxAccessor` test; runtime hit this on `Frame{funcInfo: …}` in `symtab`.)

The *type* half of the same accessor (`receiver.of(Type.Ꮡfield)`) needs care too. Go code routinely names a local after its own type — `m := getg().m`, where `m` is a `*m` — so taking the address of one of its fields (`&m.park`) emits `m.of(m.Ꮡpark)`, in which the bare type reference `m` binds to the **variable** `m` (a `ж<m>`, which has no `Ꮡpark`) instead of the type (CS1061). Because a converted struct is nested in its package's static class, the converter qualifies the type with that class — `m.of(runtime_package.m.Ꮡpark)` — which a same-named local cannot shadow. A bare `m` (binds the variable) and a `go.m` (the struct is not a direct member of the `go` namespace) both fail; the package-class qualifier is the correct form. This is applied **only on a collision** (the `.of()` receiver variable's name equals the type's simple name), so every other box accessor keeps its un-namespaced, Go-like form — no golden churn. (Guarded by the `VarNamedAsType` behavioral test; runtime hit this on `m`/`Δp` locals taking field addresses, ~9 CS1061.)

The same collision fires when the receiver is that variable's **lambda capture**. Inside a closure the captured variable renames to its capture copy (`mʗ1`), so the receiver-equality check alone misses it — but the *enclosing* local `m` is still visible to the C# lambda, so the accessor's bare owning-type reference binds to it all the same: runtime `rwmutex.lockSlow`'s `systemstack(func() { …; notesleep(&m.park) })` emitted `mʗ1.of(m.Ꮡpark)` → CS1061. `boxAccessorType` therefore also qualifies when the receiver is the type name plus the capture marker (`typeName + ʗ…`), yielding `mʗ1.of(runtime_package.m.Ꮡpark)`. (Guarded by a further extension to `CollisionFieldBoxAccessor` — `capturedLocalNamedAfterType`, a type-named local field-addressed inside a capturing closure, write-through verified vs Go; cleared runtime rwmutex's 2 CS1061, 91 → 89.)

The type half also needs the **type-vs-method collision rename** (above). When the accessor's owning type is itself a colliding name — `type funcInfo` versus a method `func (f *Func) funcInfo()`, so the type is declared `ΔfuncInfo` — taking the address of one of its fields must use the renamed type (`Ꮡ(f).of(ΔfuncInfo.Ꮡnfuncdata)`); a bare `funcInfo.Ꮡnfuncdata` binds to the package's static `funcInfo` method group (CS0119). The `boxAccessorType` helper applies the `Δ`-rename to a bare same-package collision name before its receiver-shadow check (the renamed name no longer matches a raw-named local, so the two disambiguations compose). (Guarded by an extension to `CollisionFieldBoxAccessor` — a global whose type is the collision type; runtime hit this in `symtab`'s `pcdatastart`/`funcdata`.)

A **collision-renamed owning type is qualified unconditionally**, not just when it equals the `.of()` receiver — because a Go local named after its type is renamed to the *same* `Δ`-name, so such a local **anywhere in the function** shadows a bare `Δp.Ꮡfield` (C# locals are function-scoped). Runtime's malloc `persistentalloc1` does `persistent = &mp.p.ptr().palloc` and then declares a local `p` further down (renamed `Δp`); the accessor `(~mp).p.ptr().of(Δp.Ꮡpalloc)` bound its bare `Δp` to that later local — CS0841 (use-before-declaration), and CS1061 regardless (the local's type has no `Ꮡpalloc`). The receiver (`(~mp).p.ptr()`) is not the colliding local, so the receiver-name check missed it. `boxAccessorType` now qualifies whenever the type name is `Δ`-prefixed (a type is never shadow-renamed — types are package-level — so a `Δ`-prefixed accessor type is always a collision rename), emitting `(~mp).p.ptr().of(runtime_package.Δp.Ꮡpalloc)`. Qualifying is value-identical to the bare form when nothing shadows, so it is safe to apply to every collision-type accessor. (Guarded by a further extension to `CollisionFieldBoxAccessor` — `localShadowsCollisionType`, a local named after the collision type declared after the accessor; cleared runtime malloc's CS0841 plus two mheap `Δp.Ꮡgcw` CS1061 of the same shape, 148 → 145.)

A related case is the **box name of a shadow-renamed receiver/parameter**. A deref-aliased pointer (a receiver or a `*T` parameter) is emitted as `ref var <name> = ref Ꮡ<raw>.val` — the `Ꮡ` companion always keeps the **raw** Go name, even when the value alias is shadow-renamed for a collision (`func (p *cpuProfile) add()` where `p` collides with the type `p` → `ref var Δp = ref Ꮡp.val`). When a pointer-receiver (capture-mode) method is then called on that receiver/parameter, the call routes through the box, and that box reference must use the raw name `Ꮡp` — the value alias `Δp` would yield `ᏑΔp`, which is not in scope (CS0103). The converter builds the box from the raw identifier name (not the shadow-renamed value form), but only when they differ — so non-renamed receivers are unaffected (no churn).

The same raw-box-name rule applies when such a shadow-renamed pointer is **captured by a closure** (where the value alias is referenced through its box, since the `ref`-local can't be captured — see the *box-ref* section below). A value use inside the closure becomes `Ꮡp.val.n` and a field-address use `Ꮡp.of(T.Ꮡn)` — both rooted at the raw box name `Ꮡp`, never the renamed `ᏑΔp`. The field-address form (`&p.field`) routes through the box-ref address path rather than the generic pointer-variable path: that generic path would prepend `Ꮡ` onto the closure's box-deref read (`Ꮡp.val`), yielding a double-boxed `ᏑᏑp.val` (CS0103). Because the captured pointer's box `Ꮡp` *is* the `ж<T>`, the field address is simply `Ꮡp.of(T.Ꮡfield)` — the same form as a captured value struct. (Guarded by the `RenamedReceiverBox` behavioral test, which exercises a shadow-renamed receiver calling a capture-mode method, plus a shadow-renamed pointer parameter both read through and field-addressed inside a closure; runtime hit this on `p`/`Δp` receivers calling methods like `p.addExtra()` and on closures capturing such pointers, ~12+ CS0103.)

A closure that captures an outer variable is emitted with a snapshot copy declared before the lambda — `var sʗ1 = s;` — and uses of the captured variable inside the lambda are rewritten to that capture name `sʗ1`. The capture-name mapping is keyed by **name**, which breaks on a **self-shadowing initializer inside the closure**: runtime `mgcsweep`'s `systemstack(func() { s := spanOf(uintptr(unsafe.Pointer(s.largeType))); … })` declares an inner `s` whose initializer reads the *outer* captured `s`. Both the captured use (the RHS `s.largeType`) and the distinct inner binding were mapped to the same `sʗ3`, so the inner declaration emitted `var sʗ3 = …(~sʗ3)…` — its RHS binding to the not-yet-initialized inner variable (CS0841). The fix records the captured **object** alongside the name, and applies the capture name only when an ident resolves to that exact outer object; the inner binding falls through to its own (shadow-renamed) name. The emission is `var sΔ1 = spanOf(…(~sʗ3)…)` — the inner `s` shadow-renamed to `sΔ1` (distinct from the capture `sʗ3`), its RHS correctly reading the captured `sʗ3`, and later uses of the inner `s` using `sΔ1`. Because the object check passes for every non-shadowing capture (the ident *is* the captured variable), it changes nothing outside this self-shadow case (zero golden churn). (Guarded by the `ClosureSelfShadowCapture` behavioral test — a captured pointer with an inner `s := f(s)` in a `systemstack`-shaped call-argument closure, output verified vs Go; cleared runtime `mgcsweep`'s CS0841.)

The same rule applies to an **escaping local** whose address is taken — `var p _panic; … preprintpanics(&p)` in runtime's `gopanic`, where `p` collides with the type `p`. The heap allocation is `ref var Δp = ref heap(new _panic(), out var Ꮡp)`, so the box is `Ꮡp` (raw) and `&p` must emit `Ꮡp`, not `ᏑΔp`. **Crucially, the box-name rule is keyed to the rename *kind*, because the two kinds name their boxes differently:** a type-**collision** rename prepends the marker (`p` → `Δp`) but keeps the raw box (`Ꮡp`), whereas a nested-scope **shadow** rename appends the marker plus a counter (`i` → `iΔ1`, `iΔ2`) and keeps the *shadow* box (`ref var iΔ1 = ref heap<nint>(out var ᏑiΔ1)`, so `&i` correctly emits `ᏑiΔ1`). The converter therefore rewrites to the raw name *only* when the alias is exactly `Δ`+rawname (the collision form); a shadow-renamed or non-renamed var keeps its existing box name. (Guarded by the `CollisionRenamedLocalBox` behavioral test, with `ForVariants`/`NestedVarShadow` covering the shadow-rename form left unchanged.)

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

The asserted type is a **type position**, so an assertion to a **pointer** type renders the pointer *type* `ж<T>`, not a value dereference: `i.(*box)` → `i._<ж<box>>()`. (The starred-operand-is-a-type case previously emitted the type form only inside a `(*T)(p)` cast; a non-parenthesized `*type` fell through to the value-deref path and emitted `box.val` — CS0426, since `val` is not a member type of `box`.) (Guarded by the `TypeAssert` behavioral test's `*box` assertion; runtime hit this in `netpoll.go`'s `arg.(*pollDesc)`.)

The types that support these tuple-returns are defined in the [`golib`](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/core/golib) library; ordinary user-code tuple returns convert as normal C# tuples without special handling.

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

**A `make` length/capacity/size-hint of a non-`int` integer type is cast to `nint`.** The golib allocating constructors all take `nint` — `slice<T>(nint length, nint capacity)`, `map<K,V>(nint capacity)`, `channel<T>(nint capacity)` — and C# does **not** implicitly convert a `uintptr`/`uint`/`uint32`/`uint64`/`int64` (C# `nuint`/`uint`/`ulong`/`long`) to `nint`. So `make([]byte, n/goarch.PtrSize)` with a `uintptr` length would leave `new slice<byte>(n / …)` with no applicable constructor, and overload resolution falls onto `slice<T>(T[])` — reported as `CS1503` ("cannot convert `nuint` to `byte[]`"); a map/chan with a `uintptr` hint is a direct `nuint`→`nint` CS1503. The converter casts each such length/capacity/hint argument to `nint`: `new slice<byte>((nint)(n / goarch.PtrSize))` (both args of `make([]byte, l, c)`), `new map<K,V>((nint)(hint))`, `new channel<T>((nint)(size))`. A plain `int` (`nint`) and an untyped-constant argument (`make([]byte, 4)` → a bare `4`) bind directly and are **left uncast** (no golden churn) — as are the widening `int8`/`int16`/`uint8`/`uint16` kinds. (Guarded by the `MakeSliceUintptrLen` behavioral test — `uintptr`/`uint`/`uint32`/`uint64` slice lengths, a `uintptr` len+cap, a `uintptr` map hint and chan size, and int/untyped controls, all `len`/`cap`/element values verified vs Go; runtime hits this in `mbitmap`'s `make([]byte, n/goarch.PtrSize)`.)

**Slicing a pointer-to-array.** Go lets a `*[N]T` be sliced directly — `p[lo:hi:max]`, `p[:]` — auto-dereferencing the array. The C# box `ж<array<T>>` has no slice/range members (its underlying `array<T>` does), so the converter dereferences first: `p[1:3:4]` → `(~p).slice(1, 3, 4)`, `p[:]` → `(~p)[..]`, `p[2:]` → `(~p)[2..]`. Without the deref the call binds to the box and fails (CS1929). The resulting slice shares the array's backing storage, matching Go. (The `(*[N]T)(ptr)[:n]` pointer-*cast* form is different — see *Pointer-cast slice* below.) A deref-aliased pointer **parameter** or **receiver** is the exception: it is emitted as the pointed-to *value*, not a box, so a `~` on it would deref a non-pointer (CS0023). When that value is a **named** array type — `b *pageBits` emitted `ref pageBits b`, where `pageBits` is `[N]uint64` — the wrapper has no slice/range members, so its underlying `array<T>` is reached via `.val`: `b[:2]` → `b.val[..2]`. When it is an **anonymous** array (`p *[N]T` → `ref array<T> p`) the value already *is* the `array<T>` and is sliced directly (`p[:]` → `p[..]`). Only a pointer-to-array **box** (a local, a field, a call result) gets the `~` deref. (Guarded by the `PointerArraySlice` behavioral test — local box, named-array receiver, and named-array parameter; runtime hits this in `select.go`'s `cas1[:ncases:ncases]` / `mprof.go`'s `stk[:n:n]` (locals) and `mpallocbits.go`'s `pageBits` receiver methods (`clear(b[:])`).)

**Pointer-cast slice (`(*[N]T)(ptr)[:n]`).** A Go conversion that casts an `unsafe.Pointer` to a pointer-to-array and slices it produces a `[]T` over the pointed-to memory. It is emitted as the golib **`slice<T>`** — the C# representation of *every* `[]T` — built from a `ReadOnlySpan<T>` over the raw pointer: `new slice<T>(new ReadOnlySpan<T>((T*)ptr, (int)n))`. (Earlier it was a bare `Span<T>`, but a `Span<T>` does **not** range as `(index, element)` tuples — `for i := range s` → CS8130 — and has no `Ꮡ(s, i)` element-address — CS0411; `slice<T>` supports both, since it is `IArray<T>`.) The `ReadOnlySpan<T>` constructor takes a C# `int`, so a Go `int`/`uint` length (`nint`/`nuint`) is narrowed via `getRangeIndexer` (through the underlying for a named numeric); an int literal is left as-is. The slice **copies** the pointed-to memory (`ReadOnlySpan.ToArray()`), which is self-consistent for code that only uses the resulting slice (e.g. runtime's `printDebugLog` ranges `state` and writes `&state[i]`, never re-reading the raw buffer; `os_windows` ranges an unsafe `[]byte` read-only). Since this is always the `(*[N]T)(ptr)` unsafe-cast form, it is memory-layout-dependent code whose raw values flow through the `unsafe.Pointer`=`nuint` round-trip (a transient `fixed` address → not GC-stable), so the runtime values are not the contract — only compilable, rangeable, element-addressable C#. (Guarded by the `PointerCastSliceRange` behavioral Compile + target test — index range, value range, and `&s[i]` element-address over a pointer-cast slice; runtime greened `debuglog`'s `printDebugLog` and `os_windows`, ~25 errors via the cascade. The length narrowing is covered by `StdLibInternalAbi`.)

An **untyped (type-inferred) composite literal** — the inner `{…}` of a `[][]rank{ key: {…} }`, which has no explicit type node — is emitted as a target-typed `new(…)` when its inferred type is a struct (the struct constructor takes the field values). When the inferred type is a **slice or array**, that form is wrong (`slice<rank>`/`array<rank>` have no element-list constructor → CS1729); the converter emits the element-array projection instead — `{rA, rB}` (inferred `[]rank`) → `new rank[]{rA, rB}.slice()`, and an inferred `[2]int` → `new nint[]{…}.array()`. When the inferred type is a **pointer-to-struct** — the `[]*T{ {…} }` shorthand for `&T{…}` — it is emitted as the boxed struct constructor `Ꮡ(new T(field: val, …))` (a bare `new(…)` would target the box `ж<T>`, whose constructor lacks the struct's fields → CS1739). (Guarded by `UntypedNestedSliceComposite`; runtime/lockrank.go's `lockPartialOrder` is a `[][]lockRank`, and runtime1.go's `dbgvars` is a `[]*dbgVar`, of these.)

An **indexed (keyed) slice/array literal** — `[]string{lockRankSysmon: "sysmon", …}` — is emitted as a golib `runtime.SparseArray<T>` collection initializer (`[index] = value`). Its indexer takes a Go `int`. When an index key's Go type is a **defined integer type** whose underlying type does not implicitly widen to C# `int` (i.e. `int`/`int64`/`uint`/`uint32`/`uint64`/`uintptr`, as opposed to `int8`/`uint8`/`int16`/`uint16`/`int32`), the key is cast to `int` so it satisfies the indexer (CS1503 otherwise): `[lockRankSysmon]` (a `type lockRank int`) → `[(int)lockRankSysmon]`. A key that already widens (e.g. a `uint8`-backed `Kind`) is left uncast.

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
therefore emits an explicit `public S() { }` for every struct, so `new S()` runs the field initializers
and each array field gets its `new(N)` backing. (C# 11 auto-defaults any field without an initializer, so
the empty body suffices; a slice/map/chan field — which has no `new(N)` initializer — stays its nil zero
value, matching Go.) This is generator-only and produces no golden churn (the `.g.cs` output is not a
golden). It is what lets `ArrayOfCrossPackageType` run as an **output-compared** test (`len(x.c)` /
`len(x.d)` print `3 2`); before the fix, indexing `&x.c[i]` threw a `NullReferenceException`, so the test
was compile+target-only.

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

## Generic Constraints
A Go generic constraint becomes a C# `where` clause. Most type-set constraints lift to the matching golib/.NET interface — a `[]T` element constraint to `ISlice<T>`, `map[K]V` to `IMap<K,V>`, `chan T` to `IChannel<T>` — plus, for operator-bearing type sets, the `System.Numerics` operator interfaces (`IAdditionOperators`, `IComparisonOperators`, …) so the body's `+`/`<`/`==` on the type parameter compile. The Go built-in `comparable` maps to golib's CRTP `comparable<T>`.

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

## Type Aliasing
Go supports two kinds of [type aliasing](https://go101.org/article/type-system-overview.html#type-definition): a "type definition" and a "type alias declaration".

### Type Definitions
For a Go "type definition" the new type is a distinct type that shares an [underlying type](https://go101.org/article/type-system-overview.html#underlying-type) with its base. Because converted types are structs (no inheritance), the converter relies on the source generators (see [Source Generators](#source-generators)) to emit the bridging needed for these to be used interchangeably while remaining distinct: implicit conversion operators down to the underlying type (via `ImplicitConvGenerator` / `TypeGenerator`), and, where the base is a built-in like `slice`, the relevant interface (`ISlice<T>`, etc.) implementation. A named type also supports the extension methods (receiver functions) of its underlying types, which the generators surface as proxy/overload methods.

When a pointer conversion `(*Target)(srcPtr)` bridges two structurally-identical structs, the converter records an **indirect** (boxing) implicit conversion `Source → ж<Target>` and `ImplicitConvGenerator` emits `implicit operator ж<Target>(Source src) => Ꮡ(new Target(<members>))`. For a **self-boxing** conversion — `Source` and `Target` are the *same* struct (`mspan → ж<mspan>`), which arises from a self-referential struct's recursive sub-struct conversions — that member-by-member reconstruction is both unnecessary and wrong: a pointer field whose target ctor parameter is itself a `ж<…>` was deref'd (`src.f?.val ?? default!`), and a value cannot bind a pointer parameter (CS1503). The generator detects self-boxing (the boxed element type equals the source) and emits `Ꮡ(src)` instead — boxing a copy of the whole struct directly, identical in effect for a pointer-free struct and correct for one with pointer fields. (Validated by the green baseline build, which regenerates every `.g.cs`, plus the `TypeConversion` behavioral test for the non-self-boxing form; runtime exercised self-boxing for `mspan`, `g`, `stackScanState`, `hmap`, etc.)

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

## Delegates to Value Receiver Instances

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

## Defer / Panic / Recover
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

## Expression Switch Statements
Go expression-based `switch` statements are flexible: cases do not fall through automatically (no `break` needed), and the `fallthrough` keyword runs the next case body bypassing its expression. Based on the [Manual Tour of Go Conversions](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions), converting to `if / else if / else` is the best choice for most cases. When every case label is a C# **compile-time constant** and there is no `fallthrough`, a traditional C# `switch` works. "Constant" here means a C# `const` — a literal, a computed literal expression (`a + b`), or a *typed* basic-type const — not merely a Go constant. A case label that references a plain variable, a struct field (`case frame.fp`), an *untyped* / named-type / cross-package const emitted as `static readonly` (`case goarch.PtrSize`), or an address-of expression (`case &g`) is **not** a C# constant, so a C# `switch` case label there is invalid (CS9135 / CS0150). Such switches fall back to the `if / else if` form comparing the tag with `==` (a temp captures the tag: `var exprᴛ1 = tag; if (exprᴛ1 == frame.fp) …`). The same constant-vs-runtime-value test also chooses `is` (constant pattern) vs `==` for a single-value case within the if-else form. A Go `break` inside a case exits the *switch* (skipping the rest of the case); in the `if / else if` form there is no enclosing C# switch for it to target (CS0139), so a case body that contains such a break is wrapped in a `do { … } while (false)` — the break exits that one-shot loop, i.e. the case. The wrap is emitted only for a case whose body actually has a switch-targeting `break` (one not caught by a nested loop/switch/select), so every other case is unchanged. (A `break` inside a *nested* loop within the case still targets that loop, as in Go.) For cases that use `fallthrough`, the cases are expanded to standalone `if` statements with a local fall-through flag and `goto` to handle break-style exits — the most complex (and least pretty) scenario. In that if-chain form a **trailing `default:` reached via fallthrough** is emitted as a *guarded* `if (fallthrough || !match) { … }` — the guard is needed so the default does not run after a matched-but-non-fallthrough case, but C# cannot prove it always executes. So when such a guarded-default switch is the last statement of a **value-returning** function and every case is terminal, C# reports CS0161 ("not all code paths return a value") even though the Go `default` makes the switch exhaustive (runtime `startpanic_m`). Because a guarded-terminal-default switch cannot be legally followed by reachable Go code (it always returns/exits), the converter emits an unreachable `return default!;` after the if-chain to satisfy C#'s definite-return analysis — gated on the enclosing function/literal actually returning a value (via its own return signature), so a `void` function or a switch that isn't terminal is unaffected. (Guarded by the `SwitchFallthroughDefaultReturn` behavioral test; cleared runtime's CS0161.) A comparison case may use a C# relational/constant pattern (`case {} when x is < 0`) only when the compared-to operand is a C# compile-time constant; for a variable (`case x == y`) or a `static readonly` const (untyped/cross-package), it falls back to a `when` guard (`case {} when x == y`) — a relational pattern there is invalid (CS9135).

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

**Go `int`/`uint` cases and the synthetic concrete case.** A Go `int` maps to C# `nint`, but an `int`-valued *literal* boxed into an interface (`do(1)`) has C# dynamic type `int32`, not `nint`. So a `case int:` emits its native form **plus** a synthetic concrete `case int32:` (and `case uint:` adds `case uint32:`) sharing the same body, to catch both boxings. The exception: if the *same* switch also lists an explicit `case int32:`/`case uint32:` (or `case rune:` ≡ int32), the synthetic is **skipped** — emitting it would duplicate the explicit case (CS8120 "unreachable case") and, being emitted first, would steal the explicit case's values and run the wrong body. With the synthetic suppressed, a typed `int` value (`nint`) hits `case int:` and a typed `int32` value hits `case int32:`, distinctly. (Runtime's `printpanicval` switches over `int, int8, …, int32, …, uint, …, uint32, …`; guarded by the `TypeSwitch` behavioral test. Note: Go `uint` and `uintptr` both map to C# `nuint`, so a switch listing *both* still collides — a golib type-identity limitation, not handled here.)

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

The **empty struct `struct{}` is never lifted** — it maps to the shared golib `EmptyStruct`, so a `struct{}{}` composite literal emits `new EmptyStruct()` and a `map[K]struct{}` ("set") emits `map<K, EmptyStruct>`. Lifting an empty struct would be doubly wrong: it has no fields to model, and the lift mis-attributes its name and identity. When the `struct{}{}` is the value assigned to a map element (`seen[k] = struct{}{}`), the enclosing assignment passes the **LHS ident** (`seen`) into the struct-conversion context to name the lift — so the empty struct was being lifted to `<func>_seen` *and registered under `seen`'s own type, the map* `map[K]struct{}`, in the lifted-type registry. That poisoned every later reference to that map type: the function parameter `seen map[K]struct{}` rendered as the phantom struct instead of `map<K, EmptyStruct>`, and its comma-ok deconstruction (`(_, ok) = seen[k]`) and two-arg indexer vanished (CS8130/CS0021), while real-map call sites mismatched (CS1503). `convStructType` now short-circuits an empty struct to `EmptyStruct` before any lift, mirroring the `!isEmptyStruct` guard that `extractStructType` already applies everywhere else. (Guarded by the `EmptyStructMapSet` behavioral test; runtime hit this on `typesEqual`'s `seen map[_typePair]struct{}` parameter.)

## Struct Type Embedding
Go structs use "[type embedding](https://go101.org/article/type-embedding.html)" instead of inheritance. Since converted structs are C# `struct`s (no inheritance), the `TypeGenerator` manages the equivalent: it adds a field for the embedded type and promotes the embedded type's fields and methods (selection shorthand). Both field and method promotion are **transitive through every embedding level**: when `top` embeds `mid` which embeds `inner`, `top` gets an accessor for `inner`'s field `n` (`top.n => ref mid.n`) and a forwarding receiver for `inner`'s method `describe` (`top.describe() => target.mid.describe()`), each resolving through `mid`'s own one-level promotion. The generator collects an embedded struct's members and methods recursively (following each field whose name equals its type's simple name — Go's embedding marker), with the closest declaration of a name winning, matching Go's promotion rules. **Pointer embeds promote too.** Go also embeds by pointer (`*traceBuf`), whose C# field type is `ж<traceBuf>`; its methods and fields are promoted exactly like a value embed (the field's ref-property is dereferenced — `target.traceBuf.val.method()` — which binds the pointer-receiver method via the `[GoRecv]` `ж<T>` overload). The embedding-marker comparison dereferences the field type first, because a pointer field's simple name carries a `.val` suffix (`traceBuf.val`) that would never match the bare embed field name. This matters most *transitively*: `traceExpWriter` embeds `traceWriter` (value) which embeds `*traceBuf` (pointer), and `traceBuf`'s `varint`/`byte` must promote all the way up — without the deref-aware marker the nested pointer embed is skipped and the upper struct silently loses the method (CS1929). (Guarded by the `NestedEmbeddingPromotion` behavioral test for value embeds and the `PointerEmbeddingPromotion` test for one-level and two-level-transitive pointer embeds; runtime relies on the field case for `stackWorkBuf` → `stackWorkBufHdr` → `workbufhdr.nobj` and the pointer case for the trace writers. Caveat: a *zero-value* struct does not initialize the embedded type's captured box, so promoted access on a `default`-constructed value NREs — construct via a composite literal.) Because the promotion is performed at conversion time by the generator, methods added later in hand-written C# are not automatically promoted; keeping the source in Go and re-converting (or using explicit interfaces) is the maintainable path.

**A pointer-receiver method promoted through a VALUE embed is routed at the call site, not by a generator forwarder.** When `timeTimer` embeds `timer` *by value* and `timer` has a pointer-receiver method (`func (t *timer) modify(…)`), the generator emits **no** `modify` forwarder on `timeTimer` (a `target.timer.modify(…)` forwarder body would copy the value field, losing the write, and would not bind the `ж<timer>` overload) — so a promoted call `t.modify(…)` on a `*timeTimer` would leave the receiver as the whole `ж<timeTimer>` box, which the promoted method's ж/`[GoRecv]`-ref overload cannot bind (CS1929). The converter instead routes the promoted call through the embedded field's box, exactly as the *explicit* `t.timer.modify(…)` already renders: `t.of(timeTimer.Ꮡtimer).modify(…)` for a pointer local, `Ꮡt.of(timeTimer.Ꮡtimer).modify(…)` for a deref'd pointer parameter (the `&receiver.field` &-machinery supplies the correct box per receiver form). Because it field-refs the real embedded storage — never a `Ꮡ(copy)` — the mutation writes through. This is detected via the method's `types.Selection.Index()` having a single embedded-field hop (`[embeddedField, method]`); it is gated to a **value** embed (a *pointer* embed already yields the box as its field value and is left to the generated forwarder — taking its address would double-box to `ж<ж<T>>`), and to a single hop (deeper chains fall through). (Guarded by the `EmbeddedValuePointerMethod` behavioral test — value embed + mutating pointer-receiver methods called via a pointer local AND a deref'd param, with write-through verified against Go; runtime relies on it for `timeTimer`'s `modify`/`stop`/`reset` in `newTimer`/`stopTimer`/`resetTimer`.)

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

Each discovered "concrete type implements interface" pairing is recorded as an assembly-level attribute in the package's `package_info.cs`, e.g. `[assembly: GoImplement<point, Stringer>]`, which `ImplementGenerator` consumes. Two rules govern how these are emitted:

* **Only impl types declared in the *current* package are recorded.** `ImplementGenerator` realizes the attribute by emitting a `partial struct <Impl> : <Interface>` into the **current package's** namespace and class — so it can only add an interface to a type defined in the *same assembly*. A pairing whose impl type is *imported* from another package (e.g. `image/color/palette` building `[]color.Color{ color.RGBA{…} }`) is therefore **not** re-emitted in the consumer: that relationship is already established in the impl type's own package (`image/color` records `[assembly: GoImplement<ΔRGBA, Color>]`). Re-emitting it in a consumer would generate a broken cross-assembly partial (a fresh empty `palette_package.ΔRGBA` rather than the real `color_package.ΔRGBA`), so the converter skips any pairing whose impl type is not local.
* **Multi-segment interface references are root-qualified.** The `GoImplement` attributes are emitted before the file's `namespace` with only `using go;` in scope; that directive imports the *types* of namespace `go` (so a top-level `io_package.Writer` resolves unqualified) but **not** its nested namespaces. A multi-segment package class such as `container.heap_package.Interface` is therefore root-qualified to `go.container.heap_package.Interface` so it resolves; single-segment refs (`io_package`, `sort_package`) are left unchanged.

## Pointers
Pointer conversions use the golib heap box [`ж<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/%D0%B6.cs) (read "zhe"). Taking the address of a value uses the address-of operator `Ꮡ` (e.g. `Ꮡx`); an escaping local is allocated via `heap(...)`, and addresses of a struct field or array element are taken through `.of(Type.ᏑField)` / `.at<T>(index)`:

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

The per-iteration box is required for Go 1.22 loop-variable semantics: each iteration's variable is distinct, so a stored `&i` must point to a different box each pass (`for i := range s { ptrs = append(ptrs, &i) }` yields `0 1 2`, not `2 2 2`). A non-escaping companion variable still declares directly in the foreach. (Guarded by the `RangeVarHeapBox` behavioral test — both a within-iteration `&i` and the stored-pointer distinctness case; runtime exercises it in `for i := range stackpool` and `for _, f := range s.Fields`.)

The `at<T>(index)` element-address accessor takes a `nint` index. Go permits **any** integer type as an array/slice index and converts it to `int` for the access, but C# has no implicit `nuint`/`uint`/`ulong`→`nint` conversion, so a non-`int` index is narrowed explicitly to match Go's index-to-int conversion (CS1503 otherwise). An `int` index, or an untyped int constant (which renders as a plain int literal), is emitted as-is:

```csharp
var pi = Ꮡa.at<nint>((nint)(i));        // &a[i]      where i is a uintptr
var pe = Ꮡa.at<nint>((nint)(g % 2));    // &a[g%2]    where g is a uint (g%2 widens to long in C#)
```

This is the element-address analogue of the indexed-literal key cast (`SparseArray<T>`, above) and the `IBinaryInteger<T>` width-agnostic length params on `unsafe.Add`/`Slice`/`String`. (Guarded by the `ArrayWideIndexAddress` behavioral test.)

The address of a **slice** element uses the call form `Ꮡ(slice, index)` (golib overloads `Ꮡ<T>(IArray<T>, int)` and `(…, nint)`) rather than `at<T>`. Go `int` (→ `nint`) and the small integer types that implicitly widen to `int` bind directly, but an unsigned-32-or-wider or 64-bit index (`uint`/`uint32`/`uint64`/`uintptr`/`int64`) binds neither overload, so it is cast to `int`: `Ꮡ(s, (int)(i))`. Only those wide/unsigned types are cast — an `int`/`nint` or small-int index is emitted as-is to avoid churn. (Mirrors the runtime's `&datap.pclntable[funcoff]` / `&filetab[fileoff]`, indexed by `uint32` offsets. Guarded by the `ElementAddressUnsignedIndex` behavioral test.)

The same `(int)` narrowing (the shared `castWideIntegerToInt` helper) applies to the bounds of a **3-index (full) slice** `s[low:high:max]`, which lowers to the golib `.slice(nint low, nint high, nint max)` method: a `uintptr`/`uint`/`uint32`/`uint64`/`int64` bound is cast — `stk[:b.nstk:b.nstk]` (b.nstk a `uintptr`) → `stk.slice(-1, (int)(b.nstk), (int)(b.nstk))`. Go's own slice bounds are `int`, so the narrowing matches Go. A plain `int`/small-int bound is left uncast. (The 2-index range forms `s[lo:hi]` narrow through `getRangeIndexer` for the C# `[..]` range operator; only the 3-index `.slice()` form needed this.) (Guarded by the `Slice3IndexWideBound` behavioral test — `uintptr`/`uint`/`uint64` full-slice bounds on an array and a slice + an int control, values verified vs Go; runtime hits this in `mprof`'s `stk[:b.nstk:b.nstk]`.)

**Address of an element of an array *field* reached through a pointer or boxed struct.** When the array being indexed is a field of a heap-boxed value — `&mp.future[i]` where `mp` is a `*memRecord`, or `&g.future[i]` where `g` is an address-taken global — the *array field's* address goes through the box-field accessor first, then the element index: `Ꮡmp.of(memRecord.Ꮡfuture).at<cycle>(i)` (pointer parameter), `mp.of(...)` (pointer local), `Ꮡg.of(rec.Ꮡfuture).at<cycle>(i)` (boxed global). A naive `Ꮡ` prefix on the field read (`Ꮡ(~mp).future`) instead binds `.future` to the box value `Ꮡ(~mp)` (a `ж<memRecord>`, which has no `future` member) → CS1061. This requires a matching golib detail: `ж<T>.at<TElem>(index)` resolves the array through the `val` property, **not** the raw `m_val` field — for a field-reference pointer produced by `of(...)`, `m_val` is an empty default and the real array lives behind `val` (the same resolution `of(...)` itself uses). Reading `m_val` would miss the array → null-deref at runtime even though the C# compiled. (`array<T>` is a readonly struct over a shared backing `T[]`, so the value `val` yields still aliases the real elements; writes through the returned element pointer land.) (Guarded by the `PointerFieldArrayElementAddress` behavioral test — pointer parameter and pointer local both taking `&p.future[i]` and mutating through it.)

The same `val`-not-`m_val` rule applies to the **dereference operator** `~`. A value read through a pointer — `(~c).field`, the form the converter emits for `c.field` where `c` is a `*T` — must resolve through `val`. For a *field-reference* pointer (`c := &b.w` → `Ꮡb.of(box.Ꮡw)`) or an array-element pointer, the real storage lives behind `val` and `m_val` is an empty default, so `operator ~` returning `m_val` would read a **zero-valued copy** (`(~c).a` → `0`) — it compiles but is silently wrong. `ж<T>.operator ~` therefore returns `value.val` (which resolves struct-field / array-element references and, for a standard pointer, is exactly `m_val`), matching the `IPointer<T>.operator ~` that already did. This surfaced when a defined-type-over-struct's forwarded fields were read back through a `*wrapper`, but it is general to any `*x.field` value read. (Guarded by the `NamedTypeOverStruct` behavioral test's read-back path.)

The `at<E>(i)` **element type `E` is rendered fully-qualified** — `at<sync.atomic_package.Int32>`, not the file-local alias `at<atomic.Int32>`. A namespace-rooted type resolves inside `namespace go;` without any `using <pkg>` alias, whereas the alias form needs the file to import that package. A file can index a cross-package-typed array field of a struct without ever naming the element type (so Go requires no import, and the converter emits no `using atomic`), which would leave the alias unresolved (CS0246, e.g. runtime's `tracecpu.go` indexing `trace.cpuLogWrite`). A current-package or basic element renders identically either way, so this is churn-free. (Guarded by the `ArrayOfCrossPackageType` behavioral test's `&x.c[i]` element-address case.)

Using `ж<T>` rather than the C# `ref` keyword avoids the escape-analysis complications of passing a `ref` into code that expects a heap-allocated pointer. This is a simplification that can cost an unnecessary heap allocation when an address is taken; a future escape-analysis pass could keep such values on the stack when it is provably safe, similar to how [Go does this](https://golang.org/doc/faq#stack_or_heap) at compile time.

> Note: a package-level global whose address is taken is backed by a real heap box so that writes through `&global` (and `&global.field`) are observed, rather than mutating a copy.

### Capturing the address of a heap-boxed local in a closure
A local whose address is taken (`&m`) is heap-boxed: the converter emits `ref var m = ref heap(new T(), out var Ꮡm)`, where `Ꮡm` is the box and `m` is a `ref`-local alias of `Ꮡm.val`. When a **function literal captures such a local and takes its address inside the closure**, the variable must be referenced through the box, not snapshot-copied. A C# `ref`-local cannot be captured by a lambda (CS8175), and the older snapshot capture (`var mʗ1 = m;`) is wrong twice over: it copies the *value* out of the box (so writes through the captured `&m` are lost), and the copy declaration is a statement that has nowhere valid to land when the literal sits in an expression position — e.g. a func literal passed as a **call argument** (`run(func(){ use(&m) })`) or a local initializer (`f := func(){ use(&m) }`).

The fix: a heap-boxed local whose address is taken inside a lambda is marked *box-ref* and the snapshot is suppressed. The box `Ꮡm` is a plain local (a capturable reference), so the C# closure captures it by reference — matching Go's capture-by-reference semantics. Inside the closure the converter then renders every form through the box:

```csharp
ref var m = ref heap(new box(), out var Ꮡm);
run(() => {
    set(Ꮡm);                       // &m  → Ꮡm
    Ꮡm.val.y = Ꮡm.val.x + 1;       // value use of m → Ꮡm.val
});
// &m.field (value struct field) → Ꮡm.of(box.Ꮡfield)
```

This also covers `&m.field` (a value-struct field address inside the closure: `Ꮡm.of(box.Ꮡfield)`). The detection is scoped to the bare `&m` and value-struct `&m.field` forms (the ones with a box-ref emission form); an element address `&m[i]` keeps the existing snapshot path. The behavioral test `FuncLitArgCapture` guards the call-argument, value-use, field-address, and initializer cases.

A **pointer (or other inherently-heap) local** captured by a closure that takes its address needs the box too, but reaches it by a different route. A local of an *inherently heap-allocated* type — a pointer, slice, map, channel, interface, or func — is already a reference, so it normally gets **no** heap box (the `convertToHeapTypeDecl` path returns nothing for such types). But when one is captured by a closure that takes its address (`mToFlush := &node{…}; run(func(){ prev := &mToFlush; … *prev = mToFlush.next })`), the closure needs a *shared* box so writes through `&mToFlush` inside it reach the outer function's storage. The converter detects this as the same *box-ref* mark used above (an inherently-heap local whose address is taken inside a lambda), and for a box-ref local it now emits the heap box even though the type is inherently heap — `ref var mToFlush = ref heap<ж<node>>(out var ᏑmToFlush)` — so the box `ᏑmToFlush` (a `ж<ж<node>>`, i.e. a `**node`) exists for the closure to reference. Without it the closure emitted `ᏑmToFlush` for `&mToFlush` against a never-declared box (CS0103); a same-function `&ptr` with **no** closure still takes the `Ꮡ(ptr)` copy form (a copy is fine there — no shared storage is needed), so that case is unchanged.

Reading such a box needs care, because for a box-of-pointer the held value can legitimately be nil while the box itself is a real allocation. `Ꮡm` here is a `ж<ж<node>>` (a `**node`), so `Ꮡm.val` reads the *held pointer value* — not a dereference of `Ꮡm` — and in Go reading `*(&p)` when `p` is a nil `*T`/slice/map yields the nil value, with no dereference and no panic. The strict `ж<T>.val` getter (which panics on a null stored value by design, so a genuine `*p` on a nil pointer still throws) would wrongly panic on that read. So the converter emits the golib `ж<T>.ValueSlot` accessor for these box-of-pointer reads — identical to `.val` but without the nil-pointer-dereference check, returning the *real* slot so reads and writes both persist (unlike `DerefOrNil`, which yields a throwaway slot for a genuinely-nil box). `ValueSlot` is gated to a box-ref **local** of inherently-heap type (a deref'd pointer *parameter* keeps the strict `.val`, since its box wraps the pointed-to value and `Ꮡp.val` is a genuine dereference). The `heap(out …)` / `heap(target, out …)` helpers likewise return `ref pointer.ValueSlot`: a freshly allocated box is structurally non-nil, so the getter's nil check there is always spurious (identical to `.val` for a value-type box; it just avoids a spurious panic when establishing the `ref var mToFlush = ref heap<ж<node>>(out var ᏑmToFlush)` alias). A genuine dereference of the held pointer (the second `.val` in `ᏑmToFlush.ValueSlot.val.v`) stays strict and still panics on nil — preserving Go's "panic ⇒ panic" semantics, and complementing the deliberate strict-`.val` design behind `DerefOrNil`. (Guarded by the `ClosureCapturedPointerAddress` behavioral test — a closure that takes the address of a captured pointer local, walks a linked list by reassigning *through* that address and mutating each node, with the outer function observing both the reassignment-to-nil and the persisted mutations, proving the box is shared rather than copied. Mirrors runtime's `trace.go` `mToFlush := allm; systemstack(func(){ prev := &mToFlush; … mToFlush = mToFlush.next })`, ~4 CS0103.)

A **deref'd pointer parameter or pointer receiver** captured by a closure is box-ref'd the same way, even when only its *value* is used inside the closure (not its address). Such a parameter is emitted as the box `ж<T> Ꮡp` with `ref var p = ref Ꮡp.val`, and the `ref`-local alias cannot be captured (CS8175). Inside the closure a value use becomes `Ꮡp.val.field` and an address use `Ꮡp`, so the closure captures the box by reference — matching Go capturing the pointer. (Guarded by the behavioral test `PointerParamCapturedInClosure`; the runtime captures `*maptype` / `*m` parameters this way pervasively.)

A pointer **receiver** captured by a closure needs an extra step the parameter case does not: the box `Ꮡp` only exists if the method is emitted **direct-ж** (the box passed *as* the receiver, `this ж<T> Ꮡp`). A normal pointer-receiver method is `[GoRecv] this ref T p` (a value-ref receiver, with the `ж<T>` companion generated separately), which has no box for the closure to reference. So "the receiver is referenced inside a function literal" is a **direct-ж trigger** — a fourth one alongside taking a field's address (`&p.field`), returning the receiver (`return p`), and using the receiver as a bare pointer value (`p.next = p`, `p != q`). Mirrors runtime's `func (p *_panic) nextFrame() { systemstack(func(){ … p.lr … }) }`. A closure parameter that shadows the receiver name resolves to a distinct object, so it does not falsely trigger the promotion. (Guarded by the `ReceiverCapturedInClosure` behavioral test — receiver captured by an immediately-invoked closure that reads/writes through it, by one that takes a field's address, and by one that is *returned* so the box must outlive the call.)

Once a method is direct-ж, its receiver is the box `Ꮡc`, but the deref'd value alias `ref var c = ref Ꮡc.val` is what most uses see. When such a receiver is passed **whole** as a pointer argument — `stackcache_clear(c)` in `func (c *mcache) prepareForSweep()` — the argument must be the box `Ꮡc`, not the value alias `c` (a value cannot bind a `ж<mcache>` parameter → CS1503). A deref-aliased pointer *parameter* is already handled (it is an `identIsParameter`), but a direct-ж *receiver* is not a parameter, so the call-argument conversion recognizes it explicitly and emits the box. (Guarded by the `DirectBoxReceiverPassedWhole` behavioral test.)

**Reassigning a pointer parameter to a new pointer.** A `*T` parameter that walks memory by reassignment — `bits = addb(bits, n)` (a `*byte` step in the runtime's bitmap scanners) or `p = p.next` (a list walk) — cannot write through its value alias: `ref var bits = ref Ꮡbits.val` makes `bits` the pointed-to *value*, and a pointer RHS (`ж<byte>`) does not fit it (CS0266/CS0029). The reassignment instead repoints the **box** and re-aliases the value var — `Ꮡbits = addb(Ꮡbits, n); bits = ref Ꮡbits.val;` — reusing the same box-reassignment path that handles a direct-ж receiver's `r = r.prev` (the RHS already emits the box form). (Guarded by the `PointerParamWalk` behavioral test, a circular-list walk that reassigns the parameter and reads the pointed-to value each step.) Reassigning a *pointer local* (not a parameter) is unaffected — a local already holds the box.

**Nil-terminated walk.** A pointer-parameter walk that stops at a nil terminator — `func sumList(p *node) int { for p != nil { total += p.val; p = p.next } }` — needs two extra pieces, *modeled together*:

1. **Compare the box, not the value alias.** The loop guard `p != nil` must emit `Ꮡp != nil` (the box). Each binary operand's pointer context is otherwise taken from the *other* operand's pointer-ness, and `nil` is not a pointer type — so the param would convert in value form (`p != nil`, comparing a `node` struct value, the wrong thing). The converter forces the box form for a deref'd pointer *parameter* in a `==`/`!=` comparison. This is safe only for a parameter: a pointer *local* is already the box, and forcing it would emit a non-existent `Ꮡlocal`.
2. **Nil-safe re-alias.** On the final step `p.next` is nil, so `Ꮡp = p.next` repoints the box to nil; re-aliasing through the plain `Ꮡp.val` getter would then throw a nil-pointer dereference before the guard is re-checked. The deref/re-alias instead routes through the golib `ж<T>` extension `Ꮡp.DerefOrNil()`, which returns a `ref` to a shared `default(T)` slot when the box is nil (never read while nil — the `Ꮡp != nil` guard excludes it) rather than throwing. The entry alias uses it too, so an empty-list call (`sumList(nil)`) is nil-safe at entry.

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

`DerefOrNil()` is **not** a substitute for a genuine dereference: reading or writing `*p` on a nil pointer (`~Ꮡp` / `Ꮡp.val`) still panics, preserving Go semantics — only the re-alias, which captures a reference without reading it, uses the nil-safe form. Both pieces are gated on a pointer parameter that is **both** compared with `==`/`!=` **and** reassigned in the body (the true walk signature); a param that is merely compared (never repointed, e.g. an identity `==` check) keeps the plain `.val` form, so non-walk code is unchanged. (Guarded by the `PointerParamNilWalk` behavioral test — a nil-terminated sum, a mutate-through-the-parameter pass, and an empty-list call. The never-nil circular walk stays on the plain `.val` path via `PointerParamWalk`.)

A **package-level global** referenced inside a closure is *not* captured at all — it is a C# static, accessed live. A value snapshot (`var gʗ1 = g`) would copy the struct (so `&gʗ1` has no box → CS0103, and writes through the global from inside the closure would be lost) and is semantically wrong, since Go reads/writes the live global. For an address-taken (heap-boxed) global the closure references the static box `Ꮡg` directly — a method call routes as `Ꮡg.method()` and a field address as `Ꮡg.of(T.Ꮡfield)`. (Guarded by `GlobalCapturedInClosure`; the runtime does this in every `systemstack(func(){ … mheap_ … })`.)

### Capture-mode methods called through a value field of the receiver
A pointer-receiver method that takes the address of one of its own fields (`func (c *Counter) Add(d int32) int32 { return bump(&c.n, d) }`) is *capture-mode*: it is emitted with the heap box **as** its receiver (`this ж<Counter> Ꮡc`) so `&c.n` can field-reference the real storage as `Ꮡc.of(Counter.Ꮡn)`. When another struct embeds such a type as a **value field** and drives it through that field — `func (f *Flag) Incr() int32 { return f.c.Add(1) }` — the call needs a `ж<Counter>` aliasing the real `f.c`. The enclosing method is therefore itself promoted to capture-mode (direct-ж), and `f.c.Add(1)` is emitted as `(&f.c).Add(1)`:
```csharp
public static int32 Incr(this ж<Flag> Ꮡf) {
    ref var f = ref Ꮡf.val;
    return Ꮡf.of(Flag.Ꮡc).Add(1);   // f.c.Add(1) — nested field-address box
}
```
The nested `Ꮡf.of(Flag.Ꮡc).of(Counter.Ꮡn)` chain resolves each level through `ж<T>.val` (which honors a parent that is itself a field/array reference), so writes land on the real embedded field rather than a copy. A plain (non-capture) value method called through the same field — `f.c.Get()` — is left as a normal `f.c.Get()` value call.

This field-address routing applies only to **value** fields. When the field is itself a **pointer** — e.g. cpuProfile's `log *profBuf`, accessed as `cpuprof.log` where `cpuprof` is a heap-boxed global — its C# value is *already* a `ж<profBuf>` box, so a direct-ж method binds to it directly: `cpuprof.log.close()`. Taking the field's address (`Ꮡcpuprof.of(cpuProfile.Ꮡlog)`) would double-box to `ж<ж<profBuf>>` (CS1929). The heap-boxed-receiver routing recognizes that a field selector or indexed element whose own type is a Go pointer is already a box and skips the `&`-machinery for it. This discriminates a pointer *field* of a boxed global (already a box) from a deref'd pointer *parameter* (`s` in `s.Prev()`, a value alias whose box is `Ꮡs`): the latter is a bare identifier, not a selector/index, so it is correctly still routed through `Ꮡs`. The same exclusion applies when the pointer field is reached through a pointer **local** rather than a boxed global — `s := sl.mspan; s.gcmarkBits.bytep(…)` where `s` is a `*mspan` local — which otherwise routed through the pointer-local-field address path (`s.of(mspan.ᏑgcmarkBits)`); the field value `(~s).gcmarkBits` is already the `ж<gcBits>`. (Guarded by the `PointerFieldOfBoxedGlobal` behavioral test, covering both the boxed-global `cpuprof.log.write`/`.close` form and the pointer-local `s.log.push` form; runtime exercises both pervasively, e.g. `mspan.sweep`.)

The same applies when the value field belongs to a **package global** rather than a receiver — `ctrl.total.Add(5)` where `var ctrl controller` and `total` is an atomic field. The method's box address goes through the field-address machinery, `Ꮡctrl.of(controller.Ꮡtotal).Add(5)`, not a bare `Ꮡ` prefix on `ctrl.total` (which would bind to the box variable `Ꮡctrl`, whose value type has no `total` member → CS1061). This is the form runtime uses pervasively for `gcController`, `sched`, `memstats`, etc. The **method call itself** triggers heap-boxing the global: when a pointer-receiver method is called on a (possibly nested) value field of a package value global, the escape pass marks that global address-taken so its box exists — the call site needs the box even when the global is never explicitly `&`-addressed elsewhere. This is gated on the method being ж-only (a pointer receiver): a same-package method known to be capture-mode, **or** any pointer-receiver method whose package's capture-mode set is not locally available — the latter covers cross-package atomic methods (`func (x *Uint32) Store`), which are likewise emitted with only a box receiver, so a plain value/ref of the field cannot bind them (CS1929). The walk to the global root bails at any pointer hop (a field reached through a pointer already has a real address and is handled by the pointer-local / receiver paths), so a receiver/parameter field such as `f.c` is never disturbed. (Guarded by the `AtomicValues` behavioral test's global-atomic-field case; runtime exercises this for `prof.signalLock`, `trace.seqlock`, `scavenge.gcPercentGoal`, etc.)

It also applies when the receiver is an **indexed element** of such a field — `trace.stackTab[i].dump()` (boxed global) — where the element's address goes through the box-field accessor: `Ꮡ(trace.stackTab, i).dump()` for a slice field, or `Ꮡtrace.of(T.ᏑstackTab).at<E>(i).dump()` for an array field. The same routing covers an indexed element of an array/slice reached through a **pointer** — `bh.val[i].Load()`, where `bh` is a pointer and the element is an atomic value — emitted `bh.of(T.Ꮡval).at<E>(i).Load()`. This is gated on the called method being **direct-ж** (a box receiver): an ordinary `[GoRecv] ref` method binds to an addressable element directly, so it is left as `container[i].method()` and only a direct-ж method (which truly needs the box) is routed — avoiding needless churn on the common case. (Guarded by the `IndexedElementDirectBoxMethod` behavioral test — a direct-ж method on an array-element-through-a-pointer-parameter, with mutation persistence verified; runtime hits this on `mprof`'s `bh.val[i].Load()`/`.StoreNoWB()`.)

And it applies when the field belongs to a **pointer local** — `h.s.inc()` where `h` is a `*holder` local and `inc` has a pointer receiver. A pointer local holds the box `ж<holder>` directly, so the value `~` dereference of the field (`(~h).s`) is an rvalue; the `[GoRecv]` method needs an addressable receiver (CS1510 on the generated `ref`). The field's box address is taken instead — `h.of(holder.Ꮡs).inc()` — binding the `ж` overload. (A pointer *parameter* is deref-aliased to a value, so `p.s.inc()` already works without this and is left alone. This is the form runtime uses for `(*c).gp.set(…)` / `.cas(…)` in coro.)

Finally, the same rvalue problem occurs when the field belongs to a pointer reached through *another field* — `o.h.wait.add(…)` where `o.h` is a `*holder` field and `wait` is a value (atomic) field. `o.h` dereferences to an rvalue, so `(~o.h).wait` is not addressable. The receiver is routed through the box-field accessor `o.h.of(holder.Ꮡwait)`, which aliases the **real** field storage — *not* a `Ꮡ(value)` copy, which compiles but silently boxes a copy so the atomic write is lost (a behavioral bug, not a compile error). Both the explicit address form (`&o.h.wait`) and a pointer-receiver method call on the field are routed this way. This is deliberately scoped to a base that is itself a *field selector*: a bare-ident base is the method's own receiver or a deref'd pointer *parameter* (both emitted as an addressable `ref`, so `f.c.Get()` binds directly — routing them through `&` would emit `Ꮡf.of(…)` but a value-ref receiver has no `Ꮡf` box) or a pointer *local* (handled above). (Guarded by the `AtomicFieldThroughPointer` behavioral test — a mutate-then-read proves the real field is updated, not a copy; runtime exercises this for atomic fields reached through pointer chains such as `sgp.g.selectDone.CompareAndSwap` and `gp.m.mLockProfile.recordLock`.)

The base may also be a pointer **rvalue** — a pointer-returning **call** (`getg().schedlink.set(…)`, `q.tail.ptr().schedlink.set(…)`, `Δp.chunkOf(ci).scavenged.setRange(…)`, `getg().m.p.ptr().wbBuf.get2()`) or a pointer **element index** (`batch[i].schedlink.set(…)`). Go auto-derefs the pointer to reach the value field, so the converter renders the read as `(~rvalue).field`; the `~` deref is an rvalue, so a pointer-receiver method on it cannot bind (`CS1510` on the generated `ref`). Unlike a deref-aliased *parameter* (whose box is `Ꮡp`) or a *field* deref (handled above), the call/index value **already is** the `ж<T>` box, so the receiver is materialized straight through it via the box-field accessor — `getg().of(g.Ꮡschedlink).set(…)`, `batch[i].of(g.Ꮡschedlink).set(…)` — never a `Ꮡ(value)` copy (which would lose the write). The routing is scoped to a base that is **not** an ident and **not** a field selector (those are the param/receiver/local/field cases above) and is **not a type conversion**: a conversion `(*T)(p)` renders as a C# *cast* (`(ж<T>)(uintptr)(…)`), a low-precedence form on which a trailing `.of(…)` would mis-bind to the inner operand, so a pointer-reinterpret keeps its existing `Ꮡ(…)` form (the runtime-unsafe S1 territory). (Guarded by the `PointerRvalueFieldReceiver` behavioral test — a pointer-receiver method on a value field reached through a returning call, a method-call chain, and a pointer-element index, each with write-through verified; runtime exercises this for `guintptr.set` via `getg()`/`batch[i]`/`q.tail.ptr()`, `pallocData.setRange` via `chunkOf`, and `wbBuf.get2`/`discard` via `getg().m.p.ptr()`.)

The bare-ident-base exclusion above holds **only for `[GoRecv] ref` methods** (which bind on the addressable value alias directly). A **direct-ж** (box-receiver) method — `func (s *scavengeIndex) find(…)` and the like, emitted with a `ж<T>` receiver — needs the *box*, so calling it on a value field-chain rooted at a deref-aliased pointer **parameter or (direct-ж) receiver** is `CS1929`: `Δp.scav.index.find(force)` (root `p`, a `*pageAlloc` receiver), `mp.trace.seqlock.Load()` (root `mp`, a `*m` parameter), `h.userArena.readyList.remove(s)`. These are routed through the box-field accessor too — `Ꮡp.of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).find(force)` — never a `Ꮡ(value)` copy (which would lose an atomic write). The `&`-machinery recurses through the value field-chain to the param/receiver box: `&Δp.scav.index` builds `Ꮡp.of(…).of(…)`, where the box base is the **raw** parameter name (`Ꮡp`, not the shadow-renamed `ᏑΔp` — a deref param `p`→`Δp` is `ref var Δp = ref Ꮡp.val`, box `Ꮡp`). The routing is gated to direct-ж so a `[GoRecv] ref` method on the same chain keeps binding directly (no churn); a receiver root additionally requires the *enclosing* method to be direct-ж (only then does its receiver box `Ꮡrecv` exist). (Guarded by the `FieldChainBoxReceiver` behavioral test — a direct-ж method on a value field-chain rooted at a pointer parameter and at a direct-ж receiver, both with write-through verified; runtime exercises this pervasively for `scavengeIndex`/`mSpanList`/`timers` methods and `m.trace` atomic fields.)

For the **receiver-root** case, the enclosing method only *becomes* direct-ж through the capture-mode pre-pass's transitive fixpoint: a pointer-receiver method that calls a direct-ж method on a value field-chain of its own receiver — `func (p *pageAlloc) free(…) { … p.scav.index.free(…) }` — is promoted to direct-ж so its receiver box `Ꮡp` exists for the routing above. This detection walks the **full** value field-chain `recvName.f1.…fn.method` (every hop a value, non-pointer field), not just one level: `p.scav.index.free(…)` roots `free` at the receiver `p` through two value fields (`scav`→`index`). A one-level chain (`b.u.Load()` on an embedded atomic) was already detected; the multi-level walk generalizes it. A pointer field anywhere in the chain stops the walk — that subexpression is already a box and roots the call elsewhere (the pointer-field paths above), so it must not trigger promotion. The promotion is transitive: once `pageAlloc.free` is direct-ж, its caller `func (h *mheap) freeSpanLocked(…) { … h.pages.free(…) }` is in turn promoted (now calling a direct-ж method on `h.pages`), and so on up the call graph until a root holding the value through a real box/pointer. (The multi-level receiver-root promotion is covered by the `FieldChainBoxReceiver` test's `deep.bumpDeep` case — `d.mid.c.inc()`, a direct-ж `inc` on a two-level value field-chain of a receiver with no other direct-ж trigger, write-through verified; runtime exercises it on `pageAlloc.free`/`freeSpanLocked`.)

### Converting a Go pointer to `unsafe.Pointer`
`unsafe.Pointer` is the golib class `unsafe_package.Pointer : ж<uintptr>` (a numeric address wrapper). A `uintptr`/`unsafe.Pointer` argument converts through the implicit `uintptr ↔ Pointer` operators, but a **Go pointer** argument (`*T`, emitted as the managed box `ж<T>`) has no such conversion — a plain cast `(@unsafe.Pointer)(ж<T>)` is `CS0030` (when `T` is unrelated to `uintptr`) or a runtime `InvalidCastException` (the base→derived downcast `(@unsafe.Pointer)(ж<uintptr>)` compiles but the object is a plain `ж<uintptr>`, not a `Pointer`). So `unsafe.Pointer(ptr)` for a pointer `ptr` is emitted through the golib helper that pins the pointed-to storage:
```go
func (u *UnsafePointer) Load() unsafe.Pointer { return Loadp(unsafe.Pointer(&u.value)) }
```
```csharp
public static @unsafe.Pointer Load(this ж<UnsafePointer> Ꮡu) {
    ref var u = ref Ꮡu.val;
    return (uintptr)Loadp(@unsafe.Pointer.FromRef(ref (Ꮡu.of(UnsafePointer.Ꮡvalue)).val));
}
```
> The resulting numeric address is **not GC-stable** — the same caveat that applies to every `unsafe.Pointer`-as-`uintptr` use; the runtime intrinsics that consume it (e.g. `Loadp`, `StorepNoWB`) are assembly stubs, so this conversion is about producing compilable C#, not GC-correct pointer arithmetic. (The reinterpret pattern `*(*U)(unsafe.Pointer(&x))` is handled separately and is not affected.)

The `ref` the helper takes depends on how the pointer argument **renders**. A genuine box — an address-of expression, a local pointer variable, a pointer field, a call result — is the `ж<T>` object, so the ref goes through its boxed value: `FromRef(ref (box).val)`. But a **deref-aliased** pointer — a pointer *parameter* or pointer *receiver*, which the body renders as the pointed-to value alias (`ref var p = ref Ꮡp.val`) — is not a box; `.val` on it is `CS1061` (`nuint` has no `val` — runtime `select.go` `unsafe.Pointer(pc0)` and `heapdump.go` `unsafe.Pointer(pstk)`, both `*uintptr` parameters). The alias is itself a ref-local into the boxed storage, so the converter takes its ref directly: `FromRef(ref p)`. Detection reuses `exprIsDerefAliasedPointer` (the same discriminator the pointer-reinterpret block uses). This also let the `guintptr`/`muintptr` receiver family (`runtime2.go` `(*uintptr)(unsafe.Pointer(gp))` inside `guintptr.cas`) compile — previously `ref (gp).val` bound the `[GoType]` wrapper's `val` *property* (CS0206); the CAS it feeds (`atomic.Casuintptr`) is a `partial` asm stub, so the copy-box semantics match the established reinterpret precedent (compile-milestone bar; the faithful managed-referent `ж<T>` model for those types remains a separate effort). (Guarded by the `UnsafePointerParamPin` behavioral **output** test — the parameter and receiver shapes read through the pin and match Go, plus a field-address control that keeps the `(box).val` form.)

The **reverse** direction — reinterpreting a raw address *as* a pointer, `(*T)(p)` where `p` is an `unsafe.Pointer` (or `uintptr`) — is the reinterpret pattern referenced above. Its result is the pointer type `ж<T>`. A plain `(ж<T>)p` cast is `CS0030`: because `unsafe.Pointer` is `Pointer : ж<uintptr>`, reaching `ж<T>` needs the two chained user-defined conversions `Pointer → uintptr → ж<T>`, and C# performs at most one user-defined conversion in a cast. The converter routes explicitly through `uintptr` — `(ж<T>)(uintptr)(p)` — which reads the `T` at `p`'s address via golib's `implicit operator ж<T>(uintptr value) => new ж<T>(*(T*)value)` (with `uintptr(Pointer) => val`, the address the pointer holds). The deref `*((*unsafe.Pointer)(k))` then adds `.val`: `((ж<@unsafe.Pointer>)(uintptr)(k)).val` — Go's read of the `unsafe.Pointer` stored at `k`. This is the identical routing the *dereference* path (`(*int)(p)` inside `*(...)`) already used via its `isPointerCast` flag; the fix extends it to the two shapes that did **not** set that flag: a bare call **argument** `atomicwb((*unsafe.Pointer)(ptr), new)` (runtime `atomic_pointer.go`) and an **extra-paren** deref `*((*unsafe.Pointer)(k))` (runtime `map.go`'s indirect key — `convStarExpr`'s dereference branch sees a `ParenExpr`, not the `CallExpr`, so it never marks the cast). Gated to a **pointer-result** conversion whose **argument** is a raw address (`unsafe.Pointer`/`uintptr` basic); the pointer-to-*named*-type value conversion `(*Base)(defPtr)` (below) has a `*T` argument, is handled earlier, and is not affected. Like every reinterpret through the `uintptr` round-trip, the golib operator reads/boxes a **copy** from a `fixed` address, so this is memory-layout-dependent code whose runtime values are **not the contract** — golib's own `map<K,V>` is what actually runs; the converted `runtime/map.go` only needs to compile. (Guarded by the `UnsafePointerReinterpret` behavioral **Compile + Target** test — both the extra-paren deref and the bare-argument shapes; cleared all 21 `unsafe.Pointer → ж<unsafe.Pointer>` CS0030 in `runtime`, 137 → 114.)

The unsafe builtins `unsafe.Add`, `unsafe.Slice`, and `unsafe.String` accept a length/offset of **any integer type** (Go's `IntegerType` constraint, which includes `uintptr`/`uint`). golib's implementations therefore take a generic `IBinaryInteger` length, truncated to the `int` offset — so `unsafe.Slice(p, uintptrLen)` binds without an explicit cast (a plain `nint` parameter rejected a `uintptr`/`uint` argument with CS1503). (Guarded by `UnsafeBuiltinIntegerLen`.)

Passing an `unsafe.Pointer` **argument to an `unsafe.Pointer` parameter** keeps the `@unsafe.Pointer` struct value — `add(p, x)`, not `add(p.val, x)`. The struct is an exact match for the parameter. Passing its inner `uintptr` (`p.val`) instead would convert implicitly to *both* the `@unsafe.Pointer` parameter and any same-named method's `ж<T>` overload (golib defines `uintptr ↔ Pointer` and `uintptr ↔ ж<T>`), making the call ambiguous (CS0121) — e.g. the runtime free function `add(unsafe.Pointer, uintptr)` versus the `(*notInHeap).add(uintptr)` method's generated `ж<notInHeap>` extension. (Guarded by `UnsafePointerArgPassing`.)

**A cross-package type reference emits its `using <alias> = <namespace>;` even when the file did not import the package under a usable name.** A foreign type renders in short-alias form — `pkg.Type` (`time.Duration`, `abi.Kind`) for a named type, `@unsafe.Pointer` for the `unsafe.Pointer` basic — which resolves only through a file-local alias (`using time = time_package;`, `using @unsafe = unsafe_package;`). That alias is normally generated from a *canonical* (unaliased) `import`, but a file can reference a foreign type with no such import through three routes: **type inference** — a *same-package* function returns a foreign type, so the caller infers a local of that type but never writes `pkg.` and need not import the package (runtime `preempt.go`: `fd := funcdata(f, i)`, where `funcdata` returns `unsafe.Pointer`); a **blank import** (`_ "pkg"`, which binds the C# alias to `_`, e.g. `symtabinl.go`'s `_ "unsafe"` for `//go:linkname`); or an **aliased import** (`import u "unsafe"`, whose alias `u` differs from the canonical `pkg.Name()` prefix the type reference uses). All previously yielded CS0246. The converter now walks every emitted type (`collectTypePackages`, called from `getTypeName` — named types by `pkg.Path()`, an `unsafe.Pointer` basic by the pseudo-path `"unsafe"`, recursing through pointer/slice/array/map/chan/generic/func-signature so a `[]time.Duration` element registers too) and, at file close (`visitFile`), supplies the canonical `using <alias> = <namespace>;` for every referenced foreign package the file did not already import canonically. It is idempotent-safe — a canonical import records its path in `canonicalAliasImported`, so `visitFile` never re-emits (duplicates) it — and a non-canonical alias (`using u = unsafe_package;`) coexists with the added canonical one without conflict. This is the *type-reference* analog of the method-call `addMethodPackageNamespaceUsing`. (Guarded by `UnsafePointerInferredNoImport` — the `unsafe.Pointer` basic arm, scalar/composite/blank-import variants — and `InferredForeignTypeNoImport` — the generic named arm, an inferred `*strings.Reader` in an `fmt`-only consumer.)

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

The same block also covers a **named-numeric pointer reinterpreted to its underlying *basic* type** — `(*uint64)(head)` where `head` is a `*lfstack` (`type lfstack uint64`). This is the runtime's atomic-on-a-named-integer pattern: `atomic.Load64((*uint64)(head))` / `atomic.Cas64((*uint64)(head), …)` on the named atomic types **`lfstack`** (uint64, `lfstack.go`), **`sweepClass`** (uint32, `mgcsweep.go`), **`profAtomic`** (uint64, `profbuf.go`), and **`sysMemStat`** (uint64, `mstats.go`). `ж<lfstack>` and `ж<uint64>` are distinct generic instantiations with no conversion (`CS0030`), so the same value-convert-and-re-box applies — `atomic.Load64(Ꮡ((uint64)(head)))` — using the `[GoType("num:uint64")]` wrapper's `lfstack → uint64` value conversion. The reinterpret condition is generalized from *Named↔Named* to also fire when the **result** elem is a **basic** type whose underlying equals a **named** argument elem's (`namedToBasic`); the result C# type name comes from the result elem directly (`uint64`/`uint32`). Because it boxes a copy, a **read** through the reinterpret is faithful (golib `Load64` reads `Ꮡptr.val` = the copy = the value), which is verified against Go; a **write** through it (`atomic.Store64`/`Cas64`/`Xadd64`) targets the copy, but those intrinsics are asm stubs in the converted runtime, so there is no faithful write-through to lose. Cleared all 13 `lfstack`/`sweepClass`/`profAtomic`/`sysMemStat` `→ ж<primitive>` CS0030 (runtime 114 → 101). (Guarded by `NamedNumericPointerReinterpret` — the read path across uint64/uint32 named types, values verified vs Go.)

## Implicit Pointer Dereferencing
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
    ref var ptr = ref Ꮡptr.val;

    fmt.Printf("Value available at *ptr = %d\n", ptr);
    ptr++;
}
```

A pointer **local** that holds a `ж<T>` box (e.g. `x := list.head`, where `head` is a `*node`) dereferences on field access through the box — a read becomes `(~x).field` and a write `x.val.field = …`. This applies to **promoted** fields too: when `T` embeds another struct, a selector naming an embedded field (`x.next` where `next` is promoted from an embedded header) must still dereference. The converter decides this by checking field membership recursively through embeds, so a promoted-field access on a pointer local is not left as a bare `x.next` on the box (which has no such member, CS1061). This mirrors the Go runtime's `scanstack`, which walks `x := state.head; … x.nobj` where `nobj` is promoted into `stackObjectBuf` from an embedded header.

When the field access is the **LHS of an assignment** and the chain is *nested* — `o.stack.hi = …` where `o` is a pointer local and `stack` is a value-struct field — every dereference in the base must use the assignable `.val` form, not `~`: `(~o).stack` yields a value (an rvalue), so assigning to a field through it is not a variable/property (CS0131). The converter propagates the assignment context down the selector chain, emitting `o.val.stack.hi = …`. This mirrors runtime/cgocall.go's `g0.stack.hi = sp + 1024` where `g0` is a `*g` local.

The same applies to **`++`/`--`** on a field reached through a pointer local — increment/decrement reads *and* writes its operand, so `(~mp).ncgocall++` (a field of an rvalue) is CS1059. The converter emits the assignable `mp.val.ncgocall++`.

**Dereferencing a pointer FIELD reached through a parameter — `*p.field`.** A `*p` where `p` is a pointer parameter is emitted as the value alias `p` itself (the `ref var p = ref Ꮡp.val` local already denotes the pointed-to value), so the converter has a parameter-deref shortcut. That shortcut must fire only when the operand *is* the parameter (`*p`, or `**p`): for `*p.field` — a deref of a pointer *field* reached through `p` (`*gp.ancestors`, where `ancestors` is a `*[]ancestorInfo`) — the operand `p.field` is a distinct lvalue that still needs its own dereference. The shortcut keyed off the *root* identifier (`getIdentifier` digs through the selector to `p`), so it wrongly dropped the field deref, emitting `gp.ancestors` (the `ж<…>` pointer) instead of `gp.ancestors.val`. That silently fed a pointer where the pointed-to value was expected — `for _, a := range *gp.ancestors` ranged the box (CS8130, since a `ж<slice<…>>` is not enumerable as tuples), and `x := *p.cnt` typed a pointer as a value (CS0029). The shortcut now excludes a **selector** operand, so `*p.field` falls through to the selector-deref path and renders `p.field.val`. (Guarded by the `DerefPointerToField` behavioral test — a `for _, x := range *h.xs` over a deref'd pointer-to-slice field and a `*h.cnt` value read, both through a pointer parameter; runtime hit this on `traceback.go`'s `range *gp.ancestors`.)

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

### Reassigned range variable
A C# `foreach` iteration variable is **read-only**, but Go lets a `range` key/value variable be reassigned inside the body (it is a per-iteration copy). When the converter detects such a reassignment (`=`, `+=`, `-=`, `++`, …) of a newly-`:=`-defined range variable, it iterates a temp and declares the variable as a mutable local copy in the body, rather than binding it directly:

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

## Source Generators
Several Go semantics cannot be written directly in C#, so the converter emits compact, attributed partial declarations and lets a set of Roslyn source generators (`src/gen/go2cs-gen/`, referenced as an analyzer by every converted project) synthesize the rest at compile time. This keeps the visible converted code close to the Go original. The principal generators and attributes:

* **`TypeGenerator`** — driven by `[GoType]`. Emits the body of a converted type: a struct's members and equality, a named numeric/slice/array/map/channel type's wrapper and operators (see [Untyped Constants and Named Numeric Types](#untyped-constants-and-named-numeric-types) and [Slices and Arrays](#slices-and-arrays)), and struct-embedding field/method promotion.
* **`ImplementGenerator`** — wires up Go's duck-typed [interfaces](#interfaces): finds the concrete types that satisfy each `[GoType] partial interface` and emits the implementation glue and implicit conversions.
* **`RecvGenerator`** — emits pointer-receiver overloads for receiver methods (`[GoRecv]`), so a method written against a value (`this ref T`) is also callable through the pointer/box form.
* **`ImplicitConvGenerator`** — emits the implicit conversion operators that let a [named type](#type-definitions) and its underlying types be used interchangeably.
* **`PartialStubGenerator`** — emits a throwing `partial` implementation for any bodyless `partial` method that has no other implementing part (e.g. assembly/cgo functions with no convertible body), while leaving real hand-written companion implementations untouched.

Common attributes the converter emits for the generators (and tooling) to consume: `[GoType]` (type bodies), `[GoRecv]` (receiver methods), `[GoTag]` (struct field tags), `[GoPackage]` (package info), and the test-only `[GoTestMatchingConsoleOutput]`.

## Examples

* [Behavioral Tests](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Tests/Behavioral) — per-feature Go↔C# equivalence; the `.cs.target` goldens are current converter output and the most reliable reference for exact emitted forms.
* [Manual Tour of Go Conversions](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions)
* [Manual go101 Conversions](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20go101%20Conversions)
* [Miscellaneous](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Miscellaneous)
* Example excerpt of converted code from the Go [`errors`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/errors/errors.cs) package:
```csharp
public static partial class errors_package
{
    // New returns an error that formats as the given text.
    // Each call to New returns a distinct error value even if the text is identical.
    public static error New(@string text) {
        return new errorString(text);
    }

    // errorString is a trivial implementation of error.
    [GoType] partial struct errorString {
        public @string s;
    }

    [GoRecv] internal static @string Error(this ref errorString e) {
        return e.s;
    }
}
```
