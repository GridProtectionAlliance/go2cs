package main

import (
	"fmt"
	//"io"
)

type I interface{ m() string }

type T struct {
	name string
	I
}

func (ref T) m() string {
	return ref.name
}

type S struct {
	name string
}

func (ref S) m() string {
	return "Am I an I?"
}

func main() {
	// A type `switch` compares types instead of values.  You
	// can use this to discover the type of an interface
	// value.  In this example, the variable `t` will have the
	// type corresponding to its clause.
	whatAmI := func(i interface{}) {
		switch t := i.(type) {
		case bool:
			fmt.Println("I'm a bool")
		case int:
			fmt.Println("I'm an int")
		default:
			fmt.Printf("Don't know type %T\n", t)
		}
	}
	whatAmI(true)
	whatAmI(1)
	whatAmI("hey")

	var x interface{} = 7 // x has dynamic type int and value 7
	i := x.(int)          // i has type int and value 7
	fmt.Println(i)

	var y T

	y.name = "Me"

	f(y)

	var s interface{} = S{"you"}

	switch s.(type) {
	default:
		fmt.Println("S is not an I")
	case I:
		fmt.Println("S is an I!!")
	case nil, int:
		fmt.Println("S is nil or an int")
	}
}

func f(y I) {
	//s := y.(string)        // illegal: string does not implement I (missing method m)
	//r := y.(io.Reader)     // r has type io.Reader and the dynamic type of y must implement both I and io.Reader

	fmt.Println(y.m())
}
