package main

import "fmt"

func main() {
	defer fmt.Println("Deferred result:", add(3, 4))
	fmt.Println("Doing something else")
}

func add(x, y int) int {
	result := x + y
	fmt.Println("Calculate:", result)
	return result
}
