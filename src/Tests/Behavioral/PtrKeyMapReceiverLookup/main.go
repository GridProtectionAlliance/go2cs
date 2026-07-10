package main

import "fmt"

type conn struct {
	id int
}

type tracker struct {
	m map[*conn]string
}

// The PURE shape: the ONLY pointer-use of the receiver is the map key — the
// method must still get its receiver box for the key position (net/http
// transport.go closeConnIfStillIdle's `t.idleLRU.m[pc]` comma-ok read passed
// the VALUE where ж<persistConn> was expected).
func (c *conn) status(t *tracker) string {
	if s, ok := t.m[c]; ok {
		return s
	}
	return "unknown"
}

// Plain read and WRITE through the receiver key.
func (c *conn) rename(t *tracker, s string) {
	t.m[c] = s
}

func (c *conn) label(t *tracker) string {
	return t.m[c]
}

// The corpus shape: defer forces the receiver box independently; the comma-ok
// lookup and the delete both key by the receiver.
func (c *conn) close(t *tracker) {
	defer fmt.Println("closed", c.id)
	if _, ok := t.m[c]; ok {
		delete(t.m, c)
	}
}

func main() {
	a := &conn{id: 1}
	b := &conn{id: 2}
	t := &tracker{m: map[*conn]string{a: "idle"}}

	// Identity: only a's box matches; b is absent.
	fmt.Println(a.status(t), b.status(t))

	// Write through the receiver key, read back through the same box.
	a.rename(t, "busy")
	fmt.Println(a.label(t), len(t.m))

	// A second receiver writes its own entry (no collision with a's).
	b.rename(t, "new")
	fmt.Println(b.label(t), a.label(t), len(t.m))

	// Comma-ok + delete through the receiver inside a defer-carrying method.
	a.close(t)
	fmt.Println(len(t.m), a.status(t), b.status(t))
}
