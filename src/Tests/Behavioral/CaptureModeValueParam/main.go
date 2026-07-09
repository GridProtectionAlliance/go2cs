// CaptureModeValueParam guards the ENTRY-TIME heap boxing of a VALUE PARAMETER on which a
// capture-mode (direct-ж) pointer-receiver method is called — go/format's format() calling
// cfg.Fprint(&buf, fset, file) on its `cfg printer.Config` value parameter (CS1929 ×2 before
// the fix: the method's only emitted receiver form is the box ж<T>, and parameters never got
// one). The converter renames the incoming parameter (`t` arrives as `tʗp`) and declares
// `ref var t = ref heap(tʗp, out var Ꮡt);` in the prologue, so body uses hit the boxed
// storage and the call routes through Ꮡt. The output checks WRITE-VISIBILITY in both
// directions — a body write before the call must be seen by the callee, and the callee's
// writes through the receiver pointer must be seen by the rest of the body — which is what
// distinguishes entry-time boxing from a call-site Ꮡ(value) copy-box (the copy form compiles
// but silently drops the callee's writes). The caller's copy must stay untouched (Go passes
// the parameter by value).
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

// AddTwice inherits direct-ж transitively (it calls the direct-ж Add on its own receiver) —
// the (*Config).Fprint → (*Config).fprint shape.
func (t *Tally) AddTwice(n int) {
	t.Add(n)
	t.Add(n)
}

// bump calls the direct-ж methods on a VALUE parameter: Go auto-addresses t (&t), so the
// callee's writes through the receiver pointer must be visible in t afterwards.
func bump(t Tally, n int) (int, string) {
	t.total++ // write BEFORE the calls: the callee must observe it (go/format's cfg.Indent = …)
	t.Add(n)
	t.AddTwice(n * 10)
	return t.total, t.log // reads AFTER the calls: must see the callee's writes
}

func main() {
	t := Tally{total: 5, log: "start"}
	total, log := bump(t, 3)
	fmt.Println("bumped:", total, log)
	fmt.Println("caller copy untouched:", t.total, t.log)
}
