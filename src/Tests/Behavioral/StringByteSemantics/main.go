package main

import "fmt"

func main() {
	// Range over a string yields the BYTE index of each rune's first byte. Every multi-byte
	// rune here is FOLLOWED by more content, so the yielded byte indices diverge from the rune
	// ordinals (a=0, 2-byte cent=1, b=3, 3-byte smiley=4, 4-byte clef=7, !=11) — an enumerator
	// yielding rune ordinals cannot match. s[i] additionally pins each index to the real byte
	// offset (it must read the rune's FIRST byte).
	s := "a¢b☺\U0001d11e!"
	for i, r := range s {
		fmt.Println(i, int32(r), int32(s[i]))
	}

	// Invalid sequences where a maximal-subpart decoder would swallow SEVERAL bytes as one
	// replacement: a CESU-8 surrogate half (ED A0 80), an overlong encoding (C0 AF), and a
	// TRUNCATED 3-byte prefix at end of string (E2 98 — a valid prefix, reported as incomplete
	// with BOTH bytes consumed by .NET). Go yields one U+FFFD per invalid BYTE, advancing a
	// single byte each time: indices must be 0..8 with no gaps.
	bad := "x" + string([]byte{0xED, 0xA0, 0x80}) + string([]byte{0xC0, 0xAF}) + "y" + string([]byte{0xE2, 0x98})
	for i, r := range bad {
		fmt.Println(i, int32(r))
	}

	// The same input through []rune(s): one U+FFFD per invalid byte — nine runes, not fewer.
	runes := []rune(bad)
	fmt.Println(len(runes))
	for i := range runes {
		fmt.Println(i, int32(runes[i]))
	}

	// []byte(s) COPIES: mutating the copy must never write through into the string.
	t := "abc"
	b := []byte(t)
	b[0] = 'X'
	fmt.Println(t, string(b))
}
