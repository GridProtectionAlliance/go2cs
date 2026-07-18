// NativeIntWideConstElement guards a computed CONSTANT arithmetic expression whose folded value
// overflows int32, typed Go `int`, in NON-assignment positions — a composite-literal element, a
// call argument, and a var initializer. strings' SplitN table (`{"", "T", math.MaxInt / 4, …}`)
// put `math.MaxInt / 4` in an `n int` struct field, and the fold emitted a bare
// `2305843009213693951L` — a C# `long` with no implicit conversion to the `nint` field (CS1503).
// The fold now carries its own cast in the `(nint)(…L)` form the assignment path already
// recognizes, so NativeIntWideConstAssign's emissions stay byte-identical.
//
// Two sibling shapes are guarded here too (sort's test-suite conversion):
//   - An IN-RANGE typed-int constant whose emitted arithmetic an operand fold WIDENED to `long`
//     (`maxswap: 1<<31 - 1` — the untyped inner shift folds to `2147483648L`): the whole
//     expression now carries the same `(nint)(…)` wrap. A named-const-ref operand pair
//     (`maxInt - maxInt`) renders via Untyped* wrappers, which narrow themselves — no wrap.
//   - A FLOAT literal inside a constant expression resolved to an INTEGER context
//     (search_test's `{"descending 7", 1e9, …, 1e9 - 7}` against `n, i int` fields): the
//     literal now renders its integer form (`1000000000 - 7`), keeping the arithmetic C# `int`.
// Verified vs Go.
package main

import "fmt"

const maxInt = 1<<63 - 1 // untyped; mirrors math.MaxInt on 64-bit targets

type splitTest struct {
	name string
	n    int
}

var tests = []splitTest{
	{"quarter", maxInt / 4},   // composite-literal element -> (nint)(2305843009213693951L)
	{"eighth", maxInt/8 + 1},  // folded operator-expression element
	{"boundary", 1<<31 - 1},   // in-range value, widened operand -> (nint)(2147483648L - 1)
	{"efloat", 1e9 - 7},       // float literal in int context -> 1000000000 - 7
	{"escale", 5e8 * 2},       // float literal under multiply -> 500000000 * 2
	{"zero", maxInt - maxInt}, // named-ref operands render as wrappers; stays unwrapped
}

//go:noinline
func half(n int) int { return n / 2 }

func main() {
	for _, tt := range tests {
		fmt.Println(tt.name, tt.n)
	}

	fmt.Println(half(maxInt / 2))   // call argument
	fmt.Println(half(maxInt/4 - 3)) // call argument, folded operator expression
	fmt.Println(half(1<<31 - 1))    // call argument, in-range widened operand
	fmt.Println(half(2e9 - 8))      // call argument, float literal in int context

	var local int = maxInt / 16 // var initializer with an explicit int type
	fmt.Println(local)

	var boundary int = 1<<31 - 1 // assignment position of the widened in-range shape
	fmt.Println(boundary)
}
