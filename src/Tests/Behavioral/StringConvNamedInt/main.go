// Regression test: `string(x)` where x is a NAMED integer type (`type Delim rune`, encoding/json's
// Delim.String) is a code-point → UTF-8 conversion in Go. The named type renders as a [GoType]
// wrapper struct with no direct @string conversion, so the emitted `(@string)(d)` was CS0030. The
// converter now hops through the underlying integer first — `(@string)(rune)(d)` — matching Go's
// code-point semantics.
package main

import "fmt"

type Delim rune // named type over rune

type Code int // named type over int

func (d Delim) String() string { return string(d) }

func main() {
	d := Delim('{')
	fmt.Println(d.String()) // { (via string(d) on the receiver var)

	var d2 Delim = 65
	fmt.Println(string(d2)) // A (code point 65)

	var c Code = 0x4E2D         // a CJK code point
	fmt.Println(string(c))      // 中
	fmt.Println(len(string(c))) // 3 (UTF-8 byte length)
}
