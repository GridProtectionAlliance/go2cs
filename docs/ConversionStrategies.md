# Conversion Strategies

> Strategies updated on 7/18/2020 -- see [Manual Tour of Go Conversion Takeaways](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/Manual%20Tour%20of%20Go%20Conversion%20Takeaways.txt) for more background on current decisions. This is considered a living document, as more use cases and conversions are completed, these strategies will be updated as needed.

* Each Go package is converted into static C# partial classes, e.g.: `public static partial class fmt_package`. Using a static partial class allows all functions within separate files to be available with a single import, e.g.: `using fmt = go.fmt_package;`.

* So that Go packages are more readily usable in C# applications, all converted code is in a root `go` namespace. Package paths are also converted to namespaces, so a Go import like `import "unicode/utf8"` becomes a C# using like `using utf8 = go.unicode.utf8_package;`.

* Go projects that contain a `main` function are converted into a standard C# executable project, i.e., `<OutputType>Exe</OutputType>`. The conversion process will automatically reference and convert needed external projects as library projects, i.e., `<OutputType>Library</OutputType>` per any defined encountered `import` statements, recursively. In this manner an executable with packages compiled as project referenced assemblies can be created. To create a single executable, like the original Go counterpart, a [self-contained executable](https://docs.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained) will be created.

  Long term plan is to provide ability to use reference packages, e.g., from [NuGet](https://www.nuget.org/packages?q=%22package+in+.NET+for+use+with+go2cs%22), that have already been converted. This will require an intermediate repository of original Go package name / location mapped to published NuGet package reference. A web site has been reserved for this purpose, i.e., http://nugetgo.net/ - for the moment, this just redirects to the go2cs GitHub site.

* Go constants hold arbitrary-precision literals with expression support. Applying value to variables in Go happens at compile time, so C# conversion will need to support this operation. Ideally every numeric constant that can hold the value without overflowing should to be defined, see [example](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/basics/numeric-constants). An additional run-time lazy initialized BigInteger can be provided for simpler library usage but use of constants should be encouraged for better performance.

* Go `switch` statements are very flexible, case statements do not automatically fall-through - so a `break` statement is not required. When the Go `fallthrough` keyword is used, the next case expression is evaluated. Based on work done with the [Manual Tour of Go Conversions](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions), converting to simple `if / else if` statements seems like the logical best choice unless the original Go `case` statements all use constants, in which case a traditional C# `switch` would work fine.

* Go types are converted to C# `struct` types and used on the stack to optimize memory use and reduce the need for garbage collection. By using a # `struct` instead of a `class`, converted C# code should better match original Go operation both from a memory and performance perspective. The `struct` types will be wrapped by a C# `class` that references the type value so that heap-allocated instances of the type can exist as needed, see [`ptr<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/ptr.cs), such as, when the type needs to leave the local stack.

  > Note: In Go, a struct can be declared inline. C# does not support inline or intra-function `struct` definitions, so these dynamic structure elements will be named and declared external to the function. A per-parent static class local dictionary of defined types will need to be used during conversion to track this. Each inline C# per static class local structure will need an index for uniqueness. See [example](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/moretypes/slice-literals) with inline `struct`.

* Many go functions return a tuple of "value and success" or just a "value" where only the declared return type determines which overload to use. To accommodate similar functionality in C#, an overload is defined that takes a bool that returns the tuple style return using a constant like `WithOK`, e.g.:

  ```CSharp
  var v1 = m["Answer"];
  var (v2, ok) = m["Answer", WithOK];
  ```
  
  Similarly functions returning an "value and error" tuple can operate in the same way:
  
  ```CSharp
  var n1 = r.Read(b);
  var (n2, err) = r.Read(b, WithErr);
  ```

  See [success tuple return example](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/moretypes/mutating-maps/main_package.cs).

* Go interfaces are not explicitly implemented. Instead, if extension-style functions exist that satisfy all defined interface methods, the class is said to implicitly implement the interface. To accommodate these duck-implemented interfaces, a generic class is created for each interface so that for a given type, interface extension methods can be looked up using reflection. To speed up this operation, as assemblies are loaded, extension methods are [cached in a dictionary](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/TypeExtensions.cs#L121) for quick lookup and any type-specific lookup operations are only done once statically during [type initialization](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/error.cs#L127). To make use of typed generic class any assignments to interface variables will be cast to generic type, for example, see equivalent C# code with `As` function for handling Go duck-implemented interfaces:

  ```CSharp
  Abser a;                          // Abser is an interface with methods
  var f = (MyFloat)(-math.Sqrt(2)); // MyFloat is a custom type
  a = Abser.As(f);                  // Succeeds only if MyFloat type implements Abser interface methods
  ```

  See [interface example](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/methods/interfaces).

  > Note: In Go, all pointer types are setup to automatically dereference. For example, the following `age` property assignments are equivalent in Go:
  ```Go
  var s struct {age int}
  var ps = &s
  (*ps).age = 20
  ps.age = 20
  ```
  This automatic dereferencing also applies to extension methods, in other words, an extension-style method for a non-pointer type will work for the type as well as a pointer to the type.

* In Go all objects are said to implement an interface with no methods, this is called the `EmptyInterface`. This operates fundamentally like .NET's `System.Object` class, consequently any time the `EmptyInterface` is encountered during conversion, it is simply replaced with `object`. If there are type specific semantic use cases where this does not work, this strategy may need to be reevaluated.

* All right-hand operands in assignment expressions in Go are evaluated before assignment to left-hand operands. This is tricky, for example, consider the following Go code:

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

* Conversion of pointer types will use the C# `ref` keyword where possible. When this strategy does not work, a heap allocated instance of the base type will be created (see [`ptr<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/ptr.cs)).

  > C# unsafe pointers do not always work as a replacement for Go pointers since with C# (1) pointer types in structures cannot not refer to types that contain heap-allocated elements (e.g., arrays or slices that reference an array) as this would prevent pointer arithmetic for ambiguously sized elements, and (2) returning standard pointers to stack-allocated structures from a function is not allowed, instead you need to allocate the structure on the heap by creating a reference-type wrapper and then safely return a pointer to the reference.

* Conversion of Go slices is based on the [`slice<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/slice.cs) structure. For example, the following Go code using slice operations:

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

* Handling Go `defer / panic / recover` operations in C# requires that code conversions create a [Go function execution context](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/GoFunc.cs#L63).

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
