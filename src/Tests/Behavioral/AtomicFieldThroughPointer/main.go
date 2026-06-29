package main

import "fmt"

// A pointer-receiver method called on a VALUE field reached through a POINTER FIELD — `o.h.wait.add()`
// where `o.h` is a `*holder` field and `wait` is a value field. The pointer deref is an rvalue, so
// the value field on it is not addressable and the [GoRecv] ref / ж overload cannot bind
// (CS1510 / CS1929). The receiver must route through the box-field accessor `o.h.of(holder.Ꮡwait)`,
// which aliases the REAL storage — NOT a `Ꮡ(value)` copy, which would silently lose the write.
// Mirrors runtime's atomic fields reached through pointer chains (`sgp.g.selectDone.CompareAndSwap`,
// `gp.m.mLockProfile.recordLock`). The deeper `o.deep.prof.wait` case (a value field of a value field
// of a pointer field) recurses to the pointer root: `o.deep.of(holder.Ꮡprof).of(profile.Ꮡwait)`,
// mirroring `gp.m.mLockProfile.waitTime`. The mutate-then-read below proves the real fields update.

type atom struct{ v int64 }

func (a *atom) add(d int64) { a.v += d } // pointer receiver
func (a *atom) get() int64  { return a.v }

type profile struct{ wait atom }

type holder struct {
	wait atom    // value field (one level deep through the pointer)
	prof profile // value struct field (two levels deep)
}

type owner struct {
	h    *holder // pointer field
	deep *holder // a second pointer field, used for the nested chain
}

func bump(o *owner, d int64) {
	o.h.wait.add(d)        // one level: o.h.of(holder.Ꮡwait)
	o.deep.prof.wait.add(d) // two levels: o.deep.of(holder.Ꮡprof).of(profile.Ꮡwait)
}

func main() {
	o := &owner{h: &holder{}, deep: &holder{}}
	bump(o, 5)
	bump(o, 5)
	fmt.Println(o.h.wait.get())        // 10
	fmt.Println(o.deep.prof.wait.get()) // 10
}
