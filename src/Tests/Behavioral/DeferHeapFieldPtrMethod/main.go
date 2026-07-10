package main

import "fmt"

// tracker is a VALUE FIELD of parser below. flush() has a POINTER receiver, so Go implicitly
// takes &p.trk for `defer p.trk.flush()` — an address INTO the enclosing local's own storage.
type tracker struct {
	lines []string
}

// flush observes the FINAL state of the field — it must see every line appended AFTER the defer
// statement (Go binds &p.trk at defer time and calls flush() at function exit on the live
// variable). A value snapshot of the enclosing local taken at defer time would miss them.
func (t *tracker) flush() {
	fmt.Printf("flush: %d lines\n", len(t.lines))

	for _, l := range t.lines {
		fmt.Println("line:", l)
	}
}

// parser is the enclosing struct: the deferred receiver is a FIELD PROJECTION of a value local
// of this type (`p.trk`), not the local itself — the sibling shape of DeferHeapLocalPtrMethod.
type parser struct {
	name string
	trk  tracker
}

// seed takes the local's address, making it escape to the heap (a `ref heap` local with a
// companion box Ꮡp) — the same shape as go/internal/gcimporter's iImportData, whose
// `defer p.fake.setLines()` referenced the undeclared snapshot box `Ꮡpʗ1` (CS0103).
func seed(p *parser) {
	p.trk.lines = append(p.trk.lines, "seed")
}

// run defers a pointer-receiver method on a FIELD of the heap-boxed value local, then keeps
// mutating the field. The defer must bind the live box (`Ꮡp.of(parser.Ꮡtrk).flush`), so flush
// sees all three lines — not a snapshot copy of p taken at defer time.
func run() {
	p := parser{name: "p1"}
	seed(&p)
	defer p.trk.flush()
	p.trk.lines = append(p.trk.lines, "after-defer")
	p.trk.lines = append(p.trk.lines, "final")
	fmt.Println("run done:", p.name)
}

func main() {
	run()
}
