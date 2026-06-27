// Regression test: deferred functions modifying named return values.
//
// In Go, `return X` assigns X to the named result parameters, THEN runs the deferred
// functions (which can further modify those result parameters), THEN returns them. The
// converter wraps any defer/recover function body in a `func((defer, recover) => …)` lambda
// whose return value is captured BEFORE the defers run — so deferred modifications to named
// returns were lost (`incr()` returned 5 instead of 6; a recover that set a named result was
// discarded). The fix declares the named returns outside the wrapper and returns them after
// it runs, matching Go.
package main

import "fmt"

// defer increments the named return after the body's `return`.
func incr() (x int) {
	defer func() { x++ }()
	x = 5
	return x
}

// naked return + defer modification.
func incrBare() (x int) {
	defer func() { x += 10 }()
	x = 1
	return
}

// two named returns, both modified by the defer.
func swapAndBump() (a int, b int) {
	defer func() {
		a, b = b, a
		a += 100
	}()
	a = 1
	b = 2
	return a, b
}

// recover sets the named returns on panic; normal path leaves them.
func guarded(boom bool) (code int, msg string) {
	defer func() {
		if r := recover(); r != nil {
			code = -1
			msg = "recovered"
		}
	}()
	if boom {
		panic("kaboom")
	}
	code = 0
	msg = "ok"
	return code, msg
}

func main() {
	fmt.Println(incr())     // 6  (5, then defer x++)
	fmt.Println(incrBare()) // 11 (1, then defer x+=10)

	a, b := swapAndBump()
	fmt.Println(a, b) // 102 1  (1,2 -> swap -> 2,1 -> a+=100 -> 102,1)

	c1, m1 := guarded(false)
	fmt.Println(c1, m1) // 0 ok

	c2, m2 := guarded(true)
	fmt.Println(c2, m2) // -1 recovered
}
