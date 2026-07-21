package main

import "fmt"

// counter is embedded (PROMOTED) into holder, mirroring io.onceError embedding
// sync.Mutex: the promotion makes go2cs store the embed in a ж<counter> box, so
// default(holder) has a NULL box and any promoted access NREs.
type counter struct {
	n int
}

func (c *counter) inc() { c.n++ }

type holder struct {
	counter        // promoted embed -> ж<counter> box
	name    string // guards the shadowing rule (own field alongside the embed)
}

// slotBox has a plain VALUE field of the needy type `holder`, plus another field, so a
// PARTIAL composite literal drives the FIELD-WISE constructor (leaving `slot` defaulted) —
// the exact path io.Pipe uses when it builds a pipe omitting rerr/werr onceError fields.
type slotBox struct {
	id   int
	slot holder
}

func main() {
	// Partial literal -> field-wise ctor with `slot` omitted. In Go, slot is a usable
	// zero holder whose embedded counter is a usable zero. The UNFIXED generator left
	// slot's promotion box null, so the promoted inc() below NREs at runtime.
	b := slotBox{id: 3}
	b.slot.inc()
	b.slot.inc()
	b.slot.inc()
	fmt.Println("id:", b.id, "count:", b.slot.n, "name-empty:", b.slot.name == "")
}
