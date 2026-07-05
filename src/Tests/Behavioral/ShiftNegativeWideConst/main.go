// Regression test: a left-shift of a NEGATIVE constant whose result type is a wide integer
// (int64/uint64/…), as in archive/tar's `fitsInBase256` and debug/dwarf's sign-extension —
// `-1 << bits` typed as int64.
//
// A wide shift type does not promote to `int` in a C# shift, so the converter casts the LEFT operand
// to that type: `(int64)-1 << bits`. But `int64` is a using-ALIAS for `long`, not a C# keyword, so
// `(int64)-1` is parsed as the type `int64` MINUS `1` (CS0119: "'long' is a type … not valid in the
// given context"), not a cast. The operand must be parenthesized — `(int64)(-1)` — when it leads with
// a unary `+`/`-`. A non-negative operand (`(uint64)1 << 63`) keeps the bare form (no golden churn).
package main

import "fmt"

// mask left-shifts the negative constant -1 with an int64 result type.
func mask(bits int) int64 {
	return -1 << uint(bits)
}

// umask shifts an all-ones uint64 — the companion wide-shift form (leads with `~`, not a sign).
func umask(bits uint) uint64 {
	return ^uint64(0) << bits
}

func main() {
	fmt.Println(mask(4))
	fmt.Println(mask(8))
	fmt.Println(umask(60))
}
