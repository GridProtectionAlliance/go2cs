package main

import "fmt"

type Box[T any] struct {
	v T
}

// Generic named array type whose element is itself a generic instantiation (Box[T]).
// Before the fix the converter collapsed the element to "Box" (dropping its <T>) and
// emitted the declaration as non-generic `partial struct table;` (dropping <T>).
type table[T any] [3]Box[T]

// Generic named array type whose element is the bare type parameter (a simple ident).
type vec[T any] [2]T

func main() {
	var t table[int]
	t[0].v = 5
	t[2].v = 9
	fmt.Println(t[0].v, t[2].v, len(t))

	var u vec[int]
	u[0] = 7
	u[1] = 8
	fmt.Println(u[0], u[1], len(u))
}
