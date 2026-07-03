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

// applyTwice drives a method value passed as a plain func value.
func applyTwice(f func(int32) int32, d int32) int32 {
	f(d)
	return f(d)
}

func readVia(get func() int32) int32 { return get() }

// Method VALUES over pointer-receiver methods (internal/godebug's IncNonDefault/register
// pattern): Go binds the receiver ADDRESS at method-value creation -- `c.Add` == `(&c).Add` --
// so the emitted C# binds the method group to the receiver box, never the deref'd value (CS1113).
func (c *Counter) AddTwice(d int32) int32 { return applyTwice(c.Add, d) }   // value on the receiver itself
func (f *Flag) AddViaValue(d int32) int32 { return applyTwice(f.c.Add, d) } // value on a receiver value field
func (f *Flag) ReadViaValue() int32       { return readVia(f.c.Get) }

func main() {
	var fl Flag
	fl.label = "hits"

	fl.Reset(10)
	fmt.Println(fl.Label(), "start:", fl.Value()) // hits start: 10
	fmt.Println("Incr:", fl.Incr())               // 11
	fmt.Println("Incr:", fl.Incr())               // 12
	fmt.Println("AddN 5:", fl.AddN(5))            // 17
	fmt.Println("final:", fl.Value())             // 17
	fmt.Println("AddTwice 3:", fl.c.AddTwice(3))         // 23
	fmt.Println("AddViaValue 2:", fl.AddViaValue(2))     // 27
	fmt.Println("ReadViaValue:", fl.ReadViaValue())      // 27
	fmt.Println("local value:", applyTwice(fl.c.Add, 1)) // 29
}
