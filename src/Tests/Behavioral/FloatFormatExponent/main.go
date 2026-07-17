package main

import "fmt"

// Go renders a float's %v (and %g) as strconv.FormatFloat(f, 'g', -1, bitSize): the shortest
// decimal digits that round-trip, then 'e' form when the decimal exponent is < -4 or >= 6,
// otherwise 'f' form. The exponent is always signed and at least two digits ("1e+06", "1e-05").
// .NET's default/"R" formatting shares the shortest-round-trip digits but uses different
// thresholds and an unpadded uppercase "E+308" form, so the two disagree on both ends.
func main() {
	// --- %v across the high threshold (exponent >= 6 switches to 'e' form) ---
	var e5 float64 = 1e5
	var e6 float64 = 1e6
	var million float64 = 1000000.0
	var justUnder float64 = 999999.0
	var manyDigits float64 = 1234567.0
	var mixed float64 = 123456.789
	fmt.Println(e5, e6, million, justUnder)
	fmt.Println(manyDigits, mixed)

	// --- %v across the low threshold (exponent < -4 switches to 'e' form) ---
	var em4 float64 = 1e-4
	var em5 float64 = 1e-5
	fmt.Println(em4, em5)
	fmt.Println(0.0001, 0.00001)

	// --- large exponents: two- vs three-digit exponent rendering ---
	var e20 float64 = 1e20
	var e21 float64 = 1e21
	var e22 float64 = 1e22
	fmt.Println(e20, e21, e22)

	// --- float64 extremes (math.MaxFloat64 / math.SmallestNonzeroFloat64 values, spelled as
	// literals: the converted math package's constants are currently rendered lossily) ---
	var maxFloat64 float64 = 1.7976931348623157e+308
	var smallestNonzeroFloat64 float64 = 5e-324
	var tiniestNormal float64 = 2.2250738585072014e-308
	fmt.Println(maxFloat64, smallestNonzeroFloat64, tiniestNormal)

	// --- negative and zero values ---
	var zero float64 = 0.0
	negZero := -zero // runtime negation: an untyped -0.0 constant would fold to +0
	fmt.Println(zero, negZero)
	fmt.Println(-e6, -em5, -maxFloat64, -1.5)

	// --- float32 must use the shortest round-trip for SINGLE, not double ---
	var f32 float32 = 0.1
	var f32e6 float32 = 1e6
	var f32em5 float32 = 1e-5
	var maxFloat32 float32 = 3.4028235e+38
	var smallestNonzeroFloat32 float32 = 1e-45
	fmt.Println(f32, f32e6, f32em5)
	fmt.Println(maxFloat32, smallestNonzeroFloat32)
	fmt.Println(float32(1.0/3.0), 1.0/3.0)

	// --- %v / %g agree; %e and %f have their own defaults (prec 6) ---
	fmt.Printf("[%v] [%g] [%e] [%f]\n", e6, e6, e6, e6)
	fmt.Printf("[%v] [%g] [%e] [%f]\n", em5, em5, em5, em5)
	fmt.Printf("[%v] [%g] [%e] [%f]\n", mixed, mixed, mixed, mixed)

	// --- explicit precision on each verb ---
	fmt.Printf("[%.3g] [%.3e] [%.3f]\n", mixed, mixed, mixed)
	fmt.Printf("[%.0f] [%.1e] [%.10g]\n", mixed, mixed, mixed)

	// %g promotes a zero precision to one significant digit, and that promotion also feeds the
	// exponent-form decision, so "%.0g" of 0.7 stays "0.7" instead of rounding away to "0"
	fmt.Printf("[%.0g] [%.0g] [%.0G] [%.0e]\n", 0.7, -0.7, 0.7, 0.7)

	// --- uppercase verbs spell the exponent 'E'; three-digit exponents keep all three ---
	fmt.Printf("[%G] [%E] [%G] [%E]\n", e6, e6, smallestNonzeroFloat64, maxFloat64)

	// --- negative zero keeps its sign through every verb ---
	fmt.Printf("[%v] [%g] [%e] [%f] [%.2f]\n", negZero, negZero, negZero, negZero, negZero)
	fmt.Printf("[%v] [%g] [%e] [%f] [%.2f]\n", zero, zero, zero, zero, zero)

	// --- float32 through the verbs ---
	fmt.Printf("[%v] [%g] [%e] [%f]\n", f32, f32, f32, f32)
	fmt.Printf("[%v] [%g]\n", maxFloat32, smallestNonzeroFloat32)

	// --- Sprint / Sprintf share the same rendering ---
	fmt.Println(fmt.Sprint(e6), fmt.Sprint(em5), fmt.Sprint(negZero))
	fmt.Println(fmt.Sprintf("%v|%g", maxFloat64, smallestNonzeroFloat64))
}
