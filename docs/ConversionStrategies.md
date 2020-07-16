# Conversion Strategies

> Strategies updated on 7/15/2020 -- see [Manual Tour of Go Conversion Takeaways](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/Manual%20Tour%20of%20Go%20Conversion%20Takeaways.txt) for more background on strategic decisions

* Each Go package is converted into static C# partial classes, e.g.: `public static partial class fmt_package`. Using a static partial class allows all functions within separate files to be available with a single import, e.g.: `using fmt = go.fmt_package;`.

* So that Go packages are more readily usable in C# applications, all converted code is in a root `go` namespace. Package paths are also converted to namespaces, so a Go import like `import "unicode/utf8"` becomes a C# using like `using utf8 = go.unicode.utf8_package;`.

* Go projects that contain a `main` function are converted into a standard C# project. The conversion process will automatically reference the needed shared projects, per defined encountered `import` statements, recursively. In this manner an executablewith packages compiled as referenced assemblies can be created. To create a single executable, like the original Go counterpart, a [self-contained executable](https://docs.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained) will be created (when the .NET feature is available).

* Go constants hold arbitrary-precision literals with expression support. Applying value to variables in Go happens at compile time, so C# conversion will need to support this operation. Ideally every numeric constant that can hold the value without overflowing should to be defined. An additional run-time lazy initialized BigInteger can be provided for simpler library usage but use of constants should be encouraged for better performance.

* Go switch statements are very flexible, case statements do not automatically fall-through - so a `break` statement is not required. When the Go `fallthrough` keyword is used, the next case expression is evaluated. As such, converting to simple `if / else if` is the logical best choice for C#.

* Go types are converted to C# `struct` types and used on the stack to optimize memory use and reduce the need for garbage collection. The `struct` types can be wrapped by C# `class` types that reference the type so that heap-allocated instances of the type can exist as needed.

  In Go a struct can be declared inline, C# does not support this so these dynamic structure elements are named and declared external to the function using per-class local dictionary of defined types. Each inline C# per static class local structure will need an index for uniqueness.

* Many go functions return a tuple of "value and success" or just a "value" where only the declared return type determines which overload to use. To accommodate similar functionality in C#, an overload is defined that takes a bool that returns the tuple style return using a constant like `WithOK`, e.g.:
  ```CSharp
  var (v, ok) = m["Answer", WithOK];
  ```

* Go interfaces are not explicitly implemented. Instead if extension-style functions exist that satisfy all defined interface methods, the class is said to implicitly implement the interface. To accommodate these duck-implemented interfaces, a generic class is created for each interface so that for a given type, interface extension methods can be looked up using reflection. To speed up this operation, as assemblies are loaded, extension methods are cached in a dictionary for quick lookup and any lookup operations are only done once statically during type initialization. To make use of typed generic class any assignments to interface variables will be cast to generic type, e.g., see equivalent C# code for handling Go duck-implemented interfaces:
  ```CSharp
  Abser a;
  var f = (MyFloat)(-math.Sqrt(2));
  a = Abser.As(f); // Succeeds if MyFloat type implements Abser interface
  ```

* In Go all objects are said to implement an interface with no methods, this is called the `EmptyInterface`. This operates fundamentally like .NET's `System.Object` class, consequently any time the `EmptyInterface` is encountered during conversion, it is simply replaced with `object`.

* All right-hand operands in assignment expressions in Go are evaluated before assignment to left-hand operands. This is tricky, for example, consider the following Go code:
  ```Go
  x, y = y, x+y
  ```
  The equivalent in C# is as follows
  ```CSharp
  var _y1 = x+y;
  x = y;
  y = _y1;
  ```

* Conversion of pointer types will use the C# `ref` keyword where possible. When this strategy does not work, a heap allocated instance of the base type will be created (see [`ptr<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/ptr.cs)).

  > C# pointers do not always work as a replacement for Go pointers since with C# (1) pointer types in structures cannot not refer to types that contain heap-allocated elements (e.g., arrays or slices that reference an array) as this would prevent pointer arithmetic for ambiguously sized elements, and (2) returning standard pointers to stack-allocated structures from a function is not allowed, instead you need to allocate the structure on the heap by creating a reference-type wrapper and then safely return a pointer to the reference.

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

* Handling Go `defer / panic / recover` operations in C# requires that code conversions only create a [Go function execution context](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/GoFunc.cs#L63) for converted Go methods that reference `defer`, `panic`, or `recover`.

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
