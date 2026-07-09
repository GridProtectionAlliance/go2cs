package main

import "fmt"

type counter struct {
	n int
}

func (c *counter) bump() {
	c.n++
}

func apply(f func() int) int {
	return f()
}

// run exercises a promoted pointer-receiver method (counter's bump, through the value embed)
// called on an escaping anonymous-struct VALUE local inside closures that also mutate the
// local's fields — including a nested closure layer: the local must be captured by-box
// (shared), not snapshot-copied per closure layer, or the bumps and field writes land on a
// divorced copy.
func run() (string, int) {
	var rep struct {
		counter
		label string
	}
	rep.label = "start"

	build := func() int {
		rep.bump()
		rep.label = "built"
		return rep.n
	}

	fmt.Println("inner:", build(), rep.label)

	got := apply(func() int {
		touch := func() {
			rep.bump()
			rep.label = "applied"
		}
		touch()
		return rep.n
	})
	fmt.Println("applied:", got)

	return rep.label, rep.n
}

func main() {
	label, n := run()
	fmt.Println("outer:", label, n)
}
