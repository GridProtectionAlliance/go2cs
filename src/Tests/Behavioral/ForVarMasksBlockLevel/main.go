package main

import "fmt"

// A `for init; …` loop's `:=` variable is scoped to the for statement. When a same-named variable
// is declared LATER in the *enclosing block* (here the `for {}` while-body, not the function body),
// Go keeps them distinct; in C# the for-loop variables must be shadow-renamed (`offΔ1`/`offΔ2`) or
// they collide with the block-level `off` that encloses them (CS0136 is order-independent — the
// block `off` encloses the inner loops even though it appears after them). Mirrors the runtime's
// runGCProg (mbitmap.cs): two `for off := …` loops then `off := n - nbits` in the same while-body.
// Distinct from ForVarMasksFuncLevel, where the later same-named variable is function-level.
func compute(n int) int {
	total := 0
	count := 0
	for {
		for off := 0; off < n; off++ {
			total += off
		}
		for off := n; off > 0; off-- {
			total += off
		}
		off := n * 10 // block-level, declared after both loops in the same for{} body
		total += off
		count++
		if count >= 2 {
			return total
		}
	}
}

func main() {
	fmt.Println(compute(3)) // (0+1+2)+(3+2+1)+30 twice = 39+39 = 78
}
