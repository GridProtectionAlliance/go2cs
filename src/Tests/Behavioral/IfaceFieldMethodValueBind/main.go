package main

import "fmt"

type sizer interface {
	size(of string) int
}

type flat struct {
	n int
}

func (f flat) size(of string) int {
	return f.n + len(of)
}

type config struct {
	s sizer
}

var std sizer = flat{n: 100}

// pickSizer's method value roots at an INTERFACE field of the pointer receiver — it must
// delegate-bind directly (a synthesized lambda captures the ref receiver) and bind the
// receiver ONCE at creation, like Go: the field rebind below must not affect f.
func (c *config) pickSizer() func(string) int {
	f := std.size

	if c.s != nil {
		f = c.s.size
	}

	c.s = flat{n: 1000}
	return f
}

func main() {
	c := &config{s: flat{n: 10}}
	f := c.pickSizer()
	fmt.Println(f("abc"))
	fmt.Println(c.s.size("abc"))

	c2 := &config{}
	g := c2.pickSizer()
	fmt.Println(g("abcd"))
}
