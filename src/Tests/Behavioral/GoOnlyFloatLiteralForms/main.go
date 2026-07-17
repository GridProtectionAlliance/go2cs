// Regression test: Go float/imaginary literal forms that C# cannot parse must be re-rendered as
// decimal, not emitted verbatim with a D/F suffix pasted on. The converter wrote the literal's
// SOURCE text directly (`result.WriteString(value); result.WriteRune('D')`), which breaks for the
// two Go-only forms:
//
//	hex float      `0x1p-2`  -> `0x1p-2D`  : C# has no hex-float syntax at all (CS1002/CS1003).
//	trailing dot   `2.`      -> `2.D`      : C# requires fractional digits after the point.
//	               `1.e2`    -> `1.e2D`
//
// The imaginary arm had the SAME defect (`builtin.i(0x1p-2D)`), and with a hex INTEGER mantissa it
// was worse than a syntax error: `0x10i` emitted `builtin.i(0x10D)`, and `0x10D` is a VALID C# hex
// integer literal — 269, not 16 — so the value silently changed with no diagnostic.
//
// These forms are absent from the non-test Go stdlib corpus (Phase 3 compiles clean) but appear
// routinely in Go's own _test.go files (math tests) and in user code, so they gate Phase 4.
//
// Decimal forms must still round-trip VERBATIM (the visually-similar goal — `1.5e-3` must not
// flatten to `0.0015`); the tail of main covers that.
package main

import "fmt"

func main() {
	// --- hex floats, float64 (untyped default) ---
	a := 0x1p-2 // 0.25 — exact binary fraction
	b := 0x1.8p1
	c := 0x1.5555555555555p-2 // a long mantissa, exercising exact->float64 rounding
	fmt.Println(a, b, c)

	// --- trailing dot, float64 ---
	d := 2.
	e := 1.e2
	f := 3.e-2
	fmt.Println(d, e, f)

	// --- hex floats and trailing dot in a float32 context ---
	var g float32 = 0x1p-2
	var h float32 = 2.
	var i float32 = 1.e2
	fmt.Println(g, h, i)

	// The float32 emission must round the EXACT constant straight to float32. This value is
	// 1 + 2^-24 + a tiny residue: rounding exact->float64->float32 gives 1 (the float64 lands
	// exactly halfway, ties-to-even down), while Go's single exact->float32 rounding sees the
	// residue and rounds up to 1.0000001. Double-rounding in the converter prints 1 here.
	var j float32 = 0x1.0000010000000000001p0
	fmt.Println(j)

	// --- imaginary literals: hex float, hex INTEGER, trailing dot ---
	z := 0x1p-2i
	w := 0x10i // hex INT mantissa -> 16i; the `0x10D`=269 silent-corruption case
	v := 2.i
	fmt.Println(z, w, v)

	// --- imaginary in a complex64 context (the F-suffix overload of builtin.i) ---
	var c64 complex64 = 0x1p-2i
	var c64b complex64 = 0x10i
	fmt.Println(c64, c64b)

	// --- controls: decimal forms C# shares must survive VERBATIM, values unchanged ---
	k := 1.5e-3
	l := 0.25
	var m float32 = 0.1
	n := 2i
	fmt.Println(k, l, m, n)
}
