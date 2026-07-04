package main

import "fmt"

const (
	stateA = iota // untyped consts -> the switch lowers to an if / else-if chain
	stateB
	stateC
)

// classify has a `break` inside a switch case. In Go that break exits the SWITCH (skipping the
// rest of the case). When the switch lowers to if-else (here, because the case labels are untyped
// constants), the break has no enclosing C# switch/loop (CS0139), so the case body is wrapped in a
// `do { … } while (false)` whose break exits the case. Mirrors the runtime's casgstatus tracking
// switches in proc.go and similar.
func classify(n int, flag bool) string {
	result := "none"
	switch n {
	case stateA:
		if flag {
			break // exits the switch, skipping the assignment below
		}
		result = "a"
	case stateB:
		result = "b"
		if flag {
			break
		}
		result = "b-noflag"
	default:
		result = "other"
	}
	return result
}

// pick: a LABELED switch with `break Big` from a NESTED switch - the goto's target
// label must be DECLARED after the labeled switch (regexp/syntax parse.go's BigSwitch,
// CS0159 x11).
func pick(x, y int) string {
	out := "none"
Big:
	switch x {
	case 1:
		switch y {
		case 2:
			out = "one-two"
			break Big
		}
		out = "one-other"
	case 3:
		out = "three"
	}
	return out
}

func main() {
	fmt.Println(pick(1, 2), pick(1, 5), pick(3, 0)) // one-two one-other three
	fmt.Println(classify(stateA, true))   // break -> none
	fmt.Println(classify(stateA, false))  // a
	fmt.Println(classify(stateB, true))   // b (break before the override)
	fmt.Println(classify(stateB, false))  // b-noflag
	fmt.Println(classify(stateC, false))  // other
}
