package main

import "fmt"

// A `for init; …` loop's `:=` variable is scoped to the for statement, not the function body.
// When a real function-level variable of the same name is declared LATER (`for b := …{} … b := …`),
// Go keeps them distinct; in C# the for-loop variable must be shadow-renamed (`bΔ1`/`bΔ2`) or it
// collides with the later method-body `b` (CS0136). Mirrors the runtime's stkbucket: two
// `for b := …` loops followed by `b := newBucket(…)`.
func search(xs []int, t int) int {
	for b := 0; b < len(xs); b++ {
		if xs[b] == t {
			return b
		}
	}
	for b := len(xs) - 1; b >= 0; b-- {
		if xs[b] == t*2 {
			return b
		}
	}
	b := -1 // function-level, declared after both loops
	return b
}

func main() {
	xs := []int{5, 6, 7, 8}
	fmt.Println(search(xs, 7)) // forward loop matches xs[2]==7 -> 2
	fmt.Println(search(xs, 3)) // reverse loop matches xs[1]==3*2 -> 1
	fmt.Println(search(xs, 99)) // neither loop -> function-level b -> -1
}
