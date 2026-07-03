package main

import "fmt"

type counter struct {
	n int
}

// inc is DIRECT-ж: it takes the address of a field of its receiver (&c.n), so it is emitted as a
// box-receiver method `inc(this ж<counter>)`. Called on an array ELEMENT it therefore needs the
// element's box, not the element value — `h.arr[i].inc()` must route through `Ꮡ` / `.at<T>(i)`,
// else the box-receiver overload cannot bind (CS1929). Mirrors the runtime's `bh.Value[i].Load()`
// (an array of atomic UnsafePointer, whose Load is direct-ж via `&u.value`).
func (c *counter) inc() {
	p := &c.n
	*p++
}

// get is an ordinary [GoRecv] pointer-receiver method (no field address taken) — it binds to an
// addressable element directly and must NOT be routed through a box (would be needless churn).
func (c *counter) get() int { return c.n }

type holder struct {
	arr [3]counter
}

// bump indexes an array field of a POINTER parameter and calls the direct-ж method on the element.
func bump(h *holder, i int) {
	h.arr[i].inc()
}

func main() {
	h := &holder{arr: [3]counter{{}, {}, {}}}
	bump(h, 0)
	bump(h, 0)
	bump(h, 1)
	bump(h, 1)
	bump(h, 1)
	fmt.Println(h.arr[0].get(), h.arr[1].get(), h.arr[2].get())
}
