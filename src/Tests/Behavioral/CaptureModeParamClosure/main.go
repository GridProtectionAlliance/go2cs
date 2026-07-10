// CaptureModeParamClosure guards the CLOSURE COMPOSITIONS over an entry-time heap-boxed VALUE
// PARAMETER (see CaptureModeValueParam for the box itself): when the same function also contains
// a func literal or defer that references the boxed parameter, the in-lambda references must
// route through the box (`Ꮡt.Value…`) — never a snapshot copy (`var tʗ1 = t;`), which compiles
// but divorces the closure from the boxed storage the direct-ж callee mutates through the
// receiver pointer (Go's closure and callee share the ONE parameter variable), and never the
// ref-local alias itself (CS8175). Four compositions, each with write-visibility output checks:
// a closure READ must see the callee's later writes, a closure WRITE must be seen by the callee,
// a DEFERRED closure must see everything at return time, and a deferred direct-ж METHOD VALUE
// must write the same storage a sibling deferred observer reads. The caller's copy must stay
// untouched throughout (Go passes the parameter by value).
package main

import "fmt"

// Tally's pointer-receiver Add defers a write that touches the receiver, so its whole body is
// emitted inside the defer/recover execution context — the capture-mode (direct-ж) form whose
// only receiver overload is the box ж<Tally>.
type Tally struct {
	total int
	log   string
}

func (t *Tally) Add(n int) {
	defer func() {
		t.log = fmt.Sprintf("%s+%d", t.log, n)
	}()
	t.total += n
}

// closureRead: the func literal READS the boxed param; the read after t.Add must see the
// callee's write through the receiver pointer (a value snapshot would still read 5).
func closureRead(t Tally, n int) (int, int, string) {
	get := func() int { return t.total }
	before := get()
	t.Add(n)
	after := get()
	return before, after, t.log
}

// closureWrite: the func literal WRITES the boxed param; the callee must observe the closure's
// write, and the closure must observe the callee's write (a snapshot loses both directions).
func closureWrite(t Tally, n int) (int, string) {
	bump := func() { t.total += 100 }
	bump()
	t.Add(n)
	bump()
	return t.total, t.log
}

// deferClosure: a DEFERRED func literal reads the boxed param at return time, after both the
// direct-ж call and a plain body write.
func deferClosure(t Tally, n int) (result int, log string) {
	defer func() { result, log = t.total, t.log }()
	t.Add(n)
	t.total += 7
	return 0, ""
}

// deferMethodValue: DEFER of the direct-ж method value on the param itself; the earlier-
// registered (later-running) observer defer must see Add's writes to the same storage.
func deferMethodValue(t Tally, n int) (total int, log string) {
	defer func() { total, log = t.total, t.log }()
	defer t.Add(n)
	t.total++
	return 0, ""
}

func main() {
	t := Tally{total: 5, log: "start"}

	before, after, log := closureRead(t, 3)
	fmt.Println("closureRead:", before, after, log)

	total, log := closureWrite(t, 3)
	fmt.Println("closureWrite:", total, log)

	total, log = deferClosure(t, 3)
	fmt.Println("deferClosure:", total, log)

	total, log = deferMethodValue(t, 3)
	fmt.Println("deferMethodValue:", total, log)

	fmt.Println("caller copy untouched:", t.total, t.log)
}
