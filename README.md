![go2cs](docs/images/go2cs-small.png)
# Golang to C# Converter

Converts source code developed using the Go programming language (see [Go Language Specification](https://golang.org/ref/spec)) to the C# programming language (see [C# Language Specification](https://github.com/dotnet/csharplang/blob/master/spec/README.md)).

## Goals

* Convert Go code into visually similar C# code -- see [conversion strategies](ConversionStrategies.md).
 * Go is a minimalist language, as such it provides high-level functionality provided by the compiler. Converted C# code will have more visible code than Go used to provide equivalent functionality, however most of this code will be behind the scenes in separate files using partial class functionality.
* Convert Go units test to C# and verify results are the same (TBD).
* Conversion always tries to target managed code, this way code is more portable.
  * If there is no possible way for managed code to accomplish a specific task, an option always exists to create a [native interop library](http://www.mono-project.com/docs/advanced/pinvoke/) that works on multiple platforms, i.e., importing code from a `.dll`/`.so`/`.dylib`. Even so, the philosophy is to always attempt to use managed code, i.e., not to lean towards native code libraries, regardless of possible performance implications. Simple first.

## Project Status

Picking up this project again now that .NET 5.0 is forthcoming and some of the new changes are conducive to this project's original goals, such as [publishing as a self-contained executable](https://docs.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained).

Converted code now targets .NET Core only, specifically version 3.1 and C# 8.0 with goal to support .NET 5.0 and C# 9.0 when it comes out.

As a new strategy, sets of common Go examples have been manually converted to C# using the current [C# Go Library](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/). All relevant code samples from the "[Tour of Go](https://tour.golang.org/welcome/1)" have been converted to C#, [see converted code](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/). Ultimately would like to see this in head-to-head mode using [Try .NET](https://github.com/dotnet/try), for example:
![go2cs](src/Examples/Manual%20Tour%20of%20Go%20Conversions/HeadToHead-Small.png)
However, current Blazor C# 8.0 code targets do not run with published Try .NET -- will be watching for an update.

As releases are made for  updated `go2cs` executables, this will also include updates to pre-converted [Go Standard Library libraries for reference from NuGet](https://www.nuget.org/packages?q=%22package+in+.NET+for+use+with+go2cs%22).

## Installation

> There's no official release yet, but you can compile the code to produce a `go2cs` executable.

1. Copy the `go2cs.exe` into the `%GOBIN%` or `%GOPATH%\bin` path. The only dependency is .NET (see [prerequisites](#prerequisites) below).

2. Optionally extract the pre-converted C# Go Standard Library source code into the desired target path, by default `%GOPATH\src\go2cs\`

## Usage

1. Make sure source  application already compiles with Go (e.g., `go build`) before starting conversion. That means any needed dependencies should already be downloaded and available, e.g., with `go get`.

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
| value 0 | Required. Go source path or file name to convert. |
| value 1 | Target path for converted files. If not specified, all files (except for Go standard library files) will be converted to source path. |

### Future Options

A new command line option to prefer `var` over explicit types would be handy, e.g., specifying `-x` would request explicit type definitions otherwise defaulting to use `var` where possible.

If converted code ever gets updated where a new `import` is added, a command line option that would "rescan" the imports in a project and augment the project file to make sure all the needed imports are referenced would be ideal.

## C# to Go?

If you were looking to "go" in the other direction, a full _code based_ conversion from C# to Go is currently not an option. Even for the most simple projects, automating the conversion would end up being a herculean task with so many restrictions that it would likely not be worth the effort. However, for using compiled .NET code from within your Go applications you have options:

1. For newer [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/) applications, I would suggest trying the following project from Matias Insaurralde: https://github.com/matiasinsaurralde/go-dotnet -- this codes uses the [CLR Hosting API](https://blogs.msdn.microsoft.com/msdnforum/2010/07/09/use-clr4-hosting-api-to-invoke-net-assembly-from-native-c/) which allows you to directly use .NET based functions from within your Go applications.

2. For traditional .NET applications, a similar option would be to use [cgo](https://golang.org/cmd/cgo/) to fully self-host [Mono](https://www.mono-project.com/) in your Go application, see: http://www.mono-project.com/docs/advanced/embedding/.
