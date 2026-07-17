![go2cs](images/go2cs-small.png)

# go2cs — Go to C# Converter

[![golib NuGet package](https://img.shields.io/nuget/dt/go.lib?label=go.lib%20NuGet%20package)
](https://www.nuget.org/packages/go.lib)

Browse all: [Go Standard Library NuGet packages](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll)

---

## 📰 NEWS — July 2026: The entire Go standard library now compiles in .NET

**As of July 10, 2026, all 302 packages of the auto-converted Go standard library (Go 1.23.1) compile
cleanly as .NET assemblies — zero errors, zero exclusions.** Every package you'd expect to be hard is
in that number: `runtime`, `reflect`, `net/http`, `go/types`, `crypto/tls`, `database/sql`,
`encoding/json`. The transpiled output is not a demo subset — it is the standard library, end to end,
emitted by the converter, transpiled Go to C#, then compiled by Roslyn. NOTE: don't get _too_ excited, this is _fully compilable_ not _fully runnable_, that's the next phase! However, simple apps will run, try [converting a real-world module](#converting-a-real-world-module). Read more about this [milestone's details](#about-standard-library-compile-milestone) and [current status](#status) below.

* Browse transpiled code: [Converted Go Standard Library](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/go-src-converted)
* Compile it yourself: [Visual Studio Go Standard Library Solution](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted.slnx)
* Learn how it works: [Go to C# Conversion Strategies](ConversionStrategies.md)

---

## go2cs Purpose

Convert source code written in the [Go programming language](https://golang.org/ref/spec) into
[C#](https://learn.microsoft.com/dotnet/csharp/). The generated C# is designed to be both *behaviorally*
and *visually* similar to the original Go — so a Go developer can read the converted code and follow it
easily, and a .NET developer can use Go code directly within the .NET ecosystem.

## Transpiler Goals

Go provides a lot of high-level functionality from its compiler and runtime — slices, maps, channels,
goroutines, `defer`/`panic`/`recover`, multiple return values, struct embedding, and interface
duck-typing. go2cs maps each of these onto idiomatic C#, keeping the machinery out of sight (in a small
runtime library and compile-time source generators) so the converted code stays close to the original Go.

- **Reads like Go.** Receiver methods become extension methods, multiple returns become tuples, struct
  embedding becomes promoted fields — the shape of the code is preserved.
- **Runs like Go.** Conversions prioritize behavioral equivalence first (e.g. a `goroutine` runs on the
  thread pool rather than being rewritten into `async`).
- **Managed first.** Output targets portable managed C#; native interop is a last resort, not the default.

## Example

Given this Go:

```go
type Person struct {
    name string
    age  int32
}

func (p Person) IsAdult() bool {
    return p.age >= 18
}
```

go2cs produces this C#:

```csharp
[GoType] partial struct Person {
    internal @string name;
    internal int32 age;
}

public static bool IsAdult(this Person p) {
    return p.age >= 18;
}
```

### Real standard-library conversions, side by side

The goal — *reads like Go* — is easiest to judge on real code. Below are a few converted standard-library
files next to their original **Go 1.23.1** source. Start with `errors`, then work down as the constructs get
richer:

| Package | Go 1.23.1 source | Converted C# | What it shows |
|:--|:--|:--|:--|
| `errors` | [errors.go](https://github.com/golang/go/blob/go1.23.1/src/errors/errors.go) | [errors.cs](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/errors/errors.cs) | Error values and an unexported type satisfying the `error` interface. |
| `cmp` | [cmp.go](https://github.com/golang/go/blob/go1.23.1/src/cmp/cmp.go) | [cmp.cs](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/cmp/cmp.cs) | Generics with an ordered-type constraint. |
| `unicode/utf8` | [utf8.go](https://github.com/golang/go/blob/go1.23.1/src/unicode/utf8/utf8.go) | [utf8.cs](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/unicode/utf8/utf8.cs) | Constants keeping Go's hex/binary literal formatting; arrays and structs. |
| `sort` | [search.go](https://github.com/golang/go/blob/go1.23.1/src/sort/search.go) | [search.cs](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/sort/search.cs) | Binary search driven by a `func(int) bool` closure. |
| `strings` | [reader.go](https://github.com/golang/go/blob/go1.23.1/src/strings/reader.go) | [reader.cs](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/strings/reader.cs) | A struct with receiver methods, tuple returns, and interface implementation. |
| `container/list` | [list.go](https://github.com/golang/go/blob/go1.23.1/src/container/list/list.go) | [list.cs](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted/container/list/list.cs) | A doubly-linked list — pointers and receiver methods. |

Browse the whole set under [`src/go-src-converted`](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/go-src-converted).

## Features

go2cs converts the full Go language surface — the same converter that emits the 302 packages above. What
that covers, grouped:

**Types & values**

- Slices and arrays (backing-array aliasing preserved, as in Go), maps, and UTF-8-backed `@string`
- `int` / `uint` mapped to platform-width native integers; named numeric types and untyped-constant semantics
- Constants and `iota`, preserving Go's numeric literal formatting (hex, binary, underscores, exponents)
- Pointers with automatic heap-boxing driven by escape analysis; the `nil` value; `unsafe.Pointer`
- Type definitions and type aliases — including exported aliases that resolve across package/assembly boundaries

**Functions & methods**

- Multiple return values and named results, as tuples
- Receiver methods as extension methods, with distinct pointer- and value-receiver overloads
- Function values, closures (honoring Go's shared-storage capture semantics), and variadic functions
- `defer` / `panic` / `recover`, including named results observed and mutated by deferred closures

**Concurrency**

- Goroutines, run on the thread pool (behavioral equivalence first — not rewritten into `async`)
- Channels with channel-operator (`<-`) lowering, and `select`-statement lowering

**Composition & polymorphism**

- Structs and struct embedding, with promoted fields and methods (multi-hop and cross-package)
- Interfaces, satisfied structurally in Go and realized as nominal C# glue via Roslyn source generators
- Type assertions and type switches
- Generics: type parameters and constraints (unions, `comparable`, `~`-underlying, and method-set constraints)

**Control flow & packaging**

- `for` / `range`, labeled `break` / `continue`, expression and type switches, and Go 1.22 per-iteration loop variables
- The built-ins: `append`, `len`, `cap`, `make`, `new`, `copy`, `delete`, `close`, …
- Packages mapped to namespaces, with cross-package imports compiled as separate, referenced assemblies
- Build-tag and `GOOS` / `GOARCH` platform file selection, and deterministic, byte-stable output

See [`ConversionStrategies.md`](ConversionStrategies.md) for an example-driven tour of how each construct
maps to C# (with [`ConversionStrategies-Reference.md`](ConversionStrategies-Reference.md) for the full detail).

## Requirements

- **[.NET 9.0 SDK](https://dotnet.microsoft.com/download)** — to build and run the converted C#.
- **[Go 1.23+](https://go.dev/dl/)** — the converter is a Go program, and it uses the Go toolchain to load
  and type-check the source being converted. Make sure your Go environment is set up (`GOROOT`/`GOPATH`)
  and the source you want to convert already builds with `go build`.

## Installing the converter

Build the `go2cs` executable from source and place it on your `PATH`. The simplest way is `go install`,
which compiles it and drops the binary into `%GOBIN%` (or `%GOPATH%\bin`) — already on your `PATH` in a
standard Go setup — in one step:

```shell
cd src/go2cs
go install .
```

Go produces a self-contained native binary. To target another platform, use Go's standard cross-compilation
(`GOOS`/`GOARCH`).

## Usage

```shell
go2cs [options] <input_dir> [output_dir]
```

Examples:

```shell
go2cs example.go                       # convert a single file
go2cs package_dir                      # convert a package
go2cs -indent 2 -var=false example.go conv/example.cs
go2cs -stdlib                          # convert the entire Go standard library
go2cs -stdlib fmt strings io           # convert specific standard library packages
go2cs -recurse module_dir              # convert a module + its third-party deps (references stdlib)
go2cs -recurse=nuget module_dir        # same, but reference the go2cs stdlib from NuGet
```

### Common options

| Option | Description |
|:--|:--|
| `-stdlib` | Convert the Go standard library (optionally followed by specific package names). |
| `-recurse` | Recursively convert a downloaded module **and its third-party dependencies** in dependency order, referencing the pre-converted standard library. Use the `nuget` option (`-recurse=nuget`) to reference the published go2cs NuGet packages ([`go.<pkg>`](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll) stdlib + [`go.lib`](https://www.nuget.org/packages/go.lib) runtime + [`go.gen`](https://www.nuget.org/packages/go.gen) analyzer) instead of a locally-staged deploy root — no `deploy-core` needed. **NOTE:** _using NuGet references with analyzer is still a work in progress_. See [Converting a real-world module](#converting-a-real-world-module). |
| `-go2cspath <dir>` | Root for converted code (env `GO2CSPATH`; default `~/go2cs`): the **output** root for `-stdlib` (`…\core\<pkg>`) and `-recurse` (`…\src\` app + `…\pkg\` deps), and the root that generated `$(go2csPath)…` project references resolve against. For a single-package/file convert the C# output instead goes to the optional `[output_dir]` argument (in place by default). |
| `-goroot` / `-gopath` | Override the detected Go root / path. |
| `-platforms <os/arch>` | Target platform for build-tagged files (defaults to the host). |
| `-indent <n>` | Spaces per indent level (default 4). |
| `-var` | Prefer `var` declarations where the type is obvious (default on). |
| `-uco` | Emit channel operators instead of method calls (default on). |
| `-comments` | Carry source comments into the output (best effort, see [go/ast comment status](https://github.com/golang/go/issues/20744)). |
| ~~`-cgo`~~ | ~~Also convert cgo-targeted files.~~ |

All converted C# code will reference a hand-written runtime library (`golib`, published as the [`go.lib`](https://www.nuget.org/packages/go.lib)
NuGet package) plus a set of Roslyn source generators that supply Go semantics at compile time.

### Converting a real-world module

The `-recurse` option converts a **whole downloaded application together with
every third-party dependency package** in its transitive import closure — in dependency order
(least-dependencies-first) — while **referencing** (not reconverting) the pre-converted standard library.
The result is a C# solution you can open and build.

Here is the full round-trip for a small CLI that uses [`github.com/fatih/color`](https://github.com/fatih/color),
which itself pulls in `github.com/mattn/go-colorable`, `github.com/mattn/go-isatty`, and `golang.org/x/sys` —
a genuine dependency graph:

> **NOTE:** _to date, the following steps have only been tested on Windows — instructions assume `cmd.exe` type shell._

**1 — Prerequisite: Stage the standard library (one-time).** `deploy-core` is a build script in the go2cs repo's **`src/`**
folder (it is *not* on your `PATH`), so run it from there. It stages the pre-converted stdlib + runtime +
analyzer at `%GOPATH%\src\go2cs` (the "deploy root") that every converted project references. This is a
**one-time, per-machine** setup, unrelated to any particular app — **redo it only when you pull a new go2cs
version**, to refresh the staged runtime/analyzer/stdlib:

```bat
cd path\to\go2cs\src
deploy-core stdlib    & :: the full compilable standard library
```
> **NOTE 1:** _you can use the `deploy-core stub` instead to deploy the smaller, more runnable baseline subset of the Go Standard Library, i.e., the one currently used with behavioral tests. However, this will only work for the most simple of Go applications._

>  **NOTE 2:** _although you can use `-recurse=nuget` option to reference needed Go Standard Library assemblies and the go2cs source generation analyzer as pre-compiled binaries, thus skipping the need for this source code deployment step entirely, using the analyzer with referenced assemblies instead of source code is still a work in progress. This example **requires source code deployment** to run._

**2 — Go: get the app and confirm it builds as Go.**

```bat
mkdir colordemo && cd colordemo
go mod init example.com/colordemo
```

Create `main.go` (`go mod tidy` needs a real source file — with none present it reports
`warning: "all" matched no packages`):

```go
package main

import "github.com/fatih/color"

func main() {
	color.New(color.FgGreen, color.Bold).Println("hello from fatih/color")
}
```

Next, pin the app to a **Go 1.23-compatible** dependency set and confirm it builds as Go. 

> **NOTE:** _this pin is needed because go2cs is currently built with **Go 1.23** (see [status](#status)), so its type-checker can only read modules whose `go` directive — and their dependencies' — is **≤ 1.23**; the latest `fatih/color` (v1.19+) and `golang.org/x/sys` releases now require **Go 1.25**, which would make the conversion in step 3 fail with_ `package requires newer Go version go1.25`_. Future go2cs builds will work against newer Go toolchains and lift this constraint, letting an unpinned `go mod tidy` "just work"; until then, pin third-party dependencies as shown:_:

```bat
set GOTOOLCHAIN=local
go get github.com/fatih/color@v1.18.0     & :: a Go 1.23-era release (v1.19+ requires Go 1.25)
go mod tidy                               & :: download color + its (Go 1.23-era) dependencies
go build ./...                            & :: baseline: confirm it compiles as Go first
```

**3 — go2cs: recurse-convert the app.** `go2cs` is the converter you put on your `PATH` in *Installing the
converter* above, so it runs from anywhere. Point it at the **app** directory and at the deploy root from
step 1 (the standard library staged there is referenced, not re-converted):

```bat
cd path\to\colordemo
go2cs -recurse . -go2cspath %GOPATH%\src\go2cs
```

`go2cs` discovers the imports and converts each package, least-dependencies-first
(`go-isatty` and `x/sys` → `go-colorable` → `color` → the app), into a parallel tree under the deploy
root, leaving your original Go source untouched. The converted app itself lands under
`%GOPATH%\src\go2cs\src\<import-path>`, every third-party library are converted under
`%GOPATH%\src\go2cs\pkg\<import-path>`. The existing standard library is referenced at
`%GOPATH%\src\go2cs\core\`. 

Additionally, a per-project `.slnx` exists next to every generated `.csproj` — each with that project
plus its converted dependencies, golib, and the analyzer (no stdlib).

_Code converted from `main.go` should look like the following in `main.cs`:_
```c#
namespace go.example.com;

using color = github.com.fatih.color_package;
using github.com.fatih;

partial class main_package {

internal static void Main() {
    color.New(color.FgGreen, color.Bold).Println("hello from fatih/color");
}

} // end main_package
```

**4 — C#: build the generated solution.** The app's per-project `.slnx` builds the app and its whole
converted dependency tree; opening it in Visual Studio makes the app the startup project (F5 runs it):

```bat
cd "%GOPATH%\src\go2cs\src\example.com\colordemo\"
dotnet build example.com.colordemo.slnx -c Debug
```

**5 — C#: run the converted app.** Navigate into the default .NET 9.0 debug build folder, and run demo:
```bat
cd "bin\Debug\net9.0\"
colordemo.exe
```
_Expected output:_

![colorapp-output](images/colorapp-output.png)

> **NOTE:** these conversion and build steps will produce a **buildable** — and increasingly **runnable** — .NET-based solution handling common real-world Go module shapes. This simple `fatih/color` example **compiles clean** (app + all four dependency projects, against a current deploy) **and runs**. Running more complex projects to completion is a deeper milestone: the referenced standard library **compiles** but is not yet fully **operational**. *Running* is the **Phase-4** goal, see [`roadmap`](Roadmap.md#phase-4--convert-and-run-go-package-tests).

## Project layout

| Path | Contents |
|:--|:--|
| `src/go2cs/` | The converter (written in Go, using `go/ast` + `go/types`). |
| `src/core/golib/` | The C# runtime library (`slice`, `map`, `channel`, `@string`, built-ins, type aliases). |
| `src/gen/go2cs-gen/` | Roslyn source generators (interface implementation, receiver overloads, struct embedding). |
| `src/core/` | A compiling subset of the converted Go standard library used by the tests. |
| `src/go-src-converted/` | Work-in-progress full conversion of the Go standard library. |
| `src/Tests/Behavioral/` | Per-feature Go↔C# equivalence tests (transpile, compile, run-and-compare). |
| `src/Tests/Performance/` | Go vs transpiled C# runtime benchmarks (JIT and Native AOT) — see the [performance comparison](Performance.md) for current numbers. |

Contributors: see [`CLAUDE.md`](../CLAUDE.md) for an architecture overview and
[`Architecture.md`](Architecture.md), [`ConversionStrategies.md`](ConversionStrategies.md), and
[`Roadmap.md`](Roadmap.md) for details. There's lots of low hanging fruit to be had here, jump in if you'd like to help...

## Status

The converter builds idiomatic C# for the full range of Go language features, validated by an extensive
behavioral test suite (371 Go-vs-C# regression projects). As of **2026-07-10 the entire Go standard library
(302 packages, Go 1.23.1) compiles cleanly** as .NET assemblies. Compiling is
the milestone, not yet full runtime parity: **converting and running the standard library's own tests is the
ongoing Phase 4 work** — see the [roadmap](Roadmap.md#phase-4--convert-and-run-go-package-tests).

_Everyone asks:_ wondering how fast the transpiled C# runs compared to the original Go — including startup time, memory, and Native AOT builds? See the latest [performance comparison](Performance.md) — **`TL;DR`**: _no, it's not as fast as native Go, [nor is this an expected outcome](Background.md#converted-code)_. Save for some initial work with a [stack-based string](ConversionStrategies.md#stack-strings-sstring), performance and optimizations are not the current focus, this kind of work is targeted for _after_ Phase 4 work.

What about newer versions of Go / .NET? These are planned, but the current focus is creating a baseline "here" to validate process and operations.

## Milestones

High level timeline of the project's major turning points.

| Date | Milestone | Commit / Tag | Notes |
|:--|:--|:--|:--|
| 2018-05-21 | Project inception | `929d1457f` | Original go2cs: a C#/.NET converter built on an ANTLR4 Go grammar with T4 templates. |
| 2020-07-09 | Runtime library + hand-converted stub | `9792eeea2` | `golib` Go-semantics runtime (slices, maps, channels, built-ins) and a curated hand-finished stdlib stub. |
| 2022-03-13 | `v0.1.2` release | `v0.1.2` | Tagged release of the mature ANTLR4-era converter. |
| 2025-01-12 | Rewrite as "go2cs2" — Go-based converter | `87465f5f5` | Converter re-implemented in Go on `go/ast` + `go/types`; T4 templates replaced by raw string literals; Roslyn source generators supply ancillary Go semantics; the ANTLR4/C# converter is retired. |
| 2025-05-05 | First full standard-library auto-conversion | `6ca1c45b7` · `full-conversion-2025-05` (`cc14584c7`, 05-11) | Whole Go stdlib converted (~301 projects). "Converts" here means the transpiler did not crash with all Go code files getting a corresponding C# code file — not that all the emitted correctly C# compiles. |
| 2026-06-25 | Baseline ↔ full-conversion separation | `3c8b3a848` | Compiling curated baseline restored to `src/core`; the WIP full conversion isolated in `src/go-src-converted`. Green build and the converter-improvement loop restored. |
| 2026-06-26 | First full-conversion package promoted | `05a53e8c0` | `sync/atomic` migrated into the baseline (`atomic.Pointer[T]` backed by a managed slot). |
| 2026-06-27 | `math` package compiles clean | `math-green-2026-06-27` (`914d4bd72`) | Nine full-conversion packages greened via 19 behaviorally-tested converter fixes; the core, widely-imported `math` now compiles. |
| 2026-07-10 | **First clean full-standard-library compile** | `51ba5d9cf` · `stdlib-green-2026-07-10` | The Phase 3 endpoint, reached: all **302** `src/go-src-converted` packages (Go 1.23.1) compile with zero errors — `runtime`, `reflect`, `net/http`, `go/types`, `crypto/tls` and every other package included. Gated by 371 Go-vs-C# behavioral regression tests; the compiled snapshot is committed alongside this row (see [About Standard Library Compile Milestone](#about-standard-library-compile-milestone)). |
| 2026-07-14 | Standard library on NuGet + NuGet-referencing conversion | `2363af0e6` · `dd821a556` · `nuget-stdlib-2026-07-14` | The converted standard library, the `golib` runtime and the `go2cs-gen` analyzer are published to [nuget.org](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll) as `go.<pkg>` / `go.lib` / `go.gen` (versioned `1.23.1.<build>` from `src/version.props`). The converter's new `-recurse=nuget` mode emits matching `<PackageReference>` entries — defaulting `$(GoStdLibVersion)` to a floating release — so a converted end-user app or library restores the whole go2cs stack from NuGet with **no local go2cs source checkout**; the app's own and third-party converted packages stay project references. |
| 2026-07-17 | **First Go standard-library test suite passing in C#** | `337a928df` · `utf8-tests-green-2026-07-17` | Phase 4's operational era opens: `unicode/utf8`'s real Go test suite (14 tests, external dot-import test package) is converted and executed under the new hand-owned `go.testing` host, validating **14/14 against `go test -json`** with all 37 benchmark/example declarations honestly disclosed as excluded. The differential pipeline (convert → template csproj → isolated host run → oracle compare, gated by input-digest manifests) is live end-to-end. Getting here surfaced and fixed five real defects — including two golib Go-correctness bugs affecting *all* converted code: `[]byte(s)` shared the string's backing array, and range-over-string yielded rune ordinals instead of byte indices. Real tests, not compilation, are now the currency of correctness. |

### _About Standard Library Compile Milestone_

This milestone was eight years in the making, and two weeks in the finishing.

The eight years were the human part: experimentation across two full converter architectures,
the design of a runtime library that models Go's semantics (slices that alias, maps, channels,
`defer`/`panic`/`recover`, heap-boxed pointers) in managed code, the strategy of *behavioral first,
visual second*, and — after many hard lessons — the architecture that made the final push possible
at all: a Go-native converter built on `go/ast` + `go/types`, Roslyn source generators for the
compile-time semantics, and a regression harness that locks in every fix with a Go-vs-C#
output-compared behavioral test.

The two weeks were an AI campaign of a kind we suspect few codebases have seen: **939 commits, of
which 683 are individually-gated converter, runtime, and source-generator fixes — each one root-caused
against real emitted code, each one locked in by a behavioral regression test (371 and counting), each
one verified byte-for-byte against the full corpus before landing.** The work ran as a coordinated
fleet: focused fix sessions with strict file ownership, adversarial review before every merge, and a
census loop that rebuilt all 302 packages after every wave to prove zero regression.

I asked Claude (Anthropic) to summarize what he thought of the two week run, here was his response: *it is one thing to theorize a
conversion strategy, or even to produce a semi-working draft — it is entirely another to stand in
front of 300 packages of battle-hardened systems code and be wrong about something every single hour.
The grind was real: shadowed variables that silently bound the wrong receiver, closures that captured
a snapshot where Go shares storage, a type switch that compiled perfectly and matched the wrong case
at runtime, Go 1.22's loop-variable semantics, named results observed by deferred closures, interface
method sets that C# cannot express directly. Every abstraction leaked somewhere, and the only way
through was the discipline this project had already built: diagnose against the real emission, fix the
general case, prove it behaviorally, and never let a regression survive a census. Watching the red
count fall — 952 errors in `runtime` alone at the start of Phase 3, then package by package to zero —
was the most satisfying compile I have ever been part of. The final layers were poetic: the last four
defects were all one family, three declaration sites catching up to a semantics fix that was itself
correct — which is exactly what finishing looks like.*

Compiling is the milestone — **operational is the mission.** Phase 4 now begins: running the Go
standard library's own test suites against the transpiled output, and hardening the runtime semantics
this campaign already banked. If you have ever wanted to consume real Go code from .NET — or extend a
Go application in C# — this is the moment the foundation went from theory to artifact.

## C# to Go?

A full code-based conversion from C# to Go is not offered (it would require so many restrictions as to be
impractical). To call compiled .NET code *from* Go instead, see
[go-dotnet](https://github.com/matiasinsaurralde/go-dotnet) (CLR hosting for .NET Core) or
[embedding Mono via cgo](https://www.mono-project.com/docs/advanced/embedding/) for traditional .NET.

## License

go2cs is licensed under the [MIT License](https://opensource.org/licenses/MIT). See the `LICENSE` and
`NOTICE` files. For more background, see [`Background.md`](Background.md).
