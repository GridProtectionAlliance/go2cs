![go2cs](docs/images/go2cs-small.png)
# Golang to C# Converter

Converts source code developed using the Go programming language (see [Go Language Specification](https://golang.org/ref/spec)) to the C# programming language (see [C# Language Specification](https://github.com/dotnet/csharplang/blob/master/spec/README.md)).

##### · · · · · · _Help Wanted!_ This is a work-in-progress -- see [project status](#project-status) · · · · · ·

## Why?

I'm pretty sure for many this was the first question asked when this project was discovered. Avoiding the cliché response of "[because I could](https://www.youtube.com/watch?v=mRNX6XJOeGU)", the real answer is more innocuous. I've been programming in C# for many years, even so, I try to keep an eye on new / cool tech being developed, regardless of language -- one common choice for new applications these days is Go. Often the easiest way for me to experiment with new technology is to integrate it with some of my existing C# code, hence the desire for this project -- besides, I figured I might learn something about building apps in Go in the process.

Hopefully for those of you that code in Go all the time and have an occasion where you need to build a C# app, this conversion tool will provide you with a head start. You can pull in some of your existing code and even use the Go Standard Library functions that you are used to.

### Elephant in the room

TL;DR: Do not expect converted C# code to run as fast as the original Go code.

The .NET runtime environment is very different from Go -- a compiled .NET application actually consists of human-readable byte-code called [Common Intermediate Language](https://en.wikipedia.org/wiki/Common_Intermediate_Language) (CIL). At runtime the CIL code is compiled to native machine code using a [just-in-time compilation](https://en.wikipedia.org/wiki/Just-in-time_compilation) process. Compared to Go, which is compiled directly to native machine code, .NET incurs some compile based processing delays at startup. However, you can remove the just-in-time compilation startup delays for a .NET application by pre-compiling the application to native machine code using the [Native Image Generation](https://docs.microsoft.com/en-us/dotnet/framework/tools/ngen-exe-native-image-generator) tool in .NET (`ngen.exe`).

For this project, the philosophy taken for converted code is to produce code that is very visually similar to the original Go code as well as being as behaviorally close as possible to Go at a purely "source code" level. The focus is to make converted code more usable and understandable to a Go programmer inside of a C# environment without being hyper-focused on optimizations. That doesn't mean your converted code is going to be super slow -- it should run as fast as other comparable .NET applications -- it just may not run as fast as the original Go code. However, because of the simplicity of [Go's design](https://talks.golang.org/2012/splash.slide), converted code may have a natural performance advantage over more traditionally developed C# applications just because of its static nature.

If you are looking for a more _binary_ integration, you might consider using native compiled Go code directly within your .NET application. One option would be would be [exporting Go functions](https://golang.org/cmd/cgo/#hdr-C_references_to_Go) as C code, including the C code exports in a DLL and then [importing the functions](https://msdn.microsoft.com/en-us/library/system.runtime.interopservices.dllimportattribute%28v=vs.110%29.aspx) in C#.

### I was looking for C# to Go?

If you were looking to "go" in the other direction, a full _code based_ conversion from C# to Go is currently not an option. Even for the most simple projects, automating the conversion would end up being a herculean task with so many restrictions that it would likely not be worth the effort. However, for using compiled .NET code from within your Go applications you have options:

1. For newer [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/) applications, I would suggest trying the following project from Matias Insaurralde: https://github.com/matiasinsaurralde/go-dotnet -- this codes uses the [CLR Hosting API](https://blogs.msdn.microsoft.com/msdnforum/2010/07/09/use-clr4-hosting-api-to-invoke-net-assembly-from-native-c/) which allows you to directly use .NET based functions from within your Go applications.

2. For traditional .NET applications, a similar option would be to use [cgo](https://golang.org/cmd/cgo/) to fully self-host [Mono](https://www.mono-project.com/) in your Go application, see: http://www.mono-project.com/docs/advanced/embedding/.

## Project Status

Right now this project is still in nascent stages in that there is a "ton" of work to do. Overall project level conversions are basically complete and the code will produce proper function signatures. Strategies exist for most Go functional constructs in C# (see QuickTest project and embedded resource templates), but more work will need to be done.

Work is currently moving through all the basic code conversions using the [Golang ANTLR4 grammar](https://github.com/antlr/grammars-v4/tree/master/golang), however, initial attempts to convert all the Go Standard Library code found that the grammar needs some work. If you update the grammar file [in this project](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/go2cs/Parser), please kindly make a PR to update the original grammar on the [ANTLR site](https://github.com/antlr/grammars-v4) as well.

A "big" upcoming task will be to get the Go Standard Library operational by filling in behind code that normally compiles with [Go's Assembler](https://golang.org/doc/asm) (all those platform specific `.s` files) with equivalent .NET implementations. Please note that the strategy is to keep the code managed if at all possible, this way there are no external dependencies introduced, see [conversion strategies](#conversion-strategies) for more information.

The other item that is needed is a unit testing infrastructure. It is desirable to be able to create Go based behavioral tests then convert the tests to C# - this way the Go code could be ran along with the C# code to validate that the results are the same (somehow).

In the future, code conversions should automatically not use a Go function execution context unless it is needed, see [optimizations](#optimizations) section below (this may be low hanging fruit).

Currently the converted code targets traditional a .NET application, specifically version 4.7.1 and C# 7.2 to accommodate better return by-ref functionality for structures. However, a [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/) version should be equally possible, preferably by command line option, e.g., `-c` for .NET Core.

A new command line option to prefer `var` over explicit types would be handy, e.g., specifying `-x` would request explicit type definitions otherwise defaulting to use `var` where possible.

Ideally as releases are made for an updated `go2cs` executable, this can also include an update to a pre-converted Go Standard Library for download so that users don't have to spend time converting this themselves.

It might be desirable to produce a NuGet package that contains the Go Standard Library as a .NET library that can be imported into C# apps as this might be handy for Go programmers playing around in C#, allowing them to use familiar packages.

The conversion code looks for files that contain a `main` function and converts them into a standard C# project. The conversion process _automatically_ references needed shared projects for each encountered import statement recursively. It does this so that a single executable with no external dependencies can be created just like the original Go source code. If converted code ever gets updated where a new `import` is added, a command line option that would "rescan" the imports in a project and augment the project file to make sure all the needed imports are referenced would be ideal.

## Installation

1. Copy the `go2cs.exe` into the `%GOBIN%` or `%GOPATH%\bin` path. The only dependency is .NET (see [prerequisites](#prerequisites) below).

2. Optionally extract the pre-converted C# Go Standard Library source code into the desired target path, by default `%GOPATH\src\go2cs\`

### Prerequisites

The `go2cs` program is a command line utility that requires .NET 4.7.1. You will need to make sure that .NET 4.7.1 or comparable version of [Mono](https://www.mono-project.com/) is already installed before executing the conversion utility.

* For Windows, use the [.NET 4.7.1](https://www.microsoft.com/en-us/download/confirmation.aspx?id=56115) installer
* For other platforms, install the [latest version of Mono](https://www.mono-project.com/download/stable/#download-lin-ubuntu)

Note that code converted from Go to C# will also target .NET 4.7.1 and compile using C# 7.2. [Visual Studio 2017](https://www.visualstudio.com/downloads/) is recommended in order to compile converted code, the free Community Edition should be fine. For non-Windows platforms, you can try [Visual Studio Code](https://code.visualstudio.com/).

## Usage

1. Make sure source application already compiles with Go (e.g., `go build`) before starting conversion. That means any needed dependencies should already be downloaded and available, e.g., with `go get`.

2. Execute `go2cs` specifying the Go source path or specific file name to convert. For example:
 * Convert a single Go file: `go2cs -l Main.go`
 * Convert a Go project: `go2cs MyProject`
 * Convert Go Standard Library: `go2cs -s -r C:\\Go\src\\`

### Command Line Options

| Option | Description |
|:------:|:------------|
| -l | (Default: false) Set to only convert local files in source path. Default is to recursively convert all encountered "import" packages. |
| -o | (Default: false) Set to overwrite, i.e., reconvert, any existing local converted files. |
| -i | (Default: false) Set to overwrite, i.e., reconvert, any existing files from imported packages. |     
| -t | (Default: false) Set to show syntax tree of parsed source file. |
| -e | (Default: $.^) Regular expression to exclude certain files from conversion, e.g., "^.+\_test\\.go$". Defaults to exclude none. |
| -s | (Default: false) Set to convert needed packages from Go standard library files found in "%GOROOT%\\src". |
| -r | (Default: false) Set to recursively convert source files in subdirectories when a Go source path is specified. |
| -m | (Default: false) Set to force update of pre-scan metadata. |
| -g | (Default: %GOPATH%\\src\\go2cs) Target path for converted Go standard library source files. |
| --help | Display this help screen. |
| --version | Display version information. |   
| value pos. 0 | Required. Go source path or file name to convert. |
| value pos. 1 | Target path for converted files. If not specified, all files (except for Go standard library files) will be converted to source path. |

## Conversion Strategies

* Each Go package is converted into static C# partial classes, e.g.: `public static partial class fmt_package`. Using a static partial class allows all functions within separate files to be available with a single import, e.g.: `using fmt = go.fmt_package;`.

* So that Go packages are more readily usable in C# applications, all converted code is in a root `go` namespace. Package paths are also converted to namespaces, so a Go import like `import "unicode/utf8"` becomes a C# using like `using utf8 = go.unicode.utf8_package;`.

* All imported Go packages are converted into [shared projects](https://docs.microsoft.com/en-us/xamarin/cross-platform/app-fundamentals/shared-projects?tabs=vswin) and added as a reference to the main project so that a single executable is created, i.e., packages are not compiled into external DLL dependencies.

* Go projects that contain a `main` function are converted into a standard C# project. The conversion process will automatically reference the needed shared projects, per defined encountered `import` statements, recursively. In this manner a single executable with no external dependencies, besides .NET runtime, is created - just like its original Go counterpart.

* Go types are converted to C# `struct` types and used on the stack to optimize memory use and reduce the need for garbage collection. The `struct` types can be wrapped by C# `class` types that reference the type so that heap-allocated instances of the type can exist as needed.

* Conversion of pointer types will use the C# `ref` keyword where possible. When this strategy does not work, a heap allocated instance of the base type will be created (see [`Ref<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/goutil/Ref.cs#L37)) with an associated pointer to the heap allocated instance (see [`Ptr<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/goutil/Ptr.cs#L37), literally a reference to the reference). 

> C# pointers do not always work as a replacement for Go pointers since with C# (1) pointer types in structures cannot not refer to types that contain heap-allocated elements (e.g., arrays or slices that reference an array) as this would prevent pointer arithmetic for ambiguously sized elements, and (2) returning standard pointers to stack-allocated structures from a function is not allowed, instead you need to allocate the structure on the heap by creating a reference-type wrapper and then safely return a pointer to the reference.

* Conversion of Go slices is based on the [`slice<T>`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/goutil/Slice.cs) structure defined in the [`goutils`](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/goutil) shared project. For example, the following Go code using slice operations:

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
using fmt = go.fmt_package;
using strings = go.strings_package;

using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            // Create a tic-tac-toe board.
            var board = new slice<slice<@string>>(new[] {
                new slice<@string>(new[] {"_", "_", "_"}),
                new slice<@string>(new[] {"_", "_", "_"}),
                new slice<@string>(new[] {"_", "_", "_"}),
            });

            // The players take turns.
            board[0][0] = "X";
            board[2][2] = "O";
            board[1][2] = "X";
            board[1][0] = "O";
            board[0][2] = "X";

            {
                var i = 0;

                while (i < len(board))
                {
                    fmt.Printf("%s\n", strings.Join(board[i], " "));
                    i++
                }
            }
        }
    }
}
```

* Conversion always tries to target managed code, this way code is more portable. If there is no possible way for managed code to accomplish a specific task, an option always exists to create a [native interop library](http://www.mono-project.com/docs/advanced/pinvoke/) that works on multiple platforms, i.e., importing code from a `.dll`/`.so`/`.dylib`. Even so, the philosophy is to always attempt to use managed code, i.e., not to lean towards native code libraries, regardless of possible performance implications. Simple first.

Example excerpt of converted code from the Go [`fmt`](https://github.com/golang/go/blob/master/src/fmt/format.go#L41) package:
```CSharp
using strconv = go.strconv_package;
using utf8 = go.unicode.utf8_package;

using static go.builtin;

namespace go
{
    public static partial class fmt_package
    {
        private const string ldigits = "0123456789abcdefx";
        private const string udigits = "0123456789ABCDEFX";

        private struct fmt {
            public Ptr<buffer> buf;

            public fmtFlags fmtFlags;

            public @int wid;  // width
            public @int prec; // precision

            // intbuf is large enough to store %b of an int64 with a sign and
            // avoids padding at the end of the struct on 32 bit architectures.
            public fixed @byte intbuf[68];
        }

        private static void clearFlags(ref this fmt _this)
        {
            f.fmtFlags = new fmtFlags();
        }
    }
}
```

## Optimizations

Code conversions only create a [Go function execution context](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/goutil/GoFunc.cs#L47) for converted Go function that reference `defer`, `panic`, or `recover`.
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
using goutil;

public static partial class main_package
{
    private static void main() => func((defer, panic, recover) => {
        f();
        fmt.Println("Returned normally from f.");
    });

    private static void f() => func((defer, panic, recover) => {
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

    private static void g(int i) => func((defer, panic, recover) => {
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

Certainly for functions that call `defer`, `panic` or `recover`, the Go function execution context is required. However, if the function does not _directly_ call the functions, nor _indirectly_ call the functions through a lambda, then you should be able to safely remove the wrapping function execution context. For example, in the converted C# code above the `main` function does not directly nor indirectly call `defer`, `panic` or `recover` so the function will be safely simplified as follows:

```CSharp
private static void main() {
    f();
    fmt.Println("Returned normally from f.");
}
```