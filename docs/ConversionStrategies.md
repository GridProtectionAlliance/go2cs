# Conversion Strategies

> **A high-level, example-driven tour of how `go2cs` maps each Go construct to C#.** This is the
> readable overview -- every section ends with a **Reference →** link into the exhaustive
> [`ConversionStrategies-Reference.md`](ConversionStrategies-Reference.md), where the same topic
> is documented in full: every emitted form, edge case, phase-level fix, and the [behavioral test](Glossary.md#guard)
> that guards it. Read the summary for the *shape*; open the reference for the *why*.

The guiding goal is that the generated C# is both **behaviorally** and **visually** similar to the
original Go, so a Go developer can read the output and follow it. Two things the visible code does not
show in full make that possible: a hand-written runtime library, **`golib`** (`src/core/golib/`,
supplying `slice<T>`, `map<K,V>`, `channel<T>`, `@string`, `ж<T>`, `nil`, the builtins, …), and a set
of **[Roslyn](Glossary.md#roslyn) source generators** (`src/gen/go2cs-gen/`) that synthesize the Go semantics C# cannot spell
directly (interface satisfaction, receiver overloads, struct-embedding promotion, named-type operators).

> The C# snippets below are drawn from the actual converted standard library (`src/go-src-converted/`,
> Go 1.23.1) wherever possible, paired with their original Go source. A few use small illustrative
> programs where that reads more clearly. Glyphs you will see throughout: **`ж<T>`** a heap "box"
> (pointer, read "zhe"), **`Ꮡ`** address-of, **`Δ`** a disambiguation rename (read "delta"),
> **`@string`** the Go string type, **`default!`** = `nil` in value position, and a handful of
> operator glyphs (`ᐸꟷ`/`ꟷᐳ` channel receive, `goǃ` goroutine, `ꟷ`/`ᐧ` comma-ok/type sentinels).

---

## Contents

- **Packages & project structure**
  - [Package Conversion](#package-conversion)
  - [Package-Level Variable Initialization Order](#package-level-variable-initialization-order)
  - [Compiled Library versus Source Code](#compiled-library-versus-source-code)
- **Numbers, constants & nil**
  - [Constant Values](#constant-values)
  - [Native and Narrow Integer Types](#native-and-narrow-integer-types)
  - [Named Numeric Types and Constant Contexts](#named-numeric-types-and-constant-contexts)
  - [Nil and Zero Values](#nil-and-zero-values)
  - [Empty Interface (`any`)](#empty-interface-any)
- **Assignment & scope**
  - [Multi-Assignment and Evaluation Order](#multi-assignment-and-evaluation-order)
  - [Short Variable Redeclaration (Shadowing)](#short-variable-redeclaration-shadowing)
  - [Multi-Result Values and Comma-Ok Forms](#multi-result-values-and-comma-ok-forms)
- **Composite & named types**
  - [Slices and Arrays](#slices-and-arrays)
  - [Strings (`@string` and `sstring`)](#strings-string-and-sstring)
  - [Maps and Channels](#maps-and-channels)
  - [Generic Constraints](#generic-constraints)
  - [Type Aliasing](#type-aliasing)
- **Functions & control flow**
  - [Delegates to Value Receiver Instances](#delegates-to-value-receiver-instances)
  - [Defer / Panic / Recover](#defer--panic--recover)
  - [Expression Switch Statements](#expression-switch-statements)
  - [Type Switch Statements](#type-switch-statements)
  - [Labeled Control Flow and Loop Variables](#labeled-control-flow-and-loop-variables)
- **Types & polymorphism**
  - [Struct Types](#struct-types)
  - [Struct Type Embedding](#struct-type-embedding)
  - [Interfaces](#interfaces)
- **Pointers & memory**
  - [Pointers](#pointers)
  - [Implicit Pointer Dereferencing](#implicit-pointer-dereferencing)
- **The machinery**
  - [The `go.golib` support namespace](#the-gogolib-support-namespace)
  - [Source Generators](#source-generators)
  - [Manually-Converted Declarations](#manually-converted-declarations)
  - [Deterministic Output](#deterministic-output)

---

## At a glance

| Go | C# rendering | Machinery |
|---|---|---|
| `package foo` · top-level `func Bar()` | `partial class foo_package` · `static` methods (receiver methods → extension methods) | converter |
| `import "x/y"` | `using y = go.x.y_package;` + a `ProjectReference` | converter |
| `int` / `uint` / `uintptr` | `nint` / `nuint` / [`uintptr`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/uintptr.cs) (a distinct golib struct) | [BCL](Glossary.md#bcl) / golib |
| `int32`, `byte`, `rune`, `float64`, … | same-named C# aliases (`global using rune = System.Int32;`) | global usings |
| untyped constant | [`UntypedInt`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/UntypedInt.cs) / [`UntypedFloat`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/UntypedFloat.cs) / [`UntypedComplex`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/UntypedComplex.cs) wrapper | golib |
| `type Celsius float64` | [`[GoType("num:float64")]`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/GoTypeAttribute.cs) `partial struct Celsius` | `TypeGenerator` |
| `nil` | `nil` (golib [`NilType`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/NilType.cs)) or `default!` in value position | golib |
| `interface{}` / `any` | `any` (a global alias for `object`) | BCL |
| `[]T` slice · `[N]T` array | [`slice<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/slice.cs) · [`array<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/array.cs) | golib |
| `map[K]V` · `chan T` | [`map<K,V>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/map.cs) · [`channel<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/channel.cs) | golib |
| `string` | [`@string`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/string.cs) (heap) · [`sstring`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/sstring.cs) (non-escaping stack view) | golib |
| `v, ok := m[k]` (comma-ok) | [`var (v, ok) = m[k, ꟷ];`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/map.cs) | golib |
| `a, b = b, a` | `(a, b) = (b, a);` | C# tuples |
| `*T` · `&x` | [`ж<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/%D0%B6.cs) heap box · [`Ꮡx`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/%D0%B6.cs) address-of | golib |
| `type I interface{…}` | [`[GoType]`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/GoTypeAttribute.cs) `partial interface` + [generated implementing glue](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gen/go2cs-gen/ImplementGenerator.cs) | `ImplementGenerator` |
| struct embedding | [promoted field accessors + method forwarders](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gen/go2cs-gen/TypeGenerator.cs) | `TypeGenerator` |
| `func (t T) M()` / `func (t *T) M()` | [`[GoRecv]`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/GoRecvAttribute.cs)/`this` extension method + a [`ж<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/%D0%B6.cs) overload | `RecvGenerator` |
| `defer f()` · `panic(x)` · `recover()` | body-wrapped [`func((defer, recover) => {…})`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/GoFunc.cs); `defer(f)`; [`throw panic(x)`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/PanicException.cs) | golib |
| `go f()` · `select {…}` | [`goǃ(…)`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/builtin.cs) · [`switch (select(…))`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/builtin.cs) | golib |
| `x.(T)` · `switch x.(type)` | [`x._<T>()`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/builtin.cs) · [`x.type()`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/builtin.cs) | golib / converter |
| generic `[T Constraint]` | `where T : <lifted interface(s)>` | golib / .NET |

---

## Package Conversion

A Go package becomes a C# **static partial class** named `<pkg>_package`, inside a root `go` namespace;
the import path's leading segments become the namespace. Go's package-level functions are `static`
methods; receiver methods are emitted as **extension methods** (decorated `[GoRecv]`). Using a *partial*
class lets the functions from a package's many files coalesce under one import. A program with `main`
converts to an executable project; imported packages convert to referenced library projects.

```go
import "unicode/utf8"
```
```csharp
using utf8 = go.unicode.utf8_package;    // one alias; per-file `package_info.cs` carries the global usings
```

**Full detail:** [Reference → Package Conversion](ConversionStrategies-Reference.md#package-conversion) —
cross-package imports & assembly references, module-aware resolution, exported type aliases crossing
packages (the `ꓸ`-qualified `global using` round-trip), cross-package interface-satisfaction witnesses,
build-tag/`GOOS`/`GOARCH` file selection, and the auto-generated `.slnx` solutions (the stdlib solution, and
the `-recurse` per-project solutions grouped into `src`/`pkg`/`core` folders).

---

## Package-Level Variable Initialization Order

Go initializes package vars in **dependency order** (resolved through function calls); C# static field
initializers run in an **undefined order across** a partial class's files. A var whose initializer
depends — directly, through a package function, or via a func literal — on a var in another file (or
declared later in the same file) is emitted as a bare field plus an init method beside it, and a
generated `package_init.cs` static constructor calls those methods in Go's `InitOrder`. C# runs all
field initializers before any static-ctor body, so the relocated initializers always see their
non-relocated dependencies ready. Everything else keeps the readable inline form.

```go
var procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx") // modkernel32: another file
```
```csharp
internal static ж<LazyProc> procSetFilePointerEx;
internal static void initᴛprocSetFilePointerEx() { procSetFilePointerEx = modkernel32.NewProc("SetFilePointerEx"u8); }
// package_init.cs: static syscall_package() { …; initᴛprocSetFilePointerEx(); … }
```

**Full detail:** [Reference → Package-Level Variable Initialization Order](ConversionStrategies-Reference.md#package-level-variable-initialization-order) —
the three hazard shapes, transitive dependency analysis, moved-dependency closure, addressed globals,
and the `PackageVarInitOrder` behavioral guard.

---

## Compiled Library versus Source Code

Go compiles all source together (including the stdlib), which lets its compiler do whole-program escape
analysis. For now, the go2cs converter **assumes values can escape to the heap** except in the
simplest-to-detect cases (see [Pointers](#pointers)) — a safe default that can cost an unnecessary heap
box. A longer-term direction is publishing already-converted packages as compiled libraries (e.g. NuGet),
which fits how C# developers usually consume dependencies.

**Full detail:** [Reference → Compiled Library versus Source Code](ConversionStrategies-Reference.md#compiled-library-versus-source-code).

---

## Constant Values

A **typed** Go constant emits with its concrete C# type. An **untyped** constant emits as a golib
`Untyped*` wrapper declared `static readonly`, so it adapts to whatever numeric type its use site needs —
just like an untyped Go constant taking its type from context. Numeric literal *formatting* is preserved
where Go and C# overlap (hex, binary, `_` separators), so bit masks and addresses stay recognizable.

```go
const MaxRetries = 3          // typed by use
const win = 100               // untyped
const mask = 0x4000           // formatting preserved
```
```csharp
public const nint MaxRetries = 3;
internal static readonly UntypedInt win = 100;
internal static readonly UntypedInt mask = 0x4000;   // not flattened to 16384
```

Float constant values emit **exactly**: the Go source literal verbatim when it is valid C#, else the
shortest round-trip form — never a shortened decimal. And a **function-local** untyped constant whose
every use resolves to one concrete type is **tightened** to that type — declared with C#'s `const`
where legal, with the now-redundant per-use casts dropped (one value-changing cast stays: the
sub-int32 shift width retype) — so the emitted code reads like the Go source (math `cbrt`; uses that
stay genuinely untyped, feed other constants, or participate in constant folding conservatively keep
the wrapper form):

```go
const (
    C = 5.42857142857142815906e-01 // 19/35 = 0x3FE15F15F15F15F1
    G = 3.57142857142857150787e-01 // 5/14  = 0x3FD6DB6DB6DB6DB7
)
s := C + r*t
t *= G + F/(s+E+D/s)
```
```csharp
const float64 C = 5.42857142857142815906e-01; // 19/35     = 0x3FE15F15F15F15F1
const float64 G = 3.57142857142857150787e-01; // 5/14      = 0x3FD6DB6DB6DB6DB7
var s = C + r * t;
t *= G + F / (s + E + D / s);
```

A native-sized constant whose value doesn't fit a C# `const` (e.g. `^uintptr(0)`) falls back to
`static readonly` with an `unchecked` cast. Note `uintptr` is a **distinct golib struct**, not an alias of
`System.UIntPtr` — Go treats `uint` and `uintptr` as different types, and the struct preserves that
identity.

**Full detail:** [Reference → Constant Values](ConversionStrategies-Reference.md#constant-values) — the
exact-float and local-const tightening rules, the `unchecked` native-int cast rules, wide-unsigned named
consts, and the full `uintptr` conversion matrix.

---

## Native and Narrow Integer Types

Go's `int`/`uint` are platform-sized; C#'s are always 32-bit. C# 9's native integers `nint`/`nuint`
behave exactly like Go's, so `int` → `nint`, `uint` → `nuint` (and `uintptr` → the `uintptr` struct). The
fixed-width types keep readable same-named aliases (`int32`, `byte`, `rune`, …).

The one semantic gap is **narrow arithmetic**: Go evaluates `int8`/`uint8`/`int16`/`uint16` math at that
width with wrap-around, but C# promotes it to `int`. Where a narrow result flows into a narrow-typed slot,
the converter inserts a cast back — which both compiles and restores Go's wrapping:

```go
var a, b uint8 = 200, 100
take(a + b)         // Go: 44 (300 mod 256)
```
```csharp
uint8 a = 200, b = 100;
take((uint8)(a + b));   // wraps to 44, not 300
```

**Full detail:** [Reference → Native and Narrow Integer Types](ConversionStrategies-Reference.md#native-and-narrow-integer-types) —
narrow-arithmetic casts across argument/assignment/return contexts, wide-const overflow folding, signed
minima sign-folding, and the `Index`/`Range` `nint`→`int` caveat.

---

## Named Numeric Types and Constant Contexts

General untyped constant representation is covered in [Constant Values](#constant-values). This section
focuses on what happens after numeric context is known: Go defined types over numeric bases, constants that
must flow into those named or native-width types, and the casts needed to keep C# overload resolution and
operator binding aligned with Go.

A Go type over a numeric base — `type Celsius float64`, `type Duration int64` — becomes a `[GoType("num:…")]`
partial struct whose body (operators, comparisons, conversions to/from the underlying) the `TypeGenerator`
fills in. It's a distinct C# type that still behaves like its base, so method bodies read almost
line-for-line the same:

```go
type Duration int64                       // time/time.go
func (d Duration) Seconds() float64 {
    sec := d / Second
    nsec := d % Second
    return float64(sec) + float64(nsec)/1e9
}
```
```csharp
[GoType("num:int64")] partial struct Duration;      // time/time.cs
public static float64 Seconds(this Duration d) {
    var sec = d / ΔSecond;
    var nsec = d % ΔSecond;
    return (float64)(int64)sec + (float64)(int64)nsec / 1e9D;
}
```

The wrapper carries the full operator surface (integer underlyings also get `~`, shifts, and bitwise ops),
so `Word >> s` stays a `Word`. Converting *between* a named type and a non-underlying basic routes through
the underlying (`traceArg(procs)` → `(traceArg)(uint64)procs`), mirroring Go's numeric-conversion rules.
Unsigned unary minus lowers to `(T)0 - x` (C# forbids unary negation, i.e., `-` prefix, on unsigned).

**Full detail:** [Reference → Named Numeric Types and Constant Contexts](ConversionStrategies-Reference.md#named-numeric-types-and-constant-contexts) —
this is one of the deepest topics: `++/--` operators, to/from conversions, cross-assembly conversion
operators, named slice/array/map wrappers, `append` element casting, shift-width and bit-mask casts, and
the `&^=` bit-clear lowering.

---

## Nil and Zero Values

`nil` maps to the golib `NilType` value `nil` (from `go.builtin`), which defines comparison operators so
`x == nil` / `x != nil` work across slices, maps, channels, pointers, and interfaces — each defining what
"nil" means for it (a nil `map<K,V>` reads zero values, has `len` 0, ranges empty, panics on write). In
*value* position (a `return`, an assignment), `nil` is written **`default!`**:

```go
func Unwrap(err error) error {      // errors/wrap.go
    u, ok := err.(interface{ Unwrap() error })
    if !ok {
        return nil
    }
    return u.Unwrap()
}
```
```csharp
public static error Unwrap(error err) {     // errors/wrap.cs
    var (u, ok) = err._<Unwrap_type>(ᐧ);
    if (!ok) {
        return default!;
    }
    return u.Unwrap();
}
```

Zero-value reference-backed values are null-safe: a `default!` `@string` reads as `""` rather than
throwing.

Nilness is **representation** identity, not emptiness: `s == nil` on a slice is true exactly for the
nil slice (null backing array), so a non-nil empty (`[]byte{}`, `make([]T, 0)`, `s[len(s):]`) stays
observably non-nil, and nil survives reslicing (`nil[0:0]`) and no-op appends — the distinctions
Go programs (and the stdlib's own tests) rely on.

**Full detail:** [Reference → Nil and Zero Values](ConversionStrategies-Reference.md#nil-and-zero-values) —
null-safe zero values and pointer-to-interface assignment through selector fields; and
[Reference → Nil-vs-empty slice identity](ConversionStrategies-Reference.md#nil-vs-empty-slice-identity-s--nil-is-representation-nilness-not-emptiness)
— the full construction-identity enumeration.

---

## Empty Interface (`any`)

Every Go type satisfies `interface{}` (spelled `any`), which behaves like .NET's `object`, so the empty
interface maps directly to **`any`** (a global alias for `object`). `func(i interface{})` → `void f(any i)`;
`map[any]string` → `map<any, @string>`.

One wrinkle worth knowing: a Go string literal normally emits as a `"…"u8` `ReadOnlySpan<byte>`, which has
no conversion to `object`. So a string literal returned/assigned/sent *as `any`* is boxed through `@string`
(preserving Go string identity for a later `x.(string)`): `return (@string)"test2json";`.

The numeric twin: a bare C# integer literal is `System.Int32`, but Go boxes an untyped `int` constant as
its default type `int` (go2cs `nint`). So an untyped `int` constant boxed *as `any`* is cast through `nint`
(`Ꮡv.Store((nint)(42))`), else a later `x.(int)` — `x._<nint>()` — finds an `Int32` and panics. Variadic
`...any` arguments (`fmt.Println(42)`) are left uncast — a boxed `Int32` formats identically and its `%T`
already resolves as `int` — and `any` map keys are left uncast to keep `map[any]int` lookups consistent.

A related identity wrinkle: a deref-aliased pointer (a `*T` parameter or a pointer receiver) passed *as
`any`* renders the box `Ꮡp`, not the deref'd value alias `p` — Go boxes the *pointer*, and dropping the box
would store the pointed-to value, so a later `x.(*T)` would find a bare `T` and panic. This surfaced as
fmt's `sync.Pool` round-trip (`ppFree.Put(p)` then `Get().(*pp)`), which crashed every multi-call fmt
program before the fix.

**Full detail:** [Reference → Empty Interface (`any`)](ConversionStrategies-Reference.md#empty-interface-any) — the
`@string` and `nint` boxing across argument, return, assignment, composite-literal, and channel-send
positions, and the box for a pointer value passed to an `any` argument.

---

## Multi-Assignment and Evaluation Order

Go evaluates every right-hand operand before assigning, which C# expresses with tuple deconstruction:

```go
x, y = y, x+y
```
```csharp
(x, y) = (y, x + y);
```

Go's **partial redeclaration** (`a, b := f()` where `a` already exists) reuses `a` and declares only the
new names, so the converter emits `var` per newly-declared element: `(frac, var e) = normalize(frac);`. A
blank element is a discard with no `var` (`_ = fi;`).

**Full detail:** [Reference → Multi-Assignment and Evaluation Order](ConversionStrategies-Reference.md#multi-assignment-and-evaluation-order) —
per-element `var` mechanics, escaping/heap-boxed tuple elements, interface-converting deconstruction, and
address-taken value locals (the `Ꮡ(value)` copy-vs-box distinction).

---

## Short Variable Redeclaration (Shadowing)

C# forbids a local from shadowing an enclosing local (CS0136). Where Go's `:=` legally shadows, the
converter **renames** the inner variable with a `Δ` suffix and rewrites its references, leaving the outer
one untouched (so its value is naturally preserved):

```go
func sumWithLenLocal(buf []int) int {
    total := 0
    len := len(buf)          // a local shadowing the builtin
    for i := 0; i < len; i++ { total += i }
    return total + len
}
```
```csharp
internal static nint sumWithLenLocal(slice<nint> buf) {
    nint total = 0;
    nint lenΔ1 = len(buf);           // renamed; the builtin call stays len(...)
    for (nint i = 0; i < lenΔ1; i++) { total += i; }
    return total + lenΔ1;
}
```

The mirror case — a local shadowing a package **global** — qualifies the *global* instead
(`runtime_package.Δtrace`), which a local can never shadow. Related renames cover type-vs-method name
collisions (`Δfoo` type vs `foo` method), closure parameters, and consts.

**Full detail:** [Reference → Short Variable Redeclaration](ConversionStrategies-Reference.md#short-variable-redeclaration-shadowing) —
a large family: forward-collision detection at every block level, package-function shadowing, builtin-method
shadowing, box-name rules for renamed receivers/pointers, and nested-closure capture state.

---

## Multi-Result Values and Comma-Ok Forms

Go functions returning `(value, ok)` / `(value, error)` become ordinary C# value tuples, destructured at
the call site. The runtime's own comma-ok forms (map read, type assertion) use a discard **sentinel** to
select a second overload — `ꟷ` for indexers, `ᐧ` for assertions:

```go
func Atoi(s string) (int, error) {        // strconv/atoi.go
    i64, err := ParseInt(s, 10, 0)
    if nerr, ok := err.(*NumError); ok {
        nerr.Func = fnAtoi
    }
    return int(i64), err
}
```
```csharp
public static (nint, error) Atoi(@string s) {     // strconv/atoi.cs
    var (i64, err) = ParseInt(s, 10, 0);
    {
        var (nerr, ok) = err._<ж<NumError>>(ᐧ); if (ok) {
            nerr.Value.Func = fnAtoi;
        }
    }
    return ((nint)i64, err);
}
```

The single-value assertion `i.(T)` → `i._<T>()` panics on failure; the comma-ok `i._<T>(ᐧ)` returns safely.
An assertion to a *pointer* type renders the box type: `i.(*box)` → `i._<ж<box>>()`.

**Full detail:** [Reference → Multi-Result Values and Comma-Ok Forms](ConversionStrategies-Reference.md#multi-result-values-and-comma-ok-forms) —
package-level `var a, b = f()` component reads, variadic pointer-arg boxing, named-func-result signatures,
and variadic-closure `params` rebinding.

---

## Slices and Arrays

Go slices and arrays convert to golib `slice<T>` and `array<T>`. A composite literal builds a C# array and
projects it with `.slice()` / `.array()`; `make` uses a constructor:

```go
primes := [6]int{2, 3, 5, 7, 11, 13}   // array literal
nums := []int{10, 20, 30}              // slice literal
buf := make([]byte, 4)                 // make
```
```csharp
var primes = new nint[]{2, 3, 5, 7, 11, 13}.array();
var nums = new nint[]{10, 20, 30}.slice();
var buf = new slice<byte>(4);
```

`append`, `len`, `make`, and sub-slicing map to golib builtins/methods, and a variadic `...T` parameter
arrives as `params ꓸꓸꓸT` rebound to a slice at the top of the body. From the real stdlib:

```go
func Join(errs ...error) error {          // errors/join.go
    // ...
    e := &joinError{errs: make([]error, 0, n)}
    for _, err := range errs {
        if err != nil { e.errs = append(e.errs, err) }
    }
    return e
}
```
```csharp
public static error Join(params ꓸꓸꓸerror errsʗp) {     // errors/join.cs
    var errs = errsʗp.slice();
    // ...
    var e = Ꮡ(new joinError(errs: new slice<error>(0, n)));
    foreach (var (_, err) in errs) {
        if (err != default!) { e.Value.errs = append((~e).errs, err); }
    }
    return new joinErrorжerror(e);
}
```

Arrays are Go **values**: every transfer copies the whole array. `array<T>` is a struct over a
shared `T[]`, so the converter appends a strongly-typed `.Clone()` wherever an array value is read
out of existing storage — assignment, range elements, composite-literal elements and struct fields,
returns, channel sends, `append` elements, and function parameters (cloned in the callee preamble).
Named array types clone the same way through their generated wrapper's own `Clone()`, and the copy
is **deep** for nested arrays (`[2][3]int` copies its inner arrays too, matching Go):

```go
data := ints                          // an independent copy — writes to data never reach ints
for _, row := range m { row[0] = 9 }  // row is a per-iteration copy — m is never written
```
```csharp
var data = ints.Clone();
foreach (var (_, vᴛ1) in m) { var row = vᴛ1.Clone(); row[0] = 9; }
```

**Full detail:** [Reference → Slices and Arrays](ConversionStrategies-Reference.md#slices-and-arrays) —
named slice/array wrappers, pointer-to-array slicing, named-slice pointer reinterpretation, structural
composite rendering, array value-copy cloning (deep for nested arrays), and slice-aliasing/write-through
semantics.

---

## Strings (`@string` and `sstring`)

Go's `string` is represented by golib [`@string`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/string.cs):
an immutable byte string whose `len`, indexing, ranging, concatenation, and comparisons are byte-oriented
like Go's, not UTF-16-oriented like `System.String`. Plain string literals usually render as
`"..."u8` `ReadOnlySpan<byte>` values, then target-type into `@string` only when a heap string is actually
needed. That keeps common literal-to-slice and literal-comparison forms allocation-free:

```go
var s string = "ready"
b := []byte("hi")
```
```csharp
@string s = "ready"u8;
var b = slice<byte>("hi"u8);
```

A string↔bytes conversion is a cast over the golib types: `string(b.buf[b.off:])` →
`(@string)(b.buf[(int)(b.off)..])`, and `[]byte(s)` → `slice<byte>(s)`. `[]rune(s)` decodes through the
Go string model rather than the CLR string model. Literals with raw byte escapes that cannot be expressed
faithfully as UTF-8 source (for example high `\xHH` bytes or greedy hex escapes) emit as byte-array-backed
`@string`, preserving Go's exact bytes.

Named string types are real wrapper structs (`type relationship string`), so the generated type keeps the
string surface: indexing, sub-slicing, `len`, comparisons, constants, and method calls stay on the named
type instead of collapsing back to plain `@string`.

```go
type Token string
func (t Token) First() byte { return t[0] }
const done Token = "done"
```
```csharp
[GoType("@string")] partial struct Token;
internal static readonly Token done = "done"u8;
public static byte First(this Token t) => t[0];
```

Most `string([]byte)` conversions must copy into `@string` — the price of Go's immutable-string guarantee.
Go's own compiler *elides* that copy when the resulting string does not escape and its source is not
modified while it is alive, letting the string alias the bytes in place. The converter recovers this common
fast path with a second string type,
[`sstring`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/sstring.cs): a
stack-only `readonly ref struct` that *views* a `ReadOnlySpan<byte>` with **no allocation**.

A provably-safe `string([]byte)` conversion emits `sstring`; anything that escapes stays `@string` (the implicit
`sstring`→`@string` conversion copies to the heap at that boundary, so correctness never depends on getting
the analysis right — only performance does).

Safety is enforced two ways. Because `sstring` is a `ref struct`, the .NET compiler forbids every way a
string could escape — a field, array, map, interface box, channel, closure, or a return past its data's
lifetime — so an over-reach is a **compile error, not a silent bug**. The one hazard the compiler cannot
see — the source slice mutated while the view is alive — the converter's escape analysis rules out. So
`sstring` appears only for a non-escaping conversion used in **read-only** positions: a comparison, a
`switch` tag, `len`/index, or a concatenation operand.

```go
if string(hdr[:4]) == wantMagic { … }        // compare a slice against a []byte-derived string
switch string(cmd) { case "get": …; case "put": … }
```
```csharp
if (((sstring)(hdr[..4])) == wantMagic) { … }   // mixed sstring/@string compare — no heap copy
var exprᴛ1 = ((sstring)cmd);                     // a string switch lowers to == comparisons
if (exprᴛ1 == "get"u8) { … } if (exprᴛ1 == "put"u8) { … }
```

Comparing an `sstring` against a `"…"u8` literal, an `@string`, or another view runs zero-allocation
directly over the backing spans — which is where the win shows: the eligible comparison idiom measures
~11–12× faster than the `@string` copy-and-compare. A repeated conversion in a loop is hoisted to a single
reused view; everything that escapes simply stays `@string`.

**Full detail:** [Reference → Strings (`@string` and `sstring`)](ConversionStrategies-Reference.md#strings-string-and-sstring) —
literal rendering, string↔`[]byte`/`[]rune` conversions, named-string wrapper behavior, high-`\x`-escape
byte arrays, the exact `sstring` eligibility predicate, comparison / `switch` / concatenation forms,
loop-invariant hoisting, and the `SStringElision` guard test.

---

## Maps and Channels

Go maps and channels convert to golib `map<K,V>` and `channel<T>`; `make` becomes a constructor, and
send/receive/`select` use runtime operators. Map reads honor Go's nil-map and comma-ok semantics:

```go
m := make(map[string]int)
c := make(chan int, 3)
unit, ok := unitMap[u]          // comma-ok read (time/format.go)
```
```csharp
var m = new map<@string, nint>();
var c = new channel<nint>(3);
var (unit, ok) = unitMap[u, ꟷ];      // two-value indexer via the ꟷ sentinel
```

A goroutine over a `select` — the concurrency core — lowers to `goǃ(...)` and a `switch` over `select(...)`,
with `ᐸꟷ` marking a receive-case and `ꟷᐳ` performing the receive:

```go
go func() {                     // context/context.go
    select {
    case <-parent.Done():
        child.cancel(false, parent.Err(), Cause(parent))
    case <-child.Done():
    }
}()
```
```csharp
goǃ(() => {                     // context/context.cs
    switch (select(ᐸꟷ(parent.Done(), ꓸꓸꓸ), ᐸꟷ(child.Done(), ꓸꓸꓸ))) {
    case 0 when parent.Done().ꟷᐳ(out _): {
        child.cancel(false, parent.Err(), Cause(parent));
        break;
    }
    case 1 when child.Done().ꟷᐳ(out _): { break; }}
});
```

**Full detail:** [Reference → Maps and Channels](ConversionStrategies-Reference.md#maps-and-channels) —
named map/channel types, constrained map access through type parameters, and full `select` lowering
(terminating/empty clauses, escaping comm-clause bindings).

---

## Generic Constraints

A Go generic constraint becomes a C# `where` clause. Type-set constraints lift to the matching golib/.NET
interface (`[]T`→`ISlice<T>`, `[N]E`→`IArray<E>`, `map[K]V`→`IMap<K,V>`, `chan T`→`IChannel<T>`), and an
operator-bearing type set additionally lifts the `System.Numerics` operator interfaces so the body's
`+`/`<`/`==` compile. `comparable` maps to golib's `comparable<T>`.

```go
type Ordered interface {                  // cmp/cmp.go
    ~int | ~int8 | /* … */ | ~float64 | ~string
}
func Less[T Ordered](x, y T) bool {
    return (isNaN(x) && !isNaN(y)) || x < y
}
```
```csharp
[GoType("operators = Sum, Comparable, Ordered")]      // cmp/cmp.cs
partial interface Ordered<ΔT> { /* type set + derived operators, as comments */ }

public static bool Less<T>(T x, T y)
    where T : /* Ordered */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>,
              IComparisonOperators<T, T, bool>, new()
{
    return (isNaN(x) && !isNaN(y)) || x < y;
}
```

**Full detail:** [Reference → Generic Constraints](ConversionStrategies-Reference.md#generic-constraints) —
array-core `~[N]E` lifting, single-term pointer constraints (`[P *T]` → `ж<T>`), method-set interface
constraints and self-referential proxies, `comparable`, unions (`string | []byte`), and explicit
type-argument handling.

---

## Type Aliasing

Go has two forms. A **type definition** (`type Celsius float64`) is a distinct type sharing an underlying;
because converted types are structs (no inheritance), the source generators emit the bridging (implicit
conversions to the underlying, interface implementations, receiver-method proxies). A **type alias
declaration** (`type P = *bool`) is true aliasing, emitted as a C# **global using**:

```go
type P = *bool
type table = map[string]int
```
```csharp
global using P = go.ж<bool>;
global using table = go.map<@string, nint>;
```

**Full detail:** [Reference → Type Aliasing](ConversionStrategies-Reference.md#type-aliasing) — self-boxing
pointer conversions, keyword-safe `global using` RHS rendering, `types.Unalias` at type-switched decision
points, and same-package alias-target namespace qualification.

---

## Delegates to Value Receiver Instances

In Go a function is a value, and a **value-receiver method value** captures a *copy* of the receiver at the
moment it's taken — a subtlety that surprises non-Go programmers:

```go
d := data{name: "James"}
f1 := d.printName
f1()                 // "Name = James"
d.name = "Gretchen"
f1()                 // "Name = James" again — f1 bound a copy of d
```

To preserve this, the converter copies the receiver value into the delegate's capture (a snapshot taken at
assignment time), rather than capturing by reference. Method *expressions* (`(*T).M`), bound method values,
pointer-receiver method values, and conversions to named func types each have a tailored emission (a cast to
the concrete delegate, a box-bound method group, or `new NamedDelegate(...)`).

**Full detail:** [Reference → Delegates to Value Receiver Instances](ConversionStrategies-Reference.md#delegates-to-value-receiver-instances) —
method expressions (local & foreign), bound/interface/pointer/value-receiver method values, the go-statement
sibling, named and generic func-type conversions.

---

## Defer / Panic / Recover

`defer`, `panic`, and `recover` are supplied by wrapping the function body in
`func((defer, recover) => { … })`, which brings `defer` and `recover` into scope. A bare deferred call is a
method group `defer(fn)`; one that must capture arguments at defer-time uses `deferǃ(fn, args, defer)`.
`panic(x)` lowers to `throw panic(x)`:

```go
func withLock(lk sync.Locker, fn func()) {   // database/sql/sql.go
    lk.Lock()
    defer lk.Unlock() // in case fn panics
    fn()
}
```
```csharp
internal static void withLock(sync.Locker lk, Action fn) => func((defer, recover) => {  // database/sql/sql.cs
    lk.Lock();
    defer(lk.Unlock);
    // in case fn panics
    fn();
});
```

An unrecovered panic — even in a goroutine — crashes the process exactly as in Go: golib's
`AppDomain.UnhandledException` backstop writes the `panic: …` report to stderr and exits with code 2.

`recover()` is the same wrapper's parameter; a re-`panic` is `throw panic(err)`:

```go
if err := recover(); err != nil {   // fmt/print.go
    // ...
    if p.panicking { panic(err) }
```
```csharp
var err = recover(); if (err != default!) {   // fmt/print.cs
    // ...
    if (p.panicking) { throw panic(err); }
```

**Full detail:** [Reference → Defer / Panic / Recover](ConversionStrategies-Reference.md#defer--panic--recover) —
unrecovered-panic process exit (stderr + code 2), named-delegate/builtin callees, value-returning goroutine
wrapping, func-literal argument capture hoisting, and box-bound deferred pointer-receiver methods.

---

## Expression Switch Statements

Go's expression `switch` (no automatic fall-through) usually lowers to `if / else if`, which handles cases
whose labels aren't C# compile-time constants (variables, `static readonly` consts, addresses). When every
label *is* a constant and there's no `fallthrough`, a real C# `switch` is used. A tag-less
`switch { case cond: }` lowers to a `switch` over the sentinel `ᐧ` with each arm a `when` guard:

```go
switch {                              // path/path.go
case path[r] == '/':
    r++
case path[r] == '.' && (r+1 == n || path[r+1] == '/'):
    r++
}
```
```csharp
switch (ᐧ) {                          // path/path.cs
case {} when path[r] is (rune)'/': {
    r++;
    break;
}
case {} when path[r] == (rune)'.' && (r + 1 == n || path[r + 1] == (rune)'/'): {
    r++;
    break;
}}
```

`fallthrough` expands to an if-chain with a fall flag and `goto`; a switch-targeting `break` inside an
if-else arm is wrapped in a one-shot `do { … } while (false)`.

**Full detail:** [Reference → Expression Switch Statements](ConversionStrategies-Reference.md#expression-switch-statements) —
constant-vs-runtime label detection, `static readonly` tags, `fallthrough` + guarded-default returns, and
index/named-type case labels.

---

## Type Switch Statements

A Go type switch maps cleanly to C#'s type-pattern `switch`. The dynamic type comes from `.type()`, and
each `case T:` binds the value with a type pattern:

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
        // ...
    }}
}
```

Cases that match an *anonymous interface* (`case interface{ Unwrap() error }:`) synthesize a named
`[GoType("dyn")]` interface and test it with `case {} Δx when Δx._<is_typeᴛ1>(out var x):`.

**Full detail:** [Reference → Type Switch Statements](ConversionStrategies-Reference.md#type-switch-statements) —
the tag-evaluates-once guarantee, default-arm binding, astral rune literals, and generic/embedded arms.

---

## Labeled Control Flow and Loop Variables

Go labels sit immediately before their statement; C# reproduces the behavior with a placed label and a
`goto`:

```go
Outer:
    for i := 0; i < n; i++ {
        for j := 0; j < m; j++ {
            if done { break Outer }
        }
    }
```
```csharp
    for (nint i = 0; i < n; i++) {
        for (nint j = 0; j < m; j++) {
            if (done) { goto break_Outer; }
        }
    }
    break_Outer:;
```

**Full detail:** [Reference → Labeled Control Flow and Loop Variables](ConversionStrategies-Reference.md#labeled-control-flow-and-loop-variables) —
break vs continue label placement, labels on empty statements, and per-iteration loop-variable semantics
(Go 1.22).

---

## Struct Types

Go structs become C# `struct`s (stack-friendly; heap-boxed as `ж<T>` when they escape). The converter emits
a `[GoType]` partial struct with just the fields; the `TypeGenerator` synthesizes equality, `ISupportMake`,
and embedding promotion. Access modifiers follow Go's exported/unexported naming:

```go
type List struct {                    // container/list/list.go
    root Element
    len  int
}
```
```csharp
[GoType] partial struct List {        // container/list/list.cs
    internal Element root;
    internal nint len;
}
```

Inline/anonymous types are "lifted" out (a local `type x struct{…}` in `main` → `main_x`; an anonymous
struct → `settingsᴛ1`). The empty `struct{}` maps to the shared golib `EmptyStruct`, and an empty
`interface{}` field to `any` — neither is lifted.

**Full detail:** [Reference → Struct Types](ConversionStrategies-Reference.md#struct-types) — field-name
collisions in generated equality, combined field lines, local/anonymous-type lifting (including map-value
and slice-element structs), and recorded implicit conversions between structurally-identical anon structs.

---

## Struct Type Embedding

Go uses embedding instead of inheritance. Since C# structs can't inherit, the `TypeGenerator` adds a field
for the embedded type and **promotes** its fields and methods — transitively through every level, and for
both value and pointer (`*T`) embeds:

```go
type reverse struct {                 // sort/sort.go
    Interface                         // embedded — Len/Less/Swap promoted
}
func (r reverse) Less(i, j int) bool {
    return r.Interface.Less(j, i)     // this method overrides the promoted Less
}
```
```csharp
[GoType] partial struct reverse {     // sort/sort.cs
    public Interface Interface;        // the embed becomes an explicitly-named field
}
internal static bool Less(this reverse r, nint i, nint j) {
    return r.Interface.Less(j, i);
}
```

Promoted-embed structs construct through a generated constructor (never `default`, which would leave null
boxes). Cross-package embeds resolve through the compiled type's metadata, and pointer-receiver methods
promoted through a value embed are routed at the call site (`t.of(timeTimer.Ꮡtimer).modify(…)`) so writes
land on the real storage.

**Full detail:** [Reference → Struct Type Embedding](ConversionStrategies-Reference.md#struct-type-embedding) —
transitive/pointer promotion, zero-value construction, cross-package (metadata) embeds, interface-adapter
projection through embeds, and box-receiver primaries.

---

## Interfaces

Go interfaces are duck-typed. The converter emits each user interface as a `[GoType] partial interface`, and
the **`ImplementGenerator`** discovers which concrete types satisfy it and emits the implementing glue plus
implicit conversions — so assigning a concrete value to an interface variable is direct, no reflection:

```go
type Color interface {                // image/color/color.go
    RGBA() (r, g, b, a uint32)
}
type RGBA struct { R, G, B, A uint8 }
func (c RGBA) RGBA() (r, g, b, a uint32) { /* … */ return }
```
```csharp
[GoType] partial interface Color {    // image/color/color.cs
    (uint32 r, uint32 g, uint32 b, uint32 a) RGBA();
}
[GoType] partial struct ΔRGBA {       // Δ-renamed: the struct name collides with its RGBA() method
    public uint8 R, G, B, A;
}
public static (uint32 r, uint32 g, uint32 b, uint32 a) RGBA(this ΔRGBA c) { /* … */ return (r, g, b, a); }
```

Each "concrete implements interface" pairing is recorded as `[assembly: GoImplement<ΔRGBA, Color>]` for the
generator to consume. The well-known built-ins (`error`, `fmt.Stringer`, …) are hand-written in golib but
implemented the same duck-typed way. A cross-package satisfaction is witnessed by the idiomatic
`var _ I = T{}` assertion in the type's own package.

**Full detail:** [Reference → Interfaces](ConversionStrategies-Reference.md#interfaces) — a large topic:
cross-package pointer/value adapters, unexported-sealing markers, keyword-named method escaping, publicized
unexported types, structural (C# inheritance) satisfaction, and adapter accessibility.

---

## Pointers

Pointer conversions use the golib heap box **`ж<T>`** (read "zhe"). Taking an address uses **`Ꮡ`**; an
escaping local is allocated with `heap(...)`; a field/element address goes through `.of(Type.ᏑField)` /
`.at<T>(i)` / `Ꮡ(slice, i)`. A pointer parameter is deref-aliased with `ref var x = ref Ꮡx.Value`, and
writes through a pointer field use `.Value`:

```go
func (l *List) insert(e, at *Element) *Element {   // container/list/list.go
    e.prev = at
    e.next = at.next
    e.prev.next = e
    e.next.prev = e
    e.list = l
    l.len++
    return e
}
```
```csharp
internal static ж<Element> insert(this ж<List> Ꮡl, ж<Element> Ꮡe, ж<Element> Ꮡat) {  // list.cs
    ref var l = ref Ꮡl.Value;
    ref var e = ref Ꮡe.Value;
    ref var at = ref Ꮡat.Value;
    e.prev = Ꮡat;
    e.next = at.next;
    e.prev.Value.next = Ꮡe;      // write through the pointer field
    e.next.Value.prev = Ꮡe;
    e.list = Ꮡl;
    l.len++;
    return Ꮡe;
}
```

The box's `Value` is the strict (nil-panicking) dereference; `ValueSlot` is its no-check twin; a
package-level global whose address is taken is backed by a real box so `&global` writes are observed. Using
`ж<T>` rather than C# `ref` sidesteps escape-analysis complications, at the cost of an occasional heap
allocation.

**Full detail:** [Reference → Pointers](ConversionStrategies-Reference.md#pointers) — per-iteration
range-variable boxes, wide-index narrowing on element addresses, pointer-typed globals & double-pointer
walks, closure capture of boxed locals, `unsafe.Pointer` conversions, and reinterpret casts.

---

## Implicit Pointer Dereferencing

Go auto-dereferences pointers on field access and method calls. The converter binds a `ref` local to the
box's value for a pointer parameter, so the body reads like Go:

```go
func PrintValPtr(ptr *int) {
    fmt.Printf("Value available at *ptr = %d\n", *ptr)
    *ptr++
}
```
```csharp
public static void PrintValPtr(ж<nint> Ꮡptr) {
    ref var ptr = ref Ꮡptr.Value;
    fmt.Printf("Value available at *ptr = %d\n"u8, ptr);
    ptr++;
}
```

A pointer *local* dereferences through its box on access — a read as `(~x).field`, a write as
`x.Value.field = …` (the assignable form). Promoted fields, nested LHS chains, `++`/`--`, and indexed
targets all thread the same assignment context so the write path stays assignable.

**Full detail:** [Reference → Implicit Pointer Dereferencing](ConversionStrategies-Reference.md#implicit-pointer-dereferencing) —
selector-base deref detection, nested LHS `.Value` chains, index-expression assignment targets, and
`*p.field` field-deref through parameters/receivers.

---

## The `go.golib` support namespace

golib's hand-written support types (`SparseArray<T>`, `PinnedBuffer`, `HashCode`, …) live in the
**`go.golib`** child namespace — deliberately *not* `go.<any Go package name>`, because a child namespace
visible from every referenced assembly would win simple-name lookup over an import alias (`go.runtime` would
shadow `using runtime = runtime_package;`, CS0576). The general form of that collision — a real
parent/child package pair — is handled by **Δ-renaming the import alias** (`using Δruntime = …`).

**Full detail:** [Reference → The go.golib support namespace](ConversionStrategies-Reference.md#the-gogolib-support-namespace) —
the collision reasoning and the transitive-closure alias-rename pre-pass (incl. foreign renamed-type alias
resolution).

---

## Source Generators

Several Go semantics can't be written directly in C#, so the converter emits compact attributed partial
declarations and lets Roslyn source generators (`src/gen/go2cs-gen/`, referenced as an analyzer by every
converted project) synthesize the rest at compile time — keeping the visible code close to Go. The
principal generators:

- **`TypeGenerator`** (`[GoType]`) — struct members & equality; named numeric/slice/array/map/channel
  wrappers & operators; struct-embedding promotion.
- **`ImplementGenerator`** — finds concrete types satisfying each `[GoType] partial interface` and emits the
  implementation glue + implicit conversions.
- **`RecvGenerator`** (`[GoRecv]`) — emits the pointer/box (`ж<T>`) overload of each value-receiver method.
- **`ImplicitConvGenerator`** — the implicit operators letting a named type and its underlying interconvert.
- **`PartialStubGenerator`** — a throwing stub for any bodyless partial (asm/cgo) with no real
  implementation.

Common attributes: `[GoType]`, `[GoRecv]`, `[GoTag]`, `[GoPackage]`, and the test-only
`[GoTestMatchingConsoleOutput]`.

**Full detail:** [Reference → Source Generators](ConversionStrategies-Reference.md#source-generators).

---

## Manually-Converted Declarations

A few Go declarations can't be faithfully auto-converted because their semantics depend on hiding a
*managed pointer inside an integer* — runtime's `guintptr`/`puintptr`/`muintptr` (a `uintptr` holding a
`*g` the Go GC must not see). The CLR has the opposite constraint (a reference stored as a number is
invisible to the .NET GC), so the managed conversion stores the `ж<T>` box **directly** and the numeric
form never exists (the model precedent is `core/sync/atomic`'s hand-rewritten `Pointer<T>`). Two mechanisms
deliver this: whole-file `[module: GoManualConversion]` (skipped by the converter, restored by [overlay](Glossary.md#overlay)), and
a type-level registry that skips listed types/methods and points at a hand-written `*_impl.cs`. The same
"managed reality beats raw reinterpretation" rule also hand-owns `sync/atomic.Value`, whose Go
implementation depends on the runtime's two-word interface layout; in C# it stores the boxed `any`
directly and uses `Volatile`/`Interlocked` for the atomic operations — and the Phase-4 **reflection
bridge**: `reflect`/`internal/reflectlite` read an interface's `{type,data}` words through
`unsafe.Pointer`, so the bridged entry points instead carry the boxed managed value and a synthetic
descriptor stamped with the real `System.Type` (`ValueOf`, the value readers, canonical `Type`
interning, `Swapper`, and `DeepEqual`'s recursion keyed on managed reference identity). A small
whitelist of `//go:linkname` pulls (currently the Windows DLL-loading helpers) emits forwarders to
real hand-written targets; non-whitelisted linkname pulls remain throwing stubs.

**Full detail:** [Reference → Manually-Converted Declarations](ConversionStrategies-Reference.md#manually-converted-declarations) —
the guintptr family surface, the `unsafe.Pointer`→manual-type ctor cooperation, the runtime lock/note
model, `sync/atomic.Value`, the reflection bridge (`abi.TypeOf`/`reflect` value+type side,
`reflectlite`, `DeepEqual`), and whitelisted `//go:linkname` forwarders.

---

## Deterministic Output

Converter output is **byte-reproducible**: the same Go source with the same converter build produces
byte-identical C# every run — a guarantee the [goldens](Glossary.md#golden), the full-conversion error measurements, and any
release tag all rest on. It's enforced by converting files sequentially in sorted-filename order, a
deterministic dependency-complete stdlib queue, and sorted emission of any set-backed output.

**Full detail:** [Reference → Deterministic Output](ConversionStrategies-Reference.md#deterministic-output).

---

*This summary tracks the [technical reference](ConversionStrategies-Reference.md) — when a conversion
decision changes the headline mapping of a construct, update the matching section here (with a real
example); record the full detail in the reference. See [`../CLAUDE.md`](../CLAUDE.md), "Record the
conversion decision."*
