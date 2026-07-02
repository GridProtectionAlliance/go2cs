package main

import "fmt"

// Locks in Go's slice ALIASING semantics: every reslice expression (2-index s[a:b], open-ended
// s[a:], 3-index s[a:b:c]) is a SHARED view over the same backing array — writes through the
// sub-slice are visible through the original and vice versa — with Go's bounds semantics (a
// missing high is len(s), not cap(s); high may reach beyond len(s) up to cap(s); bounds are
// relative to the view, compounding across reslices of reslices). append within capacity writes
// the shared backing in place; append beyond capacity reallocates and detaches. golib's slice<T>
// historically copied on any reslice that did not span the whole backing array (detaching the
// sub-slice), derived capacity from the backing array's end (breaking restricted-capacity 3-index
// views), and mis-measured the room available to append when the view had a non-zero low offset.

//go:noinline
func run() {
	// copy() into a low>0 reslice writes through to the base
	base := make([]uint32, 6)
	d := base[2:5]
	copy(d, []uint32{7, 8, 9})
	fmt.Println(base, d, len(d), cap(d))

	// element writes flow both directions between base and sub-slice
	d[0] = 42
	base[3] = 43
	fmt.Println(base[2], d[1])

	// reslice of a reslice: offsets compound relative to each view
	e := d[1:3]
	e[0] = 99
	fmt.Println(base, len(e), cap(e))

	// open-ended reslice of a make(len<cap) slice: high defaults to len, not cap
	s := make([]int, 3, 10)
	t := s[1:]
	fmt.Println(len(t), cap(t))

	// a reslice may extend beyond len up to cap
	u := s[1:8]
	u[6] = 5
	fmt.Println(len(u), cap(u), u[6])

	// slice of an array, then reslice of that slice
	arr := [6]int{0, 10, 20, 30, 40, 50}
	f := arr[2:5]
	g := f[1:3]
	g[0] = 31
	fmt.Println(arr, g[0], len(g), cap(g))

	// 3-index reslice: restricted capacity, still shares
	h := base[1:3:4]
	h[0] = 77
	fmt.Println(len(h), cap(h), base[1])

	// append within capacity writes the shared backing in place
	w := base[2:4]
	x := append(w, 500)
	x[0] = 501
	fmt.Println(base, len(x), cap(x))

	// append beyond capacity reallocates and detaches
	y := append(x, 1, 2, 3)
	y[0] = 999
	fmt.Println(base[2], y[0], len(y), cap(y))

	// append to a capacity-restricted 3-index view reallocates without touching the base
	z := base[1:3:3]
	grown := append(z, 111)
	fmt.Println(base[3], grown[2], len(grown), cap(grown))
}

func main() {
	run()
}
