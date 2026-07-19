package main

import "fmt"

// A string constant used as a raw BYTE TABLE (math/bits' rev8tab shape): built by concatenation so it
// folds to one constant value with no single *ast.BasicLit, and holding bytes >= 0x80 that are NOT
// valid UTF-8. Go strings are byte sequences, so byte-indexing must return the exact bytes; a C#
// UTF-16/u8 string literal would UTF-8-re-encode each >=0x80 byte (0x80 -> 0xC2 0x80) and corrupt
// both indexing and len. Guards the byte-array-backed @string emission for such consts.
const revTab = "" +
	"\x00\x80\x40\xc0" +
	"\x20\xa0\x60\xe0"

func main() {
	for i := 0; i < len(revTab); i++ {
		fmt.Printf("%d ", revTab[i])
	}
	fmt.Println()
	fmt.Println("len:", len(revTab))
}
