// NativeIntWideConstElement guards a computed CONSTANT arithmetic expression whose folded value
// overflows int32, typed Go `int`, in NON-assignment positions — a composite-literal element, a
// call argument, and a var initializer. strings' SplitN table (`{"", "T", math.MaxInt / 4, …}`)
// put `math.MaxInt / 4` in an `n int` struct field, and the fold emitted a bare
// `2305843009213693951L` — a C# `long` with no implicit conversion to the `nint` field (CS1503).
// The fold now carries its own cast in the `(nint)(…L)` form the assignment path already
// recognizes, so NativeIntWideConstAssign's emissions stay byte-identical. Verified vs Go.
package main

import "fmt"

const maxInt = 1<<63 - 1 // untyped; mirrors math.MaxInt on 64-bit targets

type splitTest struct {
	name string
	n    int
}

var tests = []splitTest{
	{"quarter", maxInt / 4},  // composite-literal element -> (nint)(2305843009213693951L)
	{"eighth", maxInt/8 + 1}, // folded operator-expression element
}

//go:noinline
func half(n int) int { return n / 2 }

func main() {
	for _, tt := range tests {
		fmt.Println(tt.name, tt.n)
	}

	fmt.Println(half(maxInt / 2))   // call argument
	fmt.Println(half(maxInt/4 - 3)) // call argument, folded operator expression

	var local int = maxInt / 16 // var initializer with an explicit int type
	fmt.Println(local)
}
