package main

import (
	"fmt"
)

func main() {
	// A type `switch` compares types instead of values.  You
	// can use this to discover the type of an interface
	// value.  In this example, the variable `t` will have the
	// type corresponding to its clause.
	whatAmI := func(i interface{}) {
		switch t := i.(type) {
		case bool:
			fmt.Println("I'm a bool")
		case int, int64, uint64:
			fmt.Printf("I'm an int, specifically type %T\n", t)
		default:
			fmt.Printf("Don't know type %T\n", t)
		}
	}
	whatAmI(true)
	whatAmI(1)
	whatAmI(int64(2))
	whatAmI(uint64(2))
	whatAmI("hey")
}
