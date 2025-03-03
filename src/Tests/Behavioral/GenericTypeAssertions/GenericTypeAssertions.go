package main

import "fmt"

// Box is a generic wrapper for any value
type Box[T any] struct {
	Value T
}

// GetValue is a function that returns an interface{} value
func GetValue(index int) interface{} {
	values := []interface{}{
		Box[int]{Value: 42},
		Box[string]{Value: "hello"},
		Box[float64]{Value: 3.14},
	}

	if index >= 0 && index < len(values) {
		return values[index]
	}
	return nil
}

func main() {
	// Type assertion with a generic type (TypeAssertExpr)
	value := GetValue(0)

	// Type assertion to check if the value is Box[int]
	if intBox, ok := value.(Box[int]); ok {
		fmt.Printf("Found an int box with value: %d\n", intBox.Value)
	}

	// Type assertion to check if the value is Box[string]
	if strBox, ok := value.(Box[string]); ok {
		fmt.Printf("Found a string box with value: %s\n", strBox.Value)
	} else {
		fmt.Println("Not a string box")
	}

	// Type switch with generic types
	switch v := GetValue(1).(type) {
	case Box[int]:
		fmt.Printf("Int box: %d\n", v.Value)
	case Box[string]:
		fmt.Printf("String box: %s\n", v.Value)
	case Box[float64]:
		fmt.Printf("Float box: %f\n", v.Value)
	default:
		fmt.Println("Unknown box type")
	}
}
