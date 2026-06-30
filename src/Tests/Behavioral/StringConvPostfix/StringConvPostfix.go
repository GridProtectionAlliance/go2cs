// Regression test for a postfix operator applied to a `string(...)` conversion.
//
// Unlike a numeric basic, Go's `string` maps to the member-accessible golib `@string` struct, so a
// `string(x)` conversion result CAN be the receiver of a postfix — the variadic-string spread
// `string(r)...`, an index `string(b)[i]`, or `len(string(b))`. The "reduce redundant cast
// parentheses" beautify treated every basic target alike and dropped the outer wrap, so
// `((@string)(rune)ch).ꓸꓸꓸ` became `(@string)(rune)ch.ꓸꓸꓸ` — the spread `.ꓸꓸꓸ` then bound to the
// inner operand `ch` (a `nint`) instead of the cast (CS1061, uncompilable). The converter now keeps
// the defensive outer parens for a `string` target.
package main

import "fmt"

func main() {
	// string(rune)... — variadic spread of a string conversion (the exact regression).
	var b []byte
	for ch := rune('A'); ch <= 'E'; ch++ {
		b = append(b, string(ch)...)
	}
	fmt.Println(string(b)) // ABCDE

	// string(bytes)[index] — index a string conversion result.
	bs := []byte{'H', 'i'}
	fmt.Println(string(bs)[0]) // 72

	// len(string(bytes)) — a string conversion as a call argument.
	fmt.Println(len(string(bs))) // 2

	// string(rune)... spreading a multi-byte rune (3 UTF-8 bytes).
	var m []byte
	m = append(m, string(rune('世'))...)
	fmt.Println(len(m)) // 3
}
