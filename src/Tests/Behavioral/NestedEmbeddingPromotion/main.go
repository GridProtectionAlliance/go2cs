package main

import "fmt"

// Go promotes fields AND methods through EVERY embedding level. `top` embeds `mid` embeds `inner`,
// so `top.n` (inner's field) and `top.describe()` (inner's method) are promoted two levels. The
// TypeGenerator's field promotion (getStructMembers) and method promotion (PromotedStructReceivers)
// must both be transitive — a 1-level promotion alone misses the deepest members (CS1061 / CS1929).
// (runtime hits this on stackWorkBuf → stackWorkBufHdr → workbufhdr.) Constructed via a composite
// literal so the promoted struct's captured boxes are initialized.

type inner struct {
	n int
	p int
}

func (i inner) describe() string { return fmt.Sprintf("n=%d p=%d", i.n, i.p) }

type mid struct {
	inner
	m int
}

type top struct {
	mid
	t int
}

func main() {
	x := top{}
	x.n = 1 // promoted two levels: top -> mid -> inner.n
	x.p = 9 // promoted two levels
	x.m = 2 // promoted one level: top -> mid.m
	x.t = 3

	fmt.Println(x.n, x.p, x.m, x.t) // 1 9 2 3
	fmt.Println(x.inner.n, x.mid.m) // 1 2 (explicit paths still work)
	fmt.Println(x.describe())       // n=1 p=9 (two-level METHOD promotion)

	x.n = 42
	fmt.Println(x.n, x.inner.n) // 42 42 (same storage)
}
