// Regression test (stub fmt scope): Printf/Sprintf honor flags, width and fixed-point
// precision in format verbs.
//
// The baseline stub fmt proxy matched only bare verbs (`%d`, `%s`, ...) — a flag such as
// the space in `% 4d` (leave a space for the sign of a positive number, combined with
// width padding) made the whole verb unrecognized, so the format text printed literally
// (original repro: FirstClassFunctions `k =% 4d`). Width and precision that did match
// were silently discarded. The stub now parses `%[flags][width][.precision]verb` and
// applies ' '/'+' sign flags, '-'/'0'/plain width padding, and %f fixed-point precision.
//
// The `%x`/`%X` verbs and the `#` (alternate form) flag were a later gap in the same proxy:
// every verb but `%f`/`%F` fell back to ToString, so `%#x` of a uint64 printed the decimal
// value with no `0x` prefix. They are exercised here because their padding rules are the
// flag/width behavior this test owns — and they are not the obvious ones: Go's `0` flag on
// an integer sets the DIGIT count rather than padding to the width, and does not count the
// `0x` prefix (`%#08x` of 255 is the 10-char "0x000000ff", where `%#8x` is "    0xff"),
// while on a byte sequence it zero-pads the whole rendering ("000x6162").
package main

import "fmt"

func main() {
	// space flag + width (the original defect: `% 4d`)
	for _, v := range []int{5, 99, 999, 1234, -5, -999} {
		fmt.Printf("k =% 4d;\n", v)
	}

	// sign flags without width
	fmt.Printf("[% d] [% d] [%+d] [%+d]\n", 7, -7, 7, -7)

	// plain width, left justify, zero pad
	fmt.Printf("[%4d] [%-4d] [%04d] [%04d] [%+4d]\n", 42, 42, 42, -42, 7)

	// zero pad with a sign placeholder
	fmt.Printf("[% 04d] [%+04d]\n", 5, 5)

	// width on strings and %v
	fmt.Printf("[%6s] [%-6s] [%6v]\n", "ab", "ab", 12)

	// fixed-point precision, with and without width/flags
	fmt.Printf("[%.1f] [%0.1f] [%6.2f] [%-7.2f] [%.0f]\n", 45.678, 45.678, 3.14159, 3.14159, 2.71)

	// value wider than width is not truncated
	fmt.Printf("[%2d] [%2s]\n", 12345, "hello")

	// space flag is a no-op for negative numbers and strings
	fmt.Printf("[% d] [% 6s]\n", -3, "ab")

	// floating-point infinities render Go-style ("+Inf"/"-Inf", never "∞"): the sign is
	// inherent even for positives, the space flag demotes '+' to a space, and the '0' flag
	// falls back to space padding (Inf/NaN don't look like numbers)
	zero := 0.0
	posInf := 1.0 / zero
	negInf := -1.0 / zero
	nan := zero / zero
	posInf32 := float32(posInf)
	negInf32 := float32(negInf)

	fmt.Println(posInf, negInf, nan)
	fmt.Println(posInf32, negInf32)
	fmt.Printf("[%v] [%v] [%g] [%g]\n", posInf, negInf, posInf, negInf)
	fmt.Printf("[%e] [%e] [%f] [%f]\n", posInf, negInf, posInf, negInf)
	fmt.Printf("[%.2f] [%.2f] [%8.2f] [%-8.2f]\n", posInf, negInf, posInf, negInf)
	fmt.Printf("[%+v] [%+f] [% f] [% f]\n", posInf, posInf, posInf, negInf)
	fmt.Printf("[%08f] [%08f] [%08f]\n", posInf, negInf, nan)
	fmt.Printf("[%v] [%g] [%e] [%f]\n", posInf32, negInf32, posInf32, negInf32)
	fmt.Println(fmt.Sprint(posInf), fmt.Sprint(negInf))
	fmt.Printf("[%v] [%f] [%+f] [% f]\n", nan, nan, nan, nan)

	// %x/%X on integers: base 16, sign-magnitude ("-ff", never a two's-complement form),
	// with '#' adding the "0x"/"0X" prefix after any sign (the original repro is the
	// uint64 below, which printed as decimal "9218868437227405311")
	bits := uint64(9218868437227405311)
	fmt.Printf("[%x] [%X] [%#x] [%#X]\n", bits, bits, bits, bits)
	fmt.Printf("[%x] [%X] [%#x] [%#X]\n", 255, 255, 255, 255)
	fmt.Printf("[%x] [%X] [%#x] [%#X]\n", -255, -255, -255, -255)
	fmt.Printf("[%x] [%#x] [%x] [%#x]\n", 0, 0, 15, 15)

	// integer width padding: '#' counts toward a space-padded width, but the '0' flag is a
	// digit count that leaves room for the sign and excludes the prefix
	fmt.Printf("[%8x] [%-8x] [%08x] [%20x]\n", 255, 255, 255, bits)
	fmt.Printf("[%#8x] [%#-8x] [%#08x] [%#016x]\n", 255, 255, 255, bits)
	fmt.Printf("[%08x] [%#08x] [%#08X] [%04x]\n", -255, -255, -255, 255)
	fmt.Printf("[%+x] [% x] [%+#x] [%+08x] [% 08x]\n", 255, 255, 255, 255, 255)

	// integer precision is a minimum digit count and overrides the '0' flag; precision 0
	// applied to a 0 value prints no digits at all
	fmt.Printf("[%.4x] [%.4x] [%#.4x] [%8.4x] [%-8.4x]\n", 255, -255, 255, 255, 255)
	fmt.Printf("[%.0x] [%.0x] [%5.2x] [%05.2x]\n", 0, 255, 255, 255)

	// sized and unsigned integer types
	fmt.Printf("[%x] [%#x] [%x] [%x]\n", int8(-128), int8(-128), uint8(255), uint(255))
	fmt.Printf("[%x] [%#x]\n", uint64(18446744073709551615), int64(-1))

	// %x/%X on byte sequences (string, []byte) hex-encode bytewise — for a string that is
	// its UTF-8 bytes, not its chars ("ä" is c3 a4). The ' ' flag separates the bytes and
	// makes '#' prefix each one; precision limits the INPUT bytes, not the output width
	fmt.Printf("[%x] [%X] [%#x] [%#X]\n", "abc", "abc", "abc", "abc")
	fmt.Printf("[% x] [% X] [% #x]\n", "abc", "abc", "abc")
	fmt.Printf("[%x] [%#x] [% x]\n", []byte("abc"), []byte("abc"), []byte{1, 2})
	fmt.Printf("[%x] [%.2x] [%.2x] [%#.2x]\n", "äb", "äb", "abcd", "abcd")

	// byte-sequence padding zero-pads the whole rendering (prefix included), and an empty
	// sequence renders as padding alone — no "0x" prefix
	fmt.Printf("[%6x] [%-6x] [%06x] [%#08x] [%-08x]\n", "ab", "ab", "ab", "ab", "ab")
	fmt.Printf("[% 08x] [%12x] [%10.3x]\n", "ab", "abc", "abcdef")
	fmt.Printf("[%x] [%#x] [%8x] [%08x] [%#x]\n", "", "", "", "", []byte{})
}
