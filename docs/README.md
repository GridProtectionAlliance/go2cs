![go2cs](images/go2cs-small.png)
# Golang to C# Converter

Converts source code developed using the Go programming language (see [Go Language Specification](https://golang.org/ref/spec)) to the C# programming language (see [C# Language Specification](https://github.com/dotnet/csharplang/blob/master/spec/README.md)).

![CodeQL](https://github.com/GridProtectionAlliance/go2cs/workflows/CodeQL/badge.svg)

## News

* Project has been updated to use .NET 7.0 / C# 11

* String literals are encoded uing UTF-8 (C# `u8` string suffix) which uses `ReadOnlySpan<byte>` ref struct. This should make Go strings faster since strings do not have to be converted to UTF8 from UTF16. Also added an experimental [`sstring`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/sstring.cs) which is a ref struct implementation of a Go string.

* Code conversions now better match original Go code styling

* Recent example usages of `go2cs` allow the use of [Golang](https://golang.org/ref/spec) as the scripting language for the [Unity](https://unity.com/) and [Godot](https://godotengine.org/) game engine platforms. See the [GoUnity](https://github.com/ritchiecarroll/GoUnity) and [GodotGo](https://github.com/ritchiecarroll/GodotGo) projects.

## Goals

* Convert Go code into C# so that Go code can be directly used within .NET ecosystem.
  * This is the primary goal of `go2cs`.
* Convert Go code into behaviorally and visually similar C# code -- see [conversion strategies](ConversionStrategies.md).
  * Code conversions focus first on making sure C# code runs as behaviorally similar to Go code as possible. This means, for now, leaving out things like code conversions into `async` functions. Instead conversions make things operate the way they do in Go, e.g., simply running a function on the current thread or running it in on the thread pool when using a [`goroutine`](https://golang.org/ref/spec#Go_statements).
  * C# conversions attempt to make code visually similar to original Go code to make it easier to identity corresponding functionality. As Go is a minimalist language, it provides high-level functionality provided by the compiler, often much more than C# does. As such, converted C# code will have more visible code than Go for equivalent functionality, however much of this code will be behind the scenes in separate files using partial class functionality.
* Convert Go units test to C# and verify results are the same (TBD).
  * For most unit tests defined in Go, it should be possible to create an equivalent converted C# unit test. In many cases it may also be possible to successfully compare "outputs" of both unit tests as an additional validation test.
* Convert Go code into managed C# code.
  * Conversion always tries to target managed code, this way code is more portable. If there is no possible way for managed code to accomplish a specific task, an option always exists to create a [native interop library](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke) that works on multiple platforms, i.e., importing code from a `.dll`/`.so`/`.dylib`. Even so, the philosophy is to always attempt to use managed code, i.e., not to lean towards native code libraries, regardless of possible performance implications. Simple first.

## Project Status

### Automated Code Conversion of Go Standard Library

A few initial conversions of the full Go source code have been completed, you can find the latest results in the repo:
[`src/go-src-converted`](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/go-src-converted). These conversion successes are notable milestones in that they represent full successful conversions of the entire Go source library using the [ANTLR4 Golang grammar](https://github.com/antlr/grammars-v4/tree/master/golang) without failing. Each iteration improves on the next, here are few examples:

* [errors/errors.go](https://github.com/golang/go/blob/master/src/errors/errors.go) => [errors/errors.cs](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/errors/errors.cs)
* [fmt/format.go](https://github.com/golang/go/blob/master/src/fmt/format.go) => [fmt/format.cs](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/fmt/format.cs)
* [compress/gzip/gunzip.go](https://github.com/golang/go/blob/master/src/compress/gzip/gunzip.go) => [compress/gzip/gunzip.cs](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/compress/gzip/gunzip.cs)

Not all converted standard library code will compile yet in C# yet - work remaining to _properly_ parse and convert all Go source library files, with its many use cases and edge cases, can be inferred by examining the warnings in the [`build log`](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/go-src-converted/build.log) for this initial conversion. This log should help lay out a road map of remaining tasks.

Note that go2cs simple conversions currently depend on a small subset of the Go source library, [`src/gocore`](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/gocore), that was manually converted. As the project progresses, there will be a merger of automatically converted code and manually converted code. For example, the [`builtin`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/golib/builtin.cs) Go library functions will always require some special attention since many of its features are implemented outside normal Go code, such as with assembly routines.

A strategy to automate conversion of native system calls in Go code, i.e., a function declaration without a body that provides a signature for a native external function, is to create a [`partial method`](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/partial-method) in C# for the native call. A manually created file that implements the partial method can now be added that will exist along side the auto-converted files and not be overwritten during conversion.

### Recent Activity
Converted code now targets .NET 6.0 and C# 10.0. Recent updates use file-scoped namespaces, reduced indentation to better match original Go code and new command line options to control code styling and allow older C# versions.

Currently, work to improve code conversions is progressing by walking through each of the [behavioral testing](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Tests/Behavioral) projects. Iterating through each of these simple use cases improves overall automated code conversion quality.

Sets of common Go sample code have been manually converted to C# using the current [C# Go Library](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/gocore/). As an example, all relevant code samples from the "[Tour of Go](https://tour.golang.org/welcome/1)" have been converted to C#, [see converted code](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/Examples/Manual%20Tour%20of%20Go%20Conversions/). Ultimately would like to see this in head-to-head mode using [Try .NET](https://github.com/dotnet/try), for example:
![go2cs](images/HeadToHead-Small.png)
Currently converted code will not execute with latest release of Try .NET (see [posted issue](https://github.com/dotnet/try/issues/859)). Will be watching for an update.

As releases are made for updated `go2cs` executables, this will also include updates to pre-converted [Go Standard Library libraries for reference from NuGet](https://www.nuget.org/packages?q=%22package+in+.NET+for+use+with+go2cs%22).

## Testing

Before attempting conversions it is important that Go coding environment is already properly setup, especially `GOPATH` environmental variable. See [Getting Started](https://golang.org/doc/install) with Go.

Current Go to C# code conversions reference compiled assemblies of the go2cs core library code from the configured `GOPATH`, specifically `%GOPATH%\src\go2cs\`. Run the `deploy-gocore.bat` script located in the `go2cs\src` folder to handle copying source to target path and then building needed debug and release assemblies.

Once a compiled version of the current go2cs core library has been deployed, you can test conversions. For example:

```Shell
go2cs -o -i C:\Projects\go2cs\src\Tests\Behavioral\ArrayPassByValue
```

This will convert Go code to C#. You can then build and run both the Go and C# versions and compare results.

> **Debugging with Visual Studio:** After running the `deploy-gocore.bat` script you can run conversion code from within Visual Studio by right-clicking on the go2cs project, selecting "Properties" then clicking on the "Debug" tab. In the "Application arguments:" text box you can enter the command line test parameters, e.g., `-o -i -h C:\Projects\go2cs\src\Tests\Behavioral\ArrayPassByValue`. When the active solution configuration targets "Debug" you can run the go2cs project to convert Go code, then run converted code.

> **Debugging Note:** Keep in mind that you have local `gocore` source code and a copy of the source in the `GOPATH`. Compiled versions of converted code will reference the `gocore` code copy in the `GOPATH` folder. If you encounter an exception in `gocore` while debugging, Visual Studio will be displaying the code in the `GOPATH` folder. Any code changes you make might make will then be in the `GOPATH` folder instead of your local folder and be lost at the next run of the `deploy-gocore.bat` script.

## Installation

> There is an [experimental release](https://github.com/GridProtectionAlliance/go2cs/releases) available, however, for latest updates you can compile the source code to produce a `go2cs` executable.

Copy the `go2cs` executable into the `%GOBIN%` or `%GOPATH%\bin` path. The `go2cs` can compile as a standalone executable for your target platform with no external dependencies using Visual Studio, see [publish profiles](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/go2cs/Properties/PublishProfiles).

## Usage

> Before posting an issue for usage related questions consider using the [go2cs discussions forum](https://github.com/GridProtectionAlliance/go2cs/discussions).

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
| -h | (Default: false) Set to exclude header conversion comments which include original source file path and conversion time. |
| -t | (Default: false) Set to show syntax tree of parsed source file. |
| -e | (Default: $.^) Regular expression to exclude certain files from conversion, e.g., "^.+\_test\\.go$". Defaults to exclude none. |
| -s | (Default: false) Set to convert needed packages from Go standard library files found in "%GOROOT%\\src". |
| -r | (Default: false) Set to recursively convert source files in subdirectories when a Go source path is specified. |
| -m | (Default: false) Set to force update of pre-scan metadata. |
| -u | (Default: false) Set to only update pre-scan metadata and skip conversion operations. |
| -g | (Default: %GOPATH%\\src\\go2cs) Target path for converted Go standard library source files. |
| -c | (Default: false) Set to target legacy compatible code, e.g., block scoped namespaces. Required for code sets prior to C# 10. |
| -a | (Default: false) Set to use ANSI brace style, i.e., start brace on new line, instead of K&R / Go brace style. |
| -k | (Default: false) Set to skip check for "+build ignore" directive and attempt conversion anyway. |
| -C | (Default: false) Set to convert CGO files, i.e., skip check for \"+build cgo\" directive or import "C" and attempt conversion anyway. |
| -O | (Default: false) Set to convert Go OS targeted files, i.e., skip check for OS target file name suffixes and attempt conversion anyway. |
| -A | (Default: false) Set to convert Go architecture targeted files, i.e., skip check for architecture target file name suffixes attempt conversion anyway. |
| &#8209;&#8209;help | Display this help screen. |
| &#8209;&#8209;version | Display version information. |
| value 0 | Required. Go source path or file name to convert. |
| value 1 | Target path for converted files. If not specified, all files (except for Go standard library files) will be converted to source path. |

### Future Options

A new command line option to prefer explicit types over `var` would be handy, e.g., specifying `-x` would request explicit type definitions; otherwise, without applying setting, conversion would default to using `var` where possible.

If converted code ever gets manually updated, e.g., where a new `import` is added, a command line option that would "rescan" the imports in a project and augment the project file to make sure all the needed imports are referenced could be handy.

## C# to Go?

If you were looking to "go" in the other direction, a full _code based_ conversion from C# to Go is currently not an option. Even for the most simple projects, automating the conversion would end up being a herculean task with so many restrictions that it would likely not be worth the effort. However, for using compiled .NET code from within your Go applications you have options:

1. For newer [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/) applications, I would suggest trying the following project from Matias Insaurralde: https://github.com/matiasinsaurralde/go-dotnet -- this codes uses the [CLR Hosting API](https://blogs.msdn.microsoft.com/msdnforum/2010/07/09/use-clr4-hosting-api-to-invoke-net-assembly-from-native-c/) which allows you to directly use .NET based functions from within your Go applications.

2. For traditional .NET applications, a similar option would be to use [cgo](https://golang.org/cmd/cgo/) to fully self-host [Mono](https://www.mono-project.com/) in your Go application, see: http://www.mono-project.com/docs/advanced/embedding/.

## Background

For more background information, see [here](Background.md).
