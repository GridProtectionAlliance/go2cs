# Baseline vs. Full Conversion — the separation contract

Companion to [`/CLAUDE.md`](../CLAUDE.md). Defines what lives where, why, and the rules that keep the
converter-improvement loop and the full-stdlib goal from colliding again.

## The three things

1. **Baseline stdlib — `src/core/<pkg>`**
   Small, **hand-finished, compiling** subset of the Go standard library. This is what the behavioral
   tests and converter-improvement loop build against. It must always stay green.

2. **Full auto-conversion — `src/go-src-converted/`** *(target location)*
   The entire Go standard library (302 packages, Go 1.23.1) auto-converted by `go2cs -stdlib`. The
   **ultimate goal — and as of 2026-07-10 all 302 packages compile clean** (commit `51ba5d9cf`, tag
   `stdlib-green-2026-07-10`; the Phase-3 milestone). Compiling, not yet operational — running Go's own
   package tests is Phase 4.

3. **Runtime — `src/core/golib/`**
   Hand-written C# runtime (`slice`, `map`, `channel`, `@string`, `builtin`, `ж<T>`, type aliases).
   **Shared by both** baseline and full conversion. **Never auto-overwritten** — some of it (`builtin`,
   `unsafe` helpers, assembly-backed routines) can never be produced by transpilation.

### Why they must stay separate

Both baseline and full emit into `namespace go` with `<pkg>_package` static partial classes. Referencing
both from one C# project produces duplicate-type collisions. So they are kept in **separate directories**
and **never referenced together** by a single project.

## How the collision happened (history)

| Commit | Date | Event |
|---|---|---|
| `9792eeea2` | 2020-07-09 | Hand-converted stub created at `src/gocore/<pkg>` (Tour-of-Go support). |
| *(many)* | 2020–2025 | Stub maintained/refined for years; it was the working library. |
| `ba6fef6c9` | 2025-03-08 | `src/gocore` renamed → `src/core` (path change only). |
| **`3426298eb`** | 2025-05-05 01:51 | **Last clean baseline.** Stub compiles; tests green. |
| `6ca1c45b7` | 2025-05-05 01:59 | "Initial standard library conversion" — full stdlib written **on top of** `src/core`, overwriting the hand-finished packages (2,359 files, +508k lines). |
| `cc14584c7` | 2025-05-11 | Full-conversion work; tagged `full-conversion-2025-05`. |
| 2026-06-25 | 2026-06-25 | **Separation restored:** full conversion relocated to `src/go-src-converted/`; old stub restored into `src/core`; converter fixes; green baseline. |

The mistake was writing the full conversion **into the same directory** as the baseline instead of a
separate one. "All 305 packages converted successfully" meant the **transpiler did not crash** — not that
the emitted C# compiles. The overwrite replaced *compiling* `fmt`/`time`/etc. with *large machine-generated*
versions, which stalled the test loop.

The project was **originally designed** with this separation (`gocore` manual subset + `go-src-converted`
full auto-output), so restoring it realigns with the original design.

## How it was resolved (2026-06-25)

- Relocated the full conversion out of `src/core` into **`src/go-src-converted/`** (a 2604-file git rename);
  rewrote inter-package `csproj` refs and `go2cs.sln` paths; added `.gitignore` rules for the Go `debug`/
  `log` packages that collide with the VS `[Dd]ebug/`/`[Ll]og/` patterns.
- **Restored the old hand-finished stub from `3426298eb` into `src/core`.** Key finding: it **compiles
  cleanly against today's `golib`** — the feared API drift did not materialize, so it gave a green baseline
  immediately. Restored 14 packages; excluded the stub `testing` (drifted, 400 errors, referenced by no test).
- Scoped **`src/go2cs.sln`** to the baseline + tests; added **`src/go-src-converted.slnx`** for the 301 WIP
  projects.
- Result: `go2cs.sln` builds 79/79; behavioral suite green (216 tests).

## The contract (rules going forward)

1. **`src/core/<pkg>` is curated and must compile.** Treat it as hand-owned source. Do not bulk-overwrite
   it with `-stdlib` output.
2. **`src/go-src-converted/` is the full-conversion target.** All `go2cs -stdlib` runs write here via
   `-go2cspath`. It may be regenerated wholesale; nothing hand-edited lives here long-term (fixes belong in
   the converter or, for out-of-band pieces, in `golib`).
3. **`golib` is shared and never auto-generated.** Both trees reference `src/core/golib/golib.csproj`.
4. **Promotion `go-src-converted → core` is DEFERRED (strategy correction, 2026-07-01).** Earlier work
   promoted packages into `core` as they went *green* (compiling). That was premature — **compiling is not
   operating.** Promotion should happen only once a package's **converted Go unit tests pass** (Phase 4),
   and may not be needed at all (see *The corrected end-state* below). Until then, `core` stays the small
   bootstrap **stub** the behavioral tests build against (chicken-and-egg — the tests need a working library
   to run, and `go-src-converted` compiling doesn't yet mean it *works*). `sync/atomic` already living in
   `core` is fine — it remains a useful stub. **Do not promote further** on the basis of a clean compile.
   The converter is never pointed at the baseline directory.
5. **The canonical MANUAL files live in `core` and are copied BACK into `go-src-converted`.** Files marked
   `[module: GoManualConversion]` (the converter skips re-converting them) and hand-written `*_impl.cs`
   files are hand-owned in `src/core/<pkg>`. For a full-conversion **milestone** to be complete, these must
   be overlaid into their matching `src/go-src-converted/<pkg>` locations — that overlaid tree (auto-output
   + manual/asm stubs) **is the real final state.** `overlay.sh` already re-copies the `src/core` manual
   files after the cs/csproj copy; during these final compiling stages, do this **religiously**.

## The corrected end-state (2026-07-01) — compile first, operate later

The **milestone** is a **clean C# COMPILE** of the whole overlaid `go-src-converted` (auto-output + the
manual/`*_impl.cs`/asm stubs) — *not* an operational one. Operational correctness is Phase 4 (converting +
passing the Go unit tests). Getting there, for `runtime`:

- **Native-type pointer/unsafe ops are convertible.** Go and C# are both GC languages with pinning and
  unsafe pointers; native types share identical memory operations. Pointer parity for native types is the
  goal and is achievable (the hand-converted `unsafe`/`sync/atomic` code proves the overlap). Fix these in
  the converter/`golib` properly.
- **Managed-referent cases have a known model.** Where Go stashes a *managed* pointer inside a `uintptr`
  (`guintptr`/`muintptr`/`puintptr`…) to hide it from the GC, the C# equivalent holds the `ж<T>`/`object`
  **directly** (Volatile/Interlocked + `nilCanon`), never a `nuint` round-trip — exactly as
  `core/sync/atomic/type.cs`'s `atomic.Pointer<T>` and `reflectlite/value.cs`'s `object? m_target` do. A
  raw `uintptr` cannot hold a managed reference across a GC (the "compiles-but-crashes" trap).
- **Raw-metal on NON-native types is the dragon — stub it.** Memory-layout math, type-descriptor
  pointer-walking, and `*.asm` cannot be faithfully transpiled. When the loop hits this wall, the file gets
  an **immediate `[module: GoManualConversion]` task / review** — a hand-written C# equivalent, or a
  throwing stub that **won't exist in the final build** — not a converter fight. A `GoManualConversion`
  stub that makes the package COMPILE is an acceptable milestone solution; the faithful hand/asm
  implementation can follow.

So the loop no longer *stops* at the S1/CS0030 "architectural wall" — it **sorts**: convert the native-type
ops, apply the managed-referent model, and stub the genuine raw-metal dragons with `GoManualConversion`.

Once the whole stdlib compiles and the converted **Go tests** pass, a versioned build can ship to **NuGet**;
at that point the chicken-and-egg is gone and `core` can be dropped (behavioral tests reference NuGet) or
replaced with prior operational `go-src-converted` source — TBD.

## Hand-owning a package to make it OPERATIONAL (Phase 4) — two patterns + the marker

Phase 3 stubbed the raw-metal dragons just to *compile*. Phase 4 (making packages *run*) needs the opposite
in places: a **faithful native reimplementation** where the literal Go→C# conversion can compile but cannot
work. The canonical case is **`sync`** (2026-07-11): its concurrency types are a state machine over the Go
**runtime sleeping semaphore** (`//go:linkname` `Semacquire`/`Semrelease`/`notifyList`/…), which is
co-designed with the mutex (starvation-mode ownership handed to one specific waiter via an exact ticket) and
**cannot be emulated on any .NET primitive** — every emulation deterministically trips `sync: inconsistent
mutex state` / `unlock of unlocked mutex` under sustained contention. The fix is to reimplement the *types*
natively on proven .NET primitives (`Mutex`→binary `SemaphoreSlim`, `WaitGroup`→counter+latch,
`RWMutex`→writer-preferring monitor lock). Expect more of this in Phase 4 (`time`, parts of `os`/`syscall`, …).

There are **two** ways a package carries hand-owned C#, and they are NOT interchangeable:

1. **`*_impl.cs` supplement — for SOME declarations in a file.** The converter emits the file normally but,
   for types/funcs listed in `manualConversionTypes` / `manualConversionFuncs` (`manualTypeOperations.go`),
   replaces the body with a `// … hand-converted … see the package's *_impl.cs` comment and a bodyless
   `partial`. A hand-written `<name>_impl.cs` companion (no matching `.go`, so a reconvert never touches it)
   supplies the real bodies. Use when only part of a converted file needs managed semantics (e.g.
   `sync/atomic`, `runtime/lock_sema`). The `*_impl.cs` file typically also carries `[module:
   GoManualConversion]` for documentation, but does not *need* it (nothing regenerates it).

2. **Whole-file replacement — for an ENTIRE file, and it REQUIRES the marker.** When the whole `<name>.cs`
   is hand-written (replacing the converted `<name>.go` output — e.g. sync's `mutex.cs`/`waitgroup.cs`/
   `rwmutex.cs`), it MUST carry `[module: GoManualConversion]`, or a `-stdlib` reconvert regenerates the Go
   version straight over it. `main.go`'s conversion loop calls `containsManualConversionMarker(<output>.cs)`
   for each `.go` file and **drops that file from the conversion set when the marker is present**
   (`directiveOperations.go`). This is the ONLY thing that makes a whole-file native reimplementation
   durable across reconverts.

   Current whole-file replacements: sync `mutex.cs` / `waitgroup.cs` / `rwmutex.cs` (2026-07-11, live in
   `go-src-converted` only); math `unsafe.cs` (2026-07-16 — Float32/64 bits/frombits as direct
   `BitConverter` bit-cast intrinsics, replacing the literal conversion's `ж<T>`/`uintptr` round-trip that
   compiles but cannot reinterpret bits at runtime; canonical in `src/core/math`, byte-identical copy in
   `go-src-converted/math`; guarded by the `MathFloatBits` behavioral test).

Marker mechanics: `[AttributeTargets.Module, AllowMultiple = true]` (golib `GoManualConversionAttribute`), so
one per file across a package is fine. The scanner wants it **before the first class**, so place it after the
`using`s and before the file-scoped namespace, written `[module: go.GoManualConversion]` (fully qualified so
it resolves without a `using go;`). **Verify** a whole-file override survives by reconverting the package into
a dir seeded with the hand-written file and confirming it stays byte-identical
(`go2cs -stdlib -go2cspath <seeded-root> <pkg>` → the marked `.cs` is untouched).

The rule from *§5* still holds: canonical hand-owned files live under `src/core/<pkg>` and are overlaid into
`src/go-src-converted/<pkg>` (`overlay.sh`) — with the marker, an overlaid whole-file override then survives
the next reconvert instead of being clobbered.

## Regenerating the full conversion

Current Go converter (authoritative flags in `src/go2cs/main.go`):

```
# Whole stdlib into the separate target:
go2cs -stdlib -comments -go2cspath <repo>/src/go-src-converted

# Specific packages only (used when greening a closure bottom-up):
go2cs -stdlib -comments -go2cspath <repo>/src/go-src-converted fmt strings io sort time
```

> **Always pass `-comments` for stdlib conversion.** It defaults off, but the converted C# is a derivative
> work — the per-file `// Copyright … The Go Authors … BSD-style license` header **must be preserved**, and
> the Go doc-comments keep the output readable. Without the flag, headers and comments are stripped.

Package conversion is sequential (it relies on package-level converter state); output `.csproj` references are generated from detected imports.

> **Note on the `<go2cspath>/core` subdir:** the stdlib converter writes packages to `<go2cspath>/core/<pkg>`
> (a hardcoded `core` subdir). To regenerate cleanly into `src/go-src-converted` you must either point
> `-go2cspath` so that subdir lands there, or convert to a temp dir and move. Don't let it overwrite the
> baseline `src/core` packages.

## The old stub as a fallback / reference

The last clean stub (`3426298eb`) is the source of today's baseline. To inspect or recover individual files:

```
git worktree add ../go2cs-stub-ref 3426298eb      # browse the last clean baseline
git show 3426298eb:src/core/fmt/print.cs          # or per file
```

## Stale tooling

- **Fixed:** `src/deploy-core.bat` (`gocore`→`core`); `docs/README.md` (banner + corrected references).
- **Still stale:** `src/convert-gosrc.cmd` / `convert-gosrc.bat` invoke a retired `net6.0` C# `go2cs.exe`
  with old flags (`-s -r -e -g`); update to the Go converter's `-stdlib -go2cspath …` form.
