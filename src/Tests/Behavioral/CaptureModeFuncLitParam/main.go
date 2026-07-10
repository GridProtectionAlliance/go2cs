// CaptureModeFuncLitParam guards the ENTRY-TIME heap boxing of a FUNCTION LITERAL's own VALUE
// parameter on which the literal's body calls a capture-mode (direct-ж) pointer-receiver
// method — the func-literal analogue of CaptureModeValueParam, which covers declaration
// parameters (the original fix walked only *ast.FuncDecl params, so a literal's param got no
// box and the call rendered the raw value against the method's only ж<T> receiver form —
// CS1929). The literal's signature takes the incoming value as `tʗp` and its first statement
// re-declares `ref var t = ref heap(tʗp, out var Ꮡt);`, so body uses hit the boxed storage
// and the call routes through Ꮡt. Shapes covered: an assigned literal, an IIFE, a deferred
// call in the argument-passing form (`defer func(t Tally) {…}(x)`), a nested closure
// capturing the boxed param (box-ref, never a value snapshot: the closure's writes must be
// seen by the literal body afterwards), and a literal mixing a boxed value param with a
// variadic tail (the two `ʗp` prologues compose). Write-visibility is checked in both
// directions — a body write before the call must be seen by the callee, and the callee's
// writes through the receiver pointer must be seen by the rest of the body — and the caller's
// copy must stay untouched (Go passes parameters by value).
package main

import "fmt"

// Tally's pointer-receiver Add defers a write that touches the receiver, so its whole body is
// emitted inside the defer/recover execution context — the capture-mode (direct-ж) form whose
// only receiver overload is the box ж<Tally>.
type Tally struct {
	total int
	log   string
}

func (t *Tally) Add(n int) {
	defer func() {
		t.log = fmt.Sprintf("%s+%d", t.log, n)
	}()
	t.total += n
}

// runDeferred hosts the deferred literal so the shape under test stays the LITERAL's own
// value parameter `t` (the outer IIFE composition trips an unrelated pre-existing
// defer-argument capture defect — see the CS8175 chip filed 2026-07-10).
func runDeferred(base Tally) {
	defer func(t Tally) {
		t.Add(4)
		fmt.Println("deferred:", t.total, t.log)
	}(base)
}

func main() {
	// An ASSIGNED literal whose body calls the direct-ж method on its own value parameter.
	f := func(t Tally, m int) (int, string) {
		t.total++ // write BEFORE the call: the callee must observe it
		t.Add(m)
		return t.total, t.log // reads AFTER the call: must see the callee's writes
	}
	seed := Tally{total: 5, log: "s"}
	total, log := f(seed, 3)
	fmt.Println("assigned:", total, log)
	fmt.Println("caller copy untouched:", seed.total, seed.log)

	// An IIFE: the immediately-invoked form emits parameter NAMES only (the delegate cast
	// supplies the types), so the `ʗp` rename and prologue must reach that path too.
	total2, log2 := func(t Tally) (int, string) {
		t.Add(7)
		return t.total, t.log
	}(Tally{total: 1, log: "i"})
	fmt.Println("iife:", total2, log2)

	// A DEFERRED call in the argument-passing form: the value is captured at defer time and
	// boxed at entry when the deferred body runs.
	base := Tally{total: 2, log: "d"}
	runDeferred(base)
	fmt.Println("deferred source untouched:", base.total, base.log)

	// A NESTED closure capturing the literal's boxed param: Go shares the ONE parameter
	// variable between the closure and the literal body, so the closure must capture the box
	// (a value snapshot would orphan Add's writes).
	g := func(t Tally) (int, string) {
		bump := func() {
			t.Add(9)
		}
		bump()
		t.total++ // must see the closure's writes (12 → 13)
		return t.total, t.log
	}
	total4, log4 := g(Tally{total: 3, log: "n"})
	fmt.Println("nested:", total4, log4)

	// A literal with BOTH a boxed value param and a variadic tail: the variadic `ʗp` slice
	// prologue and the heap-box `ʗp` prologue compose at the top of the same block.
	h := func(t Tally, ns ...int) (int, string) {
		for _, n := range ns {
			t.Add(n)
		}
		return t.total, t.log
	}
	total5, log5 := h(Tally{total: 10, log: "v"}, 1, 2)
	fmt.Println("variadic:", total5, log5)
}
