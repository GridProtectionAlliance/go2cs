package main

import "fmt"

// grabber is an interface-typed VALUE field on the receiver, mirroring
// database/sql (*Stmt).cg (a stmtConnGrabber interface).
type grabber interface {
	tag() string
}

type connGrab struct{ name string }

func (c *connGrab) tag() string { return c.name }

// dep's pointer-receiver method takes the receiver's identity, which forces the
// enclosing method to be direct-ж (mirrors s.db.addDep(s, ...)).
type dep struct{ label string }

func (d *dep) note(x any) { fmt.Println("note:", d.label, x != nil) }

type stmt struct {
	d  *dep    // pointer field
	cg grabber // interface VALUE field -- the s.cg analog
	id int
}

// runWith mirrors s.db.retry(func(){...}): it takes a closure argument.
func (s *stmt) runWith(fn func()) { fn() }

// exec is the (*Stmt).QueryContext analog. It is direct-ж (its identity is
// captured below). Inside the closure passed to runWith there is a NESTED
// closure; the receiver field reads AFTER that nested closure must still render
// through the receiver box (Ꮡs.Value.cg) -- a bare ref-local there is CS8175.
func (s *stmt) exec() {
	s.runWith(func() {
		s.d.note(s) // captures s's identity -> exec is direct-ж
		s.runWith(func() {
			s.d.note(s) // nested closure; its exit must not clobber box state
		})
		// AFTER the nested closure: a NON-CALL receiver field read, then a
		// method call on the field, then another field read.
		if s.cg != nil {
			fmt.Println("cg:", s.cg.tag(), s.id)
		}
	})
}

func main() {
	s := &stmt{d: &dep{label: "d1"}, cg: &connGrab{name: "g1"}, id: 7}
	s.exec()
	fmt.Println("done")
}
