package main

import "fmt"

// Exercises taking the address of an array/slice element with a NON-int
// (uintptr / uint / uint64) index. Go converts any integer index to int for the
// access; the emitted C# routes &arr[i] through golib's `ж.at<T>(nint)` accessor,
// which has no implicit nuint/uint/ulong→nint conversion — so the converter must
// narrow the index explicitly. Guards the convArrayIndex (nint)-coercion fix.

func main() {
	a := [4]int{10, 20, 30, 40}

	var i uintptr = 2
	pi := &a[i]
	*pi = 99

	var u uint = 1
	pu := &a[u]
	*pu = 88

	var w uint64 = 3
	pw := &a[w]
	*pw = 77

	// An expression index whose C# result type widens (uint % 2 -> long).
	var g uint = 5
	pe := &a[g%2] // index 1
	*pe = 66

	fmt.Println(a[0], a[1], a[2], a[3])
}
