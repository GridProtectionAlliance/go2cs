// UntypedFloatConstExpr verifies that an untyped FLOAT constant whose value comes from a constant
// EXPRESSION survives conversion with its exact value intact. Such a constant has no source literal
// for the converter to re-emit, so the folded go/constant value must be rendered directly — and
// go/constant's display form (Value.String()) shortens to ~6 significant digits, which silently
// emitted a DIFFERENT number (math.MaxFloat64 arrived as 1.79769e+308, LESS than the real maximum).
//
// Values are printed as IEEE 754 BIT PATTERNS rather than as floats on purpose: the property under
// test is that the value is exact, and bit patterns compare exactly without depending on how floats
// are formatted.
package main

import (
	"fmt"
	"math"
)

// The exact constant-expression forms used by Go's own math/const.go.
const (
	MaxFloat32             = 0x1p127 * (1 + (1 - 0x1p-23))
	SmallestNonzeroFloat32 = 0x1p-126 * 0x1p-23
	MaxFloat64             = 0x1p1023 * (1 + (1 - 0x1p-52))
	SmallestNonzeroFloat64 = 0x1p-1022 * 0x1p-52
)

// Folded arithmetic over a long literal — the `Log10E = 1 / Ln10` shape, where the quotient exists
// only as a go/constant value.
const (
	Ln10   = 2.30258509299404568401799145468436420760110148862877297603332790
	Log10E = 1 / Ln10
	Pi     = 3.14159265358979323846264338327950288419716939937510582097494459
	twoPi  = 2 * Pi
	halfPi = Pi / 2
	third  = 1.0 / 3.0
)

// MyFloat exercises conversion of a negated, imported, named untyped-float constant to a
// defined float type. The C# representation of math.Sqrt2 is an UntypedFloat wrapper, so the
// generated conversion must explicitly hop through float64 before converting to MyFloat.
type MyFloat float64

func (f MyFloat) Abs() float64 {
	if f < 0 {
		return float64(-f)
	}
	return float64(f)
}

// isInf mirrors Go's math.IsInf (math/bits.go), which decides infinity by comparing against
// MaxFloat64. A truncated MaxFloat64 is smaller than the real maximum, so every finite value
// between the two is misreported as infinite.
func isInf(f float64, sign int) bool {
	return sign >= 0 && f > MaxFloat64 || sign <= 0 && f < -MaxFloat64
}

// bits64 prints a float64 constant's IEEE 754 bit pattern alongside whether it matches the expected
// pattern. The bits are printed as a DECIMAL integer and the expectation is carried in the source as
// hex: this keeps the check exact and independent of float (and %x) formatting, while a regression
// still shows the value it actually got rather than a bare "false".
func bits64(label string, got uint64, want uint64) {
	fmt.Println(label, got, got == want)
}

func bits32(label string, got uint32, want uint32) {
	fmt.Println(label, got, got == want)
}

func main() {
	fmt.Println("-- named float conversion --")
	f := MyFloat(-math.Sqrt2)
	fmt.Println(f.Abs())

	fmt.Println("-- float64 constant expressions (IEEE 754 bits, want-match) --")
	bits64("MaxFloat64            ", math.Float64bits(MaxFloat64), 0x7fefffffffffffff)
	bits64("SmallestNonzeroFloat64", math.Float64bits(SmallestNonzeroFloat64), 0x1)
	bits64("Ln10                  ", math.Float64bits(Ln10), 0x40026bb1bbb55516)
	bits64("Log10E                ", math.Float64bits(Log10E), 0x3fdbcb7b1526e50e)
	bits64("Pi                    ", math.Float64bits(Pi), 0x400921fb54442d18)
	bits64("twoPi                 ", math.Float64bits(twoPi), 0x401921fb54442d18)
	bits64("halfPi                ", math.Float64bits(halfPi), 0x3ff921fb54442d18)
	bits64("third                 ", math.Float64bits(third), 0x3fd5555555555555)

	fmt.Println("-- float32 constant expressions (IEEE 754 bits, want-match) --")
	bits32("MaxFloat32            ", math.Float32bits(MaxFloat32), 0x7f7fffff)
	bits32("SmallestNonzeroFloat32", math.Float32bits(SmallestNonzeroFloat32), 0x1)

	fmt.Println("-- exact-value identities --")
	fmt.Println("MaxFloat64 == literal:", float64(MaxFloat64) == 1.7976931348623157e+308)
	fmt.Println("SmallestNonzeroFloat64 == literal:", float64(SmallestNonzeroFloat64) == 5e-324)
	fmt.Println("MaxFloat32 == literal:", float32(MaxFloat32) == 3.4028235e+38)
	fmt.Println("Log10E == literal:", float64(Log10E) == 0.4342944819032518)

	// The IsInf regression: the real maximum must NOT read as infinite, and the truncated
	// 6-digit value must sort strictly below it.
	fmt.Println("-- IsInf boundary --")
	fmt.Println("isInf(MaxFloat64):", isInf(1.7976931348623157e+308, 1))
	fmt.Println("isInf(-MaxFloat64):", isInf(-1.7976931348623157e+308, -1))
	fmt.Println("isInf(truncated 1.79769e+308):", isInf(1.79769e+308, 1))
	fmt.Println("truncated < MaxFloat64:", 1.79769e+308 < float64(MaxFloat64))
}
