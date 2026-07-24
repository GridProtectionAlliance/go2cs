package main

import "fmt"

// Guards CANONICAL typed-nil boxing (reflection-bridge Phase-3, X2): a nil pointer
// converted into interface space keeps its Go dynamic type — any((*T)(nil)) != nil,
// %T prints *T, typed nils of one type are equal while different types are not, and
// a type switch matches the nil-holding case. The old emission boxed a bare null,
// erasing the type.

type AErr struct{ n int }

func (e *AErr) Error() string { return "a" }

type BErr struct{ n int }

func (e *BErr) Error() string { return "b" }

func main() {
	// Bare any boxing of a typed nil keeps the type.
	var x any = (*int)(nil)
	fmt.Printf("%T\n", x)
	fmt.Println("x==nil", x == nil)

	// Two typed nils of the same type compare equal as interfaces.
	var y any = (*int)(nil)
	fmt.Println("x==y", x == y)

	// Pointer-level compares still see nil.
	var p *int
	fmt.Println("p==typednil", p == (*int)(nil))

	// error interfaces holding DIFFERENT typed nils: non-nil, and unequal to each other.
	var e1 error = (*AErr)(nil)
	var e2 error = (*BErr)(nil)
	fmt.Println("e1==nil", e1 == nil)
	fmt.Println("e2==nil", e2 == nil)
	fmt.Println("e1==e2", e1 == e2)

	// The same typed nil across the any/error boundary compares equal.
	var ea any = (*AErr)(nil)
	fmt.Println("ea==e1", ea == e1)

	// A type switch matches the nil-holding case with a nil pointee.
	switch v := e1.(type) {
	case *AErr:
		fmt.Println("switch-AErr", v == nil)
	default:
		fmt.Println("switch-other")
	}
}
