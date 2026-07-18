package main

import "fmt"

// Locks in Go's rune-to-string conversion of INVALID runes: a surrogate (0xD800-0xDFFF) or an
// out-of-range value (< 0 or > 0x10FFFF) encodes as U+FFFD (RuneError, bytes EF BF BD) — one
// replacement per invalid rune — rather than failing. The golib UTF-8 encoder previously threw
// on the int-to-Rune conversion for exactly those values (strings TestCaseConsistency builds a
// string of EVERY rune 0..MaxRune, surrogates included). Runtime values are used throughout so
// neither compiler constant-folds the conversions.

func main() {
	rs := []rune{'a', 0xD800, 0xDFFF, 0x110000, -1, 0x4E16}

	s := string(rs)
	fmt.Println(len(s))

	for i := 0; i < len(s); i++ {
		fmt.Println(s[i])
	}

	// Single-rune conversions route through the same encoder.
	for _, r := range rs {
		fmt.Println(len(string(r)))
	}
}
