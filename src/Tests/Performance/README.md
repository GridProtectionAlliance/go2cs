# Go vs transpiled C# — runtime performance comparison

A small, targeted benchmark suite answering the question people ask first about go2cs: **"how fast is
the transpiled C# compared to the original Go?"** — including startup time and memory, and including
C# both on the normal JIT runtime and compiled with **Native AOT** (self-contained executables with
faster startup and lower memory, the closest deployment analog to a Go binary).

This is deliberately *not* an exhaustive benchmark game. Each benchmark is a tiny Go program (same
shape as the [behavioral tests](https://github.com/GridProtectionAlliance/go2cs/tree/master/src/Tests/Behavioral)) chosen to exercise one Go construct whose C# emulation
has a real cost model — slices, strings, maps, channels, interface dispatch — plus raw compute loops
where the two runtimes should be close. Results below give the "common expected" range of differences.

## The benchmarks

| Benchmark | What it exercises |
|---|---|
| **Startup** | Empty workload: pure process start + runtime init + one `fmt` round-trip. Wall time. |
| **Fib** | Recursive Fibonacci (`fib(34)` ×5): function-call and integer-op overhead. |
| **Sieve** | Sieve of Eratosthenes to 10M ×3: slice allocation, indexing, tight loops (`slice<T>` bounds/header emulation). |
| **MatMul** | 256×256 `float64` matrix multiply ×4: floating-point throughput, nested slice-of-slice access. |
| **String** | 10M iterations of byte-slice append → `string` conversion, indexing, concatenation (`@string` emulation). |
| **StringView** | 20M iterations of keyword checks `string(buf) == "null"/"true"/"false"` over a fixed buffer — the idiom the converter's stack-string (`sstring`) emission optimizes: a zero-copy view compared against a `u8` literal span, no per-comparison allocation. |
| **Map** | 2M inserts + 2M comma-ok lookups + 1M deletes on `map[int]int` (`map<K,V>` emulation). |
| **Sort** | `sort.Ints` on 2M deterministic pseudo-random ints (`sort.Interface` dispatch through the runtime's reflection-bound `Interface<T>`). |
| **Channel** | 1M ints producer→consumer through a buffered channel with one goroutine (`channel<T>` + goroutine scheduling emulation). |

Every benchmark prints a deterministic **checksum** (verified byte-identical across Go, C# JIT, and
C# AOT before anything is measured) plus its own workload time measured in-program via
`time.Now().UnixNano()` — so the headline numbers exclude process startup, which is reported
separately by the Startup row.

## Methodology / fairness notes

- **Three variants of the identical program:** the Go binary (`go build`, default optimized), the
  transpiled C# built `Release` framework-dependent (JIT column), and the same C# published with
  `PublishAot=true` self-contained, partial trim (Native AOT column).
- **Median of 5 runs** (configurable), after 1 discarded warmup run per variant; single-shot process
  executions, the way a Go CLI program actually runs. For the JIT column this deliberately *includes*
  in-process tiered-JIT warmup inside the workload — that is the honest cost of running a transpiled
  program once. Long-running server workloads would look better for the JIT than these numbers.
- **Peak memory** is the process peak working set, polled while the process runs.
- **Wall time and workload time are both captured**; tables report workload time (Startup row: wall).
- Benchmarks avoid nondeterminism (no `math/rand`; xorshift/LCG inline generators) so outputs are
  byte-comparable, and print timing on a filtered `elapsed_ns:` line.

## Running it

```powershell
cd src/Tests/Performance
./run-performance.ps1                    # full run: transpile, build (incl. AOT), verify, measure
./run-performance.ps1 --no-aot           # much faster while iterating (skips 8 AOT publishes)
./run-performance.ps1 --filter Map       # one benchmark
./run-performance.ps1 --runs 10 --update-readme   # refresh the results block below
```

Requirements: Go toolchain, .NET 9 SDK, and for the AOT column the MSVC C++ build tools (Visual
Studio 2022 with "Desktop development with C++" — the ILC native linker needs `link.exe`).

The runner (`PerformanceRunner`) is a dependency-free console app, structured like the behavioral
suite's `BehavioralRunner`: **Transpile → Build → Verify → Measure**. The Verify phase runs all three
binaries and requires identical (timing-filtered) stdout before any timing is recorded, so the table
can never silently report a benchmark that computes something different in C#.

## Results

<!-- PERF-RESULTS:BEGIN -->

**Environment:** 13th Gen Intel(R) Core(TM) i9-13900K · Microsoft Windows 10.0.26200 · go1.23.1 · .NET SDK 9.0.315 · 2026-07-02

C# builds: JIT = framework-dependent `Release`; Native AOT = `-p:PublishAot=true` self-contained, partial trim. Median of 5 runs (1 discarded warmup). Workload time is measured in-program and excludes process startup; the Startup row is pure process wall time. Ratios are relative to Go.

**Execution time** (milliseconds -- lower is better):

| Benchmark | Go | C# (JIT) | C# (Native AOT) |
|---|---:|---:|---:|
| Startup | 12.5 | 34.1 (2.73×) | 15.3 (1.23×) |
| Fib | 81.6 | 101.3 (1.24×) | 88.1 (1.08×) |
| Sieve | 73.1 | 98.2 (1.34×) | 135.7 (1.86×) |
| MatMul | 54.5 | 139.8 (2.57×) | 201.7 (3.70×) |
| String | 70.2 | 491.2 (7.00×) | 619.5 (8.83×) |
| StringView † | 7.5 | 37.3 (4.99×) | 34.2 (4.58×) |
| Map | 277.6 | 233.7 (0.84×) | 81.8 (0.29×) |
| Sort | 113.1 | 384.7 (3.40×) | 409.0 (3.62×) |
| Channel | 46.9 | 150.2 (3.20×) | 102.1 (2.18×) |

**Peak memory** (working set, MB -- lower is better):

| Benchmark | Go | C# (JIT) | C# (Native AOT) |
|---|---:|---:|---:|
| Startup | 2.5 | 16.3 | 2.5 |
| Fib | 5.5 | 17.9 | 10.6 |
| Sieve | 35.5 | 38.6 | 29.9 |
| MatMul | 10.5 | 26.4 | 16.8 |
| String | 5.4 | 40.4 | 28.8 |
| StringView † | 5.4 | 21.6 | 10.7 |
| Map | 158.7 | 138.6 | 128.3 |
| Sort | 21.9 | 42.5 | 28.5 |
| Channel | 5.4 | 39.8 | 10.8 |

† StringView was measured separately on 2026-07-12 (the other rows are the 2026-07-02 batch); its ratios are the meaningful figure. A full `--update-readme` on a cool machine will fold it into a single consistent batch.

<!-- PERF-RESULTS:END -->

### Reading the results

What the numbers above actually show, and why:

- **Startup:** Go wins cold process start against the JIT ~2× (runtime load + JIT-on-the-fly), and
  **Native AOT erases the gap entirely** (within a few percent of Go, at a few MB of memory). This is
  the deployment story for CLI-shaped transpiled programs — and also why C# can *appear* faster in
  casual test-harness timing comparisons that measure warm processes doing trivial work.
- **Function calls / integers (Fib):** the closest workload — the transpiled C# is within ~10–25% of Go.
- **Slices & floats (Sieve, MatMul):** 1.3–2.5×; the gap is `slice<T>` header emulation and bounds
  checks the JIT can't always elide, compounded on nested `[][]float64` access. Note **AOT is *slower*
  than the JIT here** — ILC lacks the JIT's dynamic PGO / OSR loop optimizations, so AOT trades tight-
  loop throughput for its startup and memory wins.
- **String:** the biggest honest gap (~7–9×): every `[]byte`→`string` round-trip is an allocation +
  copy through the `@string` emulation, plus the per-call `append` chain Go inlines to a few
  instructions. (Already down from ~11–14× — this suite caught a per-`append` array allocation and a
  single-element slow path in `golib`; it remains the number to watch when optimizing `@string`.) The
  String benchmark's conversions are all **ineligible** for the stack-string optimization (its `s` is a
  concat operand and its buffer is mutated), so they stay `@string` — see StringView for the eligible case.
- **StringView (~4.5–5×):** the same `[]byte`→`string` cost, but for the subset the converter can prove
  non-escaping and used only in safe reads/comparisons — where it emits a zero-copy stack string
  (`sstring`) instead of `@string` (see [ConversionStrategies-Reference](https://github.com/GridProtectionAlliance/go2cs/blob/master/docs/ConversionStrategies-Reference.md)).
  Both runtimes already stack-allocate `@string`'s byte[] here, so the win is eliminating the per-comparison
  **copy** and the **literal allocation** (`@string == "…"u8` materializes the literal every time; `sstring`
  compares spans in place): in isolation the eligible comparison runs **~12× faster than `@string` on the
  JIT and ~11× on Native AOT**. Closer to Go than String, and notably AOT ≈ JIT (unlike most rows). It is
  the number to watch as the eligibility surface widens.
- **Map:** the transpiled C# is *faster than Go* — `map<K,V>` rides .NET's heavily-optimized
  `Dictionary`, and the AOT build is ~3× faster than Go on this insert/lookup/delete churn.
- **Sort (~3.5×):** the runtime's `sort.Interface` shim (`Interface<T>`) binds `Len`/`Less`/`Swap` via
  reflection-created delegates — cached, but a delegate hop per comparison.
- **Channel (~2.5–3.5×):** `channel<T>` + goroutine emulation over managed threading vs Go's runtime
  scheduler.

### History

When the toolchain moves (e.g. .NET 9 → .NET 10), copy the current results block into this section
with its environment line before re-running `--update-readme`, so version-over-version comparisons
accumulate here.

*(no history yet — first captured on .NET 9)*
