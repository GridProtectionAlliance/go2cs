# Phase 4 autonomous campaign — charter

> Operating charter for the Phase-4 standard-library test-validation campaign. **Cleaned up
> 2026-07-21** to serve as the durable baseline for the autonomous campaign (the earlier verbatim
> loop-prompt is folded into §1–§6 below). Authoritative alongside `H:\Projects\go2cs\CLAUDE.md` and
> the docs it points to. The running per-package ledger, blockers, and hard-won detail live in the
> **memory index** (`[[go2cs-phase4-operational]]` and the other `go2cs-*` notes) — consult it every
> session and keep it current.

---

## 1. Mission

Advance Phase 4: convert and **RUN** Go's own `_test.go` suites for the standard-library packages in
`src/go-src-converted`, validating each against `go test -json` through the pipeline

```
src/go2cs/bin/go2cs.exe -tests -test-action all -test-timeout 10m "<GOROOT>/src/<pkg>" src/go-src-converted/<pkg>
```

(converts → builds the .NET test host → runs it → diffs vs `go test -json`), until every viable
package's **Test functions** pass in C#.

**State (2026-07-21): 32 packages validate** (`origin/master c9a1cf0a5`, public). The current list is
whatever has a committed `*.tests.csproj` under `src/go-src-converted` — derive it fresh, don't trust
a remembered count.

**Completion shape.** A package is DONE when its `func Test*` set matches `go test`. `Example` and
`Benchmark` declarations are **uniformly Phase-4D-deferred** (excluded corpus-wide) — never chase
them, and never let a package's Example/Benchmark count make it look "incomplete."

---

## 2. Prime directive — CORRECT FIRST, ALWAYS

The mission is **not** to finish this loop; it is to advance go2cs's long-term vision correctly.
Completion is the goal, but *correct* is the mission — **do it the right way even when it takes
longer**, in alignment with the project's defined goals (the two end-user use cases, the
nothing-throwaway principle, "reads like Go / runs like Go"). Weigh every decision against those
goals, never against reaching the end of the loop.

- **Fix at the RIGHT layer and at the ROOT.** A converter / golib / go2cs-gen change over a one-off
  hand-patch; a real root cause over a workaround; the reproducible-from-repo result over a
  deploy-only hack.
- **If the correct fix is a REWORK, a REFACTOR, or a LONGER path — take it.** A shorter path that
  leaves a latent defect, a hack, or a narrow special-case is the WRONG path even when it "works"
  today. Rework now beats refactor-under-duress later.
- **A package that "validates" via a shortcut is a FAILURE, not a win** — faking or trimming output,
  silently skipping a real test, hand-editing generated code, or a disclosed-divergence used to
  paper over a genuine bug. Do not count it.
- **Never trade a project goal for loop progress.** If a fix would compromise the architecture or a
  stated goal, don't take it: find the right fix, or surface the decision (§10).

---

## 3. Work order — eat the frog, then highest-value, then the slog

Do the **hardest, most-foundational items first**, then the items that unblock the most tests, then
the long tail. The foundational frogs are *also* the highest-leverage work — one shared-machinery
capability unblocks whole classes of packages, so paying that cost early avoids re-deriving it 40
times in the slog. **Verify branch/blocker state fresh each session** — remembered branch names and
"still-owed" claims go stale (all 10 "pending" branches once turned out already superseded).

### Tier 0 — foundational capability frogs (hardest × highest-leverage; do these first)

1. **Channels / goroutine rendezvous** *(golib, possibly runtime).* FOUR known gaps: no real
   *unbuffered* rendezvous; `make(chan T)` is conflated with `make(chan T,1)` (wrong cap/len);
   a blocking `select` fires *every* case rather than only a ready one; `select` first-match is not
   randomized. **Blocks: encoding/base32, encoding/base64, bufio, os/signal, sync, net/*, and every
   goroutine/select/timer test** — the single highest-leverage frog. A partial select-send fix may
   exist on a branch; assess it (git-cherry + content) or redo it right. This is a delicate concurrency
   redesign → **use adversarial review (§7) and design WITH the user (§10).**

2. **Reflection completeness** *(golib `GoReflect` + the hand-owned `internal/abi` bridge + `reflect`
   internals).* The 2026-07-21 csv fix landed the *comparability* half (`synthType.Equal` signal). Still
   needed: `reflect.Value.Call` / dynamic invocation (testing/quick), `Value.Set/Field/Index/MakeSlice/
   MakeMap` round-trips (encoding/binary, encoding/gob), and the `getcallersp` golib stub (errors TestAs
   → `reflect.mustBeAssignableSlow`). See `docs/Phase4/DESIGN-reflection-bridge.md`. **Blocks:
   encoding/binary, encoding/gob·json·xml, testing/quick, errors, and any reflect-driven package.**
   Deep + architectural → design WITH the user; adversarial-review the design before building.

3. **os / filesystem** *(golib `os` operational).* `os.Open` + file reads (testdata). **Blocks: strconv
   (TestFp reads testdata), errors (synth `*fs.PathError`), and every testdata-reading test.** (os/exec
   child-spawning already works on master.) High value, moderate depth.

4. **math/big** *(189 Test funcs; heavy deps: encoding/gob·json·xml, testing/quick, os/exec,
   crypto/rand).* The single largest arc — multi-session, gates `go/constant` and "all math library
   tests." Treat as its own campaign; per the operating model it may warrant a **chip (user-owned)**
   rather than a sub-agent. Depends on Tier-0 #2 (reflect) landing first.

### Tier 1 — high-value single-blocker unblockers (each nearly validating)

Each is one named blocker from validating. Fix the blocker at the right layer, then validate + bank.

- **errors** — `getcallersp` stub (Tier-0 #2) + a `multiError` interface-equality case (the csv fix
  already greened most `TestIs`).
- **unicode** (+ rebanks utf8/path) — **"Change C": black-box-only REFERENCE-model test-project
  generation** — emit a `ProjectReference` to the production `csproj` instead of recompiling the
  production `.cs` into the test assembly, resolving cross-assembly type identity (`SpecialCase`
  CS0012). Detailed, proven plan on branch **`claude/wf-unicode`** (host `testing.Benchmark` API +
  capability recognition already committed there). This is **abstract** — it helps *every* black-box-only
  external-test package, not just unicode. Gate strictly on `internal==nil`; fall back to the recompile
  model when the external test contributes production-anchored `GoImplement` records.
- **text/tabwriter** — bug 3 (empty formatted output); its other two bugs are fixed on master.
- **encoding/base32, encoding/base64** — the channel / `io.Pipe` gaps (Tier-0 #1).
- **mime** — a disclosed-alloc candidate (`TestLookupMallocs`); confirm it is provably-unsatisfiable
  before disclosing (§ gates).
- **hash/crc32** — a separate host-startup crash (`internal/cpu`/race).
- **compile-blocked cluster** — go/token, internal/fmtsort, crypto/subtle, internal/abi,
  go/doc/comment (CS0012/CS1503/CS0246 missing-type / accessibility). Worth a single batched look —
  likely one or two shared converter/gen root causes.

### Tier 2 — the campaign slog (the long tail, ~170 packages)

Per iteration: **scout with the isolation pipeline sweep** (the cheapest scout — running `-tests` on a
candidate IS its characterization: validates immediately vs a specific blocker). Prefer packages whose
dependencies already validate. Fix the tractable ones at the right layer; for one needing a Tier-0
capability, record the blocker in the memory ledger and move on — don't spin.

---

## 4. Per-iteration workflow

1. **Choose** the next unvalidated package (deps-already-validate first); consult `docs/Roadmap.md`
   (Phase 4), `docs/Phase4/*`, and the memory ledger for ordering/known blockers.
2. **Run** the `-tests -test-action all` pipeline (§1).
3. **Root-cause each failure against the REAL emitted `.cs` and runtime behavior — never assume.**
   Determine the correct layer (converter emission / golib runtime / go2cs-gen / hand-owned `_impl` /
   a genuinely-unsatisfiable-in-managed-runtime case warranting a **signature-pinned** disclosed-
   divergence). Fix it there.
4. **Lock every fix in** with a behavioral guard test (CLAUDE.md's regression-test steps) and update
   `docs/ConversionStrategies(-Reference).md` in the same change.
5. **GATE before landing (§5).** Then RE-VALIDATE the target on the post-change tree, and confirm
   **every already-validated package still validates.**
6. On a clean validation, follow the validated-package commit policy: commit the converted C# test
   sources into `src/go-src-converted/<pkg>` (Go headers intact). Commit gpg-signed to master
   (solo-project convention). **Batch NEWS/README/Milestone** — do NOT touch them per package; wait
   for a notable cross-section (`[[go2cs-doc-update-cadence]]`).
7. Move to the next package.

---

## 5. Prove it — the mandatory gates (ALL SHIPS RISE)

A fix to shared machinery (converter, golib, go2cs-gen, the test host) must improve **all** converted
code, never just the package under test. A fix that greens one package but regresses or degrades
others is **rejected** — rework it until it lifts everything. **Compiling ≠ correct.** For each change
class, the gate is non-negotiable:

| Change | Gate |
|---|---|
| **Converter** (`src/go2cs/*.go`) | `check-no-regression.ps1` **byte-identical** (except intended, individually-justified golden re-baselines) — the authoritative drift instrument; it re-transpiles unconditionally. Then reconvert + build the **302-package corpus** (`.Value`→`.ValueSlot`-style type-safe swaps aside, a converter change *can* break corpus compile). |
| **golib** (`src/core/golib`) | full behavioral suite (compile + run, `run-behavioral.ps1`) — golib is linked by everything. |
| **go2cs-gen** | full behavioral suite **and** the 302-corpus build. |
| **Any of the above** | **Operational re-validation of every already-validated package** on the post-change tree — the real all-ships-rise proof. Use the **isolation-reconvert-diff** to skip byte-identical packages (see §9). |

Disclosed-divergence is **only** for asserts the managed CLR *provably cannot* satisfy (alloc counts),
signature-pinned per test. A real bug or an unimplemented feature is **never** a disclosure candidate.

---

## 6. Operating model — delegate widely, gate centrally

The coordinator runs as **Fable 5 Ultracode**. Run the campaign as a **coordinator with parallel
sub-agents** in **isolated worktrees** (or, for extremely delicate/complex work, user-owned chips).
The coordinator's OWN work is deliberately narrow: **gating, integration, re-validation, and landing**,
plus the *most difficult, complex, and delicate* tasks that genuinely need Fable-5-caliber reasoning.
**Everything else is delegated.** Sub-agents branch-only; the coordinator gates + lands everything
gpg-signed; **never land ungated or unverified work.**

- **Resource strategy — delegate ALL that's feasible; match model + effort to the task.** Fable 5
  credits are the scarce resource — spend them on the hard/delicate frogs and the coordinator's own
  judgment calls, NOT on routine work. Delegate as parallel as feasible, and choose each sub-agent's
  **`model` + `effort`** (the Workflow/Agent options) to fit the task, using cheaper tiers **liberally**
  wherever the work is efficiently suited to them:
  - **Cheap / mechanical → a light model at low effort** (e.g. haiku / sonnet, low–medium): the
    isolation-pipeline-sweep scout, banking a validated package's test sources, re-validation runs,
    characterizing a blocker, doc/ledger updates, any well-specified single-file mechanical fix.
  - **Moderate, well-scoped fixes → a mid model at medium–high effort** (e.g. sonnet, high): a
    contained converter/golib fix with a clear root cause and a bounded blast radius.
  - **Hardest / most delicate → the top tier at high–max effort, or the coordinator itself**: the
    Tier-0 capability frogs (channels, reflect, os, math/big), architecture/design decisions,
    adversarial design review (§7), and the final all-ships-rise integration judgment. Reserve Fable 5's
    own reasoning for exactly these.
  - When unsure whether a cheaper model suffices, **start cheap** (a scout or an attempt) and escalate
    only on evidence it's under-powered — don't pre-emptively burn the top tier, and don't under-power a
    genuinely delicate task (a wrong cheap fix that has to be reworked costs more than doing it right).

- **Parallelism is now cheap and unconstrained.** Workspaces target the **C:\ NVMe** (fastest drive);
  the earlier H:\ dev-drive disk bottleneck is **gone**. The 32-core machine easily handles **multiple
  concurrent sessions / sub-agents / chips** — as long as **each works in its own isolated workspace**
  (worktree or checkout). There is **no concurrency cap**; fan out as the work needs. (Keep the NuGet
  cache on H:\ ReFS — that config is deliberate and read-mostly once warm.)
- **Serialize only where builds genuinely collide:** two `-tests` runs that rebuild a *shared*
  dependency race on its `.dll` (CS2012); serialize those or pre-build the dep. Isolated worktrees
  don't collide.
- **The coordinator's job is merging + validation**, not doing every fix — integrate sub-agent
  branches (cherry-pick + re-gate), keep the memory ledger current, and own the landing.

---

## 7. Adversarial review for delicate / complex work

For anything delicate, architectural, or high-blast-radius (Tier-0 frogs, shared-machinery changes,
new mechanisms, anything touching escape analysis / reflection / channels / the test-project model),
**invest in adversarial review up front** — it is far cheaper than the rework/refactor a wrong design
forces later. Concretely:

- **Independent verification of a fix**: spawn skeptic sub-agents prompted to *refute* the fix (find
  the input that breaks it, the regression it hides, the case it doesn't generalize to). Prefer
  **diverse lenses** (correctness / a different failing package / does-it-reproduce / does-it-hold-
  under-the-corpus) over N identical reviewers.
- **Design panels for real design decisions**: generate a few independent approaches, judge them
  against the long-term goals, synthesize from the strongest.
- **Gate the design, not just the diff**: for a Tier-0 capability, get the *design* adversarially
  reviewed (and user-blessed) before writing the implementation.

The goal is to catch "plausible-but-wrong" before it lands and metastasizes into every downstream
package.

---

## 8. Abstract & general — build for many use cases

Whenever possible, make each fix **abstract and general** — it should apply to as many **current and
future** use cases as possible, always with an eye to the long-term project goals. A converter/golib
capability that a package needs is almost never truly package-specific; find the general rule and
implement *that*. Examples from this campaign: the deref-alias fixes generalized to *every*
`*error`/`*[]T`/nilable-out-param; the reflect comparability fix corrected *all* synthetic types; the
unicode "Change C" is a general black-box-test-project model, not a unicode patch. A narrow special-
case that greens today's package but not the next ten like it is the wrong altitude — lift it.

---

## 9. Lessons learned — traps to NOT repeat

Hard-won during this campaign. Read these before touching the relevant area.

**Converter / gating**
- **Wire new analysis passes into ALL THREE conversion drivers.** The analysis phase runs in three
  places — normal (`main.go` ~1151), `-tests` (`testConversion.go` ~584), and hand-owned-sibling
  (`autoSiblingOperations.go`). A collector wired into only one **silently no-ops** for the others
  (the nil-arg fix didn't fire on `-tests` until wired everywhere). Follow `collectAddressedGlobals`.
- **Reconvert to measure a converter change.** The committed `go-src-converted` is stale (predates
  recent fixes); building it measures *old* output. To see a change's corpus impact, reconvert.
- **Isolation-reconvert-diff** narrows which validated packages a converter change actually touched:
  reconvert the candidate set to a **C:\ temp**, CRLF-normalize, diff vs committed; re-validate only
  the changed ones (most are byte-identical → skip). Isolate the change from pre-existing drift by
  diffing wave-converter vs master-converter reconverts when needed.
- **RESTORE re-validated packages; do NOT partial-rebank.** A converter change drifts many packages'
  production `.cs`. Rebanking a scattered subset = split-brain corpus. Restore them (re-validation is
  the *gate*, not a rebank trigger) and defer the whole-corpus re-baseline to **ONE deliberate clean
  regen** (kept at Go 1.23.1 — no version bump, no rug-pull). Newly-*validated* packages DO bank their
  fresh test sources.
- **False-green traps.** (a) A stale `go2cs.exe` runs old logic — force `go build -o bin/go2cs.exe`.
  (b) Runner *UpToDate* skips were fixed 2026-07-20 (both runners + MSTest now require `.cs` newer than
  the exe); `check-no-regression.ps1` re-transpiles **unconditionally** = the authoritative instrument.
  (c) `UpdateTestTargets --createTargetFiles` copies current `.cs`→`.cs.target` **without** re-
  transpiling — re-transpile first or it re-baselines stale output.

**Build / git mechanics**
- **CS2012 during a corpus build = file-LOCKS, not compile errors** — an orphaned/concurrent build
  holds `.dll` locks. `dotnet build-server shutdown`, rebuild the locked packages; never leak a
  `&`-backgrounded build. Grep for `error CS` *excluding* CS2012 to see real errors.
- **autocrlf-only "drift":** a re-validated / agent-banked test source often shows `git status`
  modified but the **content diff is empty** (CRLF↔LF). Confirm with `git diff` before chasing.
- **`.slnx` edits are byte-exact CRLF:** `sed`/`awk` strip CRLF — use `perl -0777` with explicit
  `\r\n`, or the Edit tool. Register every behavioral `.csproj` (`check-solution-integrity.ps1`); the
  harness builds by path, so a missing registration passes the suite but breaks VS.
- **`[GoTestMatchingConsoleOutput]`** goes in `package_info.cs` via the **Edit tool** (CRLF-safe), never
  `sed` (which corrupts the CRLF the transpiler's marker-parser needs).
- **Hand-owned preservation on overlay:** reconvert emits `.cs.auto` for `[module: GoManualConversion]`
  files; overlay `*.cs` **excluding `.cs.auto`** to preserve hand-owned `.cs`; also except `core\golib`
  and `core\testing` refs.
- **GPG "Timeout" = a stuck agent, not a missing key.** Do NOT kill the agent (that clears the cached
  passphrase); a bounded standalone `--clearsign` re-warms it. Launch daemons via **Gpg4win's**
  `gpgconf` (`C:\Program Files (x86)\GnuPG\bin`), not the MSYS one. Never bypass signing.

**Sub-agent / Workflow orchestration**
- **Give agents the WORKTREE path** for the `-tests` output dir, not the absolute MAIN checkout path —
  else their runs pollute the main tree. Clean stray `-tests` leftovers (git-ignored `.go` copies +
  manifests accumulate; `git clean -fdq` skips ignored files — `find … -name '*.go' -delete`).
- **Workflow worktree branches persist** (linked worktrees). Cherry-pick the agent's `claude/wf-<slug>`
  branch, re-gate, ff master. Windows long-path can block `git worktree remove` → `rm -rf` the dir +
  `git worktree prune`.
- **Don't trust a stale "still-owed" memory.** Assess stale branches with `git cherry` (patch-id) THEN
  content/ground-truth — a fix may already be on master via a divergent commit. Verify branch existence
  with `git branch` (the memory's branch claims drift out of date).

**Campaign shape**
- **The reflect `.Clone` mis-parenthesization** still blocks a fully-clean whole-corpus rebank — a
  known separate campaign; don't let it stall package work.
- **Prefer the guard test that reproduces the bug and would catch its regression** — a package's
  banked test suite is its own guard; a shared-machinery fix also needs a minimal behavioral guard.

---

## 10. Decisions, honesty, cadence

- **Decisions.** On a genuine design decision or trade-off (a new mechanism, a promotion question, a
  semantic divergence — anything shaping shared architecture), do NOT pick a shortcut to keep moving.
  If the durable choice is clear from the project goals, make it and document it. If it is a real
  judgment call the user should own, **STOP, write up the options + your recommendation, surface it**,
  and continue with other packages while it's pending. **Design WITH the user on anything long-term.**
- **Honesty.** Report real numbers at every checkpoint — what validated, what's blocked and precisely
  why, what decisions were made or pending. State partial results plainly ("N/M agree; remainder is
  <class>, owned/pending"). Compiling ≠ correct; validating-via-hack ≠ validated. Never claim a
  package validates when it does not.
- **Cadence.** Self-pace; checkpoint after each package (or small batch). Don't spin on a genuinely
  blocked package — record the blocker, surface any needed ruling, move to the next viable one. **Push
  is a deliberate, user-initiated ceremony** — land gpg-signed to local master and let the user
  initiate `git push`. Pause/end when every viable package validates or when blocked solely on user
  input.
