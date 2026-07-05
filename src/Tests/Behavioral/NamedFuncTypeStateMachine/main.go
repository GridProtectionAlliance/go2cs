// Regression test: a Go short-declaration (`:=`) from a bare function value whose type is a
// self-referential named func type — the classic state-machine pattern, as in text/template/parse's
// lex.go `state := lexText` where `lexText` is `func(*lexer) stateFn` and
// `type stateFn func(*lexer) stateFn`.
//
// Go infers the local's type as the UNNAMED signature `func(*machine) stateFn`, not the named
// `stateFn`. The converter cannot emit `var state = stateA;` (a C# method group has no inferable
// delegate type — CS8917), and typing the local structurally as `Func<ж<machine>, stateFn>` makes it
// a DISTINCT C# delegate from the `stateFn` the method group produces and that each `state = state(m)`
// reassignment yields (CS0029, no implicit conversion between two delegate types). The converter must
// declare the local with the matching package named delegate — `stateFn state = stateA;`.
package main

import "fmt"

type stateFn func(*machine) stateFn

type machine struct{ steps int }

func stateA(m *machine) stateFn {
	m.steps++
	return stateB
}

func stateB(m *machine) stateFn {
	m.steps++
	if m.steps >= 4 {
		return nil
	}
	return stateA
}

// run drives the state machine; `state := stateA` is the bare-func `:=` that must take the named
// `stateFn` delegate so the loop's `state = state(m)` reassignments type-check.
func run(m *machine) {
	state := stateA
	for state != nil {
		state = state(m)
	}
}

func main() {
	m := &machine{}
	run(m)
	fmt.Println(m.steps)
}
