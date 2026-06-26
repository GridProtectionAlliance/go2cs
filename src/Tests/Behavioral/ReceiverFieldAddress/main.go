package main

import "fmt"

// Counter mirrors the sync/atomic typed-type pattern: a pointer-receiver method takes
// the address of a receiver field and passes it to a helper that mutates through it.
type Counter struct {
	n int32
}

func bump(p *int32, delta int32) int32 {
	*p += delta
	return *p
}

func (c *Counter) Add(delta int32) int32 { return bump(&c.n, delta) }
func (c *Counter) Set(v int32)           { *(&c.n) = v }
func (c *Counter) Get() int32            { return c.n }

func main() {
	var c Counter // value receiver; pointer-receiver methods called on it
	c.Set(100)
	fmt.Println("after Set:", c.Get())   // 100
	fmt.Println("Add 10:", c.Add(10))    // 110
	fmt.Println("Add 5:", c.Add(5))      // 115
	fmt.Println("final:", c.Get())       // 115
}
