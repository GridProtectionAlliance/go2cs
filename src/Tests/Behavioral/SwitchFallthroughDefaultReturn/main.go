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

func main() {
	for _, n := range []int{0, 1, 2, 3} {
		fmt.Println(classify(n))
	}
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
}
