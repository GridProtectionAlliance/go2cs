package main

import "fmt"

// Go promotes fields through EVERY embedding level. `top` embeds `mid` embeds `inner`, so
// `top.n` (inner's field) is promoted two levels. The TypeGenerator's field promotion must be
// transitive — a 1-level promotion alone misses the deepest members (CS1061 on `top.n`). Guards
// the transitive getStructMembers in StructTypeTemplate. (runtime hits this on stackWorkBuf →
// stackWorkBufHdr → workbufhdr.nobj.) Note: constructed via a composite literal so the promoted
// struct's captured boxes are initialized (a zero-value struct leaves them null — a separate,
// pre-existing generator limitation, not exercised here).

type inner struct {
	n int
	p int
}

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

	// Mutate through the 2-level promotion and read back through the explicit path.
	x.n = 42
	fmt.Println(x.n, x.inner.n) // 42 42 (same storage)
}
