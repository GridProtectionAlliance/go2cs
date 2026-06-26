// Regression test for the Go 1.21 `min` and `max` built-ins.
//
// Go added `min` and `max` as predeclared built-in functions in Go 1.21. They accept one or
// more ordered arguments of the same type and return the smallest / largest. The converter
// emits the calls verbatim (`min(...)` / `max(...)`), so they relied on golib's `builtin`
// static class providing matching generic methods. It previously had none, so any converted
// package using `min`/`max` failed to compile with CS0103 ("the name 'min' does not exist").
// golib now provides generic `min`/`max` constrained to IComparable<T> (covers the numeric
// primitives and @string). crypto/subtle's XORBytes (`min(len(x), len(y))`) was the trigger.
package main

import "fmt"

func main() {
	// Two-argument integer min/max (the most common form, e.g. crypto/subtle).
	fmt.Println(min(3, 7)) // 3
	fmt.Println(max(3, 7)) // 7

	// Variadic (three or more arguments).
	fmt.Println(min(5, 2, 9, 1, 4)) // 1
	fmt.Println(max(5, 2, 9, 1, 4)) // 9

	// Single argument is valid in Go.
	fmt.Println(min(42)) // 42

	// Floating-point.
	fmt.Println(min(2.5, 1.5)) // 1.5
	fmt.Println(max(2.5, 1.5)) // 2.5

	// Strings are ordered, so min/max apply lexicographically.
	fmt.Println(min("banana", "apple", "cherry")) // apple
	fmt.Println(max("banana", "apple", "cherry")) // cherry

	// Used with len(), the crypto/subtle pattern.
	x := []byte{1, 2, 3}
	y := []byte{1, 2, 3, 4, 5}
	n := min(len(x), len(y))
	fmt.Println(n) // 3
}
