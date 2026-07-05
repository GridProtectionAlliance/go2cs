// Regression test: a DEFINED type whose underlying is an INTERFACE (`type Token any`, like
// encoding/xml's Token, or a defined type over a named interface) has exactly the interface's
// method set and can carry no methods of its own (Go forbids an interface receiver). It must be
// emitted as a `global using` ALIAS to that interface, not a `[GoType] partial struct` wrapper —
// a struct wrapper over `any` (= object) admits no implicit conversion FROM a concrete value
// (C# bars user-defined conversions from object), so `point -> Token` was CS0029 (16× in
// encoding/xml). The empty-interface target aliases to `object`; a named-interface target aliases
// to that interface.
package main

import "fmt"

// Defined type over the empty interface — aliased to object.
type Token any

// A named interface, and a defined type OVER it — aliased to the interface.
type Stringer interface {
	String() string
}

type Named Stringer

type point struct{ x, y int }

func (p point) String() string { return fmt.Sprintf("(%d,%d)", p.x, p.y) }

func describe(t Token) string {
	switch v := t.(type) {
	case int:
		return fmt.Sprintf("int:%d", v)
	case string:
		return "str:" + v
	case point:
		return "pt:" + v.String()
	}
	return "?"
}

func main() {
	var t Token = 42 // concrete int -> Token (aliased object)
	fmt.Println(describe(t))

	msg := "hi"
	var ts Token = msg // string var -> Token (implicit box; the encoding/xml assignment pattern)
	fmt.Println(describe(ts))

	fmt.Println(describe(point{1, 2})) // concrete struct -> Token (the StartElement -> Token case)

	var n Named = point{3, 4} // concrete point -> Named (defined over the Stringer interface)
	fmt.Println(n.String())
}
