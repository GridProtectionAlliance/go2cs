package main

import "fmt"

// Guards the converter defect where a Go `[]byte("literal")` / `[]rune("literal")` conversion of a
// bare string *literal* (not a string variable) was emitted as `slice<byte>("literal")` — a
// System.String argument with no matching overload (CS1503). The literal must be cast to golib's
// `@string` first, exactly as the string-variable path already does. See convCallExpr.go.
func main() {
	// Bare string-literal conversions — the regressed case.
	bs := []byte("hello")
	rs := []rune("héllo")

	fmt.Println(len(bs), string(bs))
	fmt.Println(len(rs), string(rs))

	// Round-trip and a direct fmt of the literal conversion.
	fmt.Println([]byte("hi"))
	fmt.Println([]rune("aΩ"))

	// String-variable conversions still work (the already-correct path) — same results.
	s := "hello"
	fmt.Println(string([]byte(s)) == string(bs))
}
