# Phase 3 Autonomous Loop — driver spec

> **Stable** protocol for an autonomous, self-paced loop that drives the full-stdlib conversion to a
> clean compile. This file is the *fixed* driver; the *live* queue + per-iteration kickoff lives in
> [`Phase3-Handoff.md`](Phase3-Handoff.md) (rewritten every iteration). Read [`CLAUDE.md`](../CLAUDE.md)
> and the handoff first. **Goal:** `dotnet build src/go-src-converted.sln -c Debug` → 0 errors across
> all ~305 packages, baseline (`src/go2cs.slnx`) + full behavioral suite still green.

## Shape (why a loop, not a fan-out)

The grind is **sequential** and mutates **one shared artifact** (the converter, `src/go2cs/*.go`); the
measurement (reconvert → bucket) is **global and nondeterministic (±10)**. So roots **cannot** be fixed
in parallel — they collide and attribution becomes impossible. The driver is therefore a **single
self-paced loop, one root per iteration**, with git commits + this handoff as the durable, resumable
ledger (survives compaction/restart). Parallelism is used only **inside** an iteration for (a)
adversarial verification of a candidate fix and (b) read-only bucket/exploration.

## Per-iteration protocol (the ALL-SHIPS-RISE Workflow)

1. **Build the converter fresh** — `cd src/go2cs && go build -o bin/go2cs.exe .` (NON-NEGOTIABLE: the
   runner false-greens on a stale binary).
2. **Measure** — reconvert (`-stdlib -comments -parallel 4`), overlay (incl. `src/core` manual files),
   build `runtime` (or the current frontier project), bucket by `CS####` then file/message.
3. **Pick ONE highest-impact CONTAINED root**, OR sort one CS0030/S1 site (convert native / model
   managed-referent / stub raw-metal — see *S1 is a FORK to SORT*). Escalate only the genuinely
   un-sortable (see *Stop conditions*).
4. **Fix it in the correct component** — converter / `golib` / generator. NEVER hack `golib` or a
   generator just to compile; fix the genuine root (a compile-only fix that leaves `golib`
   behaviorally wrong is a regression in disguise).
5. **Add a behavioral test** that exercises the construct (Go↔C# output parity), per CLAUDE.md
   "Adding a regression test." Verify Go output first with `go run .`.
6. **Adversarially verify** (revert-trap defense): spawn 2–3 skeptic sub-agents to independently judge
   "compiles — but is it *behaviorally* correct, or a compile-only mirage?" Default to REVERT if a
   majority can't confirm the runtime behavior is right. (S4 nil-NRE, S5 dead-box, `pallocBits`
   lost-writes all compiled green and were correctly reverted — a green compile is necessary but **not
   sufficient**.)
7. **Validate no regression** — full behavioral suite green (`run-behavioral.ps1`) AND zero unexplained
   golden churn (`check-no-regression.ps1`; classify any churn as pre-existing-stale vs my-change vs
   noise via file:line cross-reference). Re-baseline goldens ONLY for intended output changes.
8. **Record** the conversion decision in [`ConversionStrategies.md`](ConversionStrategies.md) if it's a
   new/changed emitted form.
9. **Commit** to `master` (solo project) — ONE focused commit per root. **Do NOT push** (see knobs).
10. **Closing ritual** — update the handoff (check off the item + result note, refresh count/date,
    rewrite the Next-session prompt for the next root), commit the doc.
11. **Schedule the next iteration** (self-paced; short wake while a reconvert/build runs, longer when
    genuinely idle) — unless a stop condition fired.

## S1 / CS0030 is a FORK to SORT, not a wall to stop at (2026-07-01)

The old rule "park S1 and surface to the user" is **superseded**. The milestone is a clean **COMPILE**
of the overlaid `go-src-converted` (operational correctness is Phase 4 / Go tests), and the CS0030
`unsafe.Pointer(*T)`→`uintptr` family is not one wall — it forks three ways. **Sort each occurrence:**

1. **Native-type pointer/unsafe op → CONVERT it faithfully** (converter/`golib`). Both languages are GC
   with pinning + unsafe pointers; for native types the memory ops are identical (the hand-converted
   `unsafe`/`sync/atomic` code proves the overlap). This is normal loop work.
2. **Managed-referent (`guintptr`/`muintptr`/`puintptr`/`gclinkptr`/… hiding a *managed* pointer in a
   `uintptr`) → apply the ж<T> MODEL.** Hold the `ж<T>`/`object` **directly** (Volatile/Interlocked +
   `nilCanon`), never a `nuint` round-trip — exactly like `core/sync/atomic` `atomic.Pointer<T>` and
   `reflectlite` `object? m_target`. A raw round-trip compiles-but-crashes; this modelled form does not.
   Approachable per-site; no longer an automatic stop.
3. **Raw-metal on NON-native types (memory-layout math, type-descriptor pointer-walking, `*.asm`) → STUB
   it with `[module: GoManualConversion]`.** These genuine dragons can't be faithfully transpiled; a
   hand-written equivalent or a throwing stub that **won't exist in the final build** is an acceptable
   *milestone* solution (file a review note). Don't fight the converter over them.

Only escalate to the user when a specific site resists ALL THREE — you can't tell native from
managed-referent from raw-metal, or the ж<T> model needs a design decision you can't make. See
[`Baseline-vs-FullConversion.md`](Baseline-vs-FullConversion.md) *The corrected end-state*.

## Stop conditions — escalate to the user (do NOT guess)

- **A CS0030/S1 site you cannot sort** into convert / model / stub per the fork above (genuinely
  ambiguous, or the ж<T> model needs a design call). Park that one; keep sorting the rest.
- Any root the handoff flags **REVERTED / multi-session / needs-user-model** (S4 nil-safe re-alias, S5
  pointer-to-pointer aliasing).
- **Behavioral validation impossible** for a candidate fix (e.g. inherently cross-assembly, no
  single-module repro) AND the fix is non-trivial — don't commit on a compile-only basis; surface it.
- **No-progress circuit breaker:** 2 consecutive iterations that fail to land a clean committed root
  (compile won't green, churn won't resolve, verification keeps rejecting) → STOP and report.
- **Cost ceiling reached** (per the user's budget) → pause and report.
- **Suite goes red** and can't be made green within the iteration → revert the iteration's changes,
  STOP, report.

## Boundaries

- Stay on `master`; one commit per root; **local commits only — do NOT `git push`** unless authorized.
- Don't commit `go-src-converted` regens (regenerable; restore with `git checkout HEAD --` + `git clean
  -fdq`). The unit of work is the converter/golib/generator fix.
- `getSanitizedFunctionName` / converter↔generator name agreement is the invariant — treat name-shape
  changes as radioactive.
- When `runtime` hits 0, the frontier moves UP — re-measure the now-visible upper stdlib (`bufio`/
  `bytes`/`strings`/`os`/`fmt`/`reflect`/…), append S7+ queue items to the handoff, continue.

## Effort dial

Coordinator session: **high**. Spawned tasks per root-class: mechanical → low–medium; subtle
converter/generator routing → high; adversarial verifiers → medium–high; read-only explore → low.
Reserve **xhigh** for a single genuinely-hard correctness review (via a verifier), not the whole loop.
**Not max** (wasteful over a long loop); **not medium** for the coordinator (revert-rate too high).

## Definition of done — a clean COMPILE (not operational)

`dotnet build src/go-src-converted.sln -c Debug` → **0 errors** across all ~305 packages (auto-output +
overlaid `[module: GoManualConversion]`/`*_impl.cs`/asm stubs), with the baseline (`src/go2cs.slnx`) and
the full behavioral suite still green. **This is the milestone.** Operational correctness (Go unit tests)
is Phase 4.

- **Do NOT promote `go-src-converted → core`** on a clean compile. Promotion is deferred to post-Go-test
  (Phase 4) and may not be needed; `core` stays the bootstrap stub. (Superseded the old "promote as they
  go green" rule — see [`Baseline-vs-FullConversion.md`](Baseline-vs-FullConversion.md) contract rules 4–5.)
- **Copy the hand-owned manual files BACK into `go-src-converted` religiously** — `overlay.sh` re-copies
  the `src/core` `[module: GoManualConversion]`/`*_impl.cs` files after the cs/csproj copy; the overlaid
  tree (auto + manual/asm stubs) is the real final state that must compile.
- When `runtime` hits 0 the frontier moves UP — re-measure the now-visible upper stdlib and continue.
