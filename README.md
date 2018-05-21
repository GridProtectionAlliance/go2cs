![go2cs](docs/images/go2cs-small.png)
# Golang to C# Converter

Converts source code developed using the Go programming language (see [Go Language Specification](https://golang.org/ref/spec)) to the C# programming language (see [C# Language Specification](https://github.com/dotnet/csharplang/blob/master/spec/README.md)).

#### _Help Wanted!_ This is a work-in-progress -- see [project status](#project-status)

## Why?

I'm pretty sure for many this was the first question asked when this project was discovered. Avoiding the cliché response of "[because I could](https://www.youtube.com/watch?v=mRNX6XJOeGU)", the real answer is more innocuous. I've been programming in C# for many years, even so, I try to keep an eye on new / cool tech being developed, regardless of language -- a common choice for new applications these days is Go. Often the easiest way for me to experiment with new technology is to integrate it with some of my existing C# code, hence the desire for this project. Besides, I figured I might learn something about building apps in Go in the process.

Hopefully for those of you that code in Go all the time and have an occasion where you need to build a C# app, this conversion tool will provide you with a head start. You can pull in some of your existing code and even use the Go Standard Library functions that you are used to.

### Elephant in the room

TL;DR: Do not expect converted C# code to run as fast as the original Go code.

The .NET runtime environment is very different from Go -- a compiled .NET application actually consists of human-readable byte-code called [Common Intermediate Language](https://en.wikipedia.org/wiki/Common_Intermediate_Language) (CIL). At runtime the CIL code is compiled to native machine code using a [just-in-time compilation](https://en.wikipedia.org/wiki/Just-in-time_compilation) process. Compared to Go, which is compiled directly to native machine code, .NET incurs some compile based processing delays at startup. However, you can remove the just-in-time compilation startup delays for a .NET application by pre-compiling the application to native machine code using the [Native Image Generation](https://docs.microsoft.com/en-us/dotnet/framework/tools/ngen-exe-native-image-generator) tool in .NET (`ngen.exe`).

For this project, the philosophy taken for converted code is to produce code that is very visually similar to the original Go code as well as being as behaviorally close as possible to Go at a purely "source code" level. The focus is to make converted code more usable and understandable to a Go programmer inside of a C# environment without being hyper-focused on optimizations. That doesn't mean your converted code is going to be super slow -- it should run as fast as any other comparable .NET application. However, because of the simplicity of [Go's design](https://talks.golang.org/2012/splash.slide), converted code may have a natural performance advantage over more traditionally developed C# applications just because of its static design.

If you are looking for a more _binary_ integration, you might consider using native compiled Go code directly within your .NET application. One option would be would be [exporting Go functions](https://golang.org/cmd/cgo/#hdr-C_references_to_Go) as C code, including the C code exports in a DLL and then [importing the functions](https://msdn.microsoft.com/en-us/library/system.runtime.interopservices.dllimportattribute%28v=vs.110%29.aspx) in C#.

### I was looking for C# to Go?

If you are looking to "go" in the other direction, a full _code based_ conversion from C# to Go is currently not an option. Even for the most simple projects this would end up being a herculean task with so many restrictions that it would likely not be worth the effort. However, for using compiled .NET code from within your Go applications you have options.

For newer [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/) applications, I would suggest trying the following project from Matias Insaurralde: https://github.com/matiasinsaurralde/go-dotnet -- this codes uses the [CLR Hosting API](https://blogs.msdn.microsoft.com/msdnforum/2010/07/09/use-clr4-hosting-api-to-invoke-net-assembly-from-native-c/) which allows you to directly use .NET based functions from within your Go applications.

For traditional .NET applications, a similar option would be to use [cgo](https://golang.org/cmd/cgo/) to fully self-host [Mono](https://www.mono-project.com/) in your Go application, see: http://www.mono-project.com/docs/advanced/embedding/.

## Project Status

Right now this project is still in nascent stages in that there is a "ton" of work to do. Overall project level conversions are basically complete and the code will produce proper function signatures. Strategies exist for most Go functional constructs in C# (see QuickTest project and embedded resource templates), but more work will need to be done.

Work is currently moving through all the basic code conversions using the ANTLR grammar, however, initial attempts to convert all the Go Standard Library code found that the [Golang grammar](https://github.com/antlr/grammars-v4/tree/master/golang), needs some work. If you update grammar in this project, please kindly make a PR to update the original grammar on the ANTLR site as well.

A "big" upcoming task will be to get the Go Standard Library operational by filling in behind code that normally compiles with [Go's Assembler](https://golang.org/doc/asm) (all those platform specific `.s` files) with equivalent .NET implementations. Please note that the strategy is to keep the code managed if at all possible, this way there are no external dependencies introduced, see [conversion strategies](#conversion-strategies) for more information.

The other item that is needed is a unit testing infrastructure. It is desired to be able to create Go based behavioral tests then convert the tests to C# - this way the Go code can be ran along with the C# code to validate that the results are the same.

Ideally as releases are made for an updated `go2cs` executable, this can also include an update to a pre-converted Go Standard Library for download so that users don't have to spend time converting this themselves.

## Installation

1. Copy the `go2cs.exe` into the `%GOBIN%` or `%GOPATH%\bin` path. The only dependency is .NET (see [prerequisites](#prerequisites) below).

2. Optionally extract the pre-converted C# Go Standard Library source code into the desired target path, by default `%GOPATH\src\go2cs\`

### Prerequisites

The `go2cs` program is a command line utility that requires .NET 4.7.1. You will need to make sure that .NET 4.7.1 or comparable version of [Mono](https://www.mono-project.com/) is already installed before executing the conversion utility.

* For Windows, use the [.NET 4.7.1](https://www.microsoft.com/en-us/download/confirmation.aspx?id=56115) installer
* For other platforms, install the [latest version of Mono](https://www.mono-project.com/download/stable/#download-lin-ubuntu)

Note that code converted from Go to C# will also target .NET 4.7.1 and compile using C# 7.2. [Visual Studio 2017](https://www.visualstudio.com/downloads/) is recommended in order to compile converted code, the free Community Edition is should be fine. For non-Windows platforms, you can try [Visual Studio Code](https://code.visualstudio.com/).

## Usage

1. Make sure source application already compiles with Go (e.g., `go build`) before starting conversion. That means any needed dependencies should already be downloaded and available, e.g., with `go get`.

2. Execute `go2cs` specifying the Go source path or specific file name to convert. For example:
 * Convert a single Go file: `go2cs -l Main.go`
 * Convert a Go project: `go2cs MyProject`

### Command Line Options

| Option | Description |
|:------:|:------------|
| -l | (Default: false) Set to only convert local files in source path. Default is to recursively convert all encountered "import" packages. |
| -o | (Default: false) Set to overwrite, i.e., reconvert, any existing local converted files. |
| -i | (Default: false) Set to overwrite, i.e., reconvert, any existing files from imported packages. |     
| -t | (Default: false) Set to show syntax tree of parsed source file. |
| -e | (Default: $.^) Regular expression to exclude certain files from conversion, e.g., "^.+\_test\\.go$". Defaults to exclude none. |
| -s | (Default: false) Set to convert needed packages from Go standard library files found in "%GOROOT%\\src". |
| -g | (Default: %GOPATH%\\src\\go2cs) Target path for converted Go standard library source files. |
| --help | Display this help screen. |
| --version | Display version information. |   
| value pos. 0 | Required. Go source path or file name to convert. |
| value pos. 1 | Target path for converted files. If not specified, all files (except for Go standard library files) will be converted to source path. |

## Conversion Strategies

* Each Go package is converted into static C# partial classes, e.g.: `public static partial class fmt_package`. Using a static partial class allows all functions within separate files to be available with a single import, e.g.: `using fmt = go.fmt_package;`.

* So that Go packages are readily usable in C# applications, all converted code is in a root `go` namespace. Package paths are also converted to namespaces, so a Go import like `import "unicode/utf8"` becomes a C# using like `using utf8 = go.unicode.utf8_package;`.

* All imported Go packages are converted into [shared projects](https://docs.microsoft.com/en-us/xamarin/cross-platform/app-fundamentals/shared-projects?tabs=vswin) and added as a reference to the main project so that a single executable is created, i.e., packages are not compiled into external DLL dependencies.

* Conversion of pointer types will use the C# `ref` keyword where possible. When this strategy does not work, a regular pointer will be required -- in these contexts imported packages will be marked as `unsafe` to allow pointers.

* Conversion always tries to target managed code, this way code is more portable. If there is no possible way for managed code to accomplish a specific task, an option always exists to create a [native interop library](http://www.mono-project.com/docs/advanced/pinvoke/) that works on multiple platforms, i.e., importing code from a `.dll`/`.so`/`.dylib`. Even so, the philosophy is to always attempt to use managed code, i.e., not to lean towards native code libraries, regardless of possible performance implications. Simple first.

Example excerpt of converted code from the Go [`fmt`](https://github.com/golang/go/blob/master/src/fmt/format.go#L41) package:
```
using strconv = go.strconv_package;
using utf8 = go.unicode.utf8_package;

namespace go
{
    public static unsafe partial class fmt_package
    {
        private const string ldigits = "0123456789abcdefx";
        private const string udigits = "0123456789ABCDEFX";

        private struct fmt
        {
            public buffer* buf;

            public fmtFlags fmtFlags;

            public int wid;  // width
            public int prec; // precision

            // intbuf is large enough to store %b of an int64 with a sign and
            // avoids padding at the end of the struct on 32 bit architectures.
            public fixed byte intbuf[68];
        }

        private static void clearFlags(ref this fmt _this) => func(ref _this, (ref fmt f, Defer defer, Panic panic, Recover recover) =>
        {
            f.fmtFlags = new fmtFlags();
        });
    }
}
```
