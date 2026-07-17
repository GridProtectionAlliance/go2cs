package main

import (
	"fmt"
	"math"
)

func main() {
	// --- float64: sign of zero survives the bit cast ---
	z := 0.0
	nz := -z // runtime negation; the constant -0.0 folds to +0
	fmt.Println(math.Float64bits(z))
	fmt.Println(math.Float64bits(nz))

	// --- float64: +/-Inf constructed from bits ---
	pinf := math.Float64frombits(0x7FF0000000000000)
	ninf := math.Float64frombits(0xFFF0000000000000)
	fmt.Println(pinf > 1e308)  // a real +Inf value, not just a bit pattern
	fmt.Println(ninf < -1e308) // a real -Inf value
	fmt.Println(math.Float64bits(pinf))
	fmt.Println(math.Float64bits(ninf))
	fmt.Println(math.Float64bits(1 / ninf)) // -Inf sign propagates: 1/-Inf == -0

	// --- float64: NaN payload preserved through the round-trip ---
	nan := math.Float64frombits(0x7FF8000000000001)
	fmt.Println(nan != nan)
	fmt.Println(math.Float64bits(nan))

	// --- float64: smallest subnormal ---
	sub := math.Float64frombits(1)
	fmt.Println(sub > 0)
	fmt.Println(math.Float64bits(sub))
	fmt.Println(math.Float64bits(5e-324))

	// --- float64: ordinary values and round-trip identity ---
	fmt.Println(math.Float64bits(1.0))
	fmt.Println(math.Float64frombits(0x400921FB54442D18))
	fmt.Println(math.Float64bits(math.Float64frombits(0x400921FB54442D18)) == 0x400921FB54442D18)

	// --- float64: cbrt's magic-constant bit trick (B1 = 715094163) ---
	x := 8.0
	t := math.Float64frombits(math.Float64bits(x)/3 + uint64(715094163)<<32)
	fmt.Println(math.Float64bits(t))

	// --- float32: sign of zero ---
	fz := float32(0.0)
	fnz := -fz
	fmt.Println(math.Float32bits(fz))
	fmt.Println(math.Float32bits(fnz))

	// --- float32: quiet-NaN payload and signaling-NaN bit preserved (no double transit) ---
	qnan := math.Float32frombits(0x7FC00001)
	snan := math.Float32frombits(0x7F800001)
	fmt.Println(qnan != qnan)
	fmt.Println(snan != snan)
	fmt.Println(math.Float32bits(qnan))
	fmt.Println(math.Float32bits(snan))

	// --- float32: smallest subnormal and round-trip identity ---
	fsub := math.Float32frombits(1)
	fmt.Println(fsub > 0)
	fmt.Println(math.Float32bits(fsub))
	fmt.Println(math.Float32bits(math.Float32frombits(0x3F9D70A4)) == 0x3F9D70A4)
	fmt.Println(math.Float32bits(float32(1.0)))
	fmt.Println(math.Float32frombits(0x40490FDB))
}
