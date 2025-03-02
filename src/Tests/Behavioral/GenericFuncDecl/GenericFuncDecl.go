package main

import "fmt"

// Swap is a generic function that swaps two values of the same type
func Swap[T any](a, b T) (T, T) {
	return b, a
}

func main() {
	// Call the generic function with int type
	a, b := 10, 20
	a, b = Swap[int](a, b)
	fmt.Printf("After swap: a=%d, b=%d\n", a, b)

	// Type inference allows omitting the type argument
	x, y := "hello", "world"
	x, y = Swap(x, y)
	fmt.Printf("After swap: x=%s, y=%s\n", x, y)
}
