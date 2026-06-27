// Regression test: a function literal (closure) that itself uses defer/recover — assigned to a
// variable, returned from a function, or passed as an argument — must get its OWN func()
// execution context so its deferred code runs (and recovers) when the closure is invoked.
//
// defer/recover are scoped per function: an enclosing function is no longer wrapped just because
// a nested literal uses them, so a nested literal that uses defer/recover now carries its own
// `func((defer, recover) => …)` context. A deferred-call target stays the exception (its
// recover() recovers the enclosing function), which the existing defer/recover tests cover.
package main

import "fmt"

// returns a closure that defers.
func makeGreeter(name string) func() {
	return func() {
		defer fmt.Println("bye", name)
		fmt.Println("hi", name)
	}
}

func main() {
	// (1) assigned closure with a defer.
	f := func() {
		defer fmt.Println("deferred")
		fmt.Println("body")
	}
	f()

	// (2) assigned closure that recovers a panic in its own scope.
	divPrint := func(a, b int) {
		defer func() {
			if r := recover(); r != nil {
				fmt.Println("closure recovered:", r)
			}
		}()
		fmt.Println(a / b)
	}
	divPrint(20, 4) // 5
	divPrint(1, 0)  // closure recovered: runtime error: integer divide by zero

	// (3) closure returned from a function still defers correctly when called later.
	greet := makeGreeter("go2cs")
	greet() // hi go2cs \n bye go2cs

	// (4) closure passed as an argument runs its defer when invoked.
	run := func(fn func()) { fn() }
	run(func() {
		defer fmt.Println("arg-closure deferred")
		fmt.Println("arg-closure body")
	})

	// (5) deferred-call target keeps recovering the ENCLOSING function (not its own scope).
	func() {
		defer func() {
			if r := recover(); r != nil {
				fmt.Println("outer recovered:", r)
			}
		}()
		panic("from-iife")
	}()
	fmt.Println("done")
}
