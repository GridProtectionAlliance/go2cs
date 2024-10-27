package main

import "fmt"

func main() {
	if a := -1; a < 0 {
		fmt.Println("a is less than 0")
	}

	if a := 1; a > 0 {
		fmt.Println("a is greater than 0")
	}

	b := 99
	fmt.Println("b before =", b)

	if b := -1; b < 0 {
		fmt.Println("b now =", b)
		fmt.Println("b is less than 0")
	}

	if b := 1; b > 0 {
		fmt.Println("b now =", b)
		fmt.Println("b is greater than 0")
	}

	fmt.Println("b after =", b)
}
