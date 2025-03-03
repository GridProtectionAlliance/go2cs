package main

import "fmt"

// Result is a generic type to represent a computation result
type Result[T any] struct {
	Value T
	Error error
}

// Container is a struct with fields that use generic types
type Container struct {
	IntResult    Result[int]
	StringResult Result[string]
	FloatValues  []Result[float64]       // Slice of generic type
	Mappings     map[string]Result[bool] // Map with generic value type
}

func main() {
	// Create a Container with various generic field types
	container := Container{
		IntResult: Result[int]{
			Value: 42,
			Error: nil,
		},
		StringResult: Result[string]{
			Value: "success",
			Error: nil,
		},
		FloatValues: []Result[float64]{
			{Value: 3.14, Error: nil},
			{Value: 2.71, Error: nil},
		},
		Mappings: map[string]Result[bool]{
			"completed": {Value: true, Error: nil},
			"verified":  {Value: false, Error: fmt.Errorf("verification pending")},
		},
	}

	// Access fields that use generic types
	fmt.Printf("Int result: %d\n", container.IntResult.Value)
	fmt.Printf("String result: %s\n", container.StringResult.Value)
	fmt.Printf("First float value: %f\n", container.FloatValues[0].Value)
	fmt.Printf("Completion status: %t\n", container.Mappings["completed"].Value)
}
