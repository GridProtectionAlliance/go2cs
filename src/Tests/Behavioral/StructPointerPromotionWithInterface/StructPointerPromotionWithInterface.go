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

func main() {
	e:= MyError{time.Now(), "Hello"}
	a:=MyCustomError{"New One", nil, &e}

	a.Message = "New"
	a.What = "World"

	fmt.Println("MyError What =", e.What)
	fmt.Println("MyCustomError What =", a.What)
}