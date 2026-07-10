// WrittenCaptureParam guards the entry-time heap box of a VALUE parameter the capture
// analysis routes to SHARED storage: a param captured by a closure and WRITTEN after the
// capture point (see processPotentialCapture's varShareFacts arm) is referenced through its
// box inside every capturing lambda (`Ꮡs.ValueSlot`), so the parameter prologue must declare
// that box — `ref var s = ref heap(sʗp, out var Ꮡs);` — with the signature taking the incoming
// value under the `ʗp` name (the same convention as a capture-mode boxed param, see
// CaptureModeValueParam, but WITHOUT any direct-ж method in sight). The stdlib casualties were
// database/sql beginDC's `ctx` (redeclared by a body-top-level `ctx, cancel := …` after
// withLock's closure captured it) and go/types nify's `x, y` (swapped after the trace defer
// captured them): both rendered a box that was never declared (CS0103). Every arm here
// output-compares against Go to prove the box IS the one shared variable: a closure invoked
// after the write sees the new value, a closure's own write is seen by the body, and a
// deferred observer sees the FINAL values at return time.
package main

import "fmt"

type label struct {
	name string
}

func (l label) String() string {
	return l.name
}

func swap(s fmt.Stringer) (fmt.Stringer, int) {
	return label{name: "swapped:" + s.String()}, 1
}

// declRedeclare: the beginDC shape — a closure captures the interface param, a body-top-level
// mixed `:=` REDECLARES (writes) the parameter object after the capture point, and the closure
// invoked after the write must see the new value (Go closures share the ONE parameter variable;
// the `:=` reuses the param per the spec's function-body redeclaration rule, which is also what
// escape-marks it through the define walker).
func declRedeclare(s fmt.Stringer) (string, string) {
	get := func() string { return s.String() }
	before := get()
	s, n := swap(s)
	_ = n
	after := get()
	return before, after
}

// declDeferObserver: the nify shape — named result + defer puts the body in the execution
// wrapper, the DEFERRED closure reads both params at return time, and the body swaps and
// rewraps them after the defer registered. The deferred read must see the FINAL values.
func declDeferObserver(x, y fmt.Stringer) (result string) {
	defer func() {
		result = "final:" + x.String() + "/" + y.String()
	}()
	x, y = y, x
	x, n := swap(x)
	_ = n
	return ""
}

// declClosureWrite: the closure WRITES the param and the body reads it after the call — the
// write-visibility direction a snapshot copy silently loses.
func declClosureWrite(s fmt.Stringer) string {
	set := func() { s = label{name: "closure-set"} }
	s, n := swap(s)
	set()
	return fmt.Sprintf("%s n=%d", s.String(), n)
}

// litRedeclare: the declRedeclare shape on a FUNC LITERAL's own parameter — the nested closure
// captures the literal's param, the literal's body-top-level `:=` redeclares it after, and the
// nested closure invoked after the write must see the new value (the literal's prologue must
// declare the box, the convFuncLit sibling of the declaration-param preamble).
func litRedeclare(seed fmt.Stringer) (string, string) {
	f := func(s fmt.Stringer) (string, string) {
		get := func() string { return s.String() }
		before := get()
		s, n := swap(s)
		_ = n
		after := get()
		return before, after
	}
	return f(seed)
}

// declSliceRedeclare: the same routing for an inherently-heap SLICE param — the closure must
// observe the reassigned (grown) slice, not a snapshot of the incoming one.
func declSliceRedeclare(v []int) (int, int) {
	sum := func() int {
		t := 0
		for _, e := range v {
			t += e
		}
		return t
	}
	before := sum()
	v, extra := grow(v)
	after := sum() + extra
	return before, after
}

func grow(v []int) ([]int, int) {
	return append(v[:len(v):len(v)], 99), 1
}

func main() {
	a := label{name: "alpha"}
	b := label{name: "beta"}

	before, after := declRedeclare(a)
	fmt.Println("declRedeclare:", before, after)

	fmt.Println("declDeferObserver:", declDeferObserver(a, b))

	fmt.Println("declClosureWrite:", declClosureWrite(b))

	before, after = litRedeclare(a)
	fmt.Println("litRedeclare:", before, after)

	x, y := declSliceRedeclare([]int{1, 2, 3})
	fmt.Println("declSliceRedeclare:", x, y)
}
