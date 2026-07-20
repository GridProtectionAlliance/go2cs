package main

import "fmt"

// Unary negation on a TYPE PARAMETER. The converter lifts a Go constraint's operator needs onto
// C# System.Numerics interfaces; `-x` needs IUnaryNegationOperators<T, T>, which the numeric
// (Arithmetic) operator set did not carry — every generic doing this was CS0023
// ("Operator '-' cannot be applied to operand of type 'T'"). math/rand/v2's rand_test.go has
// exactly this shape.
func negate[T int | uint | int32 | uint32 | int64 | uint64](x T) T {
	return -x
}

// Negation reached through arithmetic, so the lift cannot be satisfied by a special case on a
// bare `-x` return.
func negateSum[T int | uint | int32 | uint32 | int64 | uint64](a T, b T) T {
	return -(a + b)
}

// A NAMED Go type over an unsigned underlying type: its go2cs-gen wrapper must also satisfy the
// lifted constraint. Go defines `-x` on unsigned as the wrap-around `0 - x`; C# has no unary minus
// for ulong at all, so the generated operator is written as that subtraction under `unchecked`.
type counter uint64

// A named type over a SIGNED underlying type takes the ordinary negation path.
type offset int32

func negateNamed[T ~uint64](x T) T {
	return -x
}

func negateNamedSigned[T ~int32](x T) T {
	return -x
}

func main() {
	fmt.Println(negate(5))
	fmt.Println(negate(int32(-7)))
	fmt.Println(negate(int64(1 << 40)))

	// Unsigned negation wraps, in both Go and the emitted C#.
	fmt.Println(negate(uint32(1)))
	fmt.Println(negate(uint64(0)))
	fmt.Println(negate(uint64(1)))
	fmt.Println(negate(uint(3)))

	fmt.Println(negateSum(10, 32))
	fmt.Println(negateSum(uint32(2), uint32(3)))

	fmt.Println(negateNamed(counter(0)))
	fmt.Println(negateNamed(counter(1)))
	fmt.Println(negateNamed(counter(1 << 63)))
	fmt.Println(negateNamedSigned(offset(9)))
	fmt.Println(negateNamedSigned(offset(-9)))

	// Double negation restores the original value on every width.
	fmt.Println(negate(negate(uint64(12345))))
	fmt.Println(negateNamed(negateNamed(counter(999))))
}
