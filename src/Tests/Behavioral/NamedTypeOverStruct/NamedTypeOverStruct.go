package main

import "fmt"

type inner struct {
	a int64
	b int64
}

// wrapper is a DEFINED type whose underlying is the inner struct. In Go the underlying struct's
// fields are accessible on the named type (w.a, w.b); the C# wrapper forwards them as get/set
// properties over a mutable value so a write through a *wrapper persists.
type wrapper inner

type box struct {
	w wrapper
}

// fill writes the forwarded fields through a *wrapper obtained from a struct field — mirrors
// runtime's winlibcall `c.val.fn = fn` (c is a *winlibcall field accessor).
func fill(b *box) {
	c := &b.w
	c.a = 10
	c.b = 20
}

// readBack reads the forwarded fields back through a *wrapper.
func readBack(b *box) int64 {
	c := &b.w
	return c.a + c.b
}

func main() {
	var b box
	fill(&b)
	fmt.Println(b.w.a, b.w.b) // 10 20 — write-through the forwarded fields persisted
	fmt.Println(readBack(&b)) // 30 — read back through the pointer
}
