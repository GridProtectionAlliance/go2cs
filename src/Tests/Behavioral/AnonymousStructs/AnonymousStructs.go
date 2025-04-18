package main

import (
	"fmt"
)

type Person struct {
	Name string
	Age  int
}

// Function that takes an anonymous struct as a parameter
func processAnonymousStruct(data struct {
	Name string
	Age  int
}) {
	fmt.Printf("Processing: %s, %d years old\n", data.Name, data.Age)
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
	
	fmt.Println("Named struct with identical fields:", ok) // Will be false
	
	// Using the anonymous struct as a function parameter
	fmt.Println("\n=== Function Parameter Tests ===")

	// Pass anonymous struct literal directly
	processAnonymousStruct(struct {
		Name string
		Age  int
	}{Name: "Charlie", Age: 40})
	
	// Pass our existing anonymous struct
	processAnonymousStruct(anonPerson)
	
	// Try to pass our named struct
	// This works because the function parameter is a structural specification
	// not a nominal type requirement!
	processAnonymousStruct(namedPerson)
}
