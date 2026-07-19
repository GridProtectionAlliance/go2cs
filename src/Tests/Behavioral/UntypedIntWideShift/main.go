package main

import "fmt"

// Package-level UNTYPED-INT constants are emitted as golib UntypedInt fields (not folded literals).
// Shifting a COMPOUND expression built from them (e.g. `bias - 1`) invokes UntypedInt.operator<< /
// operator>>: the converter keeps the compound left operand typed UntypedInt, so the shift runs on
// the 64-bit backing field. (A bare-identifier operand like `bias << n` is instead cast to the
// shift's result type first, so it would not exercise the operators.)
const bias = 1023

const bits = 0x7FF8000000000000 // a sample float64 exponent+mantissa bit pattern (fits int64)

func main() {
	// Runtime shift counts, all > 31 (the slice defeats constant folding). A shift that narrowed
	// UntypedInt to 32 bits would mask each count (count & 31) and produce the wrong result.
	s := []int{52, 40, 33}

	// UntypedInt.operator<< -- 64-bit left shifts (the Ldexp `mant << exp` shape).
	fmt.Println(uint64((bias - 1) << s[0])) // 1022 << 52 ; masked-32 would give 1022 << 20
	fmt.Println(uint64((bias - 1) << s[1])) // 1022 << 40 ; masked-32 would give 1022 <<  8

	// UntypedInt.operator>> -- 64-bit right shifts (the Frexp `bits >> exp` shape).
	fmt.Println(uint64((bits - 1) >> s[0])) // (bits-1) >> 52 ; masked-32 would give >> 20
	fmt.Println(uint64((bits - 1) >> s[2])) // (bits-1) >> 33 ; masked-32 would give >>  1
}
