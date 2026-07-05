package main

import "fmt"

// match mirrors go/constant's `func match(x, y Value) (_, _ Value)`: a multi-result function
// with BLANK result names. A C# tuple element named `_` twice collides (CS8127), so the
// converter must emit the result types UNNAMED — `(nint, nint)`, not `(nint _, nint _)`.
func match(x, y int) (_, _ int) {
	if x > y {
		return y, x
	}
	return x, y
}

// keyed mixes a NAMED result with a BLANK one — C# permits a mixed named/unnamed tuple, so the
// real name (`lo`) is kept and only the blank becomes unnamed.
func keyed(x, y int) (lo int, _ int) {
	if x < y {
		return x, y
	}
	return y, x
}

func main() {
	a, b := match(5, 2)
	fmt.Println(a, b) // 2 5
	c, d := match(1, 9)
	fmt.Println(c, d) // 1 9

	lo, hi := keyed(7, 3)
	fmt.Println(lo, hi) // 3 7
}
