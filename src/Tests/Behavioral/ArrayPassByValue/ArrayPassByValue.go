package main

import "fmt"

func main() {
	var a [2]string

	a[0] = "Hello"
	a[1] = "World"

	test(a)
	fmt.Println(a[0], a[1])
	fmt.Println()

	a[0] = "Hello"
	test2(&a)
	fmt.Println(a[0], a[1])
	fmt.Println()

	a[0] = "Hello"
	test3(a[:])
	fmt.Println(a[0], a[1])
	fmt.Println()

	primes := [6]int{2, 3, 5, 7, 11, 13}
	fmt.Println(primes)

	fmt.Println(a[0])
	stest(&a[0])
	fmt.Println(a[0])
}

func stest(p *string) {
	*p = "hello"
}

// Arrays are passed by value (a full copy)
func test(a [2]string) {
	// Update to array will be local
	fmt.Println(a[0], a[1])
	a[0] = "Goodbye"
	fmt.Println(a[0], a[1])
}

func test2(a *[2]string) {
	fmt.Println(a[0], a[1])
	a[0] = "Goodbye"
	fmt.Println(a[0], a[1])
}

func test3(a []string) {
	fmt.Println(a[0], a[1])
	a[0] = "Goodbye"
	fmt.Println(a[0], a[1])
}
