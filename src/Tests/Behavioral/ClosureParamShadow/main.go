package main

import "fmt"

// A function-literal parameter that shadows an enclosing local must be shadow-renamed in C#
// (`n`→`nΔ1`) because C# forbids a lambda parameter shadowing an enclosing local (CS0136). The
// parameter DECLARATION and the body's uses must use the SAME renamed name — emitting the bare `n`
// in the lambda signature while the body resolves to `nΔ1` leaves `nΔ1` undeclared (CS0103). Guards
// the getIdentName-based parameter naming in convFuncType. (runtime hits this on `mcall(func(gp *g)
// {…})` where `gp` shadows an outer `gp`.)

func run(f func(int) int) int { return f(10) }

func main() {
	n := 100

	r := run(func(n int) int { // param n shadows the outer n
		n = n * 2 // mutate the param
		return n + 1
	})

	fmt.Println(r, n) // 21 100 (outer n unchanged)

	// A second closure capturing the outer n while a param also named n shadows it inside.
	add := 7
	g := func(n int) int {
		return n + add // n is the param; add is captured
	}
	fmt.Println(g(n), n) // 107 100
}
