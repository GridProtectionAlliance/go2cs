package main

import "fmt"

func sum(nums ...int) int {
	total := 0
	for _, n := range nums {
		total += n
	}
	return total
}

func main() {
	values := []int{1, 2, 3}

	// Unpacks to: sum(1, 2, 3)
	fmt.Println(sum(values...))

	// Variadic params
	fmt.Println(sum(1, 2, 3))
}
