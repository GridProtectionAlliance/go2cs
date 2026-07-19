package main

import "fmt"

// High-precision untyped-float constants (their EXACT value carries more precision than a float64),
// emitted as golib UntypedFloat wrappers. A compile-time float expression that references one must
// FOLD to a single arbitrary-precision-rounded literal, not double-round through runtime C# double
// arithmetic (the named const is already float64-rounded, so `100000 * myPi` would round a 2nd time).
const myPi = 3.14159265358979323846264338327950288419716939937510582097494459
const myLn10 = 2.30258509299404568401799145468436420760110148862877297603332790

// A non-constant float64, to force the typed-float64 context on the `1 / myLn10` operand below.
func f64(x float64) float64 { return x }

func main() {
	// HOOK 1: a float64(constExpr) conversion.
	fmt.Println(float64(100000 * myPi))
	// HOOK 1 at float32 width — folds the EXACT constant straight to float32, not float64-then-float32.
	fmt.Println(float64(float32(100000 * myPi)))
	// HOOK 2: a computed float const operand (1/myLn10) in a typed-float64 multiplication.
	fmt.Println(f64(2) * (1 / myLn10))
}
