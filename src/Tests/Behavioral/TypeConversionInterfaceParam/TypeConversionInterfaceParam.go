package main

import (
	"fmt"
)

// Define an interface with a method that takes an anonymous struct
type DataProcessor interface {
	Process(data struct {
		ID    int
		Name  string
		Valid bool
	})
}

// Implement the interface
type Processor struct{}

func (p Processor) Process(data struct {
	ID    int
	Name  string
	Valid bool
}) {
	fmt.Printf("Processing ID: %d, Name: %s, Valid: %t\n", data.ID, data.Name, data.Valid)
}

func main() {
	// Create an instance of Processor
	var p DataProcessor
	p = Processor{}

	// Define the same anonymous struct inline and pass it to the method
	data := struct {
		ID    int
		Name  string
		Valid bool
	}{ID: 1, Name: "Alice", Valid: true}

	// Call the method with the dynamically defined struct
	p.Process(data)
}
