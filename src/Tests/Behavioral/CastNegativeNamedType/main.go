// Regression test: a Go conversion of a NEGATIVE value to a named or using-alias type —
// x/text/unicode/bidi's `level(-1)`, archive/tar's `int64(-1)`.
//
// C# parses `(T)-value` as a CAST only when T is a keyword primitive type. For a using-ALIAS
// (int64=long, uint64=ulong, …) or a `[GoType]` NAMED type (`level`), `(level)-1` is instead parsed
// as the type `level` MINUS `1` — CS0075 ("to cast a negative value, you must enclose the value in
// parentheses") and CS0119 ("'level' is a type, which is not valid in the given context"). The
// converter must parenthesize the operand — `(level)(-1)` — for a non-keyword cast target.
package main

import "fmt"

type level int8

// neg returns a named-typed negative conversion.
func neg() level {
	return level(-1)
}

// negWide uses an alias-typed (int64) negative conversion inside an expression.
func negWide(n int) int64 {
	return int64(-2) * int64(n)
}

func main() {
	lvl := level(-1) // declaration-context conversion
	fmt.Println(int(lvl))
	fmt.Println(int(neg()))
	fmt.Println(negWide(3))
}
