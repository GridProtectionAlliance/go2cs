package main

import (
	"fmt"
)

type boxedResult struct {
	value int
	tag   string
}

// selectEscape mirrors internal/fuzz coordinatorLoop (`c.crashMinimizing = &result`): a comm-clause
// binding whose ADDRESS is taken in the clause body escapes to the heap, so the emission must
// receive into a temp and box it (`ref var res = ref heap(resᴛ1, out var Ꮡres);`) — the body's
// `&res` renders `Ꮡres` (CS0103 without the box). Writes must be visible BOTH ways: through the
// escaped pointer into the bound var, and through the bound var out to a pointer read after the
// select ends. Every select here has exactly one ready buffered channel, so output is deterministic.
func selectEscape() {
	ch := make(chan boxedResult, 1)
	ch <- boxedResult{value: 7, tag: "orig"}

	var saved *boxedResult

	select {
	case res := <-ch:
		saved = &res
		saved.value = 42                           // write through the pointer...
		fmt.Println("escape:", res.value, res.tag) // ...visible in the bound var (42, not 7)
		res.tag = "mutated"                        // write the bound var...
	}

	fmt.Println("after:", saved.value, saved.tag) // ...visible through the escaped pointer (mutated)
}

// selectEscapeCommaOk exercises the (val, ok) comm-clause form with an escaping val. Like
// internal/fuzz (`c.crashMinimizing = &result` … `&result.entry`), BOTH the whole-var address
// and a field address are taken: the whole-var `&res` forces the heap box, and the field
// address then routes through it (`Ꮡres.of(…)`) so the write is visible in the bound var.
// (A clause taking ONLY `&res.value` is a separate, known escape-analysis gap — the field-only
// form copy-boxes — and is deliberately not exercised here.)
func selectEscapeCommaOk() {
	ch := make(chan boxedResult, 1)
	ch <- boxedResult{value: 3, tag: "ok-form"}

	var whole *boxedResult
	var field *int

	select {
	case res, ok := <-ch:
		whole = &res
		field = &res.value
		*field += 10
		fmt.Println("comma-ok:", res.value, res.tag, ok)
	}

	fmt.Println("field:", *field, whole.value)
}

// selectEscapeMixed pairs an escaping binding with a plain one in the same select — the plain
// clause must keep the direct `out var` form (no churn on non-escaping bindings).
func selectEscapeMixed() {
	a := make(chan boxedResult, 1)
	b := make(chan int, 1)
	a <- boxedResult{value: 1, tag: "x"}

	var keep *boxedResult

	select {
	case r := <-a:
		keep = &r
		keep.tag = "escaped"
		fmt.Println("mixed:", r.tag)
	case n := <-b:
		fmt.Println("plain:", n)
	}
}

func main() {
	selectEscape()
	selectEscapeCommaOk()
	selectEscapeMixed()
}
