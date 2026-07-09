package main

import "fmt"

// A variadic INTERFACE parameter (`...Shape`) receiving pointer elements that implement the
// interface via POINTER receivers: every trailing argument must get the *T→interface adapter
// wrap, not just the first (the declared-parameter index). go/types' builtins.go hit this with
// `makeSig(S, S, NewSlice(T))` — the ж<Slice> call result in the SECOND variadic slot passed
// loose and failed CS1503 while the first slot converted.

type Shape interface {
	Area() int
}

type Rect struct {
	w, h int
}

func (r *Rect) Area() int {
	return r.w * r.h
}

type Circle struct {
	r int
}

func (c *Circle) Area() int {
	return 3 * c.r * c.r
}

// newRect mirrors the go/types shape: a call RESULT (a pointer) landing in a variadic slot.
func newRect(w, h int) *Rect {
	return &Rect{w: w, h: h}
}

func totalArea(scale int, shapes ...Shape) int {
	sum := 0

	for _, s := range shapes {
		sum += s.Area()
	}

	return sum * scale
}

func main() {
	// Trailing args past the first: call results and addressed composite literals.
	fmt.Println(totalArea(2, newRect(3, 4), &Circle{r: 2}, newRect(1, 5)))

	// A value pointer local mixed in after an already-interface arg.
	r := &Rect{w: 4, h: 2}
	var s Shape = &Circle{r: 3}
	fmt.Println(totalArea(1, s, r))

	// Empty variadic call.
	fmt.Println(totalArea(3))

	// Spread form stays a slice pass-through (no per-element adapter).
	shapes := []Shape{&Rect{w: 2, h: 2}, &Circle{r: 1}}
	fmt.Println(totalArea(1, shapes...))
}
