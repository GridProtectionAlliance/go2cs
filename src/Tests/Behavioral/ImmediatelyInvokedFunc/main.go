// Regression test for the immediately-invoked function literal (IIFE): `func(){ … }()`.
//
// A bare C# lambda cannot be invoked directly — `() => {…}()` is CS0149 — so the converter
// previously emitted uncompilable code for an IIFE. And an IIFE that uses defer/recover needs
// its OWN defer/recover scope: the deferred code (and any recover) must apply to the IIFE,
// letting execution continue after it. A no-argument IIFE is now emitted as a
// `func((defer, recover) => body)` execution-context call, which both wraps (its own
// defer/recover scope) and runs immediately.
package main

import "fmt"

func main() {
	// (1) void IIFE
	func() {
		fmt.Println("a")
	}()

	// (2) value-returning IIFE, assigned
	x := func() int {
		return 6 * 7
	}()
	fmt.Println(x) // 42

	// (3) IIFE that recovers a panic in its own scope; execution continues afterward.
	func() {
		defer func() {
			if r := recover(); r != nil {
				fmt.Println("recovered:", r)
			}
		}()
		panic("boom")
	}()
	fmt.Println("after recover") // printed — the panic was scoped to the IIFE

	// (4) IIFE whose result feeds directly into an expression.
	total := 10 + func() int {
		sum := 0
		for i := 1; i <= 4; i++ {
			sum += i
		}
		return sum
	}()
	fmt.Println(total) // 10 + (1+2+3+4) = 20

	// (5) nested IIFEs.
	y := func() int {
		return func() int { return 5 }() * 2
	}()
	fmt.Println(y) // 10
}
