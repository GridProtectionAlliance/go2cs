package main

import "fmt"

// Element mirrors container/list's Element: an exported `any` field that holds
// whatever value was stored into it.
type Element struct {
	Value any
}

// push boxes v into the interface field, exactly as container/list's PushBack(1)
// does. An untyped-int argument (`push(1)`) defaults to Go `int`, which go2cs boxes
// as nint (System.IntPtr).
func push(v any) *Element {
	return &Element{Value: v}
}

func main() {
	e := push(1)

	// The regression under guard: an interface value holding a boxed int compared
	// against an untyped int literal. Before the converter fix the literal boxed as
	// System.Int32 while the field held nint (System.IntPtr), so golib's reflective
	// AreEqual saw a runtime-type mismatch and reported them UNEQUAL — this branch
	// wrongly printed the BUG line (container/list TestIssue6349).
	if e.Value != 1 {
		fmt.Println("BUG: e.Value != 1")
	} else {
		fmt.Println("ok: e.Value == 1")
	}

	fmt.Println(e.Value == 1) // true
	fmt.Println(e.Value != 1) // false
	fmt.Println(1 == e.Value) // true  (literal on the left operand)
	fmt.Println(e.Value == 2) // false (different value)

	n := push(-5)
	fmt.Println(n.Value == -5) // true  (negative literal, a unary-const operand)
	fmt.Println(n.Value == 5)  // false

	// A directly var-declared interface, no helper indirection.
	var x any = 42
	fmt.Println(x == 42) // true
	fmt.Println(x != 42) // false
}
