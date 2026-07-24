package main

import "fmt"

// Guards STRUCTURAL nil-pointer identity in golib ж equality: a heap box holding a nil
// pointer VALUE (a ж<ж<T>> captured local) is a non-nil pointer holding nil — two distinct
// such addresses are unequal, and neither equals nil. The old value-peeking equality
// reported &p1 == &p2 (both true) whenever *p1 and *p2 were both nil.

func main() {
	p1 := (*int)(nil)
	p2 := (*int)(nil)

	pp1 := &p1
	pp2 := &p2

	// Distinct addresses of nil-holding pointer variables are DISTINCT.
	fmt.Println("pp1==pp2", pp1 == pp2)

	// An address is never nil, no matter what it holds.
	fmt.Println("pp1==nil", pp1 == nil)

	// The held values ARE nil.
	fmt.Println("*pp1==nil", *pp1 == nil)
	fmt.Println("*pp1==*pp2", *pp1 == *pp2)

	// Same address compares equal to itself through another alias.
	alias := pp1
	fmt.Println("alias==pp1", alias == pp1)

	// Writing through one alias is visible through the other (same storage).
	n := 42
	*alias = &n
	fmt.Println("p1 set", *pp1 != nil, **pp1)
}
