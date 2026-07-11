![go2cs](images/go2cs-small.png)

# go2cs — Go to C# Converter

## 📰 NEWS — July 2026: The entire Go standard library now compiles in .NET

**As of July 10, 2026, all 302 packages of the auto-converted Go standard library (Go 1.23.1) compile
cleanly as .NET assemblies — zero errors, zero exclusions.** Every package you'd expect to be hard is
in that number: `runtime`, `reflect`, `net/http`, `go/types`, `crypto/tls`, `database/sql`,
`encoding/json`. The transpiled output is not a demo subset — it is the standard library, end to end,
emitted by the converter, transpiled Go to C#, then compiled by Roslyn. NOTE: don't get _too_ excited, this is _fully compilable_ not _fully runnable_, that's the next phase! See [About Standard Library Compile Milestone](#about-standard-library-compile-milestone) for more details.

* Browse transpiled code: [Converted Go Standard Library](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/go-src-converted)
* Compile it yourself: [Visual Studio Go Standard Library Solution](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/go-src-converted.slnx)

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

Build the `go2cs` executable from source and place it on your `PATH` (e.g. in `%GOBIN%` or `%GOPATH%\bin`):

```shell
cd src/go2cs
go build -o go2cs .
```

Go produces a self-contained native binary. To target another platform, use Go's standard cross-compilation
(`GOOS`/`GOARCH`); matching per-platform [profiles](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/go2cs/profiles)
are included.

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
```

### Common options

| Option | Description |
|:--|:--|
| `-stdlib` | Convert the Go standard library (optionally followed by specific package names). |
| `-go2cspath <dir>` | Output root for converted standard-library code (defaults to `~/go2cs`). |
| `-goroot` / `-gopath` | Override the detected Go root / path. |
| `-platforms <os/arch>` | Target platform for build-tagged files (defaults to the host). |
| `-indent <n>` | Spaces per indent level (default 4). |
| `-var` | Prefer `var` declarations where the type is obvious (default on). |
| `-uco` | Emit channel operators instead of method calls (default on). |
| `-comments` | Carry source comments into the output (best effort, see [go/ast comment status](https://github.com/golang/go/issues/20744)). |
| ~~`-cgo`~~ | ~~Also convert cgo-targeted files.~~ |

The converted C# references a small hand-written runtime library (`golib`, published as the **`go.lib`**
NuGet package) plus a set of Roslyn source generators that supply Go semantics at compile time.

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
[`Roadmap.md`](Roadmap.md) for details.

## Status

The converter builds idiomatic C# for the full range of Go language features, validated by an extensive
behavioral test suite (371 Go-vs-C# regression projects). As of **2026-07-10 the entire Go standard library
(302 packages, Go 1.23.1) compiles cleanly** as .NET assemblies. Compiling is
the milestone, not yet full runtime parity: **converting and running the standard library's own tests is the
ongoing Phase 4 work** — see the [roadmap](Roadmap.md).

Wondering how fast the transpiled C# runs compared to the original Go — including startup time, memory,
and Native AOT builds? See the latest [performance comparison](Performance.md). Note that performance and optimizations are not the current focus, this kind of work is targeted for _after_ Phase 4 work.

## Milestones

High level timeline of the project's major turning points. Full detail lives in the git history, the
[roadmap](Roadmap.md), and [`CLAUDE.md`](../CLAUDE.md).

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
