// Regression test: a compound assignment (+=, *=, …) to a variable that SHADOWS a
// function-level variable of the same name must target the inner (shadow-renamed) variable.
//
// The converter renames an inner declaration that shadows an outer one (e.g. `x` → `xΔ1`),
// but the variable-analysis name lookup consulted the function-level declarations before the
// nested-block scopes — so `x += 1` inside the block resolved to the OUTER `x`. That either
// silently mutated the wrong variable or, in some contexts, dropped the LHS entirely (CS1525).
// Lookup now checks the scope stack (innermost first) before the function-level fallback.
package main

import "fmt"

func main() {
	// (1) nested block shadow + compound assignments.
	x := 100
	{
		x := 5
		x += 3
		x *= 2
		fmt.Println(x) // 16
	}
	fmt.Println(x) // 100 (outer x untouched)

	// (2) closure shadow + a `sum += i` accumulation loop.
	sum := 1000
	i := 50
	_ = sum
	_ = i
	total := func() int {
		sum := 0
		for i := 1; i <= 4; i++ {
			sum += i
		}
		return sum
	}()
	fmt.Println(total) // 10
	fmt.Println(sum)   // 1000 (outer sum untouched)

	// (3) shadow in a for-loop body, multiple compound ops.
	acc := 7
	for n := 0; n < 3; n++ {
		acc := 0
		acc += n
		acc -= 1
		fmt.Print(acc, " ") // -1 0 1
	}
	fmt.Println()
	fmt.Println(acc) // 7 (outer acc untouched)
}
