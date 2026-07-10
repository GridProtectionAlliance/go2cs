// IfaceFieldEmbedAdapter guards the pointer-interface adapter for a struct with SEVERAL embedded
// INTERFACE fields — httputil's `dumpConn struct { io.Writer; io.Reader }` adapted to net.Conn.
// Each promoted interface member must forward through the UNIQUE embedded field whose interface
// declares it (Pull → m_box.Value.source, Push → m_box.Value.sink); the old single-field gate
// skipped multi-field structs entirely, leaving the bare m_box forward (CS1061). Local interfaces
// stand in for io.Reader/io.Writer so the guard does not lean on the baseline io stub's surface.
// See docs/ConversionStrategies.md "Promoted methods through embedded interface fields".
package main

import "fmt"

type source interface {
	Pull() string
}

type sink interface {
	Push(s string)
}

type src struct {
	items []string
	pos   int
}

func (s *src) Pull() string {
	if s.pos >= len(s.items) {
		return ""
	}
	item := s.items[s.pos]
	s.pos++
	return item
}

type dst struct {
	log []string
}

func (d *dst) Push(s string) {
	d.log = append(d.log, s)
}

// duplex is the dumpConn shape: two embedded interface FIELDS, remaining members on the struct.
type duplex struct {
	source
	sink
}

func (d *duplex) Status() string {
	return "ok"
}

// conn mirrors the net.Conn shape: members satisfiable only through BOTH embedded fields plus one
// only the struct itself provides.
type conn interface {
	Pull() string
	Push(s string)
	Status() string
}

func main() {
	out := &dst{}
	d := &duplex{source: &src{items: []string{"alpha", "beta"}}, sink: out}
	var c conn = d
	c.Push(c.Pull())
	c.Push(c.Pull())
	fmt.Println(c.Status(), out.log)
}
