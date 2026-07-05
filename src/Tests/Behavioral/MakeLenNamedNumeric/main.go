// Regression test: a `make(T, len)` whose length/capacity argument is a NAMED numeric type
// (`type Size uint`) must compile. Such a type renders as a [GoType] wrapper struct with implicit
// conversions only to/from its UNDERLYING, so the make-length nint cast the converter inserts —
// `(nint)(size)` — has no direct conversion (CS0030, crypto's `make([]func() hash.Hash, maxHash)`
// where maxHash is `type Hash uint`). The cast must route through the underlying numeric:
// `(nint)(nuint)(size)`.
package main

import "fmt"

type Size uint // named numeric type over uint

const total Size = 3

func main() {
	s := make([]int, total) // make length is a named-numeric value
	for i := range s {
		s[i] = i * i
	}

	// Also exercise the two-arg form (len + cap) and a map size.
	var n Size = 2
	b := make([]byte, n, total) // both len and cap are named-numeric
	m := make(map[int]int, total)
	m[1] = 10

	fmt.Println(len(s), s[0], s[1], s[2]) // 3 0 1 4
	fmt.Println(len(b), cap(b))           // 2 3
	fmt.Println(len(m), m[1])             // 1 10
}
