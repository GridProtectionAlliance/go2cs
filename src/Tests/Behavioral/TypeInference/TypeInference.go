package main

import "fmt"

func main() {
	const c = 3 < 4 // c is the untyped boolean constant true

	type MyBool bool
	var x, y int
	var (
		// The result of a comparison is an untyped boolean.
		// The usual assignment rules apply.
		b3        = x == y // b3 has type bool
		b4 bool   = x == y // b4 has type bool
		b5 MyBool = x == y // b5 has type MyBool
	)

	fmt.Println(c)
	fmt.Println(b3)
	fmt.Println(b4)
	fmt.Println(b5)
}