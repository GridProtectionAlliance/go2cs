package main

import "fmt"

type registry struct {
	handles map[string]*int
}

// at and set dereference a pointer-valued map ELEMENT reached through the receiver — the
// deref belongs to the element and must not be elided by the receiver-deref shortcut.
func (r *registry) at(key string) int {
	return *r.handles[key]
}

func (r *registry) set(key string, val int) {
	*r.handles[key] = val
}

// clone is the genuine receiver deref the shortcut exists for.
func (r *registry) clone() registry {
	return *r
}

func main() {
	a, b := 1, 2
	r := &registry{handles: map[string]*int{"a": &a, "b": &b}}

	fmt.Println(r.at("a"), r.at("b"))

	r.set("a", 10)
	fmt.Println(r.at("a"), a)

	c := r.clone()
	fmt.Println(len(c.handles))
}
