// Guards a KEYED (sparse, constant-index) composite literal of a NAMED array-wrapper type —
// internal/trace/oldtrace's `timedEventArgs{1: uint64(ev.StkID)}` where `type timedEventArgs
// [4]uint64`. The keyed elements render as the C# `[i] = v` indexed initializer, which is invalid
// on a raw C# array (`new uint64[]{[1] = v}` — CS0131). The named-array-wrapper branch now backs it
// with the indexer-capable golib `array<T>(length)`: `new args(N){[1] = v}`. A POSITIONAL literal
// of the same type keeps the `.array()` form (must stay unchanged).
package main

import "fmt"

type args [4]uint64

// keyed builds the sparse-keyed literal (only index 1 and 3 set; 0 and 2 default to 0).
func keyed(a, b uint64) args {
	return args{1: a, 3: b}
}

// positional builds a fully positional literal (all 4 elements — must keep the existing
// `.array()` emission unchanged).
func positional(w, x, y, z uint64) args {
	return args{w, x, y, z}
}

func main() {
	k := keyed(11, 22)
	fmt.Println(k[0], k[1], k[2], k[3]) // 0 11 0 22

	p := positional(5, 6, 7, 8)
	fmt.Println(p[0], p[1], p[2], p[3]) // 5 6 7 8

	// a single-keyed literal, the exact oldtrace shape.
	one := args{2: 99}
	fmt.Println(one[0], one[1], one[2], one[3]) // 0 0 99 0
}
