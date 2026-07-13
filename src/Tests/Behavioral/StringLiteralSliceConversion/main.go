package main

import "fmt"

// Guards how a Go `[]byte("literal")` / `[]rune("literal")` conversion of a bare string *literal*
// (not a string variable) is emitted. A plain-text `[]byte` literal feeds the zero-allocation `u8`
// ROM span straight into the slice — `slice<byte>("hello"u8)` — skipping the intermediate heap
// `@string` the older `slice<byte>((@string)"hello")` form allocated. A `[]rune` literal keeps the
// `@string` form (it needs `@string`'s rune decoding, not raw UTF-8 bytes); a high-`\xHH`-byte
// `[]byte` literal keeps the byte-array-backed `@string` (its bytes do not round-trip through `u8`);
// and a string *variable* is already an `@string`. See convCallExpr.go / golib builtin.slice.
func main() {
	// Bare string-literal conversions: []byte → slice<byte>("…"u8), []rune → slice<rune>((@string)"…").
	bs := []byte("hello")
	rs := []rune("héllo")

	fmt.Println(len(bs), string(bs))
	fmt.Println(len(rs), string(rs))

	// Round-trip and a direct fmt of the literal conversion.
	fmt.Println([]byte("hi"))
	fmt.Println([]rune("aΩ"))

	// A raw (backtick) string literal is also a plain-text `u8` span — slice<byte>(@"ab"u8).
	fmt.Println([]byte(`ab`))

	// A high-\xHH-byte literal keeps the byte-array-backed @string form (u8 cannot carry those bytes).
	fmt.Println([]byte("\xff\xfe"))

	// String-variable conversions still work (the already-correct path) — same results.
	s := "hello"
	fmt.Println(string([]byte(s)) == string(bs))
}
