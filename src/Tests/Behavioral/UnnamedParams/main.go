package main

import "fmt"

type note struct{ x int }

// Unnamed parameters (Go allows `func(*T)` / `func(int)` with no parameter name). An unnamed
// POINTER parameter previously emitted a broken implicit deref (`ref var  = ref Ꮡ.val;`) and an
// empty box name (`ж<note> Ꮡ`). It is never referenced in the body, so it needs no deref — just a
// valid placeholder name. The runtime package hits this (sigqueue_note `func sigNoteSetup(*note)`).
var count int

func setup(*note) { count += 1 }
func discard(int) { count += 10 }

func main() {
	var n note
	setup(&n)
	discard(5)
	fmt.Println(count)
}
