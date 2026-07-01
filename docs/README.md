![go2cs](images/go2cs-small.png)

# go2cs — Go to C# Converter

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

## Features

Converted constructs include:

- Slices, arrays, maps, and strings (UTF-8 backed)
- Channels and goroutines
- `defer` / `panic` / `recover`
- Multiple return values and named results
- Structs, struct embedding (field promotion), and interface implementation
- Generics (Go 1.18+ type parameters and constraints)
- Pointers, type assertions, type switches, `iota`, and the built-ins (`append`, `len`, `cap`, `make`, …)

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
| `-parallel <1-4>` | Number of packages to convert in parallel. |
| `-platforms <os/arch>` | Target platform for build-tagged files (defaults to the host). |
| `-indent <n>` | Spaces per indent level (default 4). |
| `-var` | Prefer `var` declarations where the type is obvious (default on). |
| `-uco` | Emit channel operators instead of method calls (default on). |
| `-comments` | Carry source comments into the output (best effort, see [go/ast comment status](https://github.com/golang/go/issues/20744)). |
| `-cgo` | Also convert cgo-targeted files. |

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
| `src/Examples/` | Sample conversions. |

Contributors: see [`CLAUDE.md`](../CLAUDE.md) for an architecture overview and
[`Architecture.md`](Architecture.md), [`ConversionStrategies.md`](ConversionStrategies.md), and
[`Roadmap.md`](Roadmap.md) for details.

## Status

The converter builds idiomatic C# for the full range of Go language features, validated by an extensive
behavioral test suite. A curated subset of the Go standard library converts and compiles today; converting
the *entire* standard library cleanly is ongoing work — see the [roadmap](Roadmap.md).

## Milestones

High level timeline of the project's major turning points. Full detail lives in the git history, the
[roadmap](Roadmap.md), and [`CLAUDE.md`](../CLAUDE.md).

| Date | Milestone | Commit / Tag | Notes |
|:--|:--|:--|:--|
| 2018-05-21 | Project inception | `929d1457f` | Original go2cs: a C#/.NET converter built on an ANTLR4 Go grammar with T4 templates. |
| 2020-07-09 | Runtime library + hand-converted stub | `9792eeea2` | `golib` Go-semantics runtime (slices, maps, channels, built-ins) and a curated hand-finished stdlib stub. |
| 2022-03-13 | `v0.1.2` release | `v0.1.2` | Tagged release of the mature ANTLR4-era converter. |
| 2025-01-12 | Rewrite as "go2cs2" — Go-based converter | `87465f5f5` | Converter re-implemented in Go on `go/ast` + `go/types`; T4 templates replaced by raw string literals; Roslyn source generators supply ancillary Go semantics; the ANTLR4/C# converter is retired. |
| 2025-05-05 | First full standard-library auto-conversion | `6ca1c45b7` · `full-conversion-2025-05` (`cc14584c7`, 05-11) | Whole Go stdlib transpiled (~301 projects). "Converts" here means the transpiler did not crash — not that the emitted C# compiles. |
| 2026-06-25 | Baseline ↔ full-conversion separation | `3c8b3a848` | Compiling curated baseline restored to `src/core`; the WIP full conversion isolated in `src/go-src-converted`. Green build and the converter-improvement loop restored. |
| 2026-06-26 | First full-conversion package promoted | `05a53e8c0` | `sync/atomic` migrated into the baseline (`atomic.Pointer[T]` backed by a managed slot). |
| 2026-06-27 | `math` package compiles clean | `math-green-2026-06-27` (`914d4bd72`) | Nine full-conversion packages greened via 19 behaviorally-tested converter fixes; the core, widely-imported `math` now compiles. |
| 2026-06-25 → ongoing | Phase 3 compile grind (`runtime` 952 → 138) | `f3713df61` … | Iterative, test-locked converter fixes drive the full stdlib toward a clean compile; `runtime` — the root of the dependency graph — is down from 952 errors to 138. |
| _pending_ | **First clean full-standard-library compile** | _(tag TBD)_ | The Phase 3 endpoint: every `src/go-src-converted` package compiles. Not yet reached — this row will be filled in when the milestone lands. |

## C# to Go?

A full code-based conversion from C# to Go is not offered (it would require so many restrictions as to be
impractical). To call compiled .NET code *from* Go instead, see
[go-dotnet](https://github.com/matiasinsaurralde/go-dotnet) (CLR hosting for .NET Core) or
[embedding Mono via cgo](https://www.mono-project.com/docs/advanced/embedding/) for traditional .NET.

## License

go2cs is licensed under the [MIT License](https://opensource.org/licenses/MIT). See the `LICENSE` and
`NOTICE` files. For more background, see [`Background.md`](Background.md).
