package main

import "fmt"

// A pointer-receiver method called on a VALUE field reached through a POINTER FIELD — `o.h.wait.add()`
// where `o.h` is a `*holder` field and `wait` is a value field. The pointer deref is an rvalue, so
// the value field on it is not addressable and the [GoRecv] ref / ж overload cannot bind
// (CS1510 / CS1929). The receiver must route through the box-field accessor `o.h.of(holder.Ꮡwait)`,
// which aliases the REAL storage — NOT a `Ꮡ(value)` copy, which would silently lose the write.
// Mirrors runtime's atomic fields reached through pointer chains (`sgp.g.selectDone.CompareAndSwap`,
// `gp.m.mLockProfile.recordLock`). The mutate-then-read below proves the real field is updated.

type atom struct{ v int64 }

func (a *atom) add(d int64) { a.v += d } // pointer receiver
func (a *atom) get() int64  { return a.v }

type holder struct {
	wait atom // value field
}

type owner struct {
	h *holder // pointer field
}

func bump(o *owner, d int64) {
	o.h.wait.add(d) // o.h is *holder; wait is a value field; add is pointer-receiver
}

func main() {
	o := &owner{h: &holder{}}
	bump(o, 5)
	bump(o, 5)
	fmt.Println(o.h.wait.get()) // 10 — the real field, mutated through the pointer chain
}
