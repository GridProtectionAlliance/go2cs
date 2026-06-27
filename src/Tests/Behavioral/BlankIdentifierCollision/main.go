// Regression test for blank-identifier name-collision handling (CS0102).
//
// A package that declares blank `_` constants (e.g. to skip iota values) AND a blank
// `func _()` (a common stringer/compile-time-assertion idiom) used to break: the name-
// collision pass saw `_` as both a named element and a method name, flagged it as colliding,
// and Δ-prefixed every `_` to the same `Δ_` — so the multiple blank constants collided in C#
// (CS0102 "already contains a definition for 'Δ_'"). The fix excludes the blank identifier
// from collision analysis; each blank gets a unique generated name instead.
package main

import "fmt"

type Code int

const (
	A Code = iota // 0
	_             // 1 (skipped)
	B             // 2
	_             // 3 (skipped)
	C             // 4
)

// A blank function — the real-world trigger (go's stringer emits one as a compile-time
// assertion). Its mere presence puts `_` in both the named-element and method-name sets,
// which is what used to drive the spurious collision. It is given a unique generated name so a
// `_ = expr` discard in its body stays a discard rather than binding to the method group (CS1656).
func _() {
	if A+B+C < 0 {
		panic("unreachable")
	}
	x := A
	_ = x // a discard inside `func _()` — would bind to the method `_` without the rename
}

func main() {
	fmt.Println(A, B, C)
}
