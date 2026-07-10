package main

import "fmt"

// interesting32 mirrors internal/fuzz mutator's table: the exact int32 minimum's OPERAND
// literal (2147483648) overflows int32 when classified alone, so `-2147483648` emitted as
// `-(nint)2147483648L`, which has no implicit conversion back to an int32 slot (CS0266).
// The sign-folded value fits int32 exactly, so the plain negated literal is emitted (C#
// special-cases the negated decimal int-min, digit separators included). Non-minimal
// negatives are controls on the untouched default path.
var interesting32 = []int32{-2147483648, -2_147_483_648, -2147483647, -100663046, -32769, 2147483647}

// interesting64: the exact int64 minimum's operand (9223372036854775808) does not even
// parse as int64, so it routed through the unsigned branch to `-(nuint)…UL` — and C# defines
// no unary minus on nuint (CS0023). Emitted as `-9223372036854775808L` (the matching long
// special case). -9223372036854775807 and -2147483649 are controls on the between-minima
// path (`-(nint)…L`, implicitly long-convertible — unchanged).
var interesting64 = []int64{-9223372036854775808, -9223372036854775807, -2147483649, 9223372036854775807}

func main() {
	for _, v := range interesting32 {
		fmt.Println(v)
	}

	for _, v := range interesting64 {
		fmt.Println(v)
	}

	// The Go-int (nint) minimum takes the cast form ((nint)(-9223372036854775808L)):
	// C# has no implicit long→nint conversion.
	nmin := -9223372036854775808
	nctl := -9223372036854775807
	fmt.Println(nmin, nctl, nmin < nctl)

	// The folded int32 minimum must round-trip through comparisons and arithmetic.
	m := interesting32[0]
	fmt.Println(m == -2147483648, m+1 == -2147483647)
}
