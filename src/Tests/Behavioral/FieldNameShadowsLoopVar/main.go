// Regression test: a struct FIELD whose name equals a shadow-renamed LOCAL variable in the same
// function must keep its own name in the field selector. compress/bzip2's newHuffmanTree does
// `for i, length := range lengths { pairs[i].length = length }`; the loop var `length` is
// shadow-renamed (lengthΔ1), and the field access `.length` wrongly took that rename too, emitting
// `pairs[i].lengthΔ1` against the `length` field decl (CS1061). A field is struct-scoped — its
// selector uses the field's OWN name, never a local-var shadow rename.
package main

import "fmt"

type pair struct {
	value  int
	length int
}

func build(lengths []int) []pair {
	pairs := make([]pair, len(lengths))
	for i, length := range lengths { // loop var `length` — shadow-renamed in the converted C#
		pairs[i].value = i
		pairs[i].length = length // field `length` must stay `.length`, not the renamed loop var
	}

	// A SECOND `length` in the enclosing FUNCTION scope forces the loop var above to be
	// shadow-renamed (C# forbids reusing the name across the nested scope, CS0136) — that rename
	// is what wrongly leaked into the field selector before the fix. (Mirrors bzip2's
	// `length := uint8(32)` after the pair-building loop.)
	length := 0
	for _, p := range pairs {
		length += p.length
	}
	fmt.Println("total length:", length) // 5+3+8 = 16
	return pairs
}

func main() {
	pairs := build([]int{5, 3, 8})
	for _, p := range pairs {
		fmt.Println(p.value, p.length) // 0 5 / 1 3 / 2 8
	}
}
