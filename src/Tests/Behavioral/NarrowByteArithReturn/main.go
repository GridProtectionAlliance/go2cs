// NarrowByteArithReturn guards the narrow-integer-arithmetic cast on a RETURN — the sibling of the
// assignment-path fix (NarrowByteArithFirstOperandCast). `func lowerASCII(c byte) byte { return c +
// ('a'-'A') }` (runtime env_posix) evaluates byte arithmetic at byte width (wrapping) in Go, but C#
// promotes `byte + int` (the untyped char const) to `int`, so the returned value can't implicitly
// convert back to the byte result type (CS0266). The narrow cast is applied on the assignment /
// value-spec paths but had been omitted on the return path. It is now applied when the result type is
// narrow and the returned expr is binary/unary arithmetic — a bare ident (`return c`), a call, or an
// already-narrowed return is untouched. Verified vs Go including a wrapping case.
package main

import "fmt"

//go:noinline
func lower(c byte) byte {
	if 'A' <= c && c <= 'Z' {
		return c + ('a' - 'A') // byte + untyped-const arith return -> needs (byte) cast
	}
	return c // bare ident return -> NOT cast
}

//go:noinline
func wrapRet(x byte) byte {
	return x + x + 1 // wrapping byte-arith return
}

func main() {
	fmt.Println(lower('A'), lower('Z'), lower('a')) // 97 122 97
	fmt.Println(wrapRet(200))                       // 200+200+1 = 401 mod 256 = 145
}
