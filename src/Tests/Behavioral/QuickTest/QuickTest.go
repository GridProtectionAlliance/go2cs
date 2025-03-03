package main

import "fmt"

// Option represents an optional value
type Option[T any] struct {
	value T
	valid bool
}

func main() {
	optionPtr := &Option[string]{
		value: "hello",
		valid: true,
	}
	
	fmt.Printf("Option value: %s, valid: %t\n", optionPtr.value, optionPtr.valid)
}
