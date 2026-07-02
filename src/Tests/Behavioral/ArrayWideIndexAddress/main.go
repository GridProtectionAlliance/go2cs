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

	// A STRING indexed by a wide/unsigned integer. A string LITERAL renders as a
	// ReadOnlySpan<byte> ("…"u8) whose indexer takes int — a uintptr index was CS1503
	// (runtime heapdump.go's `"0123456789abcdef"[pc&15]`). The index routes through the
	// same (int) cast; an @string VARIABLE with a wide index takes it too.
	var pc uintptr = 0xAB
	fmt.Println("0123456789abcdef"[pc&15])   // 98 ('b')
	fmt.Println("0123456789abcdef"[pc>>4&15]) // 97 ('a')
	hex := "0123456789abcdef"
	var k uint64 = 12
	fmt.Println(hex[k]) // 99 ('c')
}
