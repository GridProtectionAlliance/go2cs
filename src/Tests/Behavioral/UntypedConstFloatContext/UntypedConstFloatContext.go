package main

import "fmt"

// A package-level untyped float constant is emitted as a golib UntypedFloat, which is what
// makes the `imaginary * gPi` arithmetic below exercise the Complex/UntypedFloat operator
// resolution (a folded literal would not).
const gPi = 3.141592653589793

// Guards contextual typing of untyped constants in float32/complex64 contexts: go/types
// resolves the context on the OUTERMOST constant expression only, leaving inner operands
// untyped, so the converter must propagate the context down (F vs D literal suffixes and
// the postfix .i()/complex() overload choice) instead of falling back to the untyped
// default float64/complex128.
func main() {
	var c64 complex64 = complex(2.5, -3.5) // builtin args through a unary minus
	var c64b complex64 = 2.5 - 3.5i        // binary operands: real constant + imaginary literal
	var a, b float32 = 2.5, -3.5           // negated float constant in direct assignment
	var nested float32 = -(1.5 + 2.0)      // context through parens and nested binary
	var quo float32 = 7.0 / 2.0            // quotient operands
	var c128 complex128 = 2.5 - 3.5i       // float64 context stays D
	var prec complex128 = 0.1 + 0.1i       // 0.1i must NOT round-trip through complex64
	var minf float32 = min(1.5, -2.5)      // min/max builtin args
	var maxf float32 = max(1.5, -2.5)
	var re float32 = real(2.5 - 3.5i)        // real/imag args are the complex counterpart
	var im float32 = imag(complex(2.5, -3.5))

	type meters float32
	var dist meters = -1.5 // named type context resolves through its underlying float32

	// UntypedFloat<->complex arithmetic: an imaginary literal multiplied by an untyped float
	// constant. This bound AMBIGUOUSLY (CS0034) while UntypedFloat converted implicitly to BOTH
	// float64 and complex128 — the complex operand could bind as `Complex * double` OR as
	// `UntypedFloat * UntypedFloat` (via the implicit complex->UntypedFloat conversion), and
	// neither was preferred. Those conversions are now explicit, so it resolves as `Complex * double`.
	cprod := 1i * gPi   // imaginary literal * untyped float const
	cprod2 := gPi * 2i  // untyped float const * imaginary literal
	csum := 1i + gPi    // and the additive form

	// Complex values print via their real/imag parts so this test guards only the
	// converter's constant typing, not golib fmt's Go-style complex rendering
	fmt.Println(real(c64), imag(c64))
	fmt.Println(real(c64b), imag(c64b))
	fmt.Println(a, b)
	fmt.Println(nested)
	fmt.Println(quo)
	fmt.Println(real(c128), imag(c128))
	fmt.Println(real(prec), imag(prec)) // old converter: imag printed 0.10000000149011612
	fmt.Println(minf, maxf)
	fmt.Println(re, im)
	fmt.Println(dist)
	fmt.Println(real(cprod), imag(cprod))
	fmt.Println(real(cprod2), imag(cprod2))
	fmt.Println(real(csum), imag(csum))
}
