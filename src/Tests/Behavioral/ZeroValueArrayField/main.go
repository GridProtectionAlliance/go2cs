package main

import "fmt"

// Locks in the zero value of a fixed-size array FIELD when a composite literal omits it: Go
// zeroes the [N]T backing (length N), and later ranging/indexing must work. The generated
// parameterized struct constructor previously overwrote the field's `= new(N)` initializer with
// the omitted argument's null-backed default, so the first walk of the field NREd (strings'
// stringFinder{pattern:…, goodSuffixSkip:…} left badCharSkip [256]int unusable — the
// TestFinderCreation/TestFinderNext blocker). Mirrors that shape: literal sets the fields
// around the array, never the array itself.

type holder struct {
	name string
	tbl  [8]int
	tail []int
}

//go:noinline
func makeHolder(name string) *holder {
	h := &holder{
		name: name,
		tail: make([]int, 2),
	}

	for i := range h.tbl {
		h.tbl[i] = len(name)
	}

	h.tbl[3] = 42

	return h
}

func main() {
	h := makeHolder("abc")
	sum := 0

	for i, v := range h.tbl {
		sum += i * v
	}

	fmt.Println(h.name, len(h.tbl), sum, h.tbl[3], len(h.tail))

	// Control: an EXPLICIT array argument in the literal must still assign. (Copy semantics of
	// the literal argument — mutating src afterward — is deliberately NOT probed here: the
	// composite-literal copy site is a separate known converter gap with its own pending chip.)
	src := [8]int{1, 2, 3, 4, 5, 6, 7, 8}
	g := holder{name: "xyz", tbl: src}
	fmt.Println(g.tbl[0], g.tbl[7], len(g.tbl))
}
