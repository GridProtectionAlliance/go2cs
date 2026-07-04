package main

import "fmt"

func next(now int64) (*int64, int64) {
	v := now * 2
	return &v, now + 1
}

// step exercises a MIXED short-declaration tuple: `pp, now := next(now)` reuses the
// existing value parameter `now` (a reassignment) and newly declares `pp`. Because `pp`
// is returned, escape analysis flags it as escaping, yet it is an already-pointer local
// that needs no heap box (convertToHeapTypeDecl yields nothing). Such an element must
// still receive a `var` in the C# deconstruction (`(var pp, now) = next(now);`) — earlier
// it was emitted as `(pp, now) = …` with `pp` never declared (CS0103). (Mirrors the
// runtime's `pp, now := pidleget(now)`, where `now int64` is a reused value param.)
func step(now int64) (*int64, int64) {
	pp, now := next(now)
	if pp == nil {
		return nil, now
	}
	return pp, now
}

func pair() (*int, *int) {
	a, b := 7, 9
	return &a, &b
}

// shadow exercises an ALL-declared SHADOWING tuple of escaping pointers: the inner
// `px, py := pair()` shadows the outer px/py; both are newly declared (and returned, so
// escaping with no heap box) → must emit a blanket `var (pxΔ, pyΔ) = pair();`.
func shadow() (*int, *int) {
	x := 100
	y := 200
	px, py := &x, &y
	if *px > 0 {
		px, py := pair()
		return px, py
	}
	return px, py
}

type cursor struct {
	pos int
	tag string
}

func makeCursor(pos int, tag string) (cursor, error) {
	return cursor{pos: pos, tag: tag}, nil
}

func bump(c *cursor) {
	c.pos++
}

// streams exercises a MIXED short-declaration tuple whose NEW element needs a HEAP BOX:
// `rbr2, err := makeCursor(…)` reuses `err` and newly declares the struct value `rbr2`,
// whose address is later taken (`bump(&rbr2)`) — the element must get its heap decl
// (`ref var rbr2 = ref heap<cursor>(out var Ꮡrbr2);`) before the deconstruction
// assignment. Earlier the escaping-element pass only ran for ALL-new tuples, so `rbr2`
// was never declared at all (internal/zstd's reverse bit readers, CS0103 ×9).
func streams() (int, int) {
	rbr1, err := makeCursor(10, "one")
	if err != nil {
		return 0, 0
	}
	rbr2, err := makeCursor(20, "two")
	if err != nil {
		return 0, 0
	}
	bump(&rbr1)
	bump(&rbr2)
	bump(&rbr2)
	return rbr1.pos, rbr2.pos
}

func main() {
	a, b := step(5)
	fmt.Println(*a, b)
	c, d := shadow()
	fmt.Println(*c, *d)
	e, f := streams()
	fmt.Println(e, f)
}
