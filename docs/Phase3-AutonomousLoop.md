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
3. **Pick ONE highest-impact CONTAINED root** (see *Stop conditions* — skip architectural ones).
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

## Stop conditions — escalate to the user (do NOT guess)

- **Architectural / managed-referent roots.** S1 (CS0030 ~45: `guintptr`/`muintptr`/`puintptr`/
  `gclinkptr`/`lfstack` storing a managed pointer as `uintptr`) MUST be done **with the user's
  managed-referent model** (hold `ж<T>` directly — the promoted `atomic.Pointer<T>` play), a dedicated
  multi-session redesign. A raw round-trip compiles-but-crashes. **Park it; surface to the user.**
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

## Definition of done

`dotnet build src/go-src-converted.sln -c Debug` → **0 errors** across all ~305 packages, with the
baseline (`src/go2cs.slnx`) and the full behavioral suite still green. Promote packages into the
baseline (`src/core/<pkg>`) as they go green and stabilize. Then stop and stand by — **the milestone**.
