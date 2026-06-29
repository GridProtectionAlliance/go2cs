// PromotedFieldPointerDeref guards the converter's dereference of a PROMOTED
// (embedded) field accessed through a pointer-typed local variable. The
// field-membership check that decides whether to dereference a `ж<T>` box when a
// field is selected must recurse into embedded fields — otherwise
// `x.PromotedField` on a pointer local emits no deref and the box has no such
// member (CS1061). This mirrors runtime's scanstack walking
// `x := state.head; … x.nobj`, where `nobj`/`next` are promoted into
// stackObjectBuf from an embedded header struct.
package main

import "fmt"

type header struct {
	next *node // promoted pointer field
	tag  int   // promoted scalar field
}

type node struct {
	header // embedded -> next/tag are promoted onto node
	val    int
}

type list struct {
	head *node
}

func main() {
	a := &node{val: 1}
	a.tag = 7
	b := &node{val: 2}
	b.tag = 9
	a.next = b

	var l list
	l.head = a

	// Walk the list through a pointer local, reading promoted fields (val direct,
	// tag promoted) and repointing the local via a promoted pointer field (next).
	x := l.head
	for x != nil {
		fmt.Println(x.val, x.tag) // 1 7  then  2 9
		x = x.next
	}

	// Promoted-field WRITE through a pointer local must reach the real node.
	y := l.head
	y.tag = 99
	fmt.Println(a.tag) // 99
}
