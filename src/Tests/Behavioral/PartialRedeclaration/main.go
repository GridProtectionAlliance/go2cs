// Regression test: Go's partial redeclaration `a, b := f()` reuses an already-declared LHS
// variable (a) and declares only the new ones (b). The converter emitted a blanket `var (a, b)`,
// which re-declared the reused variable (CS0136) and made its earlier uses "before declaration"
// (CS0841). It now emits `var` per newly-declared element: `(a, var b) = f()`. This is the
// math/ldexp `frac, e := normalize(frac)` pattern.
package main

import "fmt"

func two(x int) (int, int) {
	return x, x * 10
}

func redeclareParam(a int) int {
	if a < 0 {
		return -a // an earlier use of the reused var (would be CS0841 without the fix)
	}
	a, b := two(a) // a reused (the parameter), b new
	return a + b
}

func redeclareLocal() int {
	c := 5
	c, d := two(c) // c reused (a local), d new
	return c + d
}

func main() {
	fmt.Println(redeclareParam(3)) // (3, 30) -> 33
	fmt.Println(redeclareLocal())  // (5, 50) -> 55
}
