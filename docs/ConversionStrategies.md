# Conversion Strategies

> Strategies updated on 7/23/2020 -- see [Manual Tour of Go Conversion Takeaways](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/Manual%20Tour%20of%20Go%20Conversion%20Takeaways.txt) for more background on current decisions. This is considered a living document, as more use cases and conversions are completed, these strategies will be updated as needed.

## Package Conversion
Each Go package is converted into static C# partial classes, e.g.: `public static partial class fmt_package`. Using a static partial class allows all functions within separate files to be available with a single import, e.g.: `using fmt = go.fmt_package;`.

So that Go packages are more readily usable in C# applications, all converted code is in a root `go` namespace. Package paths are also converted to namespaces, so a Go import like `import "unicode/utf8"` becomes a C# using like `using utf8 = go.unicode.utf8_package;`.

Go projects that contain a `main` function are converted into a standard C# executable project, i.e., `<OutputType>Exe</OutputType>`. The conversion process will automatically reference and convert needed external projects as library projects, i.e., `<OutputType>Library</OutputType>` per any defined encountered `import` statements, recursively. In this manner an executable with packages compiled as project referenced assemblies can be created. To create a single executable, like the original Go counterpart, a [self-contained executable](https://docs.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained) will be created.

Long term plan is to provide ability to use reference packages that have already been converted, e.g., from NuGet. See existing [`go2cs NuGet libraries`](https://www.nuget.org/packages?q=%22package+in+.NET+for+use+with+go2cs%22). Automating library conversion using packages will require an intermediate repository of original Go package name / location mapped to published NuGet package reference. A web site has been reserved for this purpose, i.e., http://nugetgo.net/ - for the moment, this just redirects to the go2cs GitHub site.

## Literal Values
Go constants hold arbitrary-precision literals with expression support. Applying value to variables in Go happens at compile time, so C# conversion will need to support this operation. Ideally every numeric constant that can hold the value without overflowing should to be defined, see [example](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/basics/numeric-constants). An additional run-time lazy initialized BigInteger can be provided for simpler library usage but use of constants should be encouraged for best performance.

## Handling "int" and "uint" Types

In Go the `int` and `uint` types are sized according the platform build target, i.e., 32-bit or 64-bit. In C# an `int` and `uint` is always 32-bits and a `long` and `ulong` is always 64-bits. Although an option exists to create custom `@int` and `@uint` structures that are compiled to 32-bit or 64-bit lengths based on a selected target build platform, e.g., using a `#ifdef TARGET32BIT` directive, and be implicitly castable to C# integer types, the current thinking is to only target 64-bit platforms for conversion. It seems that 64-bit deployments are likely the most common use case and this helps keeps things simple for now. This means any encountered Go `int` or `uint` will simply be converted to a C# `long` or `ulong`.

Later, an option could be added to the conversion tool that would allow for 32-bit build targets where any encountered Go `int` or `uint` would be converted to a C# `int` or `uint`. This option would mean pre-compiled library packages would need to include a 32-bit build as well, so there will be cascading consequences to consider.

Note that one sticking point with this strategy is that not all C# indexing constructs currently accept a `long`, so in some places a down-cast from `long` to `int` will be required. For example, although explicit indexers in C# support a `long`, [implicit index support](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges#implicit-index-support) currently only works with an `int`, this means range operation indices will need to be cast to `int`. FYI, [issue is being discussed](https://github.com/dotnet/runtime/issues/28070) for future C# runtime updates.

## Switch Statements
Go `switch` statements are very flexible, case statements do not automatically fall-through - so a `break` statement is not required. When the Go `fallthrough` keyword is used, the next case expression is evaluated. Based on work done with the [Manual Tour of Go Conversions](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions), converting to simple `if / else if` statements seems like the logical best choice.

If the original Go `case` statements all use constants and no `fallthough` keyword is being used, a traditional C# `switch` should work OK. Because this creates additional conversion complexity for detection of the proper use case, all for the sake of simply using a switch statement, this will likely be deferred to later. However, as one of the primary `go2cs` goals is to produce visually similar code where possible, the task should not be abandoned.

> FYI, toyed around with an experimental [`SwitchExpression<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/experimental/SwitchExpression.cs) class that used chained expressions and evaluated case statements as lambdas. This generally works like original Go `switch` even with `fallthrough` statements, but resultant code was much more noisy and certainly no cleaner than `if / else if` expansions. It also began to create burdening edge cases with lambda case expressions and captured variables.

## Type Switch Statements
In the case of Go type-based switch statements, the C# type-based pattern matching works well - even as a `switch` statement. For example, the following Go code using type-switch:

```Go
package main

import "fmt"

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

func main() {
    do(21)
    do("hello")
    do(true)
}
```

would be converted to C# as:

```CSharp
using go;
using static go.builtin;

static class main_package
{
    static void @do(object i) {
        switch (i.type()) {
            case int v:
                println($"Twice {v} is {v * 2}");
                break;
            case @string v:
                println($"\"{v}\" is {len(v)} bytes long");
                break;
            default: {
                var v = i.type();
                println($"I don't know about type {GetGoTypeName(v)}!");
                break;
            }
        }
    }

    static void Main() {
        @do(21);
        @do("hello");
        @do(true);
    }
}
```

## Struct Types
Go types are converted to C# `struct` types and used on the stack to optimize memory use and reduce the need for garbage collection. By using a `struct` instead of a `class`, converted C# code should better match original Go operation both from a memory and performance perspective. The `struct` types will be wrapped by a C# `class` that references the type value so that heap-allocated instances of the type can exist as needed, see [`ptr<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/ptr.cs), such as, when the type needs to leave the local stack.

In Go, a struct can be declared inline. C# does not support inline or intra-function `struct` definitions, so these dynamic structure elements will be named and declared external to the function. A per-parent static class local dictionary of defined types will need to be used during conversion to track this. Each inline C# per static class local structure will need to be named with an index for uniqueness. See [example](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/moretypes/slice-literals) with inline `struct`. Since these dynamic structures exist without a name in Go code, they should be defined in a secondary partial class that is defined outside the mainline code.

For named Go structure definitions within a function, since C# requires the structure to be defined "outside" the function, the intra-function structure name should include parent function name to prevent name collisions. For example, if a struct called `x` was declared within function `main`, the converted struct name would be `x__main`.

The conversion code also needs to understand the structure "definition" for casting purposes, that is, if structure definitions match, they can be used interchangeably. For example, the following Go code which declares an intra-function structure and returns it as a dynamically declared structure is valid. One option for conversion is just to use the same declared C# structure when type definitions match (where possible):

```Go
package main

import "fmt"

type P = *bool
type M = map[int]int

func test() struct { string; *int; P; M } {
    var x struct {
        string // a defined non-pointer type
        *int   // a non-defined pointer type
        P      // an alias of a non-defined pointer type
        M      // an alias of a non-defined type
    }
    x.string = "Go"
    x.int = new(int)
    x.P = new(bool)
    x.M = make(M)
    return x;
}

func main() {
    x := test()
    fmt.Println(x)
}
```

## Struct Type Embedding
Go structs use "[type embedding](https://go101.org/article/type-embedding.html)" for extending type functionality instead of inheritance. Since converted Go structs use C# `struct` types and structures in C# do not support inheritance, the code conversion process has to manage equivalent field type embedding. This process adds fields of embedded types with the same name as the type. In Go, embedded type fields are flattened into a single set (selection shorthand), including nested types - so this too has to be managed by the conversion tool.

Also, when a type is embedded in another type, the derived type supports the extension methods of the embedded types. This is more tricky in C# since interfaces are duck-implemented (see [interface strategies](#interfaces)) and the type implemented for the extension method will have no "direct" relation to the derived types save it simply exists as a field in another type. The Go compiler seems to create proxy extension functions for the derived type's embedded type methods, as such this is how the conversion tool will accommodate this functionality.

This basically means that the proxied extension functions created for derived embedded type values will only be for those found during the conversion process, i.e., from a maintainability perspective these features end at conversion. If a new extension function is added in the C# code, the derived class will not automatically see it. Users will need to do one of the following: (1) maintain code in Go and reconvert, (2) manually add missing proxy extensions, or (3) move on without this Go coding construct in converted C# code, e.g., switching to explicitly implemented interfaces. That said, if things start converting very well in the future, one option could always be to continue direct coding in Go and use a [transpilation](https://en.wikipedia.org/wiki/Source-to-source_compiler) step to automatically convert the code to C# as a pre-build step.

## Return Tuples
Many Go functions for built-in types return a tuple of "value and success" or just a "value" where only the declared return type determines which overload to use. To accommodate similar functionality in C#, an overload is defined that takes a bool that returns the tuple style return using a constant like `WithOK`, e.g.:

```CSharp
var v1 = m["Answer"]; // Does not fail if value doesn't exist, just returns default value
var (v2, ok) = m["Answer", WithOK];
```

Similarly functions returning an "value and error" tuple can operate in the same way:
```CSharp
var n1 = r.Read(b);
var (n2, err) = r.Read(b, WithErr);
```

See [success tuple return example](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/moretypes/mutating-maps/main_package.cs).

Currently these optional tuple returns are not allowed in user code, so normal tuple handling for converted user code should be fine.

## Interfaces
Go interfaces are not explicitly implemented. Instead, if extension-style functions exist that satisfy all defined interface methods, the class is said to implicitly implement the interface. To accommodate these duck-implemented interfaces, a generic class is created for each interface so that for a given type, interface extension methods can be looked up using reflection. To speed up this operation, as assemblies are loaded, extension methods are [cached in a dictionary](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/TypeExtensions.cs#L121) for quick lookup and any type-specific lookup operations are only done once statically during [type initialization](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/error.cs#L127). To make use of typed generic class any assignments to interface variables will be cast to generic type, for example, see equivalent C# code with [`As`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/error.cs#L50) function for handling Go duck-implemented interfaces:

```CSharp
Abser a;                          // Abser is an interface with methods
var f = (MyFloat)(-math.Sqrt(2)); // MyFloat is a custom type
a = Abser.As(f);                  // Succeeds only if MyFloat type implements Abser interface methods
```

See [interface example](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/methods/interfaces).

## Empty Interface
In Go all objects are said to implement an interface with no methods, this is called the `EmptyInterface`. This operates fundamentally like .NET's `System.Object` class, consequently any time the `EmptyInterface` is encountered during conversion, it is simply replaced with `object`. If there are type specific semantic use cases where this does not work, this strategy may need to be reevaluated.

## Type Aliasing
Go supports two kinds of [type aliasing](https://go101.org/article/type-system-overview.html#type-definition), these are a "type definition" and a "type alias declaration".

For Go "type alias declarations" generally <sup>[[1](#ref1)]</sup> matches aliasing in C# implemented with the `using` keyword, for example, the following Go and C# code are equivalent:

Go type alias declaration:
```Go
type table = map[string]int
```

Equivalent C# alias with `using`:
```CSharp
using table = go.map<@string, int>;
````

   > <small><a name="ref1"></a>[1] When using a type alias as an embedded type, Go is picky about structure matching. Weirdly, structures definitions are only considered a match when the embedded types both use the type alias, using the base type fails, see [example](https://play.golang.org/p/97lMNpTtPAy). However, this should not be a case the converter should have to consider because this is build error in Go.</small>

One difference for this type of aliasing is that in C# `using` aliases are always local to a file. In Go, type alias declarations can be exported. To accommodate this type of exportable aliasing, the conversion tool will need to add the exported using statements to all files needing the alias. It should be easy enough to simply ensure aliases are declared any time type is imported, however, this creates an interesting situation for imported packages. If conversion tool is setup to use a package, e.g., from NuGet, instead of converting local code, there will need to be an embedded resource dictionary in the package assembly that will report all exported aliases so the conversion tool can add these to code headers when package is encountered per Go `import`.

For Go "type definitions" the aliased type becomes a new, distinct type, however the new type and the base type are said to share the same [underlying type](https://go101.org/article/type-system-overview.html#underlying-type). Since converted code is using structs, which do not allow for inheritance, and aliased type and all base types are considered equivalent, the conversion tool will need to define implicit conversion operators for all these types. This means defining implicit conversion operators for the alias type for all underlying types down to built-in or unnamed (non-defined) type. The implicit operators will allow for interchangeably using aliased type and its base types while maintaining type distinction.

Note that aliased types operate similarly to type embedding when it comes to extension method function receivers, i.e., aliased type supports the extension methods of the base types. Like with embedded types this is more tricky in C# because of duck-implemented interfaces (see [interface strategies](#interfaces)), but the solution is the same, i.e., creating proxy extension functions for the aliased type for all underlying type extensions.

## The "nil" Value
In Go `nil` is the equivalent of C# `null`. Where possible converted code will use the [`NilType`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/NilType.cs#L33) with a default instance that equals `null` defined in [`go.builtin`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/builtin.cs#L75). Generally the `NilType` will work properly for compares, but cannot be used during assignments (no overloads or operators exist for this operation) - so assignments to heap based elements, like an interface, will convert to `null`.

## Inline Assignment Order of Operations
All right-hand operands in assignment expressions in Go are evaluated before assignment to left-hand operands. This is tricky, for example, consider the following Go code:

```Go
x, y = y, x+y
```
In C#, the following will _not_ produce the same results:
```CSharp
x = y;
y = x+y;
```
Instead,  equivalent code in C# is as follows:
```CSharp
var _y1 = x+y;
x = y;
y = _y1;
```

## Pointers
Conversion of pointer types will use the C# `ref` keyword where possible. When this strategy does not work, a heap allocated instance of the base type will be created (see [`ptr<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/ptr.cs)).

> C# unsafe pointers do not always work as a replacement for Go pointers since with C# (1) pointer types in structures cannot not refer to types that contain heap-allocated elements (e.g., arrays or slices that reference an array) as this would prevent pointer arithmetic for ambiguously sized elements, and (2) returning standard pointers to stack-allocated structures from a function is not allowed, instead you need to allocate the structure on the heap by creating a reference-type wrapper and then safely return a pointer to the reference.

## Implicit Pointer Dereferencing
In Go, all pointer types are setup to automatically dereference. For example, the following `age` property assignments are equivalent in Go:
```Go
var s struct {age int}
var ps = &s
(*ps).age = 20
ps.age = 20
```
This automatic dereferencing also applies to extension methods, in other words, an extension-style method for a non-pointer type will work for the type as well as a pointer to the type.

## Slices
Conversion of Go slices is based on the [`slice<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/slice.cs) structure. For example, the following Go code using slice operations:

```Go
package main

import (
    "fmt"
    "strings"
)

func main() {
    // Create a tic-tac-toe board.
    board := [][]string{
            []string{"_", "_", "_"},
            []string{"_", "_", "_"},
            []string{"_", "_", "_"},
    }

    // The players take turns.
    board[0][0] = "X"
    board[2][2] = "O"
    board[1][2] = "X"
    board[1][0] = "O"
    board[0][2] = "X"

    for i := 0; i < len(board); i++ {
        fmt.Printf("%s\n", strings.Join(board[i], " "))
    }
}
```

would be converted to C# as:

```CSharp
using go;
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;

static class main_package
{
    static void Main() {
        // Create a tic-tac-toe board.
        var board = slice(new[]{
                slice(new @string[]{"_", "_", "_"}),
                slice(new @string[]{"_", "_", "_"}),
                slice(new @string[]{"_", "_", "_"}),
        });

        // The players take turns.
        board[0][0] = "X";
        board[2][2] = "O";
        board[1][2] = "X";
        board[1][0] = "O";
        board[0][2] = "X";

        for (var i = 0; i < len(board); i++) {
            fmt.Printf("%s\n", strings.Join(board[i], " "));
        }
    }
}
```

### Handling Defer, Panic and Recover Operations
Handling Go `defer / panic / recover` operations in C# requires that code conversions create a [Go function execution context](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/GoFunc.cs#L63).

The function execution context is required in order to create a [defer](https://golang.org/ref/spec#Defer_statements) call stack and [panic](https://golang.org/pkg/builtin/#panic) / [recover](https://golang.org/pkg/builtin/#recover) exception handling. As an example, consider the following Go code:

```Go
package main

import "fmt"

func main() {
    f()
    fmt.Println("Returned normally from f.")
}

func f() {
    defer func() {
        if r := recover(); r != nil {
            fmt.Println("Recovered in f", r)
        }
    }()
    fmt.Println("Calling g.")
    g(0)
    fmt.Println("Returned normally from g.")
}

func g(i int) {
    if i > 3 {
        fmt.Println("Panicking!")
        panic(fmt.Sprintf("%v", i))
    }
    defer fmt.Println("Defer in g", i)
    fmt.Println("Printing in g", i)
    g(i + 1)
}
```

The Go code gets converted into C# code like the following:
```CSharp
using fmt = go.fmt_package;
using static go.builtin;

public static partial class main_package
{
    private static void Main() {
        f();
        fmt.Println("Returned normally from f.");
    }

    private static void f() => func((defer, _, recover) => {
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
        fmt.Println("Returned normally from g.");
    });

    private static void g(int i) => func((defer, panic, _) => {
        if (i > 3) {
            fmt.Println("Panicking!");
            panic(fmt.Sprintf("%v", i));
        }
        defer(() => fmt.Println("Defer in g", i));
        fmt.Println("Printing in g", i);
        g(i + 1);
    });
}
```

Certainly for functions that call `defer`, `panic` or `recover`, the Go function execution context is required. However, if the function does not _directly_ call the functions, nor _indirectly_ call the functions through a lambda, then you should be able to safely remove the wrapping function execution context. For example, in the converted C# code above the `main` function does not directly nor indirectly call `defer`, `panic` or `recover` so the function is safely simplified as follows:

```CSharp
private static void main() {
    f();
    fmt.Println("Returned normally from f.");
}
```

## Examples
* Example excerpt of converted code from the Go [`errors`](https://github.com/pkg/errors/blob/master/errors.go#L102) package:
  ```CSharp
  public static partial class errors_package
  {
      // New returns an error that formats as the given text.
      // Each call to New returns a distinct error value even if the text is identical.
      public static error New(@string text) =>
          error.As(new errorString(text))!;

      // errorString is a trivial implementation of error.
      private partial struct errorString {
          public @string s;
      }

      private static @string Error(this ref errorString e) {
          return e.s;
      }
  }
  ```
