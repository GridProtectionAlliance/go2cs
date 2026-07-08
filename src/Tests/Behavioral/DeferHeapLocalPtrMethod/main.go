package main

import "fmt"

// state is a VALUE local in run() below. free() has a POINTER receiver, so Go implicitly takes
// &state for `defer state.free()`. Combined with the other pointer-receiver calls (add), state's
// address is taken, so the converter heap-boxes it (a `ref var state = ref heap<state>(out var
// Ꮡstate)` local with a companion box Ꮡstate).
type state struct {
	n     int
	label string
}

// free has a POINTER receiver and observes the FINAL value of the receiver — it must see the
// mutations made AFTER the defer statement (Go binds &state at defer time and calls free() at
// function exit on the live variable). A value snapshot taken at defer time would print the stale
// n == 0; the box form prints the correct final n.
func (s *state) free() {
	fmt.Printf("free: %s n=%d\n", s.label, s.n)
}

// add mutates the receiver through its pointer — one of the address-taking uses that make the
// value local escape to the heap.
func (s *state) add(d int) {
	s.n += d
}

// run defers a pointer-receiver method (free) on the value local `state`, then keeps mutating it.
// Without the converter fix, the deferred receiver is snapshot-copied into a plain `var stateʗ1 =
// state;` whose address box `Ꮡstateʗ1` is referenced but never declared (CS0103); the defer must
// instead bind the original heap box `Ꮡstate.free`.
func run() {
	st := state{n: 0, label: "run"}
	defer st.free()
	st.add(5)
	st.add(10)
	fmt.Printf("during run: n=%d\n", st.n)
}

// twice exercises the same shape a second time in a distinct function, verifying every occurrence
// binds its own original box rather than a shared snapshot name.
func twice() {
	st := state{n: 100, label: "twice"}
	defer st.free()
	st.add(1)
	fmt.Printf("during twice: n=%d\n", st.n)
}

func main() {
	run()
	twice()
}
