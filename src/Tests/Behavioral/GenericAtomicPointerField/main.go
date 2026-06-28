package main

import (
	"fmt"
	"sync/atomic"
)

// Mirrors crypto/internal/boring/bcache: a generic struct embeds a generic atomic.Pointer[T]
// as a value field, and methods reach it through a *pointer variable* (not the receiver) and
// build entries with a composite literal that assigns a pointer parameter to a pointer field.
type entry[V any] struct {
	key  *V                // pointer field set from a pointer parameter in a composite literal
	v    atomic.Pointer[V] // capture-mode atomic field, reached via a deref'd pointer var
	next *entry[V]
}

type Cache[V any] struct {
	head atomic.Pointer[entry[V]] // field-of-receiver capture-mode (already supported)
}

func (c *Cache[V]) Put(key *V, val *V) {
	e := &entry[V]{key: key} // pointer param `key` into a pointer composite-literal field
	e.v.Store(val)           // `e.v` is a field of the pointer var `e` (capture-mode)
	e.next = c.head.Load()
	c.head.Store(e)
}

func (c *Cache[V]) Get(key *V) *V {
	for e := c.head.Load(); e != nil; e = e.next {
		if e.key == key {
			return e.v.Load() // field-of-pointer-var capture-mode Load
		}
	}
	return nil
}

func main() {
	var c Cache[int]
	a, b := 1, 2
	av, bv := 10, 20
	c.Put(&a, &av)
	c.Put(&b, &bv)

	fmt.Println("get a:", *c.Get(&a)) // 10
	fmt.Println("get b:", *c.Get(&b)) // 20

	// Overwrite a's value atomically through the field.
	newAv := 99
	c.Put(&a, &newAv)
	fmt.Println("get a again:", *c.Get(&a)) // 99
	fmt.Println("missing:", c.Get(new(int)) == nil)
}
