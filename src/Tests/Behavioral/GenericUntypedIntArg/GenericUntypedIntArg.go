package main

import "fmt"

// Index mirrors slices.Index: a generic over `S ~[]E` taking an untyped-int literal in
// the E (element) slot. `Index(ints, 20)` must infer E = Go int (→ C# nint); a bare C#
// `20` (System.Int32) would drive C# inference to E=int and `slice<nint>` then fails the
// `~[]int` constraint (CS0315/CS0411). The byte call must infer E=byte from a bare `2`.
func Index[S ~[]E, E comparable](s S, v E) int {
	for i := range s {
		if s[i] == v {
			return i
		}
	}
	return -1
}

// appendAll mirrors slices.Insert's variadic element slot: every trailing untyped-int
// literal fills the `...E` parameter, so each must be typed at E's instantiation
// (nint / byte / int32) for C# inference to bind S and E consistently.
func appendAll[S ~[]E, E any](s S, v ...E) S {
	return append(s, v...)
}

// numbers is a named slice type — the same shape as slices' []int-backed callers, to
// confirm the cast is chosen from the resolved element type, not the surface slice type.
type numbers []int

func main() {
	ints := []int{10, 20, 30, 40}
	// untyped int literal in the E slot -> E must be nint (Go int)
	fmt.Println(Index(ints, 20)) // 1
	fmt.Println(Index(ints, 99)) // -1

	// named slice type over int
	ns := numbers{5, 6, 7}
	fmt.Println(Index(ns, 7)) // 2

	// byte element type -> E must be byte from a bare literal
	bs := []byte{1, 2, 3}
	fmt.Println(Index(bs, 2)) // 1

	// int32 (rune) element type -> a bare C# int literal already IS System.Int32,
	// so this stays a plain literal (no cast, the no-noise arm).
	rs := []int32{100, 200, 300}
	fmt.Println(Index(rs, 200)) // 1

	// variadic element literals across widths
	fmt.Println(appendAll(ints, 50, 60))  // [10 20 30 40 50 60]
	fmt.Println(appendAll(bs, 4, 5))       // [1 2 3 4 5]
	fmt.Println(appendAll(numbers{1}, 2))  // [1 2]
}
