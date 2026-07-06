// Guards method promotion of a POINTER-embedded struct's BOX-receiver primary. Inner.Add takes the
// address of a receiver field (`&n.total`), so the converter emits it as a direct-ж extension
// `Add(this ж<Inner> …)`. Outer embeds *Inner, so Go promotes Add to Outer — but the generator's
// promoted-receiver harvest only collected VALUE-receiver methods, so `o.Add(...)` had no forwarder
// on Outer (CS1929). Same shape as crypto/x/sha3's cshakeState embedding *state (state.Write is
// `this ж<state>`). The pointer embed makes `target.Inner` a ж<Inner>, so the forwarder binds directly.
package main

import "fmt"

type Inner struct {
	total int
}

// takes &n.total → box-receiver primary
func (n *Inner) Add(x int) {
	p := &n.total
	*p += x
}

type Outer struct {
	*Inner
	tag string
}

func main() {
	o := &Outer{Inner: &Inner{}, tag: "t"}
	o.Add(5) // promoted box-receiver method
	o.Add(3)
	fmt.Println(o.total, o.tag) // 8 t
}
