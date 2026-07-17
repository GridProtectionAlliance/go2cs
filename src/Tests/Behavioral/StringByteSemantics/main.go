package main

import "fmt"

func main() {
	// Go range over a string yields the BYTE index of each rune's first byte: a multi-byte
	// rune advances the next index by its encoded length, and each INVALID byte yields one
	// U+FFFD advancing a single byte.
	s := "a" + string([]byte{0x80, 0x80}) + "☺"
	for i, r := range s {
		fmt.Println(i, int32(r))
	}

	// []byte(s) COPIES: mutating the copy must never write through into the string.
	t := "abc"
	b := []byte(t)
	b[0] = 'X'
	fmt.Println(t, string(b))

	// []rune(s) yields one U+FFFD PER INVALID BYTE (single-byte advance, like range).
	r := []rune("a\x80\x80b")
	fmt.Println(len(r), int32(r[1]), int32(r[2]), int32(r[3]))
}
