// Regression test: a `var` declaration nested in a block must be shadow-renamed when it collides
// with a function-level variable of the same name declared LATER in the function. The function-
// level-declaration pre-pass collected nested `var` decls without the function-level gate that
// every other declaration kind has, so a nested `var z` was recorded as *the* function-level `z`,
// masking the real one (`z := x*x`); neither got renamed and both emitted `z` -> CS0136. This is
// the math/j0 J0/Y0 (and j1) `var z float64` + later `z := x*x` pattern.
package main

import "fmt"

func f(x int) int {
	if x >= 2 {
		var z int // nested `var` decl; collides with the later function-level z
		if x > 5 {
			z = 100
		} else {
			z = 200
		}
		return z
	}
	z := x * x // function-level, declared textually AFTER the nested `var z`
	return z
}

func main() {
	fmt.Println(f(10)) // 100
	fmt.Println(f(3))  // 200
	fmt.Println(f(1))  // 1
}
