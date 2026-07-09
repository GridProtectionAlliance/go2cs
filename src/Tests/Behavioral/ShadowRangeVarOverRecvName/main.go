package main

import "fmt"

type node struct {
	name string
}

// collect's receiver is captured by a closure (promoting the method to direct-ж), and the
// range var below SHADOWS the receiver name: the shadow must keep its own render when passed
// as a pointer argument — the receiver-box render would reference an undeclared name.
func (c *node) collect(chain []*node) []*node {
	describe := func() string {
		return c.name
	}

	var toCheck []*node
	toCheck = append(toCheck, c)

	for _, c := range chain {
		toCheck = append(toCheck, c)
	}

	fmt.Println("describe:", describe())
	return toCheck
}

// firstOr returns a SHADOW named like the receiver from the loop and the genuine receiver on
// the empty path — only the latter is a receiver-return; the shadow must not be rewritten to
// the receiver box (that silently returned the receiver instead of the chain element).
func (c *node) firstOr(chain []*node) *node {
	for _, c := range chain {
		return c
	}

	return c
}

func main() {
	root := &node{name: "root"}
	a := &node{name: "a"}
	b := &node{name: "b"}

	for _, n := range root.collect([]*node{a, b}) {
		fmt.Println("got:", n.name)
	}

	fmt.Println("first:", root.firstOr([]*node{a, b}).name)
	fmt.Println("empty:", root.firstOr(nil).name)
}
