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
	tightenGuards()
}

// tightenGuards exercises the LOCAL-const declaration-tightening rules (a function-local
// untyped const whose every use resolves to ONE concrete basic type is declared at that
// type, casts suppressed — the localPrecision consts above) and, more importantly, the
// guards that must KEEP the Untyped* wrapper: mixed-type uses, a const feeding another
// const's initializer, and any participation in folded constant arithmetic.
func tightenGuards() {
	const mixed = 0.25 // float64 AND float32 uses -> stays UntypedFloat
	var f64 float64 = mixed
	var f32 float32 = mixed
	fmt.Println(f64, f32)

	const feeder = 3.5         // feeds another const's initializer -> stays UntypedFloat
	const derived = feeder * 2 // its own use below is single-type -> tightens
	fmt.Println(derived)

	const big = 1 << 62 // wide value, single int64 use -> tightens to the exact literal
	var n int64 = 1
	fmt.Println(n + big)

	const sh = 3 // shift-count use
	var v2 uint64 = 1
	fmt.Println(v2 << sh)

	const shifted = 3 // shifted-left operand with a non-constant count
	var k uint = 2
	fmt.Println(shifted << k)

	const localMarker = 0xFFFD // append element use (the utf16 pattern, local form)
	var u16 []uint16
	u16 = append(u16, localMarker)
	fmt.Println(u16[0])

	// NARROW shifted consts: a sub-int32 tightened const as the LEFT operand of a
	// non-constant shift KEEPS the width retype cast — C# promotes a narrow shifted
	// operand to int, so without `(byte)(…)` Go's wraparound at the declared width
	// is lost (200<<1 must wrap to 144 at byte width, not compute 400).
	const cb = 200
	var sh1 uint = 1
	var b byte = 1
	fmt.Println(b + cb<<sh1) // 145, NOT 401

	const c16 = 30000
	var i16 int16 = 1
	fmt.Println(i16 + c16<<sh1) // -5535 (int16 wrap), NOT 60001

	const cu16 = 60000
	var w16 uint16 = 1
	fmt.Println(w16 + cu16<<sh1) // 54465 (uint16 wrap), NOT 120001

	// A float-KIND constant (1e6 lexes as a float literal) whose uses are all INTEGER
	// contexts tightens to the int type and must emit the INTEGER value form — a C#
	// `1e6` double literal has no implicit conversion to nint (the go/printer
	// `const infinity = 1e6; p.nodeSize(x, infinity)` pattern).
	const infinity = 1e6
	var lineCount int = 3
	if lineCount < infinity {
		fmt.Println(lineCount + infinity) // 1000003
	}

	const localDefer = 42 // deferred-call argument use
	defer fmt.Println(localDefer)
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
