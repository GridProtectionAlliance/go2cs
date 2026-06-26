package main

import (
	"fmt"
)

type Person struct {
	Name string
	Age  int
}

// settings is a package-global var whose type is inferred from an anonymous-struct
// composite literal. Its declaration must use the lifted named type, not raw
// `struct{…}` text. Taking the address of a field also exercises that such a global
// is heap-boxed so the pointer aliases the original (not a copy).
var settings = struct {
	Verbose bool
	Retries int
}{Verbose: true, Retries: 3}

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

	// Package-global anonymous-struct var: read its fields, then mutate one through a
	// pointer to a field and confirm the global itself changed (the pointer aliases it).
	fmt.Println("\n=== Package-Global Anonymous Struct ===")
	fmt.Printf("settings: Verbose=%t Retries=%d\n", settings.Verbose, settings.Retries)

	pRetries := &settings.Retries
	*pRetries = 5
	fmt.Printf("after &settings.Retries=5: *p=%d global=%d\n", *pRetries, settings.Retries)
}
