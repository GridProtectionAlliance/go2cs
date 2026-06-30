// Regression test: a Go range value variable reassigned inside the loop body.
//
// Go lets a range variable be reassigned (it is a per-iteration copy), but a C# `foreach`
// iteration variable is read-only — `r -= 0x10000` on the foreach var is CS1656. The converter
// iterates a temp and declares the range var as a mutable local copy in the body
// (`foreach (var (_, rᴛ1) in s) { var r = rᴛ1; … }`). Mirrors runtime's os_windows.go UTF-16
// surrogate-pair encoder, which subtracts 0x10000 from the rune in place.
package main

import "fmt"

func main() {
	s := "AB\U0001F600\U0001F601C"
	var out []uint16
	for _, r := range s {
		if r < 0x10000 {
			out = append(out, uint16(r))
		} else {
			r -= 0x10000
			out = append(out, uint16(0xD800+(r>>10)))
			out = append(out, uint16(0xDC00+(r&0x3FF)))
		}
	}
	for _, u := range out {
		fmt.Printf("%d ", u)
	}
	fmt.Println()
	fmt.Println("len", len(out))
}
