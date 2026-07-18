![go2cs](images/go2cs-small.png)

# 📰 go2cs News Archive

All project announcements, newest first. The latest item is always
summarized at the top of the [README](README.md), full text kept here.

---

## July 18, 2026 — `unicode/utf16` validates; disclosed-divergence generalizes

**Phase-4 package #5.** [`unicode/utf16`](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/go-src-converted/unicode/utf16)
validates its own Go test suite in C# — **8 tests agreeing outright** against `go test -json`, plus one
honestly disclosed. The structural twin of the very first validated package (`unicode/utf8`), it round-trips
UTF-16 encode/decode with results checked by `reflect.DeepEqual` — exercised here through the converted
reflection bridge — and all eight correctness tests match verdict for verdict.

Its significance is what the ninth test demonstrates. `TestAllocationsDecode` asserts that `Decode` returns
its `[]rune` with **zero** heap allocations — a result Go reaches only through compiler escape analysis, so
the test guards itself with `testenv.SkipIfOptimizationOff`. The managed runtime provably cannot match it: a
returned `slice<rune>` is always a heap allocation, no matter how the method is written. This is the same
*allocation-model* divergence `bytes` and `strings` disclosed a day earlier — and `unicode/utf16` is the
first package to reuse the [disclosed-divergence manifest](README.md#try-it-yourself--validate-a-converted-test-suite)
as a **general tool** rather than a two-package special case. Its `go2cs_test_disclosures.json` pins one
`alloc-profile` row by exact failure signature (`"Decode allocated "`), while the separate `TestDecode`
independently proves the decoded output is correct — so the disclosure covers exactly the allocation
profile and nothing else. A mechanism that generalizes cleanly to the next package is a mechanism that was
designed right.

*Phase-4 package #5 · `unicode/utf16` 8 + 1 disclosed (alloc-profile) · reproduce from a clone via
[Try it yourself](README.md#try-it-yourself--validate-a-converted-test-suite)*

---

## July 18, 2026 — `bytes` and `strings` tests pass, with disclosed-divergence

**Two more standard-library packages validate their own Go test suites in C#** — and they arrive with a
new piece of Phase-4 machinery. [`bytes`](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/go-src-converted/bytes)
validates **81 tests** and [`strings`](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/go-src-converted/strings)
**68** against `go test -json`, bringing the count of Phase-4-validated packages to four (after
`unicode/utf8` and `sort`).

Both packages contain a handful of tests that assert an **exact allocation count** via Go's
`testing.AllocsPerRun` — for example, `strings`'s `TestBuilderAllocs` insists a `Builder` heap-allocates
*exactly once*. These are unsatisfiable by design in a managed runtime: the CLR has no malloc counter (the
shim measures allocated *bytes* instead), and .NET genuinely allocates where Go's escape analysis
stack-allocates — an addressed `var b Builder` heap-boxes per run; `string(r)` materializes a `byte[]`
where Go uses a 4-byte stack buffer. A malloc-counting shim would fail these identically; the divergence
is the allocation *model*, not the measurement.

Rather than silently skip them, go2cs now discloses them at test level. Each affected package carries a
hand-owned, repo-committed `go2cs_test_disclosures.json` — reviewed like source, never generated — that
pins `{test, divergence class, expected failure signature}`. The differential oracle reclassifies a
Go-passes/C#-fails result as **disclosed-divergent** *only* when both the test name and the pinned failure
signature match; a disclosed test that fails any *other* way is still a hard mismatch, so the pin is an
integrity guard, not a blanket exemption. The validation summary reports the reclassified rows explicitly
(`… 7 disclosed-divergent (alloc-profile), …`). Packages without a manifest — `sort` and `utf8` —
compare strictly and are wholly unaffected.

*Phase-4 packages #3 and #4 · `sort` 63/63, `bytes` 81, `strings` 68 · reproduce from a clone via
[Try it yourself](README.md#try-it-yourself--validate-a-converted-test-suite)*

---

## July 17, 2026 — Go's own tests now pass in C#

**A standard-library package's own Go test suite — converted to C# — now runs and agrees with `go test`,
verdict for verdict.** [`unicode/utf8`](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/go-src-converted/unicode/utf8)'s
real test suite (Go 1.23.1) validates **14/14** through the new converted-test pipeline: the `_test.go`
files are transpiled to C#, built against the converted standard library, executed under a Go-semantics
test host, and differentially compared against a clean `go test -json` baseline by full test name — with
every benchmark and example declaration honestly disclosed rather than silently skipped. One week after
"the whole standard library *compiles*," the answer to *"but does it **run**?"* has its first
machine-checked proof — and it's one you can
[reproduce yourself from a clone](README.md#try-it-yourself--validate-a-converted-test-suite)
(tag: `utf8-tests-green-2026-07-17`). This is the Phase 4 operational era: **real Go tests, not
compilation, are now the currency of correctness** — with `sort`, `strings`, and `bytes` next in line.

*Tag: [`utf8-tests-green-2026-07-17`](https://github.com/GridProtectionAlliance/go2cs/releases/tag/utf8-tests-green-2026-07-17)
· commit `337a928df`*

---

## July 14, 2026 — The converted Go standard library is on NuGet

**The converted Go standard library, the `golib` runtime, and the `go2cs-gen` analyzer are published to
[nuget.org](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll)** as `go.<pkg>` /
[`go.lib`](https://www.nuget.org/packages/go.lib) / [`go.gen`](https://www.nuget.org/packages/go.gen),
versioned `1.23.1.<build>` from `src/version.props`. The converter's new `-recurse=nuget` mode emits
matching `<PackageReference>` entries — defaulting `$(GoStdLibVersion)` to a floating release — so a
converted end-user app or library restores the whole go2cs stack from NuGet with **no local go2cs source
checkout**; the app's own and third-party converted packages stay project references. See
[Converting a real-world module](README.md#converting-a-real-world-module) for the end-to-end walkthrough.

*Tag: [`nuget-stdlib-2026-07-14`](https://github.com/GridProtectionAlliance/go2cs/releases/tag/nuget-stdlib-2026-07-14)
· commits `2363af0e6`, `2e15eec9d`, `dd821a556`*

---

## July 10, 2026 — The entire Go standard library compiles in .NET

**All 302 packages of the auto-converted Go standard library (Go 1.23.1) compile
cleanly as .NET assemblies — zero errors, zero exclusions.** Every package you'd expect to be hard is
in that number: `runtime`, `reflect`, `net/http`, `go/types`, `crypto/tls`, `database/sql`,
`encoding/json`. The transpiled output is not a demo subset — it is the standard library, end to end,
emitted by the converter, transpiled Go to C#, then compiled by Roslyn. NOTE: don't get _too_ excited,
this is _fully compilable_ not _fully runnable_ — that's the next phase (underway; see the July 17 item
above)! However, simple apps will run, try
[converting a real-world module](README.md#converting-a-real-world-module). Read more about this
[milestone's details](README.md#about-standard-library-compile-milestone) and
[current status](README.md#status) in the README.

*Tag: [`stdlib-green-2026-07-10`](https://github.com/GridProtectionAlliance/go2cs/releases/tag/stdlib-green-2026-07-10)
· commit `51ba5d9cf`*

---

## June 27, 2026 — The `math` package compiles clean

The full-conversion **`math` package compiles clean** — a core, widely-imported standard-library
package, and with it a major step in the Phase 3 drive to compile the whole auto-converted standard
library. The session that landed it greened nine full-conversion packages (`unicode`,
`internal/trace/event`, `unicode/utf16`, `internal/platform`, `image/color`, `runtime/internal/sys`,
`runtime/internal/math`, `math/bits`, and `math`) via 19 behaviorally-tested converter and generator
fixes. The dominant theme was comprehensive untyped-constant typing, plus shadowing fixes,
namespace-collision qualification, composite self-qualification, and relational-pattern guards.

*Tag: [`math-green-2026-06-27`](https://github.com/GridProtectionAlliance/go2cs/releases/tag/math-green-2026-06-27)
· commit `914d4bd72`*

---

## May 5, 2025 — First full standard-library auto-conversion

The rewritten Go-based converter completed its **first full standard-library auto-conversion**: the
whole Go standard library (~301 projects) converted end to end. "Converted" here meant the transpiler
did not crash and every Go source file received a corresponding C# file — not yet that the emitted C#
compiles. Driving this full conversion to a clean compile became the Phase 3 campaign, finished on
July 10, 2026 (above).

*Tag: [`full-conversion-2025-05`](https://github.com/GridProtectionAlliance/go2cs/releases/tag/full-conversion-2025-05)
(`cc14584c7`, May 11) · commit `6ca1c45b7`*

---

## January 12, 2025 — The converter is rewritten in Go ("go2cs" version 2)

**Major project restructuring** — the "go2cs iteration 2" generation begins: the converter is
re-implemented **in Go** on the official `go/ast` + `go/types` toolchain, replacing the original C#
converter built on an ANTLR4 Go grammar; T4 templates are replaced by raw string literals; and Roslyn
source generators take over the auto-generated ancillary code that supplies Go semantics at compile
time. The ANTLR4/C# converter is retired.

*Commit: `87465f5f5`*

---

## November 19, 2022 — .NET 7.0, C# 11, and UTF-8 string literals

From the ANTLR4-era converter's News:

* Project has been updated to use .NET 7.0 / C# 11.
* String literals are encoded using UTF-8 (C# `u8` string suffix) which uses the `ReadOnlySpan<byte>`
  ref struct. This should make Go strings faster since strings do not have to be converted to UTF-8
  from UTF-16. Also added an experimental
  [`sstring`](https://github.com/GridProtectionAlliance/go2cs/blob/master/src/core/golib/sstring.cs),
  a ref struct implementation of a Go string.
* Code conversions now better match original Go code styling.

*Commit: `d90f267d4`*

---

## March 13, 2022 — `v0.1.2` release

**go2cs `v0.1.2` is released** — a tagged release of the mature ANTLR4-era converter. Converted code
now targets **.NET 6.0 / C# 10**, using file-scoped namespaces and reduced indentation to better match
the original Go code's styling, with new command-line options for pre-C#-10-compatible output and ANSI
brace style, options to skip GOOS/GOARCH- and cgo-targeted files, and the ANTLR4 grammar synchronized
to the official source.

*Tag: [`v0.1.2`](https://github.com/GridProtectionAlliance/go2cs/releases/tag/v0.1.2) (`289b939db`)*

---

## January 5, 2021 — Go as a scripting language for Unity and Godot

Example usages of go2cs allow [Go](https://golang.org/ref/spec) to serve as the **scripting language
for the [Unity](https://unity.com/) and [Godot](https://godotengine.org/) game-engine platforms** —
see the [GoUnity](https://github.com/ritchiecarroll/GoUnity) and
[GodotGo](https://github.com/ritchiecarroll/GodotGo) projects. The project has also been updated to
**.NET 5.0** and supports
[publishing as a self-contained executable](https://docs.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained).

*Commits: `efb497b3a`, `e5c2d7cbc`*

---

## August 29, 2020 — First full conversion of the Go standard library (ANTLR4 era)

The initial conversion of the **full Go source library** completed without failing — the converter's first end-to-end pass over the entire standard library, committed to `src/go-src-converted`.
The warnings in that conversion's build log laid out the road map of the parsing and conversion work
remaining. Converted code at the time targeted .NET Core 3.1 / C# 8.0, and simple conversions depended
on `src/gocore` — the small, manually-converted subset of the Go library that survives today as the
curated baseline in `src/core`.

*Commit: `8e2d6e8e6`*
