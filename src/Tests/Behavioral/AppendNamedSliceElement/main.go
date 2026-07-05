// Regression test: append to a NAMED slice type whose element is ALSO a named type, with an
// argument of the element's UNDERLYING type. crypto/x509/pkix's appendRDNs does
// `append(in, s)` where in is RDNSequence ([]RelativeDistinguishedNameSET) and s is
// []AttributeTypeAndValue (RelativeDistinguishedNameSET's underlying). Go implicitly converts s
// to the element type; C# append otherwise infers the element as slice<ATV>, so the result
// slice<slice<ATV>> does not bind RDNSequence (CS0029). The converter now casts the differing arg
// to the named element type: `append(in, (RelativeDistinguishedNameSET)(s))`.
package main

import "fmt"

type Inner []int   // named slice
type Outer []Inner // named slice whose element is the named Inner

func build() Outer {
	var o Outer
	s := []int{1, 2} // []int = Inner's UNDERLYING (not Inner itself)
	o = append(o, s) // append(Outer, []int) — s must be cast to the Inner element
	o = append(o, []int{3, 4, 5})
	var inner Inner = []int{6} // already the element type — appended bare
	o = append(o, inner)
	return o
}

func main() {
	o := build()
	fmt.Println(len(o)) // 3
	for _, inner := range o {
		fmt.Println(inner)
	}
	// 3 rows: [1 2] / [3 4 5] / [6]
}
