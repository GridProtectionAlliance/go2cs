# Phase 3 Handoff — Drive the full standard-library conversion to compile

> **The milestone:** the entire auto-converted Go standard library in
> `src/go-src-converted/` (~305 packages, `src/go-src-converted.sln`) compiles in C# with
> **zero errors**. This is the headline goal of the whole `go2cs` project. Read
> [`CLAUDE.md`](../CLAUDE.md) first; this doc is the focused Phase-3 playbook.

## Where things stand

- **`runtime` is the foundation and the current frontier.** It has been driven from ~2769
  compile errors to **2** (deterministically — verified across multiple clean reconverts). The 2
  are a single root: the **`Δslice` type-vs-method name collision** (see *Known blocker* below).
- **"runtime is the only failing package" is misleading.** `dotnet build` **skips the dependents of
  a failed project** rather than erroring them, and `runtime` sits near the bottom of the dependency
  graph. So while `runtime` fails, the entire upper stdlib (`bufio`, `bytes`, `strings`, `os`, the
  full `fmt`, `reflect`, …) is *not being compile-checked at all*. **Expect many more defects to
  surface once `runtime` greens** — e.g. converted `bufio` is not believed to compile yet. The true
  remaining work is "the whole library," not "2 errors."
- A large body of converter / golib / generator fixes is committed (see `git log`; the
  `go2cs-phase3-progress` memory has the iteration history and per-defect rationale).

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

## Known blocker — `Δslice` (start here, it gates everything)

`runtime` declares **both** `type slice struct{…}` and `func (a *userArena) slice(…)`. Both
reserved-rename to `Δslice` (to avoid golib's `slice<T>`), so in C# the nested type `Δslice` and the
`[GoRecv]` extension method `Δslice` collide in `runtime_package` (CS0102 ×2).

**Two natural fixes were tried and both catastrophically backfire** (each was measured repeatedly):
- ᴹ-suffixing the colliding method → **~1964 unrelated errors**.
- Un-reserving `slice`/`array`/`channel`/`map` for method names → **~1986 unrelated errors**.

Both revert cleanly to 2. The cascade errors are package-wide and unrelated to slice
(`lockRank→int`, ambiguous `add`, missing `heapStats`, `ж` ctor `name`, `Ꮡmheap_ʗ1`) — the signature
of a **go2cs-gen generator failing** (it computes method names independently of the converter, so a
renamed method desyncs an interface impl → CS8785 → all of `runtime`'s generated impls/conversions
vanish → ~2000-error cascade).

**Therefore the correct fix is a generator-coordinated rename** — the *ships-rise-together* principle
in action: teach go2cs-gen's name computation (Implement/Recv/Type generators) the *same*
method-name-vs-type-name collision rename the converter applies, so they stay in sync; *then* the
converter-side method rename is safe. (Alternatively, rename the **type** via the type-name paths —
`getSanitizedIdentifier` / `getTypeName` — but verify that doesn't cascade the same way first.) First
step: do a NON-`ErrorsOnly` build of a rename attempt and confirm the `CS8785 Generator '…' failed`
warning to pin the exact generator, then fix that generator to match.

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
