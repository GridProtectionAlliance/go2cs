package main

import "fmt"

// Pair is a generic type with two type parameters
type Pair[T, U any] struct {
	First  T
	Second U
}

func main() {
	// Instantiate the generic type with concrete types
	p := Pair[string, int]{
		First:  "answer",
		Second: 42,
	}

	fmt.Printf("Pair: %v, %v\n", p.First, p.Second)
}
