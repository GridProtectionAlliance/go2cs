package main

import "fmt"

// Guards the range-over-int index TYPE. Go's `for i := range n` yields `i` of type
// int, which go2cs maps to `nint`; golib's range(nint) must therefore yield `nint`,
// not a C# `int`. When it yielded `int`, a `var i` loop variable used in a generic
// `append(s, i)` matched two builtin.append overloads with different inferred T
// (T=nint via slice<T>/Span<T> vs T=int via ISlice/T[]) and neither won the
// argument-by-argument betterness tie -> CS0121 (ambiguous call). This is the exact
// shape that blocked the maps and slices Phase-4 test suites (their `want =
// append(want, i)` over a `range(size)` index). An explicit `nint i` always resolved;
// only the `var i` inferred from range(nint)'s element type was ambiguous.
func main() {
	size := 5
	var s []int
	for i := range size {
		s = append(s, i)
	}
	fmt.Println(s)
}
