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

Go projects that contain a `main` function are converted into a standard C# executable project, i.e., `<OutputType>Exe</OutputType>`. The conversion process can reference and convert needed external projects as library projects, i.e., `<OutputType>Library</OutputType>`, per any encountered `import` statements. In this manner an executable with packages compiled as project-referenced assemblies can be created. To create a single executable, like the original Go counterpart, a [self-contained executable](https://docs.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained) can be produced.

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

See [Untyped Constants and Named Numeric Types](#untyped-constants-and-named-numeric-types) for how these interact with native-int and named numeric types. See also [example](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/basics/numeric-constants).

## Handling "int" and "uint" Types

In Go the `int` and `uint` types are sized according to the platform build target, i.e., 32-bit or 64-bit. C#'s `int`/`uint` are always 32-bit and `long`/`ulong` are always 64-bit. As of C# 9.0, native-sized integer types exist that behave exactly like their Go counterparts: [`nint` and `nuint`](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9#performance-and-interop). The converter maps Go `int` → `nint` and Go `uint` → `nuint`; `uintptr` also maps to `nuint`. The fixed-width Go types (`int8/16/32/64`, `uint8/16/32/64`, `byte`, `rune`) are kept as readable C# aliases of the same name (e.g. `global using uint16 = System.UInt16;`).

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

## The "nil" Value
In Go, `nil` is the equivalent of C# `null`. Where possible, converted code uses the golib [`NilType`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/NilType.cs) with a default instance called `nil` (defined in [`go.builtin`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/builtin.cs)). `NilType` provides comparison operators so `x == nil` / `x != nil` work across the runtime types (slices, maps, channels, pointers, interfaces), each of which defines what "nil" means for it (e.g. a `map<K,V>` whose backing dictionary is null is the nil map: reads return the zero value, `len` is 0, ranging yields nothing, and a write panics — matching Go).

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

The same `Δ` mechanism handles a local shadowing a called built-in (as above), a nested-block variable shadowing a function-level one, and an IIFE/closure parameter colliding with an outer local.

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

## Type Aliasing
Go supports two kinds of [type aliasing](https://go101.org/article/type-system-overview.html#type-definition): a "type definition" and a "type alias declaration".

### Type Definitions
For a Go "type definition" the new type is a distinct type that shares an [underlying type](https://go101.org/article/type-system-overview.html#underlying-type) with its base. Because converted types are structs (no inheritance), the converter relies on the source generators (see [Source Generators](#source-generators)) to emit the bridging needed for these to be used interchangeably while remaining distinct: implicit conversion operators down to the underlying type (via `ImplicitConvGenerator` / `TypeGenerator`), and, where the base is a built-in like `slice`, the relevant interface (`ISlice<T>`, etc.) implementation. A named type also supports the extension methods (receiver functions) of its underlying types, which the generators surface as proxy/overload methods.

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

## Expression Switch Statements
Go expression-based `switch` statements are flexible: cases do not fall through automatically (no `break` needed), and the `fallthrough` keyword runs the next case body bypassing its expression. Based on the [Manual Tour of Go Conversions](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions), converting to `if / else if / else` is the best choice for most cases. When every case uses constants and there is no `fallthrough`, a traditional C# `switch` works. For cases that use `fallthrough`, the cases are expanded to standalone `if` statements with a local fall-through flag and `goto` to handle break-style exits — the most complex (and least pretty) scenario.

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

## Struct Types
Go structs are converted to C# `struct` types and used on the stack to optimize memory use and reduce GC pressure; when an instance must escape the stack it is wrapped in a heap box, [`ж<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/%D0%B6.cs) (see [Pointers](#pointers)). Rather than spell out the whole struct body, the converter emits a partial struct carrying a `[GoType]` attribute, and the `TypeGenerator` source generator synthesizes the members (equality, `ISupportMake`, embedding promotion, etc.):

```csharp
[GoType] partial struct Person {
    public @string Name;
    public nint Age;
}
```

The generator also chooses the access modifier from the Go name (exported → `public`), except where the converter emits an explicit modifier — for instance, an unexported type used as the type of an *exported* field is published as `public` to satisfy C# accessibility (the converter emits `public partial struct …` and the generator honors that explicit modifier).

C# does not allow inline or intra-function type definitions, so these are "lifted" out of the function. A **named** local type is lifted with its enclosing function's name as a prefix to avoid collisions — a `type x struct{…}` declared in `main` becomes `main_x`. An **anonymous** struct (or an anonymous struct used as a field/value) is lifted to a synthesized name with a `ᴛ`*N* suffix and marked dynamic, e.g. `[GoType("dyn")] partial struct settingsᴛ1`. Struct "definitions" that match structurally remain usable interchangeably (the generator and implicit conversions handle this).

## Struct Type Embedding
Go structs use "[type embedding](https://go101.org/article/type-embedding.html)" instead of inheritance. Since converted structs are C# `struct`s (no inheritance), the `TypeGenerator` manages the equivalent: it adds a field for the embedded type and promotes the embedded type's fields and methods (selection shorthand), including through nested embedding. Because the promotion is performed at conversion time by the generator, methods added later in hand-written C# are not automatically promoted; keeping the source in Go and re-converting (or using explicit interfaces) is the maintainable path.

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
    public nint x, y;
}

[GoRecv] internal static @string String(this ref point p) {
    return fmt.Sprintf("(%d, %d)"u8, p.x, p.y);
}

internal static Stringer describe() {
    return new point(1, 2);   // implicit conversion emitted by ImplementGenerator
}
```

The well-known built-in interfaces (`error`, `fmt.Stringer`, etc.) are hand-written in `golib`/the baseline rather than `[GoType]`-generated, but concrete types implement them the same duck-typed way. (Earlier strategies used a generic `As`/reflection mechanism; that has been superseded by the compile-time source generators.)

## Pointers
Pointer conversions use the golib heap box [`ж<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/%D0%B6.cs) (read "zhe"). Taking the address of a value uses the address-of operator `Ꮡ` (e.g. `Ꮡx`); an escaping local is allocated via `heap(...)`, and addresses of a struct field or array element are taken through `.of(Type.ᏑField)` / `.at<T>(index)`:

```csharp
ref var a = ref heap(new array<@string>(2), out var Ꮡa);  // escaping local
var p = Ꮡa.at<@string>(0);                                 // &a[0]
var pField = Ꮡsettings.of(settingsᴛ1.ᏑRetries);            // &settings.Retries
```

Using `ж<T>` rather than the C# `ref` keyword avoids the escape-analysis complications of passing a `ref` into code that expects a heap-allocated pointer. This is a simplification that can cost an unnecessary heap allocation when an address is taken; a future escape-analysis pass could keep such values on the stack when it is provably safe, similar to how [Go does this](https://golang.org/doc/faq#stack_or_heap) at compile time.

> Note: a package-level global whose address is taken is backed by a real heap box so that writes through `&global` (and `&global.field`) are observed, rather than mutating a copy.

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
becomes (`continue_RowLoop:` placed at the end of the loop body):
```csharp
    foreach (var (y, row) in rows) {
        foreach (var (x, data) in row) {
            if (data == endOfRow) {
                goto continue_RowLoop;
            }
            row[x] = data + bias(x, y);
continue_RowLoop:;
        }
    }
```

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
