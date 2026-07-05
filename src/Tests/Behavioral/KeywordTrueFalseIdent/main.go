package main

import "fmt"

// setFlag has a PARAMETER named `true` — `true`/`false` are Go PREDECLARED identifiers
// (shadowable), not keywords, but they ARE C# keywords. The parameter must be @-escaped
// (`bool @true`), or the raw `bool true` is a C# syntax error (text/template/parse's
// `newBool(pos Pos, true bool)`, CS1001/CS1003). Inside, `true` references the SHADOWING
// parameter, so it also escapes; a VALUE use of the predeclared `true`/`false` (the call
// arguments below) stays the bare C# literal.
func setFlag(true bool) string {
	if true {
		return "on"
	}
	return "off"
}

func main() {
	// The arguments `true`/`false` here are the PREDECLARED constants → bare C# literals.
	fmt.Println(setFlag(true), setFlag(false)) // on off

	// A LOCAL variable shadowing the predeclared `false`.
	false := 7
	fmt.Println(false + 1) // 8
}
