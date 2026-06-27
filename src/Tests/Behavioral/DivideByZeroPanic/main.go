// Regression test: integer divide/modulo by zero panics and is recoverable, like Go.
//
// In Go, `a / b` and `a % b` with b == 0 panic with the runtime error
// "runtime error: integer divide by zero", which recover() observes. In C# the same
// operation throws System.DivideByZeroException, which is NOT a Go PanicException, so
// recover() (golib's GoFunc catches only PanicException) could not see it — the program
// crashed with a .NET exception instead. golib now maps DivideByZeroException to a Go
// runtime panic (RuntimeErrorPanic.IntegerDivideByZero) at the GoFunc boundary and in the
// top-level handler, so recover() behaves like Go and an unrecovered case reports
// "panic: runtime error: integer divide by zero".
package main

import "fmt"

func safeDiv(a, b int) (result int, recovered bool) {
	defer func() {
		if r := recover(); r != nil {
			fmt.Println("  recovered:", r)
			result, recovered = -1, true
		}
	}()
	result = a / b
	return result, false
}

func safeMod(a, b int) (result int, recovered bool) {
	defer func() {
		if r := recover(); r != nil {
			result, recovered = -2, true
		}
	}()
	result = a % b
	return result, false
}

func divide(a, b int) int { return a / b }

// the panic propagates across an intermediate (non-defer) frame to an outer recover.
func outerGuard(a, b int) (ok bool) {
	defer func() {
		if recover() != nil {
			ok = false
		}
	}()
	divide(a, b) // panic in divide() unwinds through this frame to the recover above
	return true
}

func main() {
	q, rec := safeDiv(10, 2)
	fmt.Println(q, rec) // 5 false

	q2, rec2 := safeDiv(7, 0)
	fmt.Println(q2, rec2) // -1 true (after the "recovered:" line)

	m, mrec := safeMod(9, 0)
	fmt.Println(m, mrec) // -2 true

	fmt.Println(outerGuard(4, 2)) // true
	fmt.Println(outerGuard(4, 0)) // false (panic crossed the inner frame, recovered in outer)
}
