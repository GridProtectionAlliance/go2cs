package main

import "fmt"

// Locks in the go2cs conversion of a 3-index (full) slice expression `s[low:high:max]` whose bounds are
// a WIDE integer type (uintptr/uint/uint32/uint64/int64). Such an expression lowers to the golib
// `.slice(nint low, nint high, nint max)` method, which takes nint; C# does not implicitly convert
// nuint/uint/ulong/long to nint, so an uncast bound is CS1503 ("cannot convert nuint to nint"). The
// converter casts each wide bound to int (Go's own slice bounds are int, so the narrowing matches Go
// semantics). A plain int / small-int bound binds directly and is left uncast. Runtime hits this in
// mprof's `stk[:b.nstk:b.nstk]` with a uintptr b.nstk.

//go:noinline
func run() {
	arr := [8]int{10, 11, 12, 13, 14, 15, 16, 17}
	sl := []int{20, 21, 22, 23, 24, 25}

	var n uintptr = 5
	a := arr[:n:n]           // uintptr High + Max (array)
	var lo uintptr = 1
	b := arr[lo:n:n]         // uintptr Low + High + Max
	var u uint = 4
	c := sl[:u:u]            // uint High + Max (slice)
	var q uint64 = 3
	d := sl[q:q+2 : q+3]     // uint64 bounds with arithmetic
	var k int = 6            // plain int (control: no cast)
	e := arr[:k:k]

	fmt.Println(len(a), cap(a), a[0], a[4])
	fmt.Println(len(b), cap(b), b[0])
	fmt.Println(len(c), cap(c), len(d), cap(d))
	fmt.Println(len(e), cap(e))
}

func main() {
	run()
}
