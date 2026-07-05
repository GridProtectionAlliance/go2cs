// Regression test: logical operators (!, &&, ||) applied to a NAMED boolean type in a context
// that expects the named type — e.g. an interface return, as in go/constant's UnaryOp/BinaryOp
// (`case boolVal: return !y` / `return x && y`).
//
// Go's `!b`, `x && y`, `x || y` on a defined type whose underlying is bool (`type boolVal bool`)
// yield that SAME named type, so the result still satisfies the interface. The converter models
// such a type as a [GoType("bool")] struct that has an implicit bool conversion but NO logical
// operators, so a bare `!y` / `x && y` collapses to a plain C# `bool` — which cannot implicitly
// convert to the interface (CS0029), and `!` has no operator on the struct (CS0023). The converter
// must cast each operand through `bool`, apply the operator, then cast the result back to the named
// type: `((boolVal)(!(bool)y))` / `((boolVal)((bool)x && (bool)y))`.
package main

import "fmt"

type boolVal bool

// isSet gives boolVal a method so Value below is a NON-EMPTY interface — an empty interface would
// box the bare bool result without complaint and not exercise the fix.
func (b boolVal) isSet() bool {
	return bool(b)
}

// Value is an interface the named bool implements; the functions return it, forcing the named-type
// result of each logical operator to satisfy the interface.
type Value interface {
	isSet() bool
}

// unary negates a boolVal and returns it as the interface (mirrors go/constant's UnaryOp).
func unary(x Value) Value {
	y := x.(boolVal)
	return !y
}

// binary conjoins/disjoins two boolVals and returns the interface (mirrors go/constant's BinaryOp).
func binary(op string, x, y Value) Value {
	a := x.(boolVal)
	b := y.(boolVal)
	if op == "and" {
		return a && b
	}
	return a || b
}

func main() {
	var t Value = boolVal(true)
	var f Value = boolVal(false)

	fmt.Println(unary(t).isSet())          // false
	fmt.Println(unary(f).isSet())          // true
	fmt.Println(binary("and", t, f).isSet()) // false
	fmt.Println(binary("and", t, t).isSet()) // true
	fmt.Println(binary("or", t, f).isSet())  // true
	fmt.Println(binary("or", f, f).isSet())  // false
}
