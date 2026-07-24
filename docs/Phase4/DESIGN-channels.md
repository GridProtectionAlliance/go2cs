# DESIGN — golib channels: real rendezvous, cap/len, single-fire select, uniform-random choice

> **Status: IMPLEMENTED-pending-integration (2026-07-24, branch `claude/wave3-channels`).** Design
> blessed by the user; Units 1 and 2 are implemented as specified in §3: **Unit 1** =
> `d637e9c31` (ChanCore/selectgo golib rewrite + SelectOp registration + hardened pending slot +
> unbuffered-make converter flip + gen-template de-clamp + ThreadPool floor + the 7 new behavioral
> guards), **Unit 2** = `4281074fc` (default-form `trySelect` ordinal
> lowering + the 5 default-form golden re-baselines + these doc updates). Unit 3 (waiter pooling /
> lock tuning) remains deferred until profiled. **Adversarial verification round (post-gating):**
> semantics skeptic clean; protocol skeptic findings fixed on the branch — `e9b9d80ab` (MAJOR:
> the pending receive commit is a per-thread frame STACK, so a select nested in the winning
> guard's target expression cannot destroy the outer commit; guard `NestedSelectRecvTarget`,
> §3 amendment below), `64ec36bad` (racy `SendIsReady`/`ReceiveIsReady` probe surface deleted),
> `8fcfda655` (`channel.Wait` plain timed wait, no per-call SemaphoreSlim). Gates run on the branch: CNR drift = exactly the
> intended ctor-flip/default-form re-baselines (every line inspected), full behavioral suite
> 464/464 with Output 434/0 after each unit; corpus + banked-package re-validation results are in
> the branch report. Coordinator re-gates all-ships-rise at integration before landing on master.
> Produced 2026-07-24 by an adversarial design panel (three independent design lenses + a critic
> that verified claims against the real goldens and golib source), synthesized by the campaign
> coordinator. Companion to
> [`Phase4-Autonomous-Loop-Charter.md`](Phase4-Autonomous-Loop-Charter.md) Tier-0 item 1.

## 1. The four gaps (all confirmed in `src/core/golib/channel.cs`)

1. **No real unbuffered rendezvous** — `make(chan T)` behaves as `make(chan T, 1)`: a sender
   completes with no receiver present.
2. **cap/len conflation** — the same modeling makes `cap()` report 1 (not 0) and `len()` wrong for
   unbuffered channels; `make(chan T)` and `make(chan T,1)` emit byte-identical C#
   (`new channel<T>(1)`), so no runtime information can separate them.
3. **Blocking select is not single-fire** — send cases commit eagerly via `Sending()` /
   `ProcessSendQueue` background sends during argument evaluation, so *every* send case whose
   channel ever becomes ready delivers a phantom value (Go commits exactly one case).
4. **Ready-case choice is not uniform-random** — `select(params WaitHandle[])` resolves via
   `WaitHandle.WaitAny` (deterministic lowest-index); Go picks uniformly at random among ready cases.

Also fixed by this design (verified latent bugs): the comma-ok **closed-before-drain** bug
(`Receive(bool _)` / `Received(out,out)` return `(zero, false)` even when buffered data remains —
Go drains first), and the shared `ManualResetEventSlim` lost-wakeup/Reset races (self-healing only
via 200 ms re-polls). The `WaitHandle.WaitAny` 64-case limit dies with the plumbing.

**Consumers blocked on this work:** encoding/base32 + base64 (io.Pipe), bufio, os/signal, sync,
time (timers), net.

## 2. Panel summary

| Lens | Verdict |
|---|---|
| **D1 — ".NET-native"** (honestly rejected `System.Threading.Channels`; converged on an hchan+selectgo port behind byte-identical select glyphs) | **Strongest.** Same faithful core as D2, but preserves the emitted select contract byte-for-byte for the blocking form. Two repairable holes found (pending-slot leak on send-case wins; non-green staging). |
| **D2 — Go-faithful** (hchan + selectgo, plus a `visitSelectStmt` emission rewrite) | Right algorithm, wrong integration cost: the emission rewrite discards ~366 lines of battle-hardened select-emitter machinery and re-baselines 6 behavioral + 25 corpus select files for zero semantic gain. Its runtime-routine specs are the most precise — mined for the synthesis. Its close spec had a CAS-claim hole (fixed in synthesis). |
| **D3 — minimal-delta** | **Rejected (fatal).** Its select() polls readiness but leaves recv commits in the case guards → under concurrency a blocking select can silently execute ZERO cases (Go requires exactly one). Its WaitAny(200ms)+spin-counter→fatal(DeadLock) kills slow-but-live selects (os/signal, `time.After(5s)`). Preserves the comma-ok and lost-wakeup bugs. Dual substrate = nothing-throwaway violation. |

Key facts the panel established (verified against code/goldens):
- `System.Threading.Channels` **cannot** express Go channels: no capacity-0 rendezvous (min bounded
  capacity is 1 — exactly today's conflation), no multi-channel select, no atomic single-commit
  spanning SEND cases, no uniform-random ready choice. A custom monitor-based core is required.
- Goroutines are **synchronous ThreadPool work items** (`goǃ` → `QueueUserWorkItem`), so parked
  channel ops block pool threads. True rendezvous parks more of them than today.
- No golden contains `WaitHandle`: flipping the select-registration return types
  (`WaitHandle` → `SelectOp`) is **invisible to overload resolution at every emitted call site** —
  the blocking-select goldens stay byte-identical.
- **Staging trap (verified):** landing rendezvous *before* the select rework regresses the legacy
  `Sending()` path (losing send cases park background threads forever on a now-truly-unbuffered
  channel and later deliver phantom values) — `SelectStatement`'s fibonacci select goes red
  mid-stage. **Rendezvous + select rework must land as ONE gated unit.**

## 3. The synthesized design (recommended)

**Core (from D1, specified to D2's precision):** replace `channel<T>` internals with a single-field
struct over `ChanCore<T>`:
- Monitor lock object (`hchan.lock`), circular `T[] buf` (null when `dataqsiz == 0`),
  `sendx/recvx/qcount`, `closed`, intrusive `recvq`/`sendq` waiter queues, monotonic
  `Interlocked`-incremented `Id` (the total lock order for select).
- `Waiter` (sudog analog): boxed elem slot, ok flag, `SemaphoreSlim(0,1)` park, `SelectState sel`,
  `opIndex`. `SelectState`: `int winner = -1` claimed via `Interlocked.CompareExchange` (the
  single-fire authority) + a shared park semaphore.
- `chansend`/`chanrecv`/`closechan` follow Go's routines exactly, including the buffered-full
  parked-sender head-take/tail-enqueue rotation and the **drain-before-zero comma-ok fix**.
  **Every waker — plain send, plain recv, select commit, AND close — claims a select waiter via the
  winner CAS or skips it**; close never touches a waiter it failed to claim (the hole the critic
  found in D2's close spec).
- `cap() = dataqsiz`, `len() = qcount`, `IsUnbuffered = dataqsiz == 0` — gaps 1+2 by construction.
- Publish-before-signal discipline (set value/ok before `Release`); park = unlock **then** wait,
  never hold the channel lock across a park.

**Select (selectgo port, emitted TEXT unchanged for the blocking form):** registration methods
return a type-erased `SelectOp` descriptor instead of `WaitHandle`; `select(params SelectOp[])`:
partition out nil channels (never registered — Go semantics), `Id`-sorted `Monitor.Enter` over
distinct cores, Fisher-Yates pollorder scan (thread-local RNG), **commit exactly one ready op under
the held locks** (gaps 3+4 for the blocking form), else park one `SelectState`-linked waiter per
case; on wake re-lock, unregister losers. The committed recv value crosses to the unchanged
`case N when ch.ꟷᐳ(out v):` guard via a `[ThreadStatic]` pending slot, **hardened**: stash ONLY
recv commits, explicitly clear on send-case wins (a select can have send and recv cases on the SAME
channel — `SelectStatement` does), guards consume unconditionally, debug-assert the slot is empty on
`select()` entry. **[Amended by the adversarial verification round:** the slot is a per-thread
pending-frame STACK popped by channel-core match — the guard's out-argument target expression is
evaluated BEFORE the guard call, and legal Go can run another select there
(`case a[f()] = <-ch:` where `f()` selects), which destroyed a single slot (outer value lost or the
next buffered value stolen). Frames push/pop balanced across nesting, so the clear-on-send-win and
assert-empty-on-entry hardenings above are superseded (both are destructive in a nested context);
the accepted bounded residual — an exception unwinding between commit and consume strands an inert
frame — is documented in `SelectPending` with a Debug-only depth-growth warning. Guard:
`NestedSelectRecvTarget`.**]**

**Emission/generator footprint (the entire visible change):**
- Converter: `convCallExpr.go` unbuffered-make default literal `"1"` → `"0"` (covers plain and named
  channels). Golden churn: ~21 constructor literal lines across 8 behavioral projects (verified);
  `make(chan T, 1)` sites correctly stay `(1)`. Corpus sites regenerate on reconvert.
- go2cs-gen `IChannelTypeTemplate.cs`: 3 forwarder return types + remove the `size < 1 ? 1 : size`
  clamp (named channels can finally be unbuffered).
- golib: the rewrite above; the `Sending`/`Receiving`/`ProcessSendQueue`/`WaitHandle` plumbing is
  deleted with the rework.
- **No change** to the emitted select/send/recv shapes for the blocking form — blocking-select
  goldens stay byte-identical, and the battle-hardened `visitSelectStmt` emitter is untouched.

**Staged landing:**
- **Unit 1 (ONE gated commit):** ChanCore + chansend/chanrecv/closechan + SelectOp/selectgo +
  hardened pending slot + ctor-accepts-0 + the make-default converter flip + gen-template changes +
  a `ThreadPool.SetMinThreads` floor at golib module init. (Rendezvous and selectgo cannot be split —
  the verified staging trap.) Gate: CNR (expect exactly the ~21 ctor-literal flips, zero
  select-golden drift), `UpdateTestTargets --createTargetFiles` AFTER the CNR re-transpile, full
  behavioral suite, corpus recompile (gen change ⇒ suite + corpus per standing rule), re-validate
  all validated Phase-4 packages.
- **Unit 2 (follow-up):** default-form (non-blocking) select uniform-randomness — the one thing
  provably impossible against an ordered C# `switch`: route the default form through a non-blocking
  `trySelect` returning an ordinal; re-baselines the 3 default-form goldens. Until it lands, the
  divergence is documented and fairness-only (the default form is already single-fire because C#
  evaluates ordered guards until the first true).
- **Unit 3 (deferred until profiled):** waiter pooling / SpinLock tuning.

**Test plan (new behavioral projects, each output-compared vs `go run` unless noted):**
`ChannelRendezvous` (send blocks until receive; cap==0/len==0), `ChannelCapLen` (buffered fill/
drain), `SelectSingleFire` (two send-ready cases; drain both channels after; exactly one delivery —
deterministic), `SelectSendRecvMix` (only the committed op mutates state), `SelectRandomFairness`
(N iterations of a 2-ready blocking select; both branches taken — tolerance-bounded, not a stdout
golden), `CloseWakesBlockedSenders`/`Receivers` + `CloseDuringBlockedSelect` in both directions
(parked select-send → panic on wake; parked select-recv → zero,false), `NilChannelInSelect`,
comma-ok drain-after-close on a buffered channel, unbuffered named channel (exercises the
de-clamped template). Operational proof: drive encoding/base32+base64, bufio, os/signal, sync, time
through the `-tests` pipeline; re-validate all banked packages 0-fail.

## 4. Documented divergences / notes

- **Blocked goroutines hold pool threads.** True rendezvous increases simultaneously-parked pool
  threads; `ThreadPool.SetMinThreads` floor is a mitigation, not a fix. Programs with thousands of
  blocked goroutines remain out of reach until a cooperative scheduler exists (explicitly out of
  scope here). Documented divergence.
- **Deadlock detection** stays the existing nil/all-nil approximation; a genuinely deadlocked
  all-real-channel program now parks forever (more Go-correct than the old accidental escape).
- **NuGet lockstep:** golib signatures and the gen template change together — `go.lib` and `go.gen`
  must version-bump in the same release or `-recurse=nuget` apps can restore mismatched pairs.

## 5. Decision requested (user)

1. **Bless the synthesized design** (hchan+selectgo core behind unchanged blocking-form emission)?
2. **Unit 2 timing** — implement immediately after Unit 1 (recommended; small, completes gap 4), or
   defer with the documented fairness-only divergence?
3. **Accept the pool-thread divergence note** as the standing position on goroutine scheduling?

Coordinator recommendation: bless as specified; Unit 1 as one gated commit implemented by a
top-tier agent (or the coordinator) with adversarial review on the park/claim paths; Unit 2
immediately after as its own gated commit.
