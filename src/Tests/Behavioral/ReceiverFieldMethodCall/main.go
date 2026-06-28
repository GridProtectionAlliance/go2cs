package main

import "fmt"

// Counter mirrors the sync/atomic typed-type pattern: a pointer-receiver method takes the
// address of a receiver field (capture-mode), so it must be invoked through its receiver box.
type Counter struct {
	n int32
}

func bump(p *int32, delta int32) int32 {
	*p += delta
	return *p
}

func (c *Counter) Add(delta int32) int32 { return bump(&c.n, delta) }
func (c *Counter) Set(v int32)           { *(&c.n) = v }
func (c *Counter) Get() int32            { return c.n } // a plain value method (not capture-mode)

// Flag embeds Counter as a value FIELD and drives it by calling the capture-mode methods
// through that field — `f.c.Add(1)` / `f.c.Set(v)`. Each such call must route through the
// field-address box `Ꮡf.of(Flag.Ꮡc)` so the mutation lands on the real embedded field.
type Flag struct {
	c     Counter
	label string
}

func (f *Flag) Incr() int32        { return f.c.Add(1) }     // capture-mode call through a value field
func (f *Flag) AddN(d int32) int32 { return f.c.Add(d) }     // capture-mode call, forwarded arg
func (f *Flag) Reset(v int32)      { f.c.Set(v) }            // capture-mode (void) call through a field
func (f *Flag) Value() int32       { return f.c.Get() }      // plain value-method call through a field
func (f *Flag) Label() string      { return f.label }

func main() {
	var fl Flag
	fl.label = "hits"

	fl.Reset(10)
	fmt.Println(fl.Label(), "start:", fl.Value()) // hits start: 10
	fmt.Println("Incr:", fl.Incr())               // 11
	fmt.Println("Incr:", fl.Incr())               // 12
	fmt.Println("AddN 5:", fl.AddN(5))            // 17
	fmt.Println("final:", fl.Value())             // 17
}
