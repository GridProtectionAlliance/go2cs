package main

import "fmt"

// noCopy is an unexported marker type, embedded as a blank field to mark a struct
// as must-not-copy (the pattern used by sync/atomic.Bool etc.).
type noCopy struct{}

func (*noCopy) Lock()   {}
func (*noCopy) Unlock() {}

// Counter is exported but embeds the unexported noCopy as a blank field. The
// generated constructor must not expose noCopy as a public-constructor parameter:
// noCopy is internal (less accessible than Counter), which is invalid (CS0051). The
// blank `_` field must therefore be excluded from the public constructor.
type Counter struct {
	_ noCopy
	v int64
}

func (c *Counter) Add(n int64) { c.v += n }
func (c *Counter) Value() int64 { return c.v }

func main() {
	var c Counter
	c.Add(5)
	c.Add(3)
	fmt.Println("counter:", c.Value())

	// A second exported struct embedding the marker, constructed as a zero value.
	var d Counter
	fmt.Println("zero:", d.Value())
}
