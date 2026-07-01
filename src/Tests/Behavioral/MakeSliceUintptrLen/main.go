package main

import "fmt"

// Locks in the go2cs conversion of `make([]T, len[, cap])` where the length/capacity is a non-int
// integer type (uintptr/uint/uint32/uint64/int64). The golib `slice<T>(nint length, nint capacity)`
// constructor takes nint; C# does not implicitly convert nuint/uint/ulong/long to nint, so without a
// cast the call falls onto `slice<T>(T[])` and fails CS1503 ("cannot convert nuint to byte[]"). The
// converter now casts such a length/cap to nint — `new slice<byte>((nint)(n / goarch.PtrSize))` — while
// leaving a plain int / untyped-constant length uncast. Runtime hits this in mbitmap's
// `make([]byte, n/goarch.PtrSize)` with a uintptr length.

//go:noinline
func run() {
	var n uintptr = 6
	a := make([]byte, n/2)              // uintptr length
	b := make([]uint64, n)              // uintptr length, []uint64 elem
	var u uint = 4
	c := make([]int, u)                 // uint length
	var w uint32 = 3
	d := make([]int32, w)               // uint32 length
	var q uint64 = 5
	e := make([]byte, q)                // uint64 length
	f := make([]byte, n/2, n)           // uintptr length AND uintptr capacity
	g := make([]byte, 2)                // untyped-constant length (control: no cast)
	var k int = 7
	h := make([]int, k)                 // plain int length (control: no cast)

	// The same nint-cast applies to a map size-hint and a chan buffer size (their golib ctors also
	// take nint); a uintptr hint/size would otherwise be a direct nuint->nint CS1503.
	mp := make(map[int]int, n)          // uintptr map size-hint
	mp[1] = 100
	ch := make(chan int, n)             // uintptr chan buffer size
	ch <- 1
	ch <- 2

	for i := range a {
		a[i] = byte(i)
	}
	fmt.Println(len(a), len(b), len(c), len(d), len(e))
	fmt.Println(len(f), cap(f), len(g), len(h))
	fmt.Println(a[0], a[1], a[2])
	fmt.Println(len(mp), cap(ch), len(ch))
}

func main() {
	run()
}
