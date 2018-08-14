package main

import "fmt"

func main() {
	var a string
	
	a = "Hello World"
	
	test(a)
	fmt.Println(a)
	fmt.Println()
	
	a = "Hello World"
	test2(&a)
	fmt.Println(a)
}

func test(a string) {
	fmt.Println(a)
	a = "Goodbye World"
	fmt.Println(a)
}

func test2(a *string) {
	fmt.Println(*a)
	*a = "Goodbye World"
	fmt.Println(*a)
}