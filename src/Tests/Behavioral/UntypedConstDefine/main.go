package main

import "fmt"

// Untyped package-level constants emit as static readonly Untyped* wrapper values;
// a `:=` from one must bind the local at Go's DEFAULT type for the constant, so a
// later conversion like string(codepoint) still compiles.
const replacementChar = '�' // untyped rune (mirrors unicode.ReplacementChar)
const scale = 2.5                // untyped float

func main() {
	codepoint := replacementChar
	s := string(codepoint)
	fmt.Println(len(s), codepoint)

	factor := scale
	fmt.Println(factor * 2)
}
