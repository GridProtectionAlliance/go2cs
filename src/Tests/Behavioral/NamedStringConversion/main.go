// NamedStringConversion guards converting a string LITERAL to a named type whose
// underlying type is string — `errorString("…")` where `type errorString string`.
// The literal renders as a `u8` ReadOnlySpan<byte>, which has no direct conversion to
// the named type (CS0030); the converter routes it through `@string`:
// `((errorString)(@string)"…"u8)`. This is the form runtime uses for every
// `panic(errorString("…"))` / `plainError("…")`.
package main

import "fmt"

type errorString string

func (e errorString) Error() string { return "err: " + string(e) }

type label string

func main() {
	var e error = errorString("kaboom")
	fmt.Println(e.Error()) // err: kaboom

	l := label("tag")
	fmt.Println(l, len(l)) // tag 3

	// The @string wrapper surface (reflect StructTag.Get shape - census F5): element
	// indexing, sub-slicing (stays the named type), and comparison against a literal.
	st := label("json,omitempty")
	fmt.Println(st[0], st[4]) // 106 44
	name := st[0:4]
	fmt.Println(name, name != "", name == "json") // json true true
}
