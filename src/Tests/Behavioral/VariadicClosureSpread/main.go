package main

import "fmt"

func main() {
	// A single-return closure that spreads its variadic parameter into another
	// variadic call (fmt.Sprintf). The variadic param arrives as a C# `params`
	// array and must be rebound to a slice inside the closure body, or the spread
	// `a...` is undefined. This also exercises the return-collapse case: the body
	// must stay a block so the rebinding prologue has somewhere to live.
	format := func(f string, a ...any) string {
		return fmt.Sprintf(f, a...)
	}
	fmt.Println(format("%s=%d", "x", 1))
	fmt.Println(format("%s=%d", "y", 2))

	// A closure that ranges its variadic parameter as a slice.
	sum := func(nums ...int) int {
		total := 0
		for _, n := range nums {
			total += n
		}
		return total
	}
	fmt.Println(sum(1, 2, 3, 4))

	// A single-return closure that spreads its variadic parameter into another
	// variadic (the captured sum), called read-only.
	forward := func(a ...int) int {
		return sum(a...)
	}
	fmt.Println(forward(10, 20, 30))
}
