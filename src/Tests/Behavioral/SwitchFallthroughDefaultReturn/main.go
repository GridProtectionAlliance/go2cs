package main

import "fmt"

// classify's switch is the LAST statement of a value-returning func; every case returns, and a `case 2:
// fallthrough` reaches the `default:`. The converter lowers this to an if-chain whose default is a GUARDED
// `if (fallthrough || !match)` block — C# can't prove it always executes → CS0161 ("not all code paths
// return a value") even though Go's `default` makes it exhaustive. Runtime `startpanic_m` has this shape.
//
//go:noinline
func classify(n int) string {
	switch n {
	case 0:
		return "zero"
	case 1:
		return "one"
	case 2:
		fallthrough // reaches default
	default:
		return "many"
	}
}

// nonTerminal has the SAME guarded-default (a `case 1: fallthrough` reaches `default:`), but its cases
// do NOT return — they fall OUT of the switch to the trailing `return acc + n`. The converter must NOT
// add a trailing `return default!;` here (it would execute and wrongly return the zero value). This
// guards the terminality gate: only a switch whose every case returns/falls-through gets the trailing
// return.
//
//go:noinline
func nonTerminal(n int) int {
	acc := 0
	switch n {
	case 0:
		acc = 10
	case 1:
		fallthrough
	default:
		acc = 20
	}
	return acc + n
}

// conditionalReturn is the subtle case: case 0's body is `if n < 0 { return 111 }` with NO else, so for
// n >= 0 the case FALLS OUT of the switch to the trailing `return 777`. A shallow "last statement is a
// return" check would wrongly treat case 0 as terminal and add a `return default!;` that executes for
// n==0, silently returning 0. Genuine Go terminating-statement analysis (an `if` without `else` is NOT
// terminating) correctly suppresses the trailing return here.
//
//go:noinline
func conditionalReturn(n int) int {
	switch n {
	case 0:
		if n < 0 {
			return 111
		}
	case 1:
		fallthrough
	default:
		return n * 1000
	}
	return 777
}

// namedDefer has NAMED results and a defer/recover — its body is emitted inside a `void` func((defer,
// recover) => …) wrapper — plus a terminal fallthrough-default switch. A value `return default!;` in that
// void wrapper would be CS8030, so the trailing return must be suppressed here (the void wrapper needs no
// return). Guards the namedReturnDefer exclusion.
//
//go:noinline
func namedDefer(n int) (r int, ok bool) {
	defer func() {
		if recover() != nil {
			r, ok = -1, false
		}
	}()
	switch n {
	case 0:
		return 100, true
	case 1:
		fallthrough
	default:
		return n * 10, true
	}
}

// keepAlive mirrors net tcpsockopt_windows.go's setKeepAliveIdleAndInterval: an expressionless
// switch whose FINAL case body is comment-only — earlier returning cases leave the stale
// last-statement-was-return flag set at the SAME indent, which suppressed the final section's
// mandatory `break;` (CS8070).
func keepAlive(idle, interval int) string {
	switch {
	case idle < 0 && interval >= 0:
		return "interval-only"
	case idle < 0 && interval < 0:
		return "none"
	case idle >= 0 && interval >= 0:
		// Nothing to do, both provided.
	}
	return "both-or-idle"
}

// leadingDefault mirrors encoding/ascii85 Encode: the `default:` comes FIRST and itself falls
// through, so the bodies must run in SOURCE order (default→3→2→1 for the unmatched value) while
// the default still runs only when NO case matches. The converter precomputes a second match var
// (`var matchᴛ2 = <OR of all case conditions>;`) to guard the leading default; without the
// default's own fallthrough being scanned the next case emitted `else if` (CS8641 cascade).
//
//go:noinline
func leadingDefault(n int) int {
	v := 0
	switch n {
	default:
		v |= 8
		fallthrough
	case 3:
		v |= 4
		fallthrough
	case 2:
		v |= 2
		fallthrough
	case 1:
		v |= 1
	}
	return v
}

// Non-constant case expressions (Go permits them in an expression switch) force the converter to
// lower the switch to an if-CHAIN rather than a C# `switch`, which is the shape os's
// (*Process).wait has: its labels are syscall.WAIT_OBJECT_0 / WAIT_FAILED, emitted as
// `static readonly UntypedInt` rather than C# constants.
var waitObject0 = 0
var waitFailed = -1

// waitShape mirrors os (*Process).wait exactly: a case whose body is a bare `break` — NOT a Go
// terminating statement, so it falls OUT of the switch — followed by a RETURNING case and a
// RETURNING `default:`. In the if-chain lowering the converter may drop the `else` before a clause
// when the immediately preceding clause returned, but that is only sound when EVERY preceding
// clause terminates. Here case waitObject0 falls out, so an unguarded `{ /* default: */ … }` block
// executes right after it and the function returns the default's value instead of "ok". A `case`
// clause would be harmless (its condition cannot match a value an earlier case matched), but
// `default:` has no condition and therefore ALWAYS runs — silently wrong, and it compiled cleanly.
// In os this made EVERY child-process wait return "os: unexpected result from WaitForSingleObject",
// so no converted program could collect a spawned child's exit status.
//
//go:noinline
func waitShape(s int) string {
	switch s {
	case waitObject0:
		break
	case waitFailed:
		return "failed"
	default:
		return "unexpected"
	}
	return "ok"
}

func main() {
	for _, n := range []int{0, 1, 2, 3} {
		fmt.Println(classify(n))
	}
	for _, n := range []int{0, 1, 2, 3, 4} {
		fmt.Println(leadingDefault(n)) // 15, 1, 3, 7, 15
	}
	fmt.Println(keepAlive(-1, 5), keepAlive(-1, -1), keepAlive(3, 5)) // interval-only none both-or-idle
	// zero / one / many (fallthrough) / many (default)
	for _, n := range []int{0, 1, 2} {
		fmt.Println(nonTerminal(n)) // 10, 21, 22 — the real acc+n, never the zero value
	}
	for _, n := range []int{0, 1, 2} {
		fmt.Println(conditionalReturn(n)) // 777, 1000, 2000 — n==0 falls out, never the zero value
	}
	for _, n := range []int{0, 1, 2} {
		r, ok := namedDefer(n)
		fmt.Println(r, ok) // 100 true / 10 true / 20 true
	}
	// ok (the break case falls OUT to the trailing return) / failed / unexpected
	for _, s := range []int{0, -1, 258} {
		fmt.Println(waitShape(s))
	}
}
