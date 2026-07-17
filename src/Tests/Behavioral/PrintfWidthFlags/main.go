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
//
// The sibling base verbs `%b`/`%o`/`%O` share that integer machinery with per-verb `#`
// forms: `%#b` gets a `0b` prefix (excluded from the `0`-flag digit count like `0x`),
// `%#o` instead guarantees a leading `0` DIGIT (so zero-padding absorbs it: `%#08o` of
// 255 is "00000377"), and `%O` always carries `0o` with the `#` rule layered beneath
// (`%#O` of 8 is "0o010"). Precision 0 of a 0 value drops everything — even the sign and
// prefix (`%#.0x` of 0 is ""). Floats get strconv's forms: `%b` is the raw mantissa with
// a power-of-two exponent and no exponent padding ("4503599627370496p-52", "8388608p-9"),
// ignoring precision; `%x`/`%X` is the hex-mantissa-exponent form ("0x1.91eb851eb851fp+01")
// with binary round-half-to-even at the precision's hex digit (carrying into the exponent:
// `%.0x` of 255.9999 is "0x1p+08"), and `%#x` restores trailing zeros against a
// six-significant-char budget that counts the 'x' itself ("0x1.8000p+00") — lowercase only,
// though `%#X` still forces the point ("0X1.P+00").
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

	// %b on integers: base 2 with the same sign/width/precision rules as %x; '#' adds a
	// "0b" prefix that is likewise excluded from the '0'-flag digit count
	fmt.Printf("[%b] [%b] [%b] [%b] [%#b] [%#b] [%#b]\n", 5, 255, -255, 0, 5, -5, 0)
	fmt.Printf("[%+b] [% b] [% #b] [%12b] [%-12b] [%012b] [%012b]\n", 5, 5, 5, 255, 255, 255, -255)
	fmt.Printf("[%#12b] [%#012b] [%+012b] [%#012b]\n", 255, 255, 255, -255)
	fmt.Printf("[%.12b] [%.12b] [%#.12b] [%16.12b] [%-16.12b] [%.0b] [%.0b]\n", 255, -255, 255, 255, 255, 0, 255)
	fmt.Printf("[%b] [%b] [%b] [%b]\n", int8(-128), uint8(255), uint64(18446744073709551615), int64(-9223372036854775808))

	// %o on integers: base 8; '#' guarantees a leading 0 DIGIT rather than a prefix, so
	// zero-padded digits absorb it ("%#08o" of 255 is "00000377", but "%#8o" is "    0377")
	fmt.Printf("[%o] [%o] [%o] [%o] [%#o] [%#o] [%#o] [%#o]\n", 8, 255, -255, 0, 8, -8, 0, 1)
	fmt.Printf("[%+o] [% o] [%+#o] [%8o] [%-8o] [%08o] [%08o]\n", 8, 8, 8, 255, 255, 255, -255)
	fmt.Printf("[%#8o] [%#08o] [%#-8o] [%#010o] [%#o] [%#5o]\n", 255, 255, 255, -8, 511, 511)
	fmt.Printf("[%.6o] [%.6o] [%#.6o] [%8.6o] [%.0o] [%.0o] [%#.0o] [%#.2o] [%#.2o]\n", 255, -255, 255, 255, 0, 255, 0, 8, 1)
	fmt.Printf("[%o] [%o] [%o]\n", int8(-128), uint8(255), uint64(18446744073709551615))

	// %O: octal with an always-present "0o" prefix (excluded from the '0'-flag digit count
	// like "0x"), and '#' layering its leading-0-digit rule beneath it ("0o010")
	fmt.Printf("[%O] [%O] [%O] [%O] [%#O] [%#O] [%#O] [% #O]\n", 8, 255, -255, 0, 8, -8, 0, 8)
	fmt.Printf("[%12O] [%-12O] [%012O] [%012O] [%+O] [% O] [%+012O]\n", 255, 255, 255, -255, 8, 8, 255)
	fmt.Printf("[%.6O] [%12.6O] [%#.6O] [%#12O] [%#012O]\n", 255, 255, 255, 255, 255)
	fmt.Printf("[%O] [%O]\n", uint64(18446744073709551615), int64(-9223372036854775808))

	// precision 0 of a 0 value prints nothing at all across the base verbs — even the
	// sign and '#'/"0o" prefix drop, and the width pads with spaces despite the '0' flag
	fmt.Printf("[%#.0x] [%+.0x] [%5.0x] [%#05.0x] [%#.0b] [%.0O] [%-3.0x]\n", 0, 0, 0, 0, 0, 0, 0)

	// %b on floats: strconv's decimalless scientific form with a power-of-two exponent —
	// the raw (unnormalized) mantissa in decimal, exponent unpadded ("p-9", "p+1"), the
	// subnormal exponent pinned (5e-324 is "1p-1074"), and precision ignored
	negZero := -zero
	fmt.Printf("[%b] [%b] [%b] [%b] [%b]\n", 1.0, 8.0, 0.5, 3.14159, -3.14159)
	fmt.Printf("[%b] [%b] [%b] [%b]\n", 0.0, negZero, 1e300, 5e-324)
	fmt.Printf("[%b] [%b] [%b] [%b]\n", float32(1.0), float32(0.1), float32(-2.5), float32(16384.0))
	fmt.Printf("[%b] [%b] [%b] [%.3b] [%#b]\n", posInf, negInf, nan, 3.14159, 1.5)
	fmt.Printf("[%25b] [%-25b] [%025b] [%025b] [%+b] [% b]\n", 1.5, 1.5, 1.5, -1.5, 1.5, 1.5)
	fmt.Printf("[%b] [%b]\n", 9007199254740992.0, 1.7976931348623157e308)

	// %x/%X on floats: strconv's hexadecimal-exponent form — mantissa normalized to a
	// leading 1 (even for subnormals), shortest form trimming trailing zero digits, a
	// two-digit-minimum exponent, and Inf/NaN rendering as ±Inf/NaN
	fmt.Printf("[%x] [%X] [%x] [%x] [%x] [%x]\n", 3.14, 3.14, -3.14, 1.0, 2.0, 0.5)
	fmt.Printf("[%x] [%x] [%x] [%x]\n", 0.0, negZero, 1e300, 5e-324)
	fmt.Printf("[%x] [%X] [%x] [%x]\n", float32(0.1), float32(0.1), float32(-2.5), float32(3.4028235e38))
	fmt.Printf("[%x] [%x] [%x] [%.2x] [%.2X]\n", posInf, negInf, nan, posInf, nan)
	fmt.Printf("[%x] [%x]\n", 255.0, 4503599627370495.5)

	// float %x precision rounds the binary mantissa half-to-even at that hex digit, with
	// the carry renormalizing into the exponent ("%.0x" of 255.9999 is "0x1p+08"); a
	// precision past the mantissa zero-fills
	fmt.Printf("[%.2x] [%.0x] [%.0x] [%.0x] [%.5x] [%.13x] [%.15x]\n", 3.14, 1.9, 1.5, 2.5, 3.14, 3.14, 3.14)
	fmt.Printf("[%.2x] [%.2x] [%.10x] [%.1x] [%.1x] [%.4x]\n", 0.0, 1.0, 3.14, 1.999999, 0.09999, 0.1)
	fmt.Printf("[%.3x] [%.1x] [%.0x]\n", 1.9999999, 1.99, 255.9999)

	// float %x width/sign flags: the '0' flag zero-pads the whole rendering naively
	// (unlike the integer digit-count reading), Inf stays space-padded
	fmt.Printf("[%20x] [%-20x] [%020x] [%020x] [%+x] [% x] [%+020x]\n", 1.5, 1.5, 1.5, -1.5, 1.5, 1.5, 1.5)
	fmt.Printf("[%030x] [%08x]\n", negInf, nan)

	// '#' on float %x restores trailing zeros against a six-significant-char budget that
	// counts the 'x' itself ("0x1.8000p+00"); %#X gets no budget but still forces the
	// decimal point ("0X1.P+00")
	fmt.Printf("[%#x] [%#x] [%#x] [%#x] [%#x] [%#x]\n", 1.0, 1.5, 3.14, 0.0, negZero, 255.0)
	fmt.Printf("[%#X] [%#X] [%#.2X] [%#.2x] [%#.0x] [%#020x] [%#16x] [%#x]\n", 1.0, 1.5, 3.14, 3.14, 3.14, 1.5, 1.5, -1.5)
}
