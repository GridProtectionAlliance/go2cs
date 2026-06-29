# Phase 3 Handoff — Drive the full standard-library conversion to compile

> **The milestone:** the entire auto-converted Go standard library in
> `src/go-src-converted/` (~305 packages, `src/go-src-converted.sln`) compiles in C# with
> **zero errors**. This is the headline goal of the whole `go2cs` project. Read
> [`CLAUDE.md`](../CLAUDE.md) first; this doc is the focused Phase-3 playbook.

## Where things stand

- **`runtime` is the foundation and the current frontier.** As of 2026-06-28 it is at **~1357**
  compile errors and dropping (8 converter root fixes landed this session). It is the bottom of the
  dependency graph, so it gates the entire upper stdlib.
- **The "2 errors" in older notes was a MEASUREMENT ARTIFACT — it has been resolved.** The
  `Δslice` type-vs-method collision (CS0102 ×2) was a **declaration conflict in the all-encompassing
  `partial class runtime_package`, which suppressed member-body semantic analysis for the whole
  class** — masking ~1960 *real* latent errors. Fixing `Δslice` (commit `1d7ecaf41`: type-side `ᴛ`
  suffix so the type `Δsliceᴛ` and method `Δslice` no longer collide) *unmasked* them. The earlier
  "generator desync → CS8785 cascade" hypothesis was **wrong**: there is no CS8785 and all generated
  companions are present. So the cascade-on-rename was simply unmasking, and the type-rename is the
  correct keystone — **the documented `Δslice` blocker is solved.**
- **"runtime is the only failing package" is misleading.** `dotnet build` **skips the dependents of
  a failed project** rather than erroring them. So while `runtime` fails, the entire upper stdlib
  (`bufio`, `bytes`, `strings`, `os`, the full `fmt`, `reflect`, …) is *not being compile-checked at
  all*. The true remaining work is "the whole library."
- A large body of converter / golib / generator fixes is committed (see `git log`; the
  `go2cs-phase3-progress` memory has the iteration history and per-defect rationale). The
  2026-06-28 session landed 8 root fixes (Δslice keystone, promoted-field deref, sparse-array
  defined-int key cast, nested pointer-assignment LHS `.val`, boxed-global atomic-field via
  &-machinery, nested boxed-global field address `.of()` chain, named-string-literal conversion via
  `@string`), each with a regression test; runtime 2769 → 1357.

## Core principle — ALL SHIPS RISE TOGETHER

The goal is **correct, idiomatic conversion**, not "make the C# compile by any means." Three
components must work in tandem and each is first-class:

1. **`go2cs`** (the converter, `src/go2cs/*.go`) — emits the C#.
2. **`golib`** (the runtime library, `src/core/golib/`) — hand-written Go semantics.
3. **`go2cs-gen`** (the Roslyn source generators, `src/gen/go2cs-gen/`) — compile-time Go semantics.

**Do NOT hack or work around `golib` or the generators just to make converted output compile.** When
a package fails, find the *root cause* and fix it in whichever component is actually wrong:
- If the converter emits wrong/uncompilable C# → fix the converter.
- If `golib` is missing a method, has wrong semantics, or a wrong constraint → fix `golib` properly
  (so the emitted Go-idiomatic call is *correct*, not merely compiling).
- If a generator computes the wrong name/shape/accessibility → fix the generator.

The converter producing correct C# is the *ultimate* goal, but `golib` and the generators are not
scaffolding to be bent around — they are the target runtime and must be made genuinely correct. A
"fix" that makes `go2cs` output compile while leaving `golib` behaviorally wrong is a regression in
disguise. **All three rise together to the finish.**

## Workflow (per defect — non-negotiable)

1. **Measure** with the loop (below); bucket errors by frequency; pick the highest-impact *root* defect.
2. **Fix** it in the correct component (converter / golib / generator).
3. **Add a behavioral test** that exercises the construct — extend an existing `src/Tests/Behavioral/*`
   project if one fits, else add a new one. Follow the CLAUDE.md *"Adding a regression test"* steps
   (scaffold + `go.mod` + `.csproj` + register in `src/go2cs.slnx` + `UpdateTestTargets --createTargetFiles`).
4. **Validate no regression** — full behavioral suite green AND zero golden churn (re-transpile all
   behavioral dirs; byte-identical `.cs` ⟹ no regression). Re-baseline goldens only for *intended*
   output changes.
5. **Record the conversion decision** in [`ConversionStrategies.md`](ConversionStrategies.md) if it's
   a new/changed emitted form (per CLAUDE.md).
6. **Commit** (directly to `master` — solo project). One focused commit per root fix.
7. Rinse and repeat until the full library compiles.

## The measurement loop (foundation-up)

Because dependents are skipped, work **bottom-up**: get the lowest packages green first so the next
layer becomes measurable. Concretely:

```bash
# 1. Reconvert the whole stdlib (ALWAYS -comments; license headers are required).
#    Use -parallel 1 for a DETERMINISTIC result when chasing a specific package; -parallel 4 is
#    faster (~3.5 min) for broad sweeps. (Per-file work is sub-second; cost is the type graph load.)
bin/go2cs.exe -stdlib -comments -parallel 1 -go2cspath scratchpad/recon   # writes scratchpad/recon/core/<pkg>

# 2. Overlay fresh .cs + regenerated .csproj onto src/go-src-converted (keeps golib shared in core).
bash scratchpad/overlay.sh scratchpad/recon/core      # recreate overlay.sh from the measurement-loop memory

# 3. Build a package (deps build first; a failed dep's dependents are SKIPPED):
dotnet build src/go-src-converted/<pkg>/<pkg>.csproj -c Debug -clp:ErrorsOnly
#    or the whole solution: dotnet build src/go-src-converted.sln -c Debug -clp:ErrorsOnly

# 4. Bucket by error code, then by message/file, to find the highest-frequency ROOT defect.
```

**Metric = packages-compiling, not raw error count.** Fixing a file-inclusion or a foundational
defect often *raises* the error count by un-skipping dependents that then surface their own latent
bugs — that is progress.

## The `Δslice` blocker is SOLVED — and the real lesson

`runtime` declared **both** `type slice struct{…}` and `func (a *userArena) slice(…)`. Both
reserved-renamed to `Δslice`, colliding as a nested type + a `[GoRecv]` extension method (CS0102 ×2).
**The fix (commit `1d7ecaf41`):** when a collision name is also golib-reserved, the type-side
collision-avoidance (`getCollisionAvoidanceIdentifier`) appends the `ᴛ` type marker, so the TYPE
becomes `Δsliceᴛ` while the METHOD stays `Δslice`. Only the type is renamed — the method, its call
sites, and the go2cs-gen-generated overload are untouched, so the converter and generators stay in
sync (no need to coordinate the generators after all).

**The important lesson for future blockers:** the old notes claimed renaming `slice` "catastrophically
backfires to ~1964 unrelated errors / generator desync (CS8785)". That diagnosis was **wrong**. There
is no CS8785 and all generated companions are present. The ~1964 errors are **real latent defects**
that the CS0102 was *masking*: a declaration conflict in the single `partial class runtime_package`
makes Roslyn suppress member-body semantic analysis for the entire class. So "fixing Δslice explodes
the error count" was *unmasking*, i.e. **progress** (per the packages-compiling metric), not a
cascade. When a foundational fix raises the count, inspect a sample of the "new" errors — if they are
genuine converter defects unrelated to the fix (as `ᏑtΔ2` = `&t` on an already-pointer local was
here), they were masked, not caused.

## Current frontier — grind runtime's now-visible defects

Re-bucket a fresh reconvert (`build_rt*.log`) and attack the highest-frequency *root*. As of the
2026-06-28 handoff the open buckets are (see the `go2cs-phase3-progress` memory for specifics):
- **CS0103 ~183** — `Ꮡmheap_ʗ1`/`Ꮡtraceʗ1` (closure-captured boxed global — hard); `gpΔ1`
  defer-closure shadow-rename mismatch (lambda param `gp` but body refs renamed `gpΔ1`).
- **CS1929 ~128** — `atomic.Uint32.Load()` on a VALUE (internal/runtime/atomic asm-stub
  pointer-receiver methods not routed through the box); `.slice()` on `ж<array<…>>`.
- **CS1503 ~110** — scattered `uint`↔`nint`↔`int` call-argument coercions.
- **CS0030 ~92** — `unsafe.Pointer → ж<T>` needs the `(uintptr)` intermediate for a standalone
  `(*T)(p)` whose source is `unsafe.Pointer` (a 4-line convCallExpr fix was prototyped — extend the
  isPointerCast path — but the unsafe.Pointer area has interacting gaps that make a clean behavioral
  test hard; the round-trip is a known runtime limitation).
- **CS0266 ~35** — narrow-integer arithmetic promotes to `int` (`it.i = i+1` on uint8;
  `return c+('a'-'A')`; **`&^=` on a narrow/unsigned LHS** needs `flags &= unchecked((uint8)~X)` —
  characterized in memory, blocked only by the `operator` string being written at ~5 sites that each
  need the matching `unchecked(…)` close-paren).
- CS1510 ~81, CS1059 ~81, CS0121 ~54, CS0021 ~43.

The remaining roots are progressively harder (closure-capture, internal/runtime/atomic method
routing, scattered numeric coercion). Prefer **contained** roots; verify churn carefully; do NOT
grind risky numeric/multi-site edits under fatigue (it has caused regressions before).

## Gotchas (these cost real time — see CLAUDE.md + memory for more)

- **Overlay file locks.** A killed/stray `dotnet`/`testhost` can leave a corrupt `unsafe.dll`
  ("PE image doesn't contain managed metadata", CS0009) or a `Permission denied` on a `.cs` during
  overlay, producing phantom cascades. Clear stale processes, `dotnet build-server shutdown`,
  `rm -rf` the package's `obj/bin/Generated`, re-overlay, rebuild.
- **Stale/partial overlays mask real errors.** A boxing bug in `iface.cs` was hidden for several
  measurements by a partial overlay. Always re-overlay cleanly and clean `obj/bin/Generated` before
  trusting a count.
- **`getSanitizedFunctionName` is radioactive** — any change cascades through the generators (see
  blocker). Treat converter↔generator name agreement as the invariant.
- **Full `dotnet test` testhost hangs** intermittently in long sessions (environmental). Validate via
  the baseline solution build + filtered behavioral batches (e.g. `--filter "FullyQualifiedName~CheckA…"`)
  + the zero-churn re-transpile check. A fresh session/machine clears the hang.
- **Commit messages:** use `git commit -F - <<'EOF'` heredocs — `-m "…"` with `$()`/backticks/parens
  gets mangled by the shell.

## Definition of done

`dotnet build src/go-src-converted.sln -c Debug` reports **0 errors** across all ~305 packages, with
the green baseline (`src/go2cs.slnx`) and the full behavioral suite still passing. Promote packages
into the baseline (`src/core/<pkg>`) as they go green and stabilize (see CLAUDE.md). Then stop and
stand by — this is THE milestone.
