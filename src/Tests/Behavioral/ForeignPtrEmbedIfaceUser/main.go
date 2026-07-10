// ForeignPtrEmbedIfaceUser is the consumer half of the foreign-pointer-embed interface-adapter
// guard. It adapts (1) a LOCAL struct embedding a FOREIGN pointer to an interface by VALUE and by
// POINTER (the net/http http2timeTimer and FlushAfterChunkWriter shapes), and (2) a FOREIGN struct
// with TWO pointer embeds by pointer (the bufio.ReadWriter shape) — all promoted methods live on
// the embeds' ж-extensions, visible here only through metadata.
package main

import (
	"fmt"

	"ForeignPtrEmbedIfaceLib"
)

// accumulator is satisfied by meterBox purely through the promoted *Meter methods.
type accumulator interface {
	Add(delta int) int
	Total() int
}

// combo needs members from BOTH of Pair's pointer embeds — each must route to the unique embed
// declaring it.
type combo interface {
	Add(delta int) int
	Set(v int)
	Get() int
}

// meterBox embeds a FOREIGN pointer — the local-struct half of the guard.
type meterBox struct {
	*ForeignPtrEmbedIfaceLib.Meter
}

func main() {
	// VALUE-form conversion: the generated partial struct forwards Add/Total through the
	// embedded ж<Meter> box; the interface value's copy still aliases the same Meter.
	box := meterBox{Meter: ForeignPtrEmbedIfaceLib.NewMeter()}
	var a accumulator = box
	a.Add(5)
	a.Add(7)
	fmt.Println("value:", a.Total(), box.Total())

	// POINTER-form conversion of the same local struct: the IжAdapter forwards through
	// m_box.Value.Meter.
	var p accumulator = &box
	p.Add(10)
	fmt.Println("pointer:", p.Total(), box.Total())

	// FOREIGN struct with TWO pointer embeds adapted by pointer: Add routes to Meter,
	// Set/Get to Gauge.
	pair := ForeignPtrEmbedIfaceLib.NewPair()
	var c combo = pair
	c.Add(3)
	c.Set(42)
	fmt.Println("pair:", c.Get(), c.Add(1), pair.Total())
}
