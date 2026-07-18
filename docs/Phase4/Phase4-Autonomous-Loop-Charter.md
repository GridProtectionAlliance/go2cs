# Phase 4 autonomous loop — charter (verbatim)

> This file persists the operating goals of the Phase-4 autonomous validation loop, placed here at
> the user's request (`/loop … place loop goals (verbatim, top of session) in reference MD file for
> persistence and reference`). The body below is the mission prompt **verbatim** as given at the top
> of the session. A short **Amendments** section follows for rulings made during the loop that
> refine (not replace) it.

---

Phase 4 autonomous loop — go2cs standard-library test validation.

MISSION: Advance Phase 4 — convert and RUN Go's own _test.go suites for the standard-library
packages in src/go-src-converted, validating each against `go test -json` through the
`-tests -test-action all` pipeline, until every viable package's test suite passes in C#.
Work package by package.

PRIME DIRECTIVE — quality over completion, ALWAYS. The objective is NOT to finish this loop;
it is to advance go2cs's long-term vision correctly. Treat H:\Projects\go2cs\CLAUDE.md and the
docs/ it points to as authoritative, and weigh EVERY decision against the project's DEFINED goals
(the two end-user use cases, the nothing-throwaway principle, "reads like Go / runs like Go") —
never against reaching the end of the loop. Concretely:
  - Fix at the RIGHT layer and at the ROOT: a converter / golib / go2cs-gen change over a
    one-off hand-patch; a real root cause over a workaround; the reproducible-from-repo result
    over a deploy-only hack.
  - If the correct fix is a REWORK, a REFACTOR, or a LONGER path — take it. A shorter path that
    leaves a latent defect, a hack, or a narrow special-case is the WRONG path even when it
    "works" today.
  - A package that "validates" via a shortcut — faking or trimming output, silently skipping a
    real test, hand-editing generated code, or a disclosed-divergence used to paper over a genuine
    bug — is a FAILURE, not a win. Do not count it.
  - NEVER trade a project goal for loop progress. If a fix would compromise the architecture or a
    stated goal, don't take it: find the right fix, or surface the decision (below).

ALL SHIPS RISE: a fix to shared machinery (converter, golib, go2cs-gen) must improve ALL converted
code, never just the package under test. Prove it — CNR byte-identity for converter changes; the
FULL behavioral suite + the 302-package go-src-converted corpus build for any go2cs-gen change. A
fix that greens one package but regresses or degrades others is rejected; rework it until it lifts
everything.

PER-ITERATION WORKFLOW:
  1. Choose the next unvalidated package — prefer one whose dependencies already validate; consult
     docs/Roadmap.md (Phase 4), docs/Phase4/*, and the memory ledger for known blockers/ordering.
  2. Run: src/go2cs/bin/go2cs.exe -tests -test-action all -test-timeout 10m
     "<GOROOT>/src/<pkg>" src/go-src-converted/<pkg>   (converts -> builds .NET host -> runs it
     -> diffs vs `go test -json`).
  3. For each failure, root-cause against the REAL emitted .cs and runtime behavior — never assume.
     Determine the correct layer (converter emission / golib runtime / go2cs-gen / hand-owned
     _impl / a genuinely-unsatisfiable-in-managed-runtime case that warrants a SIGNATURE-PINNED
     disclosed-divergence). Fix it there.
  4. Lock every fix in with a behavioral guard test (CLAUDE.md's regression-test steps) and update
     docs/ConversionStrategies(-Reference).md in the same change.
  5. GATE before landing: converter change -> CNR byte-identical (+ relevant filtered/full run);
     gen change -> full suite + corpus; golib change -> full behavioral suite. Then RE-VALIDATE the
     package on the POST-change tree (do NOT assume base-tree numbers hold), and confirm every
     already-validated package (utf8, sort, bytes, strings, and any you add) STILL validates — a
     hard regression gate.
  6. On a clean validation, follow the validated-package commit policy: commit the converted C#
     test sources into src/go-src-converted/<pkg> (Go headers intact), refresh the README
     "Try it yourself" list, add a NEWS/milestone entry for a notable first. Commit gpg-signed to
     master (solo-project convention).
  7. Move to the next package.

DECISIONS: when a genuine design decision or trade-off arises (a new mechanism, a promotion
question, a semantic divergence, anything that shapes shared architecture), do NOT pick a shortcut
to keep moving. If the durable choice is clear from the project goals, make it and document it. If
it is a real judgment call the user should own (the disclosed-divergence manifest was one), STOP,
write up the options + your recommendation, and surface it — then continue with other packages
while it's pending. Design WITH the user on anything long-term.

PARALLELISM: you may use subagents (the Agent tool) for independent investigation, adversarial
verification of a fix, or scouting a package's blockers — but the loop session GATES and lands
everything; never land ungated or unverified work.

HONESTY: report real numbers at every checkpoint — what validated, what's blocked and precisely
why, what decisions were made or are pending. State partial results plainly ("N/M agree; remainder
is <class>, owned/pending"). Compiling != correct; validating-via-hack != validated. Never claim a
package validates when it does not.

CADENCE: self-pace; checkpoint after each package (or small batch). Don't spin on a genuinely
blocked package — record the blocker, surface any needed ruling, move to the next viable one.
Pause/end when every viable package validates or when blocked solely on user input.

---

## Amendments (rulings made during the loop — refinements, not replacements)

- **Doc-update cadence (2026-07-18).** Step 6's "refresh the README 'Try it yourself' list, add a
  NEWS/milestone entry" is **batched, not per-package**: do NOT touch NEWS / the Milestones table /
  the README validated-package list for each individual package. Wait until a notable cross-section
  of the standard library has moved, then update in one batch. The **technical** conversion-strategy
  docs (`docs/ConversionStrategies-Reference.md`, and the summary when the headline mapping changes)
  are still updated per real converter/golib/gen decision — that is unchanged. See the memory note
  `go2cs-doc-update-cadence`.
