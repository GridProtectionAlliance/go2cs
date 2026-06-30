package main

import "fmt"

// idx is a defined type over uint (→ C# nuint). Go allows ++/-- on a named integer; the C# named-numeric
// wrapper must generate operator ++/-- returning the named type (the runtime uses this for chunkIdx /
// arenaIdx / statDep loop counters; without it, c++ promoted through the implicit conversion → CS0266).
type idx uint
type sidx int // signed named numeric (→ C# nint)

func main() {
	// ++ on an unsigned named numeric, observed via a plain iteration counter (avoids int(named)).
	n := 0
	for c := idx(0); c < idx(5); c++ {
		n++
	}
	fmt.Println(n) // 5

	// -- on the same
	m := 0
	for c := idx(4); c > idx(0); c-- {
		m++
	}
	fmt.Println(m) // 4

	// ++/-- on a signed named numeric, observed by comparing to a named value.
	var d sidx = 3
	d++
	d++
	d--
	fmt.Println(d == sidx(4)) // true
}
