package main

import "fmt"

type relationship string

const (
	equivalent   relationship = "equivalent"
	moreGeneral  relationship = "moreGeneral"
	moreSpecific relationship = "moreSpecific"
)

func (r relationship) tag() string { return "rel:" + string(r) }

// Comparisons between values and consts of the named string type must be unambiguous
// (net/http pattern.go shape: typed consts materialized as @string made every `==`
// ambiguous, CS0034).
func describe(r relationship) string {
	if r == equivalent {
		return "same"
	}
	if r == moreGeneral || r == moreSpecific {
		return "related:" + string(r)
	}
	return "other"
}

func main() {
	fmt.Println(describe(equivalent))
	fmt.Println(describe(moreGeneral))
	fmt.Println(describe(relationship("x")))

	// A local typed const keeps the named type too.
	const localRel relationship = "moreSpecific"
	fmt.Println(describe(localRel))

	// Methods still bind on a const of the named type.
	fmt.Println(equivalent.tag())

	// An untyped string const stays a plain string.
	const plain = "plain"
	fmt.Println(plain)
}
