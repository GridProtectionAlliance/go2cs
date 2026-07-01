// NativeIntWideConstAssign guards a computed CONSTANT arithmetic RHS whose folded value overflows int32,
// assigned to a NATIVE-width integer LHS (uintptr/uint/int → C# nuint/nint). Go's `pattern = 1<<maxBits
// - 1` (runtime mbitmap) folds `1<<maxBits` to `144115188075855872L` (a signed C# long, > int32) and the
// whole RHS stays `long`, which has no implicit conversion to the native target (CS0266). A `UL`/`(nuint)`
// suffix wouldn't help (ulong→nuint is also explicit). The converter now wraps the whole RHS in the
// native target's cast — `(uintptr)(144115188075855872L - 1)`. Values fit the 64-bit native type; verified
// vs Go.
package main

import "fmt"

const maxBits = 57 // 2^57 = 144115188075855872, well past int32

//go:noinline
func run() (uintptr, uint, int) {
	var p uintptr
	p = 1<<maxBits - 1 // uintptr = large const -> (uintptr)(…L - 1)
	var u uint
	u = 1<<maxBits + 5 // uint (nuint) = large const
	var n int
	n = 1<<maxBits - 100 // int (nint, signed) = large const
	return p, u, n
}

func main() {
	p, u, n := run()
	fmt.Println(p) // 144115188075855871
	fmt.Println(u) // 144115188075855877
	fmt.Println(n) // 144115188075855772
}
