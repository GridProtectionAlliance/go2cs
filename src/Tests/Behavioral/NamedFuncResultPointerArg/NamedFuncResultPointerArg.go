// Guards a call whose CALLEE is the result of a function returning a NAMED func type, with a
// pointer argument that must be boxed. getFunctionSignature's *ast.CallExpr arm did a bare
// `t.(*types.Signature)`, which fails for a named func-type result (t is *types.Named), so it
// returned nil and the per-argument pointer treatment never fired — the pointer receiver `s`
// rendered as its value alias where the `ж<State>` delegate slot wanted the box `Ꮡs` (CS1503).
// Same shape as encoding/json's `valueEncoder(v)(e, v, opts)` (valueEncoder returns encoderFunc).
package main

import "fmt"

type State struct {
	sum int
}

// a methodless NAMED func type taking a *State
type addFunc func(s *State, x int)

func adder() addFunc {
	return func(s *State, x int) { s.sum += x }
}

func (s *State) apply(x int) {
	// adder() returns the named func type; s (*State receiver) must box to ж<State>.
	adder()(s, x)
}

func main() {
	s := &State{}
	s.apply(5)
	s.apply(3)
	fmt.Println(s.sum) // 8
}
