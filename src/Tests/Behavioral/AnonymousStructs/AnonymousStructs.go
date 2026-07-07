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

// cycleMemo mirrors encoding/json's sliceEncoder cycle detection: an IN-FUNCTION anonymous struct
// with an EMPTY `interface{}` field (holding a pointer, "always an unsafe.Pointer but avoids a
// dependency on package unsafe") plus an int length, memorized once into a variable and used as a
// map key (Go's `ptr := struct{…}{v.UnsafePointer(), v.Len()}; e.ptrSeen[ptr]`). The empty-interface
// field must convert to `any` — the converter previously lifted it to a named empty `[GoType("dyn")]`
// marker interface that no concrete value could satisfy, so constructing the struct from a pointer
// failed (CS1503). Reading the pointer back through the field and keying a set by the struct exercise
// both that the field is `any` and that the anonymous struct stays comparable.
func cycleMemo() {
	seen := map[any]struct{}{}
	a := 10

	memo := struct {
		ptr interface{}
		len int
	}{&a, 2}

	_, before := seen[memo] // absent before insert
	seen[memo] = struct{}{}
	_, after := seen[memo] // present after insert

	got := memo.ptr.(*int) // the empty-interface field round-trips as `any`
	fmt.Printf("cycleMemo: val=%d len=%d before=%t after=%t\n", *got, memo.len, before, after)
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

	// An IN-FUNCTION `var` decl over a slice of an anonymous struct: the declared type
	// must resolve to the lifted struct name (the initializer's conversion lifts it),
	// not raw `struct{…}` Go syntax (debug/plan9obj NewFile).
	fmt.Println("\n=== In-Function var Slice of Anonymous Struct ===")

	var sects = []struct {
		name string
		size uint32
	}{
		{"text", 100},
		{"data", 200},
		{"syms", 300},
	}

	total := uint32(0)

	for _, sect := range sects {
		total += sect.size
	}

	fmt.Printf("sections=%d total=%d first=%s\n", len(sects), total, sects[0].name)

	// Lifted in-function anonymous struct with an empty interface{} field, used as a map key.
	fmt.Println("\n=== Anonymous Struct With Empty Interface Field ===")
	cycleMemo()
}
