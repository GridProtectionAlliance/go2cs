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
		*MyError
	}
)

// The following takes precedence over *MyError promoted field call to Time()
func (myErr *MyCustomError) Time() float64 {
	return 0.0
}

func (myErr MyError) Time() float64 {
	return float64(myErr.When.Unix())
}

type Inner struct {
	Value string
}

type Middle struct {
	*Inner // This works - single pointer promotion
}

type Outer struct {
	ptr **Inner // This works but requires a field name
}

func main() {
	e := MyError{time.Now(), "Hello"}
	a := MyCustomError{"New One", nil, &e}

	a.Message = "New"
	a.What = "World"

	fmt.Println("MyError What =", e.What)
	fmt.Println("MyCustomError What =", a.What)
	fmt.Println("MyCustomError method =", a.Time())

	inner := &Inner{Value: "hello"}
	innerPtr := &inner

	// Single pointer promotion works
	middle := Middle{Inner: inner}
	fmt.Println(middle.Value) // Prints "hello"

	// Multiple pointers require explicit field access
	outer := Outer{ptr: innerPtr}
	fmt.Println((*outer.ptr).Value) // Prints "hello"
}
