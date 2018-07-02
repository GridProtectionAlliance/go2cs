package main

import (
	"fmt"
	"time"
)

// Above type comment
type (
	// Top inner type comment
	Abser interface {
		Abs() float64 // To the right comments
	}
	
	// Middle type comment
	MyError struct {
		When time.Time
		What string
	}
	// Weirdly placed comment

	MyCustomError struct {
		Message string // My custom error message
		Abser
		MyError
		error
	}
	
	// Bottom inner type comment
)
// Below type comment

// The following takes precedence over instance call to Abs()
func (myErr *MyCustomError) Abs() float64 {
	return 0.0
}

func main() {
	a:= MyCustomError{"New One", nil, MyError{time.Now(), "Hello"}}
	a.Abs()
	a.Message = "New"
	fmt.Println("MyCustomError method =", a.Abs())
}
