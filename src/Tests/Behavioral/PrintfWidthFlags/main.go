// Regression test (stub fmt scope): Printf/Sprintf honor flags, width and fixed-point
// precision in format verbs.
//
// The baseline stub fmt proxy matched only bare verbs (`%d`, `%s`, ...) — a flag such as
// the space in `% 4d` (leave a space for the sign of a positive number, combined with
// width padding) made the whole verb unrecognized, so the format text printed literally
// (original repro: FirstClassFunctions `k =% 4d`). Width and precision that did match
// were silently discarded. The stub now parses `%[flags][width][.precision]verb` and
// applies ' '/'+' sign flags, '-'/'0'/plain width padding, and %f fixed-point precision.
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
}
