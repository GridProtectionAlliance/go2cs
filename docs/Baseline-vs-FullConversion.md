# Baseline vs. Full Conversion — the separation contract

Companion to [`/CLAUDE.md`](../CLAUDE.md). Defines what lives where, why, and the rules that keep the
converter-improvement loop and the full-stdlib goal from colliding again.

## The three things

1. **Baseline stdlib — `src/core/<pkg>`**
   Small, **hand-finished, compiling** subset of the Go standard library. This is what the behavioral
   tests and converter-improvement loop build against. It must always stay green.

2. **Full auto-conversion — `src/go-src-converted/`** *(target location)*
   The entire Go standard library (~305 packages) auto-converted by `go2cs -stdlib`. The **ultimate
   goal**, but **work in progress — does not all compile yet**.

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
- Scoped **`src/go2cs.sln`** to the baseline + tests; added **`src/go-src-converted.sln`** for the 301 WIP
  projects.
- Result: `go2cs.sln` builds 79/79; behavioral suite green (216 tests).

## The contract (rules going forward)

1. **`src/core/<pkg>` is curated and must compile.** Treat it as hand-owned source. Do not bulk-overwrite
   it with `-stdlib` output.
2. **`src/go-src-converted/` is the full-conversion target.** All `go2cs -stdlib` runs write here via
   `-go2cspath`. It may be regenerated wholesale; nothing hand-edited lives here long-term (fixes belong in
   the converter or, for out-of-band pieces, in `golib`).
3. **`golib` is shared and never auto-generated.** Both trees reference `src/core/golib/golib.csproj`.
4. **Promotion is one-directional and deliberate:** when a full-conversion package compiles cleanly and
   matches behavior, it may be promoted into the baseline (recorded in [`Roadmap.md`](Roadmap.md)). The
   converter is never pointed at the baseline directory.

## Regenerating the full conversion

Current Go converter (authoritative flags in `src/go2cs/main.go`):

```
# Whole stdlib into the separate target:
go2cs -stdlib -go2cspath <repo>/src/go-src-converted

# Specific packages only (used when greening a closure bottom-up):
go2cs -stdlib -go2cspath <repo>/src/go-src-converted fmt strings io sort time
```

`-parallel 1..4` controls concurrency; output `.csproj` references are generated from detected imports.

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
