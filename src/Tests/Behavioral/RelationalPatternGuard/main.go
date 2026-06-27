// Regression test: an expression-switch (switch-true) comparison case is converted to a C#
// relational/constant pattern (`case {} when x is <op> Y`) only when Y is a C# compile-time
// constant. When Y is a variable — `case x == y` (y a parameter) — or a const emitted as
// `static readonly` (untyped/cross-package), a relational pattern is invalid (CS9135); it must
// be a `when` guard. The converter previously always used the pattern. This is the math/nextafter
// `switch { case x == y: }` pattern.
package main

import "fmt"

func classify(x, y int) string {
	switch {
	case x < 0: // literal RHS -> relational pattern is fine
		return "neg"
	case x == y: // variable RHS -> must be a `when` guard, not `x is y`
		return "equal"
	case x > y: // variable RHS -> guard
		return "greater"
	}
	return "less"
}

func main() {
	fmt.Println(classify(-1, 5))
	fmt.Println(classify(5, 5))
	fmt.Println(classify(7, 3))
	fmt.Println(classify(2, 8))
}
