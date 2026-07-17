package main

import "fmt"

// Untyped package-level constants emit as static readonly Untyped* wrapper values;
// a `:=` from one must bind the local at Go's DEFAULT type for the constant, so a
// later conversion like string(codepoint) still compiles.
const replacementChar = '�' // untyped rune (mirrors unicode.ReplacementChar)
const scale = 2.5                // untyped float

// A high-precision float const must COMPILE to its exact value: go/constant's String()
// shortens to ~6 significant digits, so the emitted literal was silently truncated
// (`0.542857`) with the exact value surviving only in a comment. The source literal is
// now emitted verbatim (valid C#); a folded const expression emits the shortest exact
// round-trip form instead. This is the math cbrt constants pattern.
const cbrtC = 5.42857142857142815906e-01  // 19/35     = 0x3FE15F15F15F15F1
const cbrtD = -7.05306122448979611050e-01 // -864/1225 = 0xBFE691DE2532C834
const folded = 19.0 / 35.0                // folded expression: no single literal form

func main() {
	codepoint := replacementChar
	s := string(codepoint)
	fmt.Println(len(s), codepoint)

	factor := scale
	fmt.Println(factor * 2)

	// Would print 0.542857 / -0.705306 if the compiled literal were truncated.
	fmt.Println(cbrtC)
	fmt.Println(cbrtD)
	fmt.Println(folded)
	fmt.Println(localPrecision(2.0))
}

// localPrecision exercises the FUNCTION-LOCAL untyped const path (same emission arm,
// in-function branch).
func localPrecision(x float64) float64 {
	const c = 5.42857142857142815906e-01
	const d = -7.05306122448979611050e-01
	const smallestNormal = 2.22507385850720138309e-308 // 2**-1022 = 0x0010000000000000
	if x < smallestNormal {
		return d
	}
	return c + x*d
}
