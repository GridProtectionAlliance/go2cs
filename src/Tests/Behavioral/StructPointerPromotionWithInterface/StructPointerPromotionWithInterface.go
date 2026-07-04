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

type Device struct {
	name string
	hits int
}

// Receiver escapes (address of a receiver field is returned), so the
// converter emits the direct-ж primary form for this method
func (d *Device) Tag() *int {
	return &d.hits
}

func (d *Device) Describe() string {
	return d.name
}

type Describer interface {
	Describe() string
	Tag() *int
}

type deviceHandle struct {
	*Device
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

	// Interface satisfied through a pointer embed: Tag's receiver escapes
	// (direct-ж primary), Describe's does not — both must bind through the hop
	dev := &Device{name: "sensor", hits: 3}
	var dsc Describer = deviceHandle{Device: dev}
	fmt.Println(dsc.Describe())
	p := dsc.Tag()
	*p = 7
	fmt.Println(dev.hits) // Prints 7 - box aliasing through the hop
}
