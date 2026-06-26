package main

import "fmt"

// Box mirrors the atomic.Pointer[T] shape: a GENERIC pointer-receiver type whose methods
// take the address of a receiver field (&b.v) and mutate through it. A static ThreadLocal
// capture cannot hold the receiver's type parameter, so go2cs emits these with the heap box
// AS the receiver (direct-ж): `this ж<Box<T>> Ꮡb` + `Ꮡb.of(Box<T>.Ꮡv)`. Calling such a
// method on a value receiver must heap-box the value and route through the ж overload.
type Box[T any] struct {
	v T
}

func setT[T any](p *T, val T) { *p = val }
func getT[T any](p *T) T      { return *p }

func (b *Box[T]) Set(val T) { setT(&b.v, val) }
func (b *Box[T]) Get() T    { return getT(&b.v) }

func main() {
	var bi Box[int] // value receiver
	bi.Set(42)
	bi.Set(bi.Get() + 1)
	fmt.Println("int:", bi.Get()) // 43

	var bs Box[string]
	bs.Set("hello")
	fmt.Println("string:", bs.Get()) // hello
}
