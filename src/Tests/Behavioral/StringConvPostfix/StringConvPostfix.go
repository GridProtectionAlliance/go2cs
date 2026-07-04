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

	// A string LITERAL spread — `append(b, "prefix: "...)` (runtime error.go's message builder).
	// The literal renders as a `"…"u8` ReadOnlySpan<byte>, which has no spread property; the
	// converter wraps it as the member-accessible @string (the same wrap string(r)... uses).
	var eb []byte
	eb = append(eb, "runtime error: "...)
	eb = append(eb, "oops"...)
	fmt.Println(string(eb), len(eb)) // runtime error: oops 19

	// A string-literal CONCAT as an object-vararg argument (runtime stack.go's newline+tab
	// print-join shape): the u8 suppression must propagate INTO the BinaryExpr's operands —
	// both halves otherwise render as `"..."u8` spans, which cannot box to object and have
	// no `+` operator.
	tag := 7
	fmt.Println("g="+"\tm=", tag) // g=<TAB>m= 7
	fmt.Println("a" + "b" + "c")  // abc — nested concat, all plain strings

	// string(named-byte-slice) hops through the written underlying (strings replace.go
	// appendSliceWriter shape): the wrapper converts only to slice<byte>, and
	// slice<byte>->string is a second user conversion C# will not chain (CS0030).
	var w sink
	w = append(w, 'h', 'i')
	fmt.Println(string(w), len(w)) // hi 2

	// An ASTRAL rune literal (beyond the BMP) cannot be a C# char literal - it emits as
	// the code point (html's entity table, CS1012 x133); BMP literals keep their source
	// text verbatim.
	glyphs := map[string]rune{
		"frak": '\U0001D504',
		"ae":   '\U000000C6',
	}
	fmt.Println(glyphs["frak"], glyphs["ae"], string(glyphs["frak"])) // 120068 198 𝔄
}

// sink mirrors strings.appendSliceWriter: a named byte slice converted to string.
type sink []byte
