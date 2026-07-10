# Glossary — process terms used in commit messages, memory logs, and reviews

> Definitions as **actually used** in this repository's development process — especially the
> Phase-3 "full stdlib compile" campaign (2026-06/07) whose commit messages, chip briefs, and
> review verdicts lean on this shorthand heavily. For *conversion* terminology (how Go constructs
> map to C#: `ж<T>`, heap boxes, adapters, direct-ж, shadow renames) see
> [`ConversionStrategies.md`](ConversionStrategies.md); this file covers the **process** vocabulary.
> Companion docs: [`CLAUDE.md`](../CLAUDE.md) (authoritative workflow),
> [`Baseline-vs-FullConversion.md`](Baseline-vs-FullConversion.md).

## Gates (the checks a change must pass before it lands)

**CNR — check-no-regression** (`src/Tests/Behavioral/check-no-regression.ps1`).
Force-rebuilds `go2cs.exe` from current source, re-transpiles **every** behavioral test project,
and reports any generated `.cs` that differs from the committed tree. The pass verdict is
**"byte-identical"**: the converter change produced exactly the committed output everywhere it
wasn't intended to change something. Mandatory for any converter (`src/go2cs/*.go`) change.
Scope caveat: CNR checks *transpile output only* — it cannot see compilation, source-generator
(`go2cs-gen`) output, or runtime behavior. A new test's own files appearing as untracked (`??`)
is the expected signature of adding a guard, not a failure.

**A/B (full-stdlib reconvert-diff).**
Two complete stdlib conversions — one with the **base** converter (built from `HEAD` before the
fix, usually via the *stash dance*, below) and one with the **fixed** converter — written to two
scratch directories and compared with `diff -r`. The set of differing files is the change's
**footprint**. Discipline: *every* differing file is inspected line-by-line; the footprint must be
exactly the intended transform (an unexplained third file means the fix over-fires — stop and
re-derive). A second sense, **A/B determinism**, runs the *same* exe twice and requires zero
differences (the converter is byte-deterministic; any diff means introduced nondeterminism).

**Stash dance.**
How the A/B base exe is built when the fix is uncommitted or freshly committed:
`git stash push -- <fix files>` (or checkout of the parent) → `go build -o bin/go2cs_base.exe` →
restore → build the fixed `bin/go2cs.exe`. The temporary base exe is deleted afterward so a stale
binary can't produce false results later.

**Full behavioral suite** (`src/Tests/Behavioral/run-behavioral.ps1`, standalone runner).
All behavioral projects through four phases: **Transpile** (converter runs), **Target** (golden
byte-compare, line-ending-insensitive), **Compile** (C# and Go), **Output** (run both, compare
stdout; only projects marked `[GoTestMatchingConsoleOutput]`). Required — in addition to CNR —
whenever a change touches `go2cs-gen` (the source generator) or `golib` (the runtime), because
those act at compile/run time where CNR is blind. A gen change additionally requires the
**forced gen rebuild** ritual first: delete `src/gen/go2cs-gen/{obj,bin}` and run
`dotnet build-server shutdown`, otherwise MSBuild serves the cached analyzer and the suite
silently tests the *old* generator.

**Census** (`dotnet build src/go-src-converted.slnx` + an own-DLL count script).
The Phase-3 progress metric: how many of the ~302 auto-converted stdlib projects **emit their own
assembly** (`bin/Debug/net9.0/<AssemblyName>.dll` exists after a full solution build). Reported as
`N / 302`. The metric is **packages-compiling, not error count** — clearing an error family can
*raise* the raw error count by unmasking latent errors behind it, and that is progress.
Each census diff reports **GREENED** (was red, now green) and **REGRESSED** (must be empty —
the zero-regression invariant).

**Own-errors vs blocked dependents.**
In a census build, MSBuild only compiles a project whose references succeeded; a project whose
dependency failed is **skipped**, contributing zero error lines. So the red set divides into
**leaf failures** (packages with their *own* build errors — the only ones that need fixing) and
**blocked dependents** (red only because a dependency is). Corollary: a package that is red with
*zero* error lines anywhere is either a skipped dependent or a bookkeeping artifact (historically:
a csproj on disk that was never registered in the `.sln`, so it was counted but never built).

**Overlay.**
The ritual that makes a census measure the *current* converter rather than the stale committed
tree: full reconvert to a scratch dir → copy the fresh `.cs` **and** `.csproj` over
`src/go-src-converted/` (rewriting project references `core\` → `go-src-converted\`, **except**
`core\golib`, which is the shared runtime) → restore the hand-owned manual files from `src/core`
(`*_impl.cs`, `unsafe`, `sync/atomic`, …) that auto-conversion must not clobber → clear
`bin/obj/Generated` → build. Skipping the manual-file restore craters the census (the `unsafe` →
`runtime` cascade). The overlay is *regenerable scaffolding*: it is never committed, and the tree
is restored afterward (`git checkout HEAD -- src/go-src-converted` + `git clean`).

## Fleet coordination (multi-session development)

**Chip.**
A focused, isolated work session (its own git worktree and branch) assigned **one root family**,
with a self-contained brief: verified diagnosis, binding **file ownership** (see below), the gate
list, guard requirements, and the commit protocol (gpg-signed commits on its own branch; never
master; never push). Chips end with a **final summary** (per-root status, commit hashes, gate
results, footprints, anything escalated). A **sub-chip** is a chip spawned from within another
chip's session for an out-of-scope discovery.

**Coordinator.**
The session that runs the campaign: diagnoses (or fleets out diagnosis), spawns chips, answers
their escalations against project goals, reviews landings (adversarially where flagged),
cherry-picks branches to master in **batches**, runs the consolidated gates, and keeps the memory
log. The chip/coordinator split exists so each fix session stays small and expendable while
integration discipline stays centralized.

**File ownership.**
The concurrency rule that let many chips run in parallel safely: each converter/gen/golib file has
exactly **one** active owner; chip briefs name both the owned files and the explicitly forbidden
ones. Shared hot files (e.g. `main.go`) are partitioned by *function*, with each chip required to
name the exact functions it touched in its summary for merge planning.

**Precondition (subject-based).**
A chip whose work builds on another chip's unmerged fix starts by checking master for the
prerequisite and **stops cleanly** if absent. Checks must grep commit **subjects** (or test for
guard files), never original branch hashes — integration is by cherry-pick, which mints new
hashes, so `git merge-base --is-ancestor <original>` is never true even when the content landed.

**Batch.**
The integration unit: one or more reviewed chip branches cherry-picked (`-x`) onto master
together, followed by one consolidated gate pass (converter build → CNR → suite if gen/golib is in
the batch → reconvert → census). Completeness after a batch is verified with `git cherry`
(patch-id) *plus* a subject-level check — a lock collision can silently skip a pick mid-loop, and
conflict resolutions change patch-ids.

**Adversarial review / refuter.**
Pre-merge review whose prompt is to **refute** the work — assume a subtle semantic error and hunt
for it, with branch-specific "trap" lists (the known failure modes of that code area). Verdicts:
`APPROVE`, `APPROVE_WITH_NOTES` (concerns become banked follow-ups), `BLOCK` (demonstrated error —
the branch does not merge). Distinct from the cheaper **audit**, which checks discipline
compliance per commit: gpg signature, guard registered in the solution, no hardcoded marker
glyphs, docs updated, one-root scoping.

**Scout.**
A read-only diagnosis session in its own worktree: builds the packages *about to be un-gated*,
peels masking layers with **shims**, and reports exact roots — no fixes, no commits. Shims are
documented diagnostic byte-patches applied only to the scout's uncommitted overlay to let a
dependent compile so its *own* latent errors surface; they prove a fix *shape* compiles but are
never fixes themselves.

**Wave / frontier.**
A **wave** is a coordinated group of chips spawned from one diagnosis round. The **frontier** (or
**red set**) is the current list of red census packages; the campaign advances by collapsing the
frontier's leaf failures wave by wave.

**Banked / bank.**
A verified-but-not-fixed item recorded durably (memory log + a banked-follow-ups note) instead of
fixed now — typically a latent sibling with zero occurrences in the current corpus, or a concern
from review. Banking is legitimate *only* with a written diagnosis; several banked items later
materialized in new packages and were fixed from the bank without re-diagnosis.

**Root / root family.**
A **root** is one distinct converter/generator/runtime defect, stated as the *general* construct
it mishandles (never as "package X fails"). **One root per commit.** A root family is a set of
roots sharing a mechanism (e.g. the u8/string-boxing family: the same gate missing at assignment,
composite-literal, tuple-return, and channel-send positions).

## Test artifacts & conventions

**Guard.**
A behavioral regression test locking in a fix: a minimal Go program exercising the exact
construct, its transpiled `.cs`, and a **golden**. Requirements that recur in reviews: solution
registration in `src/go2cs.slnx` **plus grep-verification** (the *silent-drop gotcha*: the harness
builds by path, so an unregistered guard still passes and nothing else catches it);
**negative-check** where feasible (prove the pre-fix converter fails the guard with the diagnosed
error); **write-visibility** proof for pointer/box fixes (the program observes a write through the
fixed path in its output, compared against Go — distinguishing a faithful fix from a
compiles-but-copies shortcut).

**Golden / re-baseline.**
The committed expected transpiler output (`*.cs.target`), byte-compared (line-ending-insensitive)
by the Target phase. When an *intended* emission change alters existing goldens, they are
**re-baselined** — regenerated from converter output (via the `UpdateTestTargets` utility with
`--createTargetFiles`), never hand-edited — and the Output phase must stay green to prove behavior
was preserved. Gotcha: `UpdateTestTargets` copies the **on-disk** transpiled `.cs`, it does not
run the converter — a fresh transpile (CNR does one) must precede it, or the re-baseline silently
captures stale output. **Cross-chip interaction drift**: a guard baselined on a chip branch may drift once
*sibling* chips merge (the combined converter emits differently than any single branch saw); each
drift line is attributed to a specific landed fix before re-baselining.

**TestMethods regeneration.**
The four `*Tests.cs` classes contain generated `// <TestMethods>` blocks. After any merge that
touches them, the blocks are regenerated with `UpdateTestTargets` rather than trusting textual
conflict resolution — hand-merged unions have both dropped `[TestMethod]` attributes (MSTest then
silently skips the test; the standalone runner, which discovers by directory, is unaffected) and
duplicated methods.

**gpg-G.**
`git log --format='%G?'` showing `G` — a good signature. All campaign commits are gpg-signed;
signing is never bypassed, and cherry-picks re-sign automatically.

**Marker glyphs / `Symbols.cs`.**
The emitted C# uses reserved glyphs (`ж` pointer box, `Ꮡ` address-of, `Δ` shadow/collision rename,
`ꓸ` type-alias dot, `ᴛ` temp, …). Converter and generator **source** must reference named
constants, never literal glyphs: the C# side (golib, go2cs-gen) uses `Symbols.cs`
(`PointerPrefix`, `AddressPrefix`, `ShadowVarMarker`, …, via `using static go2cs.Symbols`); the
Go converter uses its mirrored constants in `src/go2cs/main.go` (kept in sync by comment-contract
with `Symbols.cs`). Audits flag violations. Glyphs inside goldens, docs examples, and comments
are fine — they *are* the output.

**OWED.**
Annotation in memory logs for a known debt: an *OWED merge* (a finished chip branch not yet
integrated), an *OWED guard* (a fix landed with its regression test deferred — rare and always
tracked), or an owed documentation/log compaction.
