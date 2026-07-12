package main

import "fmt"

// box is a PLAIN struct. Go's `b == nil` on a *box receiver is a POINTER comparison.
// The value-difference guard: &box{} is a NON-NIL pointer to a ZERO box, so `b == nil`
// is false. The pre-fix converter deref'd the receiver and emitted a VALUE comparison
// `box.operator==(box, NilType)` == `value.Equals(default(box))` == (0 == 0) == TRUE —
// the wrong answer, distinguishable by output without any crash.
type box struct{ val int }

func (b *box) isNil() bool  { return b == nil }
func (b *box) notNil() bool { return b != nil }

// same compares the pointer RECEIVER to another pointer — a pointer-IDENTITY comparison
// (not nil). The box form applies here too: Go compares pointer identity, so the receiver
// must render as its box `Ꮡb`, not the deref'd value.
func (b *box) same(other *box) bool { return b == other }

// embedder has a PROMOTED EMBED, so the pre-fix value comparison also NRE'd on the null
// embed box of default(embedder) — the exact os.File shape (File embeds *file).
type embedder struct {
	box     // promoted embed
	tag int
}

func (e *embedder) isNil() bool { return e == nil }

// checkGuard is the common idiom: guard on nil, then use the receiver's fields.
func (e *embedder) checkGuard() string {
	if e == nil {
		return "nil"
	}
	return fmt.Sprintf("val=%d tag=%d", e.val, e.tag)
}

func main() {
	b := &box{}
	fmt.Println(b.isNil(), b.notNil())     // false true — non-nil pointer to a zero struct
	fmt.Println(b.same(b), b.same(&box{}))  // true false — pointer identity vs a fresh pointer

	e := &embedder{}
	e.val = 7 // promoted field through the embed
	e.tag = 9
	fmt.Println(e.isNil(), e.checkGuard()) // false val=7 tag=9
}
