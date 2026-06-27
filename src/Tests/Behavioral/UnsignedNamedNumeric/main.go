// Regression test: a named numeric type with an unsigned `uint`/`uintptr` underlying maps to
// C# `nuint`. The generated numeric type (NumericTypeTemplate) must NOT emit a unary `-`
// operator for it — `-value.m_value` is invalid on `nuint` (CS0023), even though Go permits
// unary minus on unsigned (it wraps, == 0 - x). IsUnsignedType omitted `nuint`/`uint`, so
// `type Experiment uint` (internal/trace/event) emitted the invalid operator. Go's unary minus
// on the value is lowered by the converter to `(T)0 - x`, so the operator is unneeded.
package main

import "fmt"

type Flags uint // -> num:nuint

type Mask uintptr // -> num:uintptr (already covered; included for breadth)

func main() {
	var a Flags = 6
	var b Flags = 2
	fmt.Println(a + b) // 8
	fmt.Println(a - b) // 4
	fmt.Println(a * b) // 12
	fmt.Println(a / b) // 3
	fmt.Println(-a + a) // 0 (unsigned wrap-around)

	var m Mask = 5
	fmt.Println(m | 2) // 7
}
