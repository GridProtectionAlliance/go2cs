// Regression test: a conversion between two DIFFERENT named types that share a COMPOSITE
// underlying (map/slice) — net/mail's textproto.MIMEHeader(h) where both Header and MIMEHeader
// wrap map[string][]string. Each is a [GoType] wrapper with implicit conversions only to/from its
// OWN underlying, and C# will not chain A->map->B in one cast (CS0030). The converter hops through
// the shared underlying: (B)(map<@string, nint>)a. A STRUCT underlying is excluded (anonymous, not
// a valid cast term, and the reverse *underlying->*named reinterpret case).
package main

import "fmt"

type A map[string]int
type B map[string]int

type S1 []int
type S2 []int

func main() {
	a := A{"x": 1, "y": 2}
	b := B(a) // named map -> named map, shared underlying
	fmt.Println(b["x"], b["y"], len(b))

	s := S1{10, 20, 30}
	s2 := S2(s) // named slice -> named slice, shared underlying
	fmt.Println(s2[0], s2[2], len(s2))
}
