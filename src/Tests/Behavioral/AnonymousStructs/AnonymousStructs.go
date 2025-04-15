package main

import (
	"fmt"
)

type Person struct {
	Name string
	Age  int
}

func main() {
	// Create values of both named and anonymous struct types
	namedPerson := Person{Name: "Alice", Age: 30}
	anonPerson := struct {
		Name string
		Age  int
	}{Name: "Bob", Age: 25}

	// Test with anonymous struct type
	var someInterface interface{} = anonPerson

	// Type assertion with the same anonymous struct type
	_, ok := someInterface.(struct {
		Name string
		Age  int
	})

	fmt.Println("Anonymous struct type assertion:", ok) // Should be true

	// Now test with named struct
	someInterface = namedPerson

	_, ok = someInterface.(struct {
		Name string
		Age  int
	})

	fmt.Println("Named struct with identical fields:", ok) // Will be false as your results showed
}
