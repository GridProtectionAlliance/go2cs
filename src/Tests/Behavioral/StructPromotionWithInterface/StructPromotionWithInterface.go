package main

import (
	"fmt"
	"time"
)

type (
	Abser interface {
		Abs() float64
	}

	MyError struct {
		When time.Time
		What string
	}

	MyCustomError struct {
		Message string
		Abser
		MyError
		error
	}

	MyAbser struct {
	}
)

// The following takes precedence over Abser promoted field call to Abs()
func (myErr *MyCustomError) Abs() float64 {
	return 0.0
}

func (myAbs MyAbser) Abs() float64 {
	return 1.0
}

func main() {
	a := MyCustomError{"New One", MyAbser{}, MyError{time.Now(), "Hello"}, nil}
	a.Abs()
	a.Message = "New"
	fmt.Println("MyCustomError method =", a.Abs())
}
