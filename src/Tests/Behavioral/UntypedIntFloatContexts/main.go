// Regression test: an untyped int constant (emitted as the golib UntypedInt wrapper) used in a
// floating-point or complex context must VALUE-convert, not bit-reinterpret. The wrapper's
// implicit float operators formerly reinterpreted the int64 payload as IEEE-754 bits, so
// `var fl float64 = m` (m = 3) produced 1.5e-323 (the denormal double with bit pattern 3)
// instead of 3. Mixed int+float uses of ONE constant keep it a wrapper rather than letting
// the converter type it away.
package main

import "fmt"

// Package-level untyped int consts render as `static readonly UntypedInt` wrapper statics.
const pkgConst = 1000     // used in both integer and float contexts below
const hugeConst = 1 << 63 // exceeds int64 — emitted as a ulong literal (unsigned payload)

func main() {
	// A local untyped const used in BOTH integer and floating-point contexts.
	const m = 3
	var i int = m
	var fl float64 = m
	var f32 float32 = m
	fmt.Println(i, fl, f32) // 3 3 3

	// Arithmetic in the float domain seeded from the wrapper.
	var half float64 = m
	fmt.Println(half / 2) // 1.5

	// Package-level wrapper static in float and integer contexts.
	var pf float64 = pkgConst
	var pi int = pkgConst
	fmt.Println(pf/8, pi) // 125 1000

	// Unsigned-range payload: 1<<63 does not fit int64, so the wrapper holds uint64-flavored
	// bits; a float conversion must keep the uint64 magnitude, not read the bits as negative.
	// Scaled by a direct 1<<60 divisor (the float-context const-shift fold renders it exactly)
	// to keep %v out of exponent notation.
	var hf float64 = hugeConst
	var hu uint64 = hugeConst
	fmt.Println(hf/(1<<60), hu) // 8 9223372036854775808

	// Complex contexts (asserted via real/imag to stay independent of complex %v formatting).
	var c complex128 = m
	var c64 complex64 = m
	fmt.Println(real(c), imag(c), real(c64)) // 3 0 3

	// Negative payload through the float path.
	const neg = -7
	var nf float64 = neg
	var ni int = neg
	fmt.Println(nf/2, ni) // -3.5 -7
}
