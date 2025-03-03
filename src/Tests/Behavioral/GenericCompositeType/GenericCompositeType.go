package main

import "fmt"

// Option represents an optional value
type Option[T any] struct {
	value T
	valid bool
}

func NewOption[T any](value T) Option[T] {
	return Option[T]{
		value: value,
		valid: true,
	}
}

func NewEmptyOption[T any]() Option[T] {
	var zero T
	return Option[T]{
		value: zero,
		valid: false,
	}
}

func main() {
	// Pointer to a generic type (StarExpr)
	optionPtr := &Option[string]{
		value: "hello",
		valid: true,
	}
	fmt.Printf("Option value: %s, valid: %t\n", optionPtr.value, optionPtr.valid)

	// Slice of a generic type (ArrayType)
	options := []Option[int]{
		NewOption(42),
		NewEmptyOption[int](),
		NewOption(100),
	}
	fmt.Printf("Options count: %d\n", len(options))

	// Map with generic key and value types (MapType)
	cache := map[string]Option[float64]{
		"pi":  NewOption(3.14159),
		"e":   NewOption(2.71828),
		"phi": NewEmptyOption[float64](),
	}
	fmt.Printf("Cache size: %d\n", len(cache))

	// Channel of a generic type (ChanType)
	resultChan := make(chan Option[bool], 1)
	resultChan <- NewOption(true)
	result := <-resultChan
	fmt.Printf("Channel result valid: %t\n", result.valid)
}
