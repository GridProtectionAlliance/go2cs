package main

import "fmt"

// build exercises a zero-value string concatenated in a switch case that runs
// before any direct assignment to s (the reported NRE repro).
func build(x int) string {
	var s string
	switch x {
	case 1:
		s += "one"
	case 2:
		s += "two"
	}
	s += "!"
	return s
}

func main() {
	// Direct zero-value concatenation.
	var s string
	s += "a"
	s += "b"
	fmt.Println(s)

	// Zero-value string: length, printing, and comparison to "".
	var z string
	fmt.Println(len(z), z == "", "["+z+"]")

	// Switch-case concat before assignment.
	fmt.Println(build(1))
	fmt.Println(build(2))
	fmt.Println(build(3))
}
